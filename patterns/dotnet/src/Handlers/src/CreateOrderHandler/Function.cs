using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using CreateOrderHandler.Customer;
using CreateOrderHandler.Orders;
using MessageProcessor.Orders;
using MessageProcessor.Shared;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace CreateOrderHandler;

public class Function
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly IOrderRepository _orderRepository;

    public Function() : this(null, null)
    {
        AWSSDKHandler.RegisterXRayForAllServices();
    }

    public Function(IMessagePublisher? messagePublisher, IOrderRepository? orderRepository)
    {
        _messagePublisher = messagePublisher ?? new MessagePublisher();
        _orderRepository = orderRepository ?? new DynamoDbOrderRepository(new AmazonDynamoDBClient());
    }
    
    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent input)
    {
        var failures = new List<SQSBatchResponse.BatchItemFailure>();
        
        foreach (var record in input.Records)
        {
            try
            {
                await ProcessMessage(record);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failure processing message");
                
                failures.Add(new SQSBatchResponse.BatchItemFailure
                {
                    ItemIdentifier = record.MessageId
                });
            }
        }

        return new SQSBatchResponse(failures);
    }

    private async Task ProcessMessage(SQSEvent.SQSMessage record)
    {
        Logger.LogInformation($"Inbound request is {record.Body}");
        Logger.LogInformation($"Message attributes {JsonSerializer.Serialize(record.MessageAttributes)}");
        
        var request = JsonSerializer.Deserialize<CreateOrderCommand>(record.Body);

        if (request == null)
        {
            throw new Exception("CreateOrderCommand cannot be null");
        }

        var customer = await new GetCustomerByNameQuery(request.OrderData.CustomerName).Send();
        
        Logger.LogInformation($"Received request for order '{request.OrderId}'");

        var order = new Order(request.OrderId)
        {
            Customer = new CustomerDetail(customer.CustomerId, customer.FirstName, customer.LastName),
            Items = request.OrderData.OrderItems.Select(p => new OrderItem(p.Key, 10, p.Value)).ToList(),
        };

        // Errors here for demonstration of dead letter queues.
        if (Environment.GetEnvironmentVariable("FORCE_FAILURE") == "Y" && order.Customer.CustomerFirstName == "failure")
        {
            throw new Exception("Forced failure");
        }

        if (!string.IsNullOrEmpty(request.OrderData.DiscountCode))
        {
            var discountAmount = 20;
            order.ApplyDiscount(discountAmount);
        }

        await this._orderRepository.Store(order);
    }
}

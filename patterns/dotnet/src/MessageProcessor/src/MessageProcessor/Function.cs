using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;
using MessageProcessor.Customer;
using MessageProcessor.Orders;
using MessageProcessor.Shared;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace MessageProcessor;

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
        var request = JsonSerializer.Deserialize<CreateOrderCommand>(record.Body);

        if (request == null)
        {
            throw new Exception("CreateOrderCommand cannot be null");
        }
        
        Tracing.AddAnnotation("orderId", request.OrderId);
        Tracing.AddAnnotation("customerName", request.OrderData.CustomerName);

        var customer = await new GetCustomerByNameQuery(request.OrderData.CustomerName).Send();

        var order = new Order(request.OrderId)
        {
            CustomerId = customer.CustomerId,
            Items = request.OrderData.OrderItems.Select(p => new OrderItem()
                { Price = 10, ProductCode = p.Key, Quantity = p.Value }).ToList(),
            CustomerFirstName = customer.FirstName,
            CustomerLastName = customer.LastName
        };

        await this._orderRepository.Store(order);

        await _messagePublisher.Publish(new OrderCreatedEvent
        {
            OrderId = order.OrderId,
            CustomerName = request.OrderData.CustomerName,
            TotalValue = order.Items.Sum(p => p.Price),
            Items = order.Items,
        });

        if (string.IsNullOrEmpty(request.OrderData.DiscountCode))
            return;

        var discountPercentage = 20;

        order.DiscountApplied = discountPercentage;
        order.AmountToPay = order.TotalValue * (1 - (discountPercentage / 100));

        await this._messagePublisher.Publish(new OrderDiscountAppliedEvent
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            DiscountAmount = order.TotalValue - order.AmountToPay,
            PriceBeforeDiscount = order.TotalValue,
            PriceAfterDiscount = order.AmountToPay,
        });
    }
}

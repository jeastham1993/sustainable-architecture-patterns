using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AWS.Lambda.Powertools.Logging;
using CreateOrderHandler.Orders;
using MessageProcessor.Orders;
using MessageProcessor.Shared;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace OrderStatusChangeEventPublisher;

public class Function
{
    private readonly IMessagePublisher _messagePublisher;

    public Function() : this(null)
    {
        AWSSDKHandler.RegisterXRayForAllServices();
    }

    public Function(IMessagePublisher? messagePublisher)
    {
        _messagePublisher = messagePublisher ?? new MessagePublisher();
    }
    
    public async Task FunctionHandler(DynamoDBEvent input)
    {
        foreach (var record in input.Records)
        {
            try
            {
                await ProcessMessage(record);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failure processing message");
            }
        }
    }

    private async Task ProcessMessage(DynamoDBEvent.DynamodbStreamRecord record)
    {
        if (record.EventName == OperationType.INSERT)
        {
            Logger.LogInformation("Order created event");
            
            
            var order = JsonSerializer.Deserialize<Order>(Document.FromAttributeMap(record.Dynamodb.NewImage["Data"].M)
                .ToJson());
            
            Logger.LogInformation($"Order is {order.OrderId}, publishing");

            await this._messagePublisher.Publish(new OrderCreatedEvent()
            {
                OrderId = order.OrderId,
                CustomerId = order.Customer.CustomerId,
                TotalValue = order.TotalValue,
                Items = order.Items,
            });
        }
    }
}

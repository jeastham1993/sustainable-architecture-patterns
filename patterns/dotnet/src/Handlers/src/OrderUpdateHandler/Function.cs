using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using MessageProcessor.Orders;
using MessageProcessor.Shared;
using OrderUpdateHandler.Customer;
using OrderUpdateHandler.Orders;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace OrderUpdateHandler;

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
    
    public async Task FunctionHandler(SQSEvent input)
    {
        foreach (var record in input.Records)
        {
            var request = JsonSerializer.Deserialize<UpdateOrderCommand>(record.Body);

            var order = await new GetOrderQuery(request.OrderId).Send();

            var updatedOrder = new Order(order.OrderId)
            {
                CustomerId = order.CustomerId,
                CustomerFirstName = request.CustomerFirstName,
                CustomerLastName = request.CustomerLastName,
                Items = order.Items
            };

            await this._messagePublisher.Publish(new OrderUpdatedEvent()
                { OldOrder = order, NewOrder = updatedOrder });
        }
    }
}

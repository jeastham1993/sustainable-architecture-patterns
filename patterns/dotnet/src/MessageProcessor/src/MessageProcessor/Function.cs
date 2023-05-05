using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using MessageProcessor.Customer;
using MessageProcessor.Orders;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace MessageProcessor;

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
            var request = JsonSerializer.Deserialize<CreateOrderRequest>(record.Body);

            var customer = await _messagePublisher.Query(new GetCustomerByName(request.CustomerName));

            await _messagePublisher.Send(new CreateOrderCommand(request.CustomerName));

            await _messagePublisher.Publish(new OrderCreatedEvent()
            {
                CustomerName = request.CustomerName,
                OrderId = Guid.NewGuid().ToString(),
                OrderNumber = "ORDER1234"
            });
        }
    }
}

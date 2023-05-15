using System.Text.Json;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using AWS.Lambda.Powertools.Logging;
using AWS.Lambda.Powertools.Tracing;

namespace MessageProcessor.Shared;

public class MessagePublisher : IMessagePublisher
{
    private readonly AmazonEventBridgeClient _eventBridgeClient;
    private readonly AmazonSimpleNotificationServiceClient _snsClient;
    private readonly AmazonSQSClient _sqsClient;

    public MessagePublisher()
    {
        this._eventBridgeClient = new AmazonEventBridgeClient();
        this._snsClient = new AmazonSimpleNotificationServiceClient();
        this._sqsClient = new AmazonSQSClient();
    }
    
    public async Task Send<T>(T message) where T : Command
    {
        var wrapper = new MessageWrapper<T>(message, message.ResponseChannelEndpoint);
        
        Logger.LogInformation($"Sending message to {message.MessageChannelEndpoint}:");
        Logger.LogInformation(JsonSerializer.Serialize(wrapper));
        
        if (message.MessageChannelEndpoint.Contains("sns"))
        {
            await this._snsClient.PublishAsync(new PublishRequest(message.MessageChannelEndpoint,
                JsonSerializer.Serialize(wrapper)));
        }
        else if (message.MessageChannelEndpoint.Contains("sqs"))
        {
            await this._sqsClient.SendMessageAsync(message.MessageChannelEndpoint, JsonSerializer.Serialize(wrapper));
        }
    }
    
    public async Task Publish<T>(T message) where T : Message
    {
        var wrapper = new MessageWrapper<T>(message);
        
        Logger.LogInformation($"Publishing message to '{Environment.GetEnvironmentVariable("EVENT_BUS_NAME")}'");
        Logger.LogInformation(JsonSerializer.Serialize(wrapper));
        
        await this._eventBridgeClient.PutEventsAsync(new PutEventsRequest()
        {
            Entries = new List<PutEventsRequestEntry>(1)
            {
                new PutEventsRequestEntry
                {
                    Detail = message.MessageType,
                    DetailType = JsonSerializer.Serialize(wrapper),
                    EventBusName = Environment.GetEnvironmentVariable("EVENT_BUS_NAME"),
                    Source = "com.orders",
                    TraceHeader = Tracing.GetEntity().TraceId,
                }
            }
        });
    }
}
using System.Text.Json;
using AWS.Lambda.Powertools.Logging;
using MessageProcessor.Customer;

namespace MessageProcessor;

public class MessagePublisher : IMessagePublisher
{
    public async Task Send<T>(T message) where T : Command
    {
        var wrapper = new MessageWrapper<T>(message);
        
        Logger.LogInformation($"Sending message to {message.MessageChannelEndpoint}:");
        Logger.LogInformation(JsonSerializer.Serialize(wrapper));
    }
    
    public async Task Publish<T>(T message) where T : Message
    {
        var wrapper = new MessageWrapper<T>(message);
        
        Logger.LogInformation("Publishing message:");
        Logger.LogInformation(JsonSerializer.Serialize(wrapper));
    }

    public async Task<string> Query<TQuery>(TQuery message) where TQuery : Query
    {
        var wrapper = new MessageWrapper<TQuery>(message);
        
        Logger.LogInformation($"Querying on channel {message.RequestChannelEndpoint} and expecting a response to {message.ResponseChannelEndpoint}");
        Logger.LogInformation(JsonSerializer.Serialize(wrapper));

        return JsonSerializer.Serialize(new GetCustomerByNameResponse());
    }
}
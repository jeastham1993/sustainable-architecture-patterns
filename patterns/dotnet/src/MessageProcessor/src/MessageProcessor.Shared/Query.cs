using System.Text.Json.Serialization;

namespace MessageProcessor.Shared;

public abstract class Query : Message
{
    [JsonIgnore]
    public abstract string RequestChannelEndpoint { get; }
    
    [JsonIgnore]
    public abstract string ResponseChannelEndpoint { get; }
}
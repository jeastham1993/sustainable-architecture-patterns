using System.Text.Json.Serialization;
using AWS.Lambda.Powertools.Tracing;

namespace MessageProcessor.Shared;

public class MessageWrapper<T> where T : Message
{
    public MessageWrapper(T data, string? responseChannel = null)
    {
        Metadata = new Metadata(data.MessageType, responseChannel);
        Data = data;
    }
    
    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; private set; }
    
    [JsonPropertyName("data")]
    public T Data { get; private set; }
}

public class Metadata
{
    public Metadata(string messageType, string? responseChannel = null)
    {
        MessageType = messageType;
        TraceParent = Tracing.GetEntity().TraceId;
        MessageId = Guid.NewGuid().ToString();
        DateSent = DateTime.Now;
        ResponseChannel = responseChannel;
    }
    
    [JsonPropertyName("traceparent")]
    public string TraceParent { get; private set; }
    
    [JsonPropertyName("messageId")]
    public string MessageId { get; private set; }
    
    [JsonPropertyName("messageType")]
    public string MessageType { get; }
    
    [JsonPropertyName("responseChannel")]
    public string? ResponseChannel { get; }
    
    [JsonPropertyName("dateSent")]
    public DateTime DateSent { get; }
}
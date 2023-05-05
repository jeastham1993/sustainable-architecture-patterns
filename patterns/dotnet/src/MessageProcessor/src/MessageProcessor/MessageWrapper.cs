using System.Text.Json.Serialization;
using AWS.Lambda.Powertools.Tracing;

namespace MessageProcessor;

public class MessageWrapper<T> where T : Message
{
    public MessageWrapper(T data)
    {
        Metadata = new Metadata(data.MessageType);
        Data = data;
    }
    
    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; private set; }
    
    [JsonPropertyName("data")]
    public T Data { get; private set; }
}

public class Metadata
{
    public Metadata(string messageType)
    {
        MessageType = messageType;
        TraceParent = Tracing.GetEntity().TraceId;
        MessageId = Guid.NewGuid().ToString();
    }
    
    [JsonPropertyName("traceparent")]
    public string TraceParent { get; private set; }
    
    [JsonPropertyName("messageId")]
    public string MessageId { get; private set; }
    
    [JsonPropertyName("messageType")]
    public string MessageType { get; }
}
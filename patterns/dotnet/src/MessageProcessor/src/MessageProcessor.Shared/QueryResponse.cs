using System.Text.Json.Serialization;

namespace MessageProcessor.Shared;

public class QueryResponse<T>
{
    [JsonPropertyName("payload")]
    public T Payload { get; set; }
}
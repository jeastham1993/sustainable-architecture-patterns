using System.Text.Json.Serialization;

namespace MessageProcessor;

public abstract class Message
{
    [JsonIgnore]
    public abstract string MessageType { get; }
}
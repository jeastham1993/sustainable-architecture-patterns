using System.Text.Json.Serialization;

namespace MessageProcessor.Shared;

public abstract class Message
{
    [JsonIgnore]
    public abstract string MessageType { get; }

    [JsonIgnore]
    public abstract string Version { get; }
}
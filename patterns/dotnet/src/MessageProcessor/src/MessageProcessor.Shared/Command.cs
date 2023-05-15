namespace MessageProcessor.Shared;

public abstract class Command : Message
{
    public abstract string MessageChannelEndpoint { get; }
    
    public abstract string? ResponseChannelEndpoint { get; }
}
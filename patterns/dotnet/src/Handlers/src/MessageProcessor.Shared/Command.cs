namespace MessageProcessor.Shared;

public abstract class Command : Message
{
    public abstract string? MessageChannelEndpoint { get; set; }
    
    public abstract string? ResponseChannelEndpoint { get; set; }
}
namespace MessageProcessor;

public abstract class Command : Message
{
    public abstract string MessageChannelEndpoint { get; }
}
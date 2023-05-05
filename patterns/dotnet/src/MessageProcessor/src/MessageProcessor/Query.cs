namespace MessageProcessor;

public abstract class Query : Message
{
    public abstract string RequestChannelEndpoint { get; }
    
    public abstract string ResponseChannelEndpoint { get; }
}
namespace MessageProcessor;

public interface IMessagePublisher
{
    Task Send<T>(T message) where T : Command;
    
    Task Publish<T>(T message) where T : Message;
    
    Task<string> Query<TQuery>(TQuery message) where TQuery : Query;
}
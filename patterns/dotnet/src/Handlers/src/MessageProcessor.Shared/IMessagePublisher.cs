namespace MessageProcessor.Shared;

public interface IMessagePublisher
{
    Task SendCommand<T>(T message) where T : Command;
    
    Task SendQuery<T>(T message) where T : Query;
    
    Task Publish<T>(T message) where T : Event;
}
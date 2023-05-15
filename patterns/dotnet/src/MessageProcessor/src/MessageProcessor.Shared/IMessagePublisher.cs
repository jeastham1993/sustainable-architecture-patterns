namespace MessageProcessor.Shared;

public interface IMessagePublisher
{
    Task Send<T>(T message) where T : Command;
    
    Task Publish<T>(T message) where T : Message;
}
using System.Text.Json;
using Amazon.Lambda.SQSEvents;
using MessageProcessor.Customer;
using MessageProcessor.Orders;
using Moq;
using Xunit;

namespace MessageProcessor.Tests;

public class FunctionTest
{
    private readonly Mock<IMessagePublisher> mockMessagePublisher;

    private List<MessageWrapper<CreateOrderCommand>> sentMessage = new();
    private List<MessageWrapper<OrderCreatedEvent>> publishedMessages = new();
    private List<MessageWrapper<GetCustomerByName>> queriesSent = new();
    
    public FunctionTest()
    {
        mockMessagePublisher = new Mock<IMessagePublisher>();

        mockMessagePublisher.Setup(p => p.Send(It.IsAny<Command>()))
            .Callback((Command e) => { sentMessage.Add(new MessageWrapper<CreateOrderCommand>(e as CreateOrderCommand)); });

        mockMessagePublisher.Setup(p => p.Publish(It.IsAny<Message>()))
            .Callback((Message e) => { publishedMessages.Add(new MessageWrapper<OrderCreatedEvent>(e as OrderCreatedEvent)); });

        mockMessagePublisher.Setup(p => p.Query(It.IsAny<Query>()))
            .Callback((Query e) => { queriesSent.Add(new MessageWrapper<GetCustomerByName>(e as GetCustomerByName)); });
    }
    
    [Fact]
    public async Task TestMessagePublishing()
    {
        // Arrange
        var function = new Function(mockMessagePublisher.Object);
        var testSqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>(1)
            {
                new SQSEvent.SQSMessage
                {
                    Body = JsonSerializer.Serialize(new CreateOrderRequest())
                }
            }
        };
        
        // Act
        await function.FunctionHandler(testSqsEvent);

        Assert.Single(sentMessage);
        Assert.Single(publishedMessages);
        Assert.Single(queriesSent);

        var sentCommand = JsonSerializer.Serialize(sentMessage[0]);
        var publishedEvent = JsonSerializer.Serialize(publishedMessages[0]);
        var query = JsonSerializer.Serialize(queriesSent[0]);
    }
}

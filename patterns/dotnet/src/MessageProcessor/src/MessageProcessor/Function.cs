using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using MessageProcessor.Customer;
using MessageProcessor.Orders;
using MessageProcessor.Shared;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace MessageProcessor;

public class Function
{
    private readonly IMessagePublisher _messagePublisher;

    public Function() : this(null)
    {
        AWSSDKHandler.RegisterXRayForAllServices();
    }

    public Function(IMessagePublisher? messagePublisher)
    {
        _messagePublisher = messagePublisher ?? new MessagePublisher();
    }
    
    public async Task FunctionHandler(SQSEvent input)
    {
        foreach (var record in input.Records)
        {
            var request = JsonSerializer.Deserialize<CreateOrderCommand>(record.Body);

            var customer = await new GetCustomerByNameQuery(request.CustomerName).Send();

            var order = new Order()
            {
                CustomerId = customer.CustomerId,
                Items = request.OrderItems.Select(p => new OrderItem()
                    { Price = 10, ProductCode = p.Key, Quantity = p.Value }).ToList(),
                CustomerFirstName = customer.FirstName,
                CustomerLastName = customer.LastName
            };

            await _messagePublisher.Publish(new OrderCreatedNotificationEvent()
            {
                OrderId = order.OrderId,
            });

            await _messagePublisher.Publish(new OrderCreatedEvent
            {
                OrderId = order.OrderId,
                CustomerName = request.CustomerName,
                TotalValue = order.Items.Sum(p => p.Price),
                Items = order.Items,
            });

            if (string.IsNullOrEmpty(request.DiscountCode)) 
                continue;
            
            var discountPercentage = 20;
                
            order.DiscountApplied = discountPercentage;
            order.AmountToPay = order.TotalValue * (1 - (discountPercentage / 100));

            await this._messagePublisher.Publish(new OrderDiscountAppliedEvent
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                DiscountAmount = order.TotalValue - order.AmountToPay,
                PriceBeforeDiscount = order.TotalValue,
                PriceAfterDiscount = order.AmountToPay,
            });
        }
    }
}

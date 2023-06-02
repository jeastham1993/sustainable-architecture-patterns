using System.Text.Json.Serialization;
using MessageProcessor.Orders;
using MessageProcessor.Shared;

namespace OrderUpdateHandler.Orders;

public class GetOrderQuery : Query
{
    [JsonIgnore]
    public override string MessageType => "get-order";

    [JsonIgnore]
    public override string Version => "1.0";

    public GetOrderQuery(string orderId)
    {
        OrderId = orderId;
    }
    
    [JsonPropertyName("orderId")]
    public string OrderId { get; }

    public override string RequestChannelEndpoint { get; }
    public override string ResponseChannelEndpoint { get; }

    public async Task<Order> Send()
    {
        return new Order("1234")
        {
            CustomerId = "JAMESEASTHAM",
            Items = new List<OrderItem>(1){ new OrderItem(){ProductCode = "PROD123", Price = 10, Quantity = 1}},
            CustomerFirstName = "James",
            CustomerLastName = "Eastham"
        };
    }
}
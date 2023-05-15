using System.Text.Json.Serialization;
using MessageProcessor.Shared;

namespace MessageProcessor.Orders;

public class OrderCreatedEvent : Message
{
    [JsonIgnore]
    public override string MessageType => "order-created";
    
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    
    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; }
    
    [JsonPropertyName("totalValue")]
    public decimal TotalValue { get; set; }
    
    [JsonPropertyName("items")]
    public List<OrderItem> Items { get; set; }
}
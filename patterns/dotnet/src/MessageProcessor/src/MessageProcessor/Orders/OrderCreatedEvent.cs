using System.Text.Json.Serialization;

namespace MessageProcessor.Orders;

public class OrderCreatedEvent : Message
{
    [JsonIgnore]
    public override string MessageType => "order-created";
    
    [JsonPropertyName("orderNumber")]
    public string OrderNumber { get; set; }
    
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    
    public string CustomerName { get; set; }
}
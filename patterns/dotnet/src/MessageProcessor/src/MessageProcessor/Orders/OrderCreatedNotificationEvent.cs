using System.Text.Json.Serialization;
using MessageProcessor.Shared;

namespace MessageProcessor.Orders;

public class OrderCreatedNotificationEvent : Message
{
    [JsonIgnore]
    public override string MessageType => "order-created";
    
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
}
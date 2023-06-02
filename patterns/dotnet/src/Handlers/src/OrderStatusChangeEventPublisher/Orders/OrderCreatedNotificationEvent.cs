using System.Text.Json.Serialization;
using MessageProcessor.Shared;

namespace CreateOrderHandler.Orders;

public class OrderCreatedNotificationEvent : Event
{
    [JsonIgnore]
    public override string MessageType => "order-created";

    [JsonIgnore]
    public override string Version => "1.0";
    
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
}
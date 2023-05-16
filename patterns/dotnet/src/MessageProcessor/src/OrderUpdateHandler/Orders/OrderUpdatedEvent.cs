using System.Text.Json.Serialization;
using MessageProcessor.Orders;
using MessageProcessor.Shared;

namespace OrderUpdateHandler.Orders;

public class OrderUpdatedEvent : Event
{
    [JsonIgnore]
    public override string MessageType => "order-updated";
    
    [JsonIgnore]
    public override string Version => "1.0";
    
    [JsonPropertyName("oldOrder")]
    public Order OldOrder { get; set; }
    
    [JsonPropertyName("newOrder")]
    public Order NewOrder { get; set; }
}
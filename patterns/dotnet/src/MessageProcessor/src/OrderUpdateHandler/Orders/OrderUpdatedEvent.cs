using System.Text.Json.Serialization;
using MessageProcessor.Orders;
using MessageProcessor.Shared;

namespace OrderUpdateHandler.Orders;

public class OrderUpdatedEvent : Message
{
    [JsonIgnore]
    public override string MessageType => "order-updated";
    
    [JsonPropertyName("oldOrder")]
    public Order OldOrder { get; set; }
    
    [JsonPropertyName("newOrder")]
    public Order NewOrder { get; set; }
}
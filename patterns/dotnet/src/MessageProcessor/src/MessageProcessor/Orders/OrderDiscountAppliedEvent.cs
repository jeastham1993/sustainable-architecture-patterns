using System.Text.Json.Serialization;
using MessageProcessor.Shared;

namespace MessageProcessor.Orders;

public class OrderDiscountAppliedEvent : Event
{
    public string CustomerId { get; set; }
    
    public string OrderId { get; set; }
    
    public decimal DiscountAmount { get; set; }
    
    public decimal PriceBeforeDiscount { get; set; }
    
    public decimal PriceAfterDiscount { get; set; }
    public override string MessageType => "order-discount-applied";

    [JsonIgnore]
    public override string Version => "1.0";
}
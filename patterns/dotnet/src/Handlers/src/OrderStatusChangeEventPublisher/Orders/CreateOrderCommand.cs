using System.Text.Json.Serialization;

namespace CreateOrderHandler.Orders;

public record CreateOrderCommand
{
    [JsonPropertyName("identifier")]
    public string OrderId { get; set; }
    
    [JsonPropertyName("payload")]
    public OrderData OrderData { get; set; }
}

public record OrderData
{
    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; }
    
    [JsonPropertyName("discountCode")]
    public string? DiscountCode { get; set; }
    
    [JsonPropertyName("orderItems")]
    public Dictionary<string, int> OrderItems { get; set; }
}
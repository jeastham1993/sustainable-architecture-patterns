using System.Text.Json.Serialization;
using MessageProcessor.Shared;

namespace CreateOrderHandler.Orders;

public class CreateOrderCommand : Command
{
    [JsonPropertyName("identifier")]
    public string OrderId { get; set; }
    
    [JsonPropertyName("payload")]
    public OrderData OrderData { get; set; }

    public override string MessageType => "create.order";
    public override string Version => "1";
    public override string? MessageChannelEndpoint { get; set; }
    public override string? ResponseChannelEndpoint { get; set; }
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
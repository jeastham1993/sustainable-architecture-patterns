using System.Text.Json.Serialization;

namespace MessageProcessor.Orders;

public record CreateOrderRequest
{
    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; }
}
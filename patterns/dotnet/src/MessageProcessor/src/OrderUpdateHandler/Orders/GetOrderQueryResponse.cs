using System.Text.Json.Serialization;
using MessageProcessor.Orders;
using MessageProcessor.Shared;

namespace OrderUpdateHandler.Orders;

public class GetOrderQueryResponse
{
    [JsonPropertyName("order")]
    public Order Order { get; }
}
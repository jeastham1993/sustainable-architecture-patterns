using System.Text.Json.Serialization;
using MessageProcessor.Shared;

namespace OrderUpdateHandler.Orders;

public class UpdateOrderCommand : Command
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    
    [JsonPropertyName("customerFirstName")]
    public string CustomerFirstName { get; set; }
    
    [JsonPropertyName("customerLastName")]
    public string CustomerLastName { get; set; }

    [JsonIgnore]
    public override string MessageType => "updated-order";
    
    [JsonIgnore]
    public override string MessageChannelEndpoint => Environment.GetEnvironmentVariable("UPDATE_ORDER_COMMAND_TOPIC");

    [JsonIgnore]
    public override string? ResponseChannelEndpoint => null;
}
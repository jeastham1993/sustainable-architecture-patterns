using System.Text.Json.Serialization;
using MessageProcessor.Shared;

namespace MessageProcessor.Orders;

public class CreateOrderCommand : Command
{
    public CreateOrderCommand(string customerName, Dictionary<string, int> orderItems)
    {
        CustomerName = customerName;
        OrderItems = orderItems;
    }
    
    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; }
    
    [JsonPropertyName("discountCode")]
    public string? DiscountCode { get; set; }
    
    [JsonPropertyName("orderItems")]
    public Dictionary<string, int> OrderItems { get; set; }

    [JsonIgnore]
    public override string MessageType => "create-order";
    
    [JsonIgnore]
    public override string MessageChannelEndpoint => Environment.GetEnvironmentVariable("CREATE_ORDER_COMMAND_TOPIC");

    [JsonIgnore]
    public override string? ResponseChannelEndpoint => null;
}
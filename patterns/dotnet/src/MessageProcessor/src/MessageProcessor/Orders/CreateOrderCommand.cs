using System.Text.Json.Serialization;

namespace MessageProcessor.Orders;

public class CreateOrderCommand : Command
{
    public CreateOrderCommand(string customerName)
    {
        CustomerName = customerName;
    }
    
    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; }

    [JsonIgnore]
    public override string MessageType => "create-order";
    
    [JsonIgnore]
    public override string MessageChannelEndpoint => Environment.GetEnvironmentVariable("CREATE_ORDER_COMMAND_TOPIC");
}
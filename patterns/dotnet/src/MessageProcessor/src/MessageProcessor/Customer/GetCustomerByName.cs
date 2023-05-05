using System.Text.Json.Serialization;

namespace MessageProcessor.Customer;

public class GetCustomerByName : Query
{
    [JsonIgnore]
    public override string MessageType => "getCustomerByName";

    public GetCustomerByName(string customerName)
    {
        CustomerName = customerName;
    }
    
    [JsonPropertyName("customerName")]
    public string CustomerName { get; }

    public override string RequestChannelEndpoint =>
        Environment.GetEnvironmentVariable("GET_CUSTOMER_BY_NAME_TOPIC_ENDPOINT");
    
    public override string ResponseChannelEndpoint =>
        Environment.GetEnvironmentVariable("GET_CUSTOMER_BY_NAME_RESPONSE_CHANNEL");
}
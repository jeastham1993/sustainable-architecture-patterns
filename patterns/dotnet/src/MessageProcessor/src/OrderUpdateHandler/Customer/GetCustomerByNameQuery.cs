using System.Text.Json.Serialization;
using MessageProcessor.Shared;

namespace OrderUpdateHandler.Customer;

public class GetCustomerByNameQuery : Query
{
    [JsonIgnore]
    public override string MessageType => "get-customer-by-name";
    
    [JsonIgnore]
    public override string Version => "1.0";

    public GetCustomerByNameQuery(string customerName)
    {
        CustomerName = customerName;
    }
    
    [JsonPropertyName("customerName")]
    public string CustomerName { get; }

    [JsonIgnore]
    public override string RequestChannelEndpoint =>
        Environment.GetEnvironmentVariable("GET_CUSTOMER_BY_NAME_TOPIC_ENDPOINT");
    
    [JsonIgnore]
    public override string ResponseChannelEndpoint => "https://com.orders/customer-response";

    // Dummy implementation ready for a propery async query implementation.
    public async Task<GetCustomerByNameResponse> Send()
    {
        return new GetCustomerByNameResponse()
        {
            CustomerId = "JAMES123",
            FirstName = "James",
            LastName = "Eastham"
        };
    }
}
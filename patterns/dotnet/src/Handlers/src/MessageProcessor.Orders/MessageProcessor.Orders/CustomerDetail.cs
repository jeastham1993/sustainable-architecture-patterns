using System.Text.Json.Serialization;

namespace MessageProcessor.Orders;

public class CustomerDetail
{
    [JsonConstructor]
    private CustomerDetail(){}

    public CustomerDetail(string customerId, string customerFirstName, string customerLastName)
    {
        this.CustomerId = customerId;
        this.CustomerFirstName = customerFirstName;
        this.CustomerLastName = customerLastName;
    }
    
    [JsonPropertyName("customerId")]
    public string CustomerId { get; private set; }
    
    [JsonPropertyName("customerFirstName")]
    public string CustomerFirstName { get; private set; }
    
    [JsonPropertyName("customerLastName")] 
    public string CustomerLastName { get; private set; }
}
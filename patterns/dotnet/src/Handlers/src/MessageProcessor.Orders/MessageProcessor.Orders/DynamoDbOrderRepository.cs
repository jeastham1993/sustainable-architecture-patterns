using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace MessageProcessor.Orders;

public class DynamoDbOrderRepository : IOrderRepository
{
    private readonly IAmazonDynamoDB _client;

    public DynamoDbOrderRepository(IAmazonDynamoDB client)
    {
        _client = client;
    }
    
    public async Task Store(Order order)
    {
        await this._client.PutItemAsync(Environment.GetEnvironmentVariable("TABLE_NAME"),
            new Dictionary<string, AttributeValue>(3)
            {
                { "PK", new AttributeValue($"ORDER#{order.OrderId}") },
                { "SK", new AttributeValue($"ORDER#{order.OrderId}") },
                {
                    "Data", new AttributeValue()
                    {
                        M = Document.FromJson(JsonSerializer.Serialize(order)).ToAttributeMap()
                    }
                }
            });
    }

    public async Task<Order> RetrieveOrder(string orderId)
    {
        var res = await this._client.GetItemAsync(
            Environment.GetEnvironmentVariable("TABLE_NAME"),
            new Dictionary<string, AttributeValue>(1)
            {
                { "PK", new AttributeValue($"ORDER#{orderId}") },
                { "SK", new AttributeValue($"ORDER#{orderId}") },
            });

        var order = JsonSerializer.Deserialize<Order>(Document.FromAttributeMap(res.Item["Data"].M).ToJson());

        return order;
    }
}
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
                { "PK", new AttributeValue(order.OrderId) },
                {
                    "Data", new AttributeValue()
                    {
                        M = Document.FromJson(JsonSerializer.Serialize(order)).ToAttributeMap()
                    }
                }
            });
    }
}
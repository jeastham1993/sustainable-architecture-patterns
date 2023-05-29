using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Events;
using ArchitecturePatterns.NET.CDK.Patterns.StorageFirstApi;
using Constructs;

namespace ArchitecturePatterns.NET.CDK;

public class DotnetStack : Stack
{
    internal DotnetStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        var eventBus = new EventBus(this, "AppPattensEventBus", new EventBusProps
        {
            EventBusName = "app-patterns-event-bus",
        });

        var orderDataStore = new Table(this, "OrderDataStore", new TableProps
        {
            TableName = "OrderDataStore",
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Attribute
            {
                Name = "PK",
                Type = AttributeType.STRING
            }
        });
        
        var createOrderEndpoint = new StorageFirstApi(
            this,
            "CreateOrderAPi",
            new StorageFirstApiProps(
                StorageType.WorkflowWithQueue,
                "CreateOrderEndpoint",
                "/order/new"));

        var messageProcessor = new MessageProcessor(this, "MessageProcessor", new MessageProcessorProps(createOrderEndpoint.Queue, eventBus, orderDataStore));

        var mainApiUrlOutput = new CfnOutput(
            this,
            "MainApiUrl",
            new CfnOutputProps
            {
                Value = createOrderEndpoint.Api.Url,
                Description = "Endpoint of the pre-configured storage first API.",
                ExportName = "MainApiUrl"
            });
    }
}
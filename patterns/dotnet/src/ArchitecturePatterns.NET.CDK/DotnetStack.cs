using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using ArchitecturePatterns.NET.CDK.Patterns.StorageFirstApi;
using Constructs;

namespace ArchitecturePatterns.NET.CDK;

public class DotnetStack : Stack
{
    internal DotnetStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var eventBus = new EventBus(this, "AppPattensEventBus", new EventBusProps
        {
            EventBusName = "app-patterns-event-bus",
        });
        
        var createOrderEndpoint = new StorageFirstApi(
            this,
            "CreateOrderAPi",
            new StorageFirstApiProps(
                StorageType.Queue,
                "CreateOrderEndpoint",
                "/order/new"));

        var updateOrderResource = createOrderEndpoint.Api.Root.AddResource("update");

        var updateOrderRoute = new StorageFirstRoute(
            this,
            "UpdateOrderRoute",
            new StorageFirstRouteProps(StorageType.Queue, "UpdateOrder", updateOrderResource));

        var messageProcessor = new MessageProcessor(this, "MessageProcessor", new MessageProcessorProps(createOrderEndpoint.Queue, eventBus));
        
        var updateOrderProcessor = new UpdateOrderHandler(this, "UpdateOrderHandler", new UpdateOrderHandlerProps(updateOrderRoute.Queue, eventBus));

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
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Events;
using ArchitecturePatterns.NET.CDK.Functions;
using ArchitecturePatterns.NET.CDK.Patterns.ApplicationRoute;
using ArchitecturePatterns.NET.CDK.Patterns.StorageFirstApi;
using Constructs;
using HttpMethod = Amazon.CDK.AWS.Lambda.HttpMethod;

namespace ArchitecturePatterns.NET.CDK;

public class OrdersStack : Stack
{
    internal OrdersStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        // Shared
        var persistence = new DataPersistence(this, "DataPersistence", new DataPersistenceProps("OrdersTable"));
        
        var eventBus = new EventBus(this, "AppPattensEventBus", new EventBusProps
        {
            EventBusName = "app-patterns-event-bus",
        });
        
        // Business Logic
        var createOrderHandler = new CreateOrderHandler(this, "CreateOrderHandler", new CreateOrderHandlerProps(eventBus, persistence.Table));
        var getOrderStatusHandler = new GetOrderStatusHandler(this, "GetOrderStatus",
            new GetOrderStatusHandlerProps(persistence.Table));
        var orderStatusChange = new OrderStatusChangeEventPublisher(this, "OrderStatusChangeEventPublisher", new OrderStatusChangeEventPublisherProps(eventBus, persistence.Table));

        // API
        var api = new RestApi(
            this,
            "OrdersApi",
            new RestApiProps
            {
                CloudWatchRole = true,
            });
        
        var createOrderEndpoint = new StorageFirstRoute(
            this,
            "CreateOrderAPi",
            new StorageFirstRouteProps(
                StorageType.WorkflowWithQueue,
                "CreateOrderEndpoint",
                api.Root,
                createOrderHandler.CreateOrderHandlerFunction));

        var getOrderResource = api.Root.AddResource("{orderId}");

        var getOrderStatusEndpoint = new ApplicationRoute(this, "GetOrderStatusRoute",
            new ApplicationRouteProps(getOrderStatusHandler.Function, getOrderResource, HttpMethod.GET));

        var mainApiUrlOutput = new CfnOutput(
            this,
            "MainApiUrl",
            new CfnOutputProps
            {
                Value = api.Url,
                Description = "Endpoint of the pre-configured storage first API.",
                ExportName = "MainApiUrl"
            });
    }
}
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.SQS;
using ArchitecturePatterns.NET.CDK.Orders.Functions;
using ArchitecturePatterns.NET.CDK.Patterns.ApplicationRoute;
using ArchitecturePatterns.NET.CDK.Patterns.StorageFirstApi;
using Constructs;
using HttpMethod = Amazon.CDK.AWS.Lambda.HttpMethod;

namespace ArchitecturePatterns.NET.CDK.Orders;

public class OrdersStack : Stack
{
    internal OrdersStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        var importEventBusArn = Fn.ImportValue("SharedEventBusArn");

        var eventBus = EventBus.FromEventBusArn(this, "SharedEventBus", importEventBusArn);

        // Shared
        var persistence = new DataPersistence(this, "DataPersistence", new DataPersistenceProps("OrdersTable"));

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

        var reprocessFailedMessages = new ReprocessFailedMessages(this, "ReprocessFailedMessages",
            new ReprocessFailedMessagesProps(createOrderEndpoint.ErrorQueue, createOrderEndpoint.Queue));

        var getOrderResource = api.Root.AddResource("{orderId}");

        var getOrderStatusEndpoint = new ApplicationRoute(this, "GetOrderStatusRoute",
            new ApplicationRouteProps(getOrderStatusHandler.Function, getOrderResource, HttpMethod.GET));

        var responseChannelExample = new Queue(this, "ResponseChannel");

        var mainApiUrlOutput = new CfnOutput(
            this,
            "MainApiUrl",
            new CfnOutputProps
            {
                Value = api.Url,
                Description = "Endpoint of the pre-configured storage first API.",
                ExportName = "MainApiUrl"
            });

        var responseChannelOutput = new CfnOutput(
            this,
            "ResponseChannelURL",
            new CfnOutputProps
            {
                Value = responseChannelExample.QueueUrl,
                Description = "Response channel URL",
                ExportName = "ResponseChannelURL"
            });
    }
}
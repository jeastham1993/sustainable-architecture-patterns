using System.Collections.Generic;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Constructs;
using XaasKit.CDK.AWS.Lambda.DotNet;

namespace ArchitecturePatterns.NET.CDK.Orders.Functions;

public record CreateOrderHandlerProps(IEventBus EventBus, ITable OrderDataStore);

public class CreateOrderHandler : Construct
{
    public IFunction CreateOrderHandlerFunction { get; private set; }
    public CreateOrderHandler(
        Construct scope,
        string id,
        CreateOrderHandlerProps props) : base(
        scope,
        id)
    {
        this.CreateOrderHandlerFunction = new DotNetFunction(this, "message-processor", new DotNetFunctionProps()
        {
            Runtime = Runtime.DOTNET_6,
            MemorySize = 1024,
            LogRetention = RetentionDays.ONE_DAY,
            Handler = "CreateOrderHandler::CreateOrderHandler.Function::FunctionHandler",
            ProjectDir = "src/Handlers/src/CreateOrderHandler/",
            Environment = new Dictionary<string, string>(1)
            {
                {"EVENT_BUS_NAME", props.EventBus.EventBusName},
                {"TABLE_NAME", props.OrderDataStore.TableName},
                {"POWERTOOLS_SERVICE_NAME", "orders"},
            },
            Tracing = Tracing.PASS_THROUGH,
        });

        props.EventBus.GrantPutEventsTo(CreateOrderHandlerFunction);
        props.OrderDataStore.GrantWriteData(CreateOrderHandlerFunction);
    }
}
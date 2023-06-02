using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SQS;
using Constructs;
using XaasKit.CDK.AWS.Lambda.DotNet;

namespace ArchitecturePatterns.NET.CDK.Functions;

public record OrderStatusChangeEventPublisherProps(IEventBus EventBus, ITable OrderDataStore);

public class OrderStatusChangeEventPublisher : Construct
{
    public IFunction Function { get; private set; }
    public OrderStatusChangeEventPublisher(
        Construct scope,
        string id,
        OrderStatusChangeEventPublisherProps props) : base(
        scope,
        id)
    {
        this.Function = new DotNetFunction(this, "OrderStatusChangeEventPublisher", new DotNetFunctionProps()
        {
            Runtime = Runtime.DOTNET_6,
            MemorySize = 1024,
            LogRetention = RetentionDays.ONE_DAY,
            Handler = "OrderStatusChangeEventPublisher::OrderStatusChangeEventPublisher.Function::FunctionHandler",
            ProjectDir = "src/Handlers/src/OrderStatusChangeEventPublisher/",
            Environment = new Dictionary<string, string>(1)
            {
                {"EVENT_BUS_NAME", props.EventBus.EventBusName},
                {"POWERTOOLS_SERVICE_NAME", "orders"},
            },
            Tracing = Tracing.PASS_THROUGH
        });

        props.EventBus.GrantPutEventsTo(Function);
        
        this.Function.AddEventSource(new DynamoEventSource(props.OrderDataStore, new DynamoEventSourceProps()));
    }
}
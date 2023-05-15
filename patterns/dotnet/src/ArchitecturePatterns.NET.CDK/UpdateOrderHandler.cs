using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SQS;
using Constructs;
using XaasKit.CDK.AWS.Lambda.DotNet;
using BundlingOptions = Amazon.CDK.BundlingOptions;

namespace ArchitecturePatterns.NET.CDK;

public record UpdateOrderHandlerProps(IQueue RequestQueue, IEventBus EventBus);

public class UpdateOrderHandler : Construct
{
    public UpdateOrderHandler(
        Construct scope,
        string id,
        UpdateOrderHandlerProps props) : base(
        scope,
        id)
    {
        var buildOption = new BundlingOptions
        {
            Image = Runtime.DOTNET_6.BundlingImage,
            User = "root",
            OutputType = BundlingOutput.ARCHIVED,
            Command = new[]{
                "/bin/sh",
                "-c",
                " dotnet tool install -g Amazon.Lambda.Tools"+
                " && dotnet build"+
                " && dotnet lambda package --output-package /asset-output/function.zip"
            }
        };
        
        var updateOrderHandlerFunction = new DotNetFunction(this, "update-order", new DotNetFunctionProps()
        {
            Runtime = Runtime.DOTNET_6,
            MemorySize = 1024,
            LogRetention = RetentionDays.ONE_DAY,
            Handler = "OrderUpdateHandler::OrderUpdateHandler.Function::FunctionHandler",
            ProjectDir = "src/MessageProcessor/src/OrderUpdateHandler/",
            Environment = new Dictionary<string, string>(1)
            {
                {"EVENT_BUS_NAME", props.EventBus.EventBusName}
            }
        });
        
        props.EventBus.GrantPutEventsTo(updateOrderHandlerFunction);
        
        updateOrderHandlerFunction.AddEventSource(new SqsEventSource(props.RequestQueue, new SqsEventSourceProps
        {
            Enabled = true
        }));
    }
}
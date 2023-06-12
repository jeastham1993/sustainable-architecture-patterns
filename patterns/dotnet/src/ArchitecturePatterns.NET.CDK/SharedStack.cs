using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Constructs;

namespace ArchitecturePatterns.NET.CDK;

public class SharedStack : Stack
{
    public IEventBus EventBus { get; private set;}
    
    internal SharedStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        EventBus = new EventBus(this, "AppPattensEventBus", new EventBusProps
        {
            EventBusName = "sustainable-architecture-bus",
        });

        var outputEventBusName = new CfnOutput(this, "EventBusName", new CfnOutputProps
        {
            Value = EventBus.EventBusName,
            Description = "Name of the shared event bus",
            ExportName = "SharedEventBusName"
        });
        
        var outputEventBusArn = new CfnOutput(this, "EventBusArn", new CfnOutputProps
        {
            Value = EventBus.EventBusArn,
            Description = "ARN of the shared event bus",
            ExportName = "SharedEventBusArn"
        });
    }
}
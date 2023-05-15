using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SQS;
using Constructs;
using XaasKit.CDK.AWS.Lambda.DotNet;
using AssetOptions = Amazon.CDK.AWS.S3.Assets.AssetOptions;
using BundlingOptions = Amazon.CDK.BundlingOptions;

namespace ArchitecturePatterns.NET.CDK;

public record MessageProcessorProps(IQueue RequestQueue, IEventBus EventBus);

public class MessageProcessor : Construct
{
    public MessageProcessor(
        Construct scope,
        string id,
        MessageProcessorProps props) : base(
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
        
        var messageProcessingFunction = new DotNetFunction(this, "message-processor", new DotNetFunctionProps()
        {
            Runtime = Runtime.DOTNET_6,
            MemorySize = 1024,
            LogRetention = RetentionDays.ONE_DAY,
            Handler = "MessageProcessor::MessageProcessor.Function::FunctionHandler",
            ProjectDir = "src/MessageProcessor/src/MessageProcessor/",
            Environment = new Dictionary<string, string>(1)
            {
                {"EVENT_BUS_NAME", props.EventBus.EventBusName}
            }
        });

        props.EventBus.GrantPutEventsTo(messageProcessingFunction);
        
        messageProcessingFunction.AddEventSource(new SqsEventSource(props.RequestQueue, new SqsEventSourceProps
        {
            Enabled = true
        }));
    }
}
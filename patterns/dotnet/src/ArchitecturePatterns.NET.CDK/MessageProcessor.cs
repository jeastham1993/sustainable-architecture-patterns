using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SQS;
using Constructs;
using AssetOptions = Amazon.CDK.AWS.S3.Assets.AssetOptions;

namespace ArchitecturePatterns.NET.CDK;

public record MessageProcessorProps(IQueue RequestQueue);

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
        
        var messageProcessingFunction = new Function(this, "message-processor", new FunctionProps
        {
            Runtime = Runtime.DOTNET_6,
            MemorySize = 1024,
            LogRetention = RetentionDays.ONE_DAY,
            Handler = "MessageProcessor::MessageProcessor.Function::FunctionHandler",
            Code = Code.FromAsset("src/MessageProcessor/src/MessageProcessor/", new AssetOptions
            {
                Bundling = buildOption
            }),
        });
        
        messageProcessingFunction.AddEventSource(new SqsEventSource(props.RequestQueue, new SqsEventSourceProps
        {
            Enabled = true
        }));
    }
}
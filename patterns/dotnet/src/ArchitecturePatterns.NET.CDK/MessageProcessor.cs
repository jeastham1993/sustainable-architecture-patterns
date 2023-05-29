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

namespace ArchitecturePatterns.NET.CDK;

public record MessageProcessorProps(IQueue RequestQueue, IEventBus EventBus, ITable OrderDataStore);

public class MessageProcessor : Construct
{
    public MessageProcessor(
        Construct scope,
        string id,
        MessageProcessorProps props) : base(
        scope,
        id)
    {
        var messageProcessingFunction = new DotNetFunction(this, "message-processor", new DotNetFunctionProps()
        {
            Runtime = Runtime.DOTNET_6,
            MemorySize = 1024,
            LogRetention = RetentionDays.ONE_DAY,
            Handler = "MessageProcessor::MessageProcessor.Function::FunctionHandler",
            ProjectDir = "src/MessageProcessor/src/MessageProcessor/",
            Environment = new Dictionary<string, string>(1)
            {
                {"EVENT_BUS_NAME", props.EventBus.EventBusName},
                {"TABLE_NAME", props.OrderDataStore.TableName},
            },
            Tracing = Tracing.PASS_THROUGH
        });

        props.EventBus.GrantPutEventsTo(messageProcessingFunction);
        props.OrderDataStore.GrantWriteData(messageProcessingFunction);
        
        messageProcessingFunction.AddEventSource(new SqsEventSource(props.RequestQueue, new SqsEventSourceProps
        {
            BatchSize = 10,
            Enabled = true,
            MaxBatchingWindow = Duration.Seconds(5),
            ReportBatchItemFailures = true,
        }));
    }
}
using System.Collections.Generic;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SQS;
using Constructs;
using XaasKit.CDK.AWS.Lambda.DotNet;

namespace ArchitecturePatterns.NET.CDK.Orders.Functions;

public record ReprocessFailedMessagesProps(IQueue SourceQueue, IQueue DestinationQueue);

public class ReprocessFailedMessages : Construct
{
    public IFunction ReprocessFailedMessageFunction { get; private set; }
    public ReprocessFailedMessages(
        Construct scope,
        string id,
        ReprocessFailedMessagesProps props) : base(
        scope,
        id)
    {
        this.ReprocessFailedMessageFunction = new DotNetFunction(this, "reprocess-messages", new DotNetFunctionProps()
        {
            Runtime = Runtime.DOTNET_6,
            MemorySize = 1024,
            LogRetention = RetentionDays.ONE_DAY,
            Handler = "ReprocessFailedMessages::ReprocessFailedMessages.Function::FunctionHandler",
            ProjectDir = "src/Handlers/src/ReprocessFailedMessages/",
            Environment = new Dictionary<string, string>(1)
            {
                {"SOURCE_QUEUE", props.SourceQueue.QueueArn},
                {"DESTINATION_QUEUE", props.DestinationQueue.QueueArn},
                {"POWERTOOLS_SERVICE_NAME", "orders"},
            },
            Tracing = Tracing.PASS_THROUGH
        });

        props.SourceQueue.GrantConsumeMessages(this.ReprocessFailedMessageFunction);
        props.DestinationQueue.GrantSendMessages(this.ReprocessFailedMessageFunction);

        this.ReprocessFailedMessageFunction.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
        {
            Actions = new[]
            {
                "sqs:StartMessageMoveTask",
            },
            Effect = Effect.ALLOW,
            Resources = new string[]
            {
                props.SourceQueue.QueueArn
            },
            Sid = "AllowMoveMessage",

        }));
    }
}
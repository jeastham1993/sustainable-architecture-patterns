using System.Collections.Generic;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SQS;
using Constructs;
using XaasKit.CDK.AWS.Lambda.DotNet;

namespace ArchitecturePatterns.NET.CDK.Customers.Functions;

public record CustomerOrderCreatedEventHandlerProps(IQueue SourceQueue);

public class CustomerOrderCreatedEventHandler : Construct
{
    public IFunction CustomerOrderCreatedEventHandlerFunction { get; private set; }
    
    public CustomerOrderCreatedEventHandler(
        Construct scope,
        string id,
        CustomerOrderCreatedEventHandlerProps props) : base(
        scope,
        id)
    {
        this.CustomerOrderCreatedEventHandlerFunction = new DotNetFunction(this, "CustomerOrderEventCreatedHandler",
            new DotNetFunctionProps()
            {
                Runtime = Runtime.DOTNET_6,
                MemorySize = 1024,
                LogRetention = RetentionDays.ONE_DAY,
                Handler =
                    "CustomerOrderCreatedEventHandler::CustomerOrderCreatedEventHandler.Function::FunctionHandler",
                ProjectDir = "src/Handlers/src/Customers/CustomerOrderCreatedEventHandler",
                Environment = new Dictionary<string, string>(1)
                {
                    { "POWERTOOLS_SERVICE_NAME", "customers" },
                },
                Tracing = Tracing.PASS_THROUGH,
            });
        
        this.CustomerOrderCreatedEventHandlerFunction.AddEventSource(new SqsEventSource(props.SourceQueue, new SqsEventSourceProps
        {
            // Only 5 messages will ever be processed at the same time
            BatchSize = 5,
            MaxConcurrency = 10,
            ReportBatchItemFailures = true
        }));
    }
}
using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.SQS;
using ArchitecturePatterns.NET.CDK.Customers.Functions;
using Constructs;

namespace ArchitecturePatterns.NET.CDK.Customers;

public class CustomersStack : Stack
{
    internal CustomersStack(Construct scope, string id, IStackProps stackProps = null) : base(scope, id, stackProps)
    {
        var importEventBusArn = Fn.ImportValue("SharedEventBusArn");

        var eventBus = Amazon.CDK.AWS.Events.EventBus.FromEventBusArn(this, "SharedEventBus", importEventBusArn);
        
        var orderCreatedQueue = new Queue(this, "CustomerOrderCreatedQueue", new QueueProps());

        var rule = new Rule(this, "CustomerOrderCreatedRule", new RuleProps()
        {
            EventBus = eventBus,
            EventPattern = new EventPattern
            {
                DetailType = new string[]
                {
                    "order-created"
                },
                Source = new string[]
                {
                    "com.orders"
                },
            },
            Targets = new []{ new SqsQueue(orderCreatedQueue) }
        });

        var customerOrderCreatedHandler = new CustomerOrderCreatedEventHandler(this, "CustomerOrderCreatedHandler",
            new CustomerOrderCreatedEventHandlerProps(orderCreatedQueue));
    }
}
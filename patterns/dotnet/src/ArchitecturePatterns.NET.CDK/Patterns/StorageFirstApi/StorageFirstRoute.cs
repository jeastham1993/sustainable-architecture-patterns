using System;
using System.Collections.Generic;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.SQS;
using Constructs;

namespace ArchitecturePatterns.NET.CDK.Patterns.StorageFirstApi;

public record StorageFirstRouteProps(
    StorageType StorageType,
    string IntegrationName,
    IResource Resource,
    IFunction processor);

public class StorageFirstRoute : Construct
{
    /// <summary>
    /// Populated if the <see cref="StorageType"/> is set to DynamoDB.
    /// </summary>
    public ITable? Table { get; private set; }
    
    /// <summary>
    /// Populated if the <see cref="StorageType"/> is set to Queue.
    /// </summary>
    public IQueue? Queue { get; private set; }
    
    /// <summary>
    /// Populated if the <see cref="StorageType"/> is set to Queue.
    /// </summary>
    public IQueue? ErrorQueue { get; private set; }
    
    public StorageFirstRoute(
        Construct scope,
        string id,
        StorageFirstRouteProps props) : base(
        scope,
        id)
    {
        var integrationRole = new Role(
            this,
            "SqsApiGatewayIntegrationRole",
            new RoleProps
            {
                AssumedBy = new ServicePrincipal("apigateway.amazonaws.com")
            });

        AwsIntegration? integration = null;
        
        switch (props.StorageType)
        {
            case StorageType.Queue:
                
                var sqsIntegration = new SqsApiIntegration(
                    this,
                    "SqsApiIntegration",
                    integrationRole,
                    props.IntegrationName);
                Queue = sqsIntegration.Queue;
                ErrorQueue = sqsIntegration.ErrorQueue;
                props.processor.AddEventSource(new SqsEventSource(Queue, new SqsEventSourceProps()
                {
                    ReportBatchItemFailures = true
                }));
                integration = sqsIntegration.QueueIntegration;
                
                break;
            case StorageType.DynamoDB:
                var dynamoIntegration = new DynamoDbApiIntegration(
                    this,
                    "SqsApiIntegration",
                    integrationRole,
                    props.IntegrationName);
                Table = dynamoIntegration.Table;
                props.processor.AddEventSource(new DynamoEventSource(dynamoIntegration.Table, new DynamoEventSourceProps()));
                integration = dynamoIntegration.DynamoIntegration;
                break;
            case StorageType.WorkflowWithQueue:
                var workflowIntegration = new SqsWithUniqueIdGeneration(this, "SqsWorkflowApiIntegration",
                    integrationRole, props.IntegrationName);
                Queue = workflowIntegration.Queue;
                ErrorQueue = workflowIntegration.ErrorQueue;
                props.processor.AddEventSource(new SqsEventSource(Queue, new SqsEventSourceProps()
                {
                    ReportBatchItemFailures = true
                }));
                integration = workflowIntegration.WorkflowQueueIntegration;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        props.Resource.AddMethod(
            "POST",
            integration,
            new MethodOptions
            {
                MethodResponses = new IMethodResponse[]
                {
                    new MethodResponse { StatusCode = "200" },
                    new MethodResponse { StatusCode = "400" },
                    new MethodResponse { StatusCode = "500" }
                }
            });
    }
}
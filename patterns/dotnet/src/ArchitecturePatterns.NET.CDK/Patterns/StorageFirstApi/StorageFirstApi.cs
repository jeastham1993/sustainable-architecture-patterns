using System;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.FraudDetector;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SQS;
using Constructs;
using Resource = Amazon.CDK.AWS.APIGateway.Resource;

namespace ArchitecturePatterns.NET.CDK.Patterns.StorageFirstApi;

public record StorageFirstApiProps(
    StorageType StorageType,
    string IntegrationName,
    string ApiRoute);

public class StorageFirstApi : Construct
{
    public RestApi Api { get; }
    
    /// <summary>
    /// Populated if the <see cref="StorageType"/> is set to DynamoDB.
    /// </summary>
    public ITable? Table { get; }
    
    /// <summary>
    /// Populated if the <see cref="StorageType"/> is set to Queue.
    /// </summary>
    public IQueue? Queue { get; }
    
    public StorageFirstApi(
        Construct scope,
        string id,
        StorageFirstApiProps props) : base(
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
                Queue = sqsIntegration.SqsQueue;
                integration = sqsIntegration.QueueIntegration;
                
                break;
            case StorageType.DynamoDB:
                var dynamoIntegration = new DynamoDbApiIntegration(
                    this,
                    "SqsApiIntegration",
                    integrationRole,
                    props.IntegrationName);
                Table = dynamoIntegration.Table;
                integration = dynamoIntegration.DynamoIntegration;
                break;
            case StorageType.WorkflowWithQueue:
                var workflowIntegration = new SqsWithUniqueIdGeneration(this, "SqsWorkflowApiIntegration",
                    integrationRole, props.IntegrationName);
                Queue = workflowIntegration.Queue;
                integration = workflowIntegration.WorkflowQueueIntegration;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var api = new RestApi(
            this,
            "FrontendApi",
            new RestApiProps
            {
                CloudWatchRole = true,
            });

        Resource? lastResource = null;  

        foreach (var pathSegment in props.ApiRoute.Split('/'))
        {
            var sanitisedPathSegment = pathSegment.Replace(
                "/",
                "");

            if (string.IsNullOrEmpty(sanitisedPathSegment))
            {
                continue;
            }

            if (lastResource == null)
            {
                lastResource = api.Root.AddResource(sanitisedPathSegment);
                continue;
            }

            lastResource = lastResource.AddResource(sanitisedPathSegment);
        }
        
        lastResource?.AddMethod(
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

        Api = api;
    }
}
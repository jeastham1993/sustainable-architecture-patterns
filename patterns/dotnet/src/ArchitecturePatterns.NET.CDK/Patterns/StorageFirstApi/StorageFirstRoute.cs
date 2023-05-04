namespace ArchitecturePatterns.NET.CDK.Patterns.StorageFirstApi;

using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SQS;

using Constructs;

public record StorageFirstRouteProps(
    StorageType StorageType,
    string IntegrationName,
    Resource Resource);

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

        AwsIntegration integration = null;
        
        switch (props.StorageType)
        {
            case StorageType.Queue:
                var sqsIntegration = new SqsApiIntegration(
                    this,
                    "SqsApiIntegration",
                    integrationRole,
                    props.IntegrationName);
                this.Queue = sqsIntegration.SqsQueue;
                integration = sqsIntegration.QueueIntegration;
                
                break;
            case StorageType.DynamoDB:
                var dynamoIntegration = new DynamoDbApiIntegration(
                    this,
                    "SqsApiIntegration",
                    integrationRole,
                    props.IntegrationName);
                this.Table = dynamoIntegration.Table;
                integration = dynamoIntegration.DynamoIntegration;
                break;
        }

        props.Resource.AddMethod(
            "POST",
            integration,
            new MethodOptions
            {
                MethodResponses = new[]
                {
                    new MethodResponse { StatusCode = "200" },
                    new MethodResponse { StatusCode = "400" },
                    new MethodResponse { StatusCode = "500" }
                }
            });
    }
}
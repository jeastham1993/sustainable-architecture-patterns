namespace ArchitecturePatterns.NET.CDK;

using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;

using ArchitecturePatterns.NET.CDK.Patterns.StorageFirstApi;

using Constructs;

public class DotnetStack : Stack
{
    internal DotnetStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var mainApi = new RestApi(
            this,
            "ApiSample",
            new RestApiProps { });

        var storageFirstSqsRoute = new StorageFirstRoute(
            this,
            "StorageFirstSqsRoute",
            new StorageFirstRouteProps(
                StorageType.Queue,
                "RouteExample",
                mainApi.Root.AddResource("queue")));

        var storageFirstDynamoRoute = new StorageFirstRoute(
            this,
            "StorageFirstDynamoRoute",
            new StorageFirstRouteProps(
                StorageType.DynamoDB,
                "RouteExample",
                mainApi.Root.AddResource("dynamo")));

        var api = new StorageFirstApi(
            this,
            "StorageFirstDynamoApi",
            new StorageFirstApiProps(
                StorageType.DynamoDB,
                "SampleIntegration",
                "/inbound/dynamo/request"));
            
        var sqsApi = new StorageFirstApi(
            this,
            "StorageFirstSqsApi",
            new StorageFirstApiProps(
                StorageType.Queue,
                "SampleIntegration",
                "/inbound/sqs/request"));
            
        var apiUrlOutput = new CfnOutput(
            this,
            "ApiUrl",
            new CfnOutputProps
            {
                Value = api.Api.Url,
                Description = "Endpoint of the storage first API.",
                ExportName = "ApiUrl"
            });
            
        var mainApiUrlOutput = new CfnOutput(
            this,
            "MainApiUrl",
            new CfnOutputProps
            {
                Value = api.Api.Url,
                Description = "Endpoint of the pre-configured storage first API.",
                ExportName = "MainApiUrl"
            });
    }
}
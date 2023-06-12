using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AWS.Lambda.Powertools.Tracing;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace ReprocessFailedMessages;

public class Function
{
    private readonly AmazonSQSClient _sqsClient;

    public Function() : this(null)
    {
    }

    public Function(AmazonSQSClient? sqsClient)
    {
        AWSSDKHandler.RegisterXRayForAllServices();
        _sqsClient = sqsClient ?? new AmazonSQSClient();
    }
    
    public async Task FunctionHandler(CloudWatchEvent<string> evt)
    {   
        await this._sqsClient.StartMessageMoveTaskAsync(new StartMessageMoveTaskRequest
        {
            DestinationArn = Environment.GetEnvironmentVariable("DESTINATION_QUEUE"),
            SourceArn = Environment.GetEnvironmentVariable("SOURCE_QUEUE"),
        });
    }
}

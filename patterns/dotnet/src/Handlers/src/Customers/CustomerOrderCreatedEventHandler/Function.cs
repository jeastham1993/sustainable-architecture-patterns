using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AWS.Lambda.Powertools.Logging;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace CustomerOrderCreatedEventHandler;

public class Function
{
    private readonly HttpClient _httpClient;
    
    public Function() : this(null)
    {
        AWSSDKHandler.RegisterXRayForAllServices();
    }

    public Function(HttpClient client = null)
    {
        this._httpClient = client ?? new HttpClient();
    }
    
    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent input)
    {
        var failures = new List<SQSBatchResponse.BatchItemFailure>();
        
        foreach (var record in input.Records)
        {
            try
            {
                await ProcessMessage(record);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failure processing message");
                
                failures.Add(new SQSBatchResponse.BatchItemFailure
                {
                    ItemIdentifier = record.MessageId
                });
            }
        }

        return new SQSBatchResponse(failures);
    }

    private async Task ProcessMessage(SQSEvent.SQSMessage record)
    {
        Logger.LogInformation($"Inbound request is {record.Body}");
        Logger.LogInformation($"Message attributes {JsonSerializer.Serialize(record.MessageAttributes)}");
        
        Logger.LogInformation("Making request to legacy system:");

        // Dummy HTTP request
        var res = await this._httpClient.GetAsync("https://google.com");

        Logger.LogInformation($"HTTP Result: {res.StatusCode}");
    }
}

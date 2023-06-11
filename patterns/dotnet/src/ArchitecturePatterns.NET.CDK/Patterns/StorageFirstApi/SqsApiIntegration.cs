

using System;
using System.Collections.Generic;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SQS;
using Constructs;

namespace ArchitecturePatterns.NET.CDK.Patterns.StorageFirstApi;

internal class SqsApiIntegration : Construct
{
    public AwsIntegration QueueIntegration { get; private set; }
    
    public Queue Queue { get; private set; }
    
    public Queue ErrorQueue { get; private set; }
    
    public SqsApiIntegration(
        Construct scope,
        string id,
        IRole integrationRole,
        string integrationName) : base(
        scope,
        id)
    {
        this.Queue = new Queue(
            scope,
            $"{integrationName}StorageQueue");

        this.Queue.GrantSendMessages(integrationRole);
        
        QueueIntegration = new AwsIntegration(
            new AwsIntegrationProps
            {
                Service = "sqs",
                Path = $"{Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT")}/{Queue.QueueName}",
                IntegrationHttpMethod = "POST",
                Options = new IntegrationOptions
                {
                    CredentialsRole = integrationRole,
                    RequestParameters = new Dictionary<string, string>(1)
                    {
                        { "integration.request.header.Content-Type", "'application/x-www-form-urlencoded'" }
                    },
                    RequestTemplates = new Dictionary<string, string>(1)
                    {
                        { "application/json", "Action=SendMessage&MessageBody=$input.body" }
                    },
                    IntegrationResponses = new List<IIntegrationResponse>(3)
                    {
                        new IntegrationResponse
                        {
                            StatusCode = "200"
                        },
                        new IntegrationResponse
                        {
                            StatusCode = "400"
                        },
                        new IntegrationResponse
                        {
                            StatusCode = "500"
                        },
                    }.ToArray()
                }
            });
    }
}

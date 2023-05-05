using System.Collections.Generic;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace ArchitecturePatterns.NET.CDK.Patterns.StorageFirstApi;

internal class DynamoDbApiIntegration : Construct
{
    public AwsIntegration DynamoIntegration { get; private set; }
    
    public Table Table { get; private set; }
    
    public DynamoDbApiIntegration(
        Construct scope,
        string id,
        IRole integrationRole,
        string integrationName) : base(
        scope,
        id)
    {
        Table = new Table(
            scope,
            $"{integrationName}StorageQueue",
            new TableProps
            {
                BillingMode = BillingMode.PAY_PER_REQUEST,
                PartitionKey = new Attribute
                {
                    Name = "PK",
                    Type = AttributeType.STRING,
                },
                Stream = StreamViewType.KEYS_ONLY
            });

        Table.GrantWriteData(integrationRole);
        
        DynamoIntegration = new AwsIntegration(
            new AwsIntegrationProps
            {
                Service = "dynamodb",
                Action = "PutItem",
                IntegrationHttpMethod = "POST",
                Options = new IntegrationOptions
                {
                    PassthroughBehavior = PassthroughBehavior.WHEN_NO_TEMPLATES,
                    CredentialsRole = integrationRole,
                    RequestTemplates = new Dictionary<string, string>(1)
                    {
                        { "application/json", $"{{  \"TableName\": \"{Table.TableName}\",  \"Item\": {{    \"PK\": {{      \"S\": \"$context.requestId\"    }},    \"requestBody\": {{      \"S\": \"$util.escapeJavaScript($input.json('$'))\"    }},    \"timestamp\": {{      \"S\": \"$context.requestTime\"    }}  }}}}" }
                    },
                    IntegrationResponses = new List<IIntegrationResponse>(3)
                    {
                        new IntegrationResponse
                        {
                            StatusCode = "200",
                            ResponseTemplates = new Dictionary<string, string>(1)
                            {
                                { "application/json", "{}" }
                            }
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
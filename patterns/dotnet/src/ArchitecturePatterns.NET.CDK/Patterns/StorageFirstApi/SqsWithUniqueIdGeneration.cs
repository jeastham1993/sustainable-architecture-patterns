using System;
using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.StepFunctions;
using Amazon.CDK.AWS.StepFunctions.Tasks;
using Constructs;

namespace ArchitecturePatterns.NET.CDK.Patterns.StorageFirstApi;

internal class SqsWithUniqueIdGeneration : Construct
{
    public AwsIntegration WorkflowQueueIntegration { get; private set; }
    
    public StateMachine Workflow { get; private set; }
    
    public IQueue Queue { get; private set; }
    
    public SqsWithUniqueIdGeneration(
        Construct scope,
        string id,
        IRole integrationRole,
        string integrationName) : base(
        scope,
        id)
    {
        // Create role to be assumed by the workflow.
        var workflowRole = new Role(this, $"{integrationName}WorkflowRole", new RoleProps()
        {
            AssumedBy = new ServicePrincipal("states.amazonaws.com")
        });
        
        // Generate the Queue for storing the messages, as well as a dead letter queue to handle failures.
        var dlq = new Queue(this, $"{integrationName}StorageDLQ", new QueueProps());

        this.Queue = new Queue(this, $"{integrationName}StorageQueue", new QueueProps
        {
            DeadLetterQueue = new DeadLetterQueue
            {
                MaxReceiveCount = 3,
                Queue = dlq
            },
        });

        this.Queue.GrantSendMessages(workflowRole);

        // Log group for enabling StepFunctions express workflow logs
        var logGroup = new LogGroup(this, $"{integrationName}WorkflowLogGroup", new LogGroupProps()
        {
            Retention = RetentionDays.ONE_DAY,
        });

        logGroup.GrantWrite(workflowRole);
        
        // Create the workflow definition. The workflow:
        // 1. Generates a unique identifier using the States.UUID() Intrinsic function.
        // 2. Sends the message to SQS.
        // 3. Formats the response to return.
        var workflowDefinition = new Pass(scope, "GenerateCaseId", new PassProps()
        {
            Parameters = new Dictionary<string, object>(4)
            {
                {"payload", JsonPath.EntirePayload},
                {"identifier.$", "States.UUID()" },
            }
        })
            .Next(new SqsSendMessage(this, $"{integrationName}WorkflowSendMessage", new SqsSendMessageProps
            {
                MessageBody = TaskInput.FromJsonPathAt("$"),
                Queue = this.Queue,
                ResultPath = JsonPath.DISCARD
            }))
            .Next(new Pass(scope, $"{integrationName}FormatResponse", new PassProps()
            {
                Parameters = new Dictionary<string, object>(4)
                {
                    {"identifier", JsonPath.StringAt("$.identifier")},
                }
            }));
        
        // Create the workflow.
        Workflow = new StateMachine(
            scope,
            $"{integrationName}StorageWorkflow",
            new StateMachineProps
            {
                Definition = workflowDefinition,
                Role = workflowRole,
                StateMachineType = StateMachineType.EXPRESS,
                Timeout = Duration.Seconds(30),
                TracingEnabled = true,
                Logs = new LogOptions()
                {
                    Level = LogLevel.ALL,
                    IncludeExecutionData = true,
                    Destination = logGroup
                }
            });

        // Allow the API Gateway integration role to start a synchronous execution of the workflow.
        Workflow.GrantStartSyncExecution(integrationRole);
        
        // Create the AWS integration.
        WorkflowQueueIntegration = new AwsIntegration(
            new AwsIntegrationProps
            {
                Service = "states",
                Action = "StartSyncExecution",
                IntegrationHttpMethod = "POST",
                Options = new IntegrationOptions
                {
                    CredentialsRole = integrationRole,
                    RequestTemplates = new Dictionary<string, string>
                    {
                        // The request template requires both the state machine ARN, as well as the input being passed in. Use the entire request body.
                        { "application/json", "{ \"stateMachineArn\": \"" + Workflow.StateMachineArn + "\", \"input\": \"$util.escapeJavaScript($input.json('$'))\" }" }
                    },
                    IntegrationResponses = new List<IIntegrationResponse>(3)
                    {
                        new IntegrationResponse
                        {
                            StatusCode = "200",
                            ResponseTemplates = new Dictionary<string, string>(1)
                            {
                                // For the response, include the output.
                                {"application/json", "{ \"result\": $input.json('$.output'), \"status\": \"$input.json('$.status')\" }" }
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
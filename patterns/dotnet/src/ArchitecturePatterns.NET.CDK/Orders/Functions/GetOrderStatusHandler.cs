using System.Collections.Generic;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Constructs;
using XaasKit.CDK.AWS.Lambda.DotNet;

namespace ArchitecturePatterns.NET.CDK.Orders.Functions;

public record GetOrderStatusHandlerProps(ITable OrderDataStore);

public class GetOrderStatusHandler : Construct
{
    public IFunction Function { get; private set; }
    public GetOrderStatusHandler(
        Construct scope,
        string id,
        GetOrderStatusHandlerProps props) : base(
        scope,
        id)
    {
        Function = new DotNetFunction(this, "GetOrderStatus", new DotNetFunctionProps()
        {
            Runtime = Runtime.DOTNET_6,
            MemorySize = 1024,
            LogRetention = RetentionDays.ONE_DAY,
            Handler = "GetOrderStatusHandler::GetOrderStatusHandler.Function_FunctionHandler_Generated::FunctionHandler",
            ProjectDir = "src/Handlers/src/GetOrderStatusHandler/",
            Environment = new Dictionary<string, string>(1)
            {
                {"TABLE_NAME", props.OrderDataStore.TableName},
                {"POWERTOOLS_SERVICE_NAME", "orders"},
            },
            Tracing = Tracing.PASS_THROUGH
        });
        
        props.OrderDataStore.GrantReadData(Function);
    }
}
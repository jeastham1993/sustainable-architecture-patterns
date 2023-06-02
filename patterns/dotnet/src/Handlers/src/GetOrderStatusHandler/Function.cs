using Amazon.DynamoDBv2;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using MessageProcessor.Orders;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace GetOrderStatusHandler;

public class Function
{
    private readonly IOrderRepository _orderRepository;

    public Function() : this(null)
    {
        AWSSDKHandler.RegisterXRayForAllServices();
    }

    public Function(IOrderRepository? orderRepository)
    {
        _orderRepository = orderRepository ?? new DynamoDbOrderRepository(new AmazonDynamoDBClient());
    }
    
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/{orderId}")]
    public async Task<Order> FunctionHandler(string orderId)
    {
        var order = await this._orderRepository.RetrieveOrder(orderId);

        return order;
    }
}

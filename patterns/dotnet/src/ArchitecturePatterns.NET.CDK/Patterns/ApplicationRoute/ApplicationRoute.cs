using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace ArchitecturePatterns.NET.CDK.Patterns.ApplicationRoute;

public record ApplicationRouteProps(IFunction Handler, IResource ApiResource, HttpMethod Method);

public class ApplicationRoute : Construct
{
    public ApplicationRoute(Construct scope, string id, ApplicationRouteProps props) : base(scope, id)
    {
        props.ApiResource.AddMethod(props.Method.ToString().ToUpper(), new LambdaIntegration(props.Handler));
    }
}
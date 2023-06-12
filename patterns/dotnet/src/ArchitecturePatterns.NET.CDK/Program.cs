using Amazon.CDK;
using ArchitecturePatterns.NET.CDK.Customers;
using ArchitecturePatterns.NET.CDK.Orders;

namespace ArchitecturePatterns.NET.CDK
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

            var shared = new SharedStack(app, "SharedStack");
            
            var ordersStack = new OrdersStack(app, "DotnetStack");

            var customersStack = new CustomersStack(app, "CustomerStack");
            
            app.Synth();
        }
    }
}

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
            
            var ordersStack = new OrdersStack(app, "OrdersStack");

            var customersStack = new CustomersStack(app, "CustomerStack");
            
            ordersStack.AddDependency(shared);
            customersStack.AddDependency(shared);
            
            app.Synth();
        }
    }
}

namespace MessageProcessor.Orders;

public interface IOrderRepository
{
    Task Store(Order order);
    
    Task<Order> RetrieveOrder(string orderId);
}
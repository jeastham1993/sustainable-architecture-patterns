namespace MessageProcessor.Orders;

public interface IOrderRepository
{
    Task Store(Order order);
}
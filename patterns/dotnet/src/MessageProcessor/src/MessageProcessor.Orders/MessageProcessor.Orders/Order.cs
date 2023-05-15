namespace MessageProcessor.Orders;

public class Order
{
    public Order()
    {
        OrderId = Guid.NewGuid().ToString();
    }
    
    public string OrderId { get; set; }
    
    public string CustomerId { get; set; }
    
    public string CustomerFirstName { get; set; }
    
    public string CustomerLastName { get; set; }

    public decimal TotalValue => this.Items.Sum(p => p.Price * p.Quantity);
    
    public decimal DiscountApplied { get; set; }
    
    public decimal AmountToPay { get; set; }
    
    public List<OrderItem> Items { get; set; }
}

public record OrderItem
{
    public string ProductCode { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal Price { get; set; }
}
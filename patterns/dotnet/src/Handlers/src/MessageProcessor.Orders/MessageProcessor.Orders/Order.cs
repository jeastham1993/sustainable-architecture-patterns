using System.Text.Json.Serialization;

namespace MessageProcessor.Orders;

public class Order
{
    [JsonConstructor]
    private Order()
    {
    }
    
    public Order(string orderId)
    {
        OrderId = orderId;
    }
    
    [JsonPropertyName("orderId")]
    public string OrderId { get; private set; }
    
    [JsonPropertyName("customer")]
    public CustomerDetail Customer { get; set; }
    
    public decimal TotalValue => this.Items.Sum(p => p.Price * p.Quantity);
    
    [JsonPropertyName("discountApplied")]
    public decimal DiscountApplied { get; set; }
    
    [JsonPropertyName("amountToPay")]
    public decimal AmountToPay { get; set; }
    
    [JsonPropertyName("items")]
    public List<OrderItem> Items { get; set; }

    public void ApplyDiscount(decimal discountPercentage)
    {
        this.DiscountApplied = discountPercentage;
        this.AmountToPay = this.TotalValue * (1.00M - (discountPercentage / 100.00M));
    }
}

public record OrderItem
{
    [JsonConstructor]
    private OrderItem(){}

    public OrderItem(string productCode, int quantity, decimal price)
    {
        this.Price = price;
        this.ProductCode = productCode;
        this.Quantity = quantity;
    }
    
    [JsonPropertyName("productCode")]
    public string ProductCode { get; private set; }
    
    [JsonPropertyName("quantity")]
    public int Quantity { get; private set; }
    
    [JsonPropertyName("price")]
    public decimal Price { get; private set; }
}
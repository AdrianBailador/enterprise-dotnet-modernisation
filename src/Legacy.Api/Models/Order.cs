namespace Legacy.Api.Models;

// ❌ Anemic domain model — no behaviour, just data bags
public class Order
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int StatusCode { get; set; }   // 1=Pending, 2=Processing, 3=Completed
    public DateTime CreatedAt { get; set; }
}

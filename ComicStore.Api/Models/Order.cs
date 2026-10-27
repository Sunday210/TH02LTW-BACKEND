namespace ComicStore.Api.Models;

public enum OrderStatus
{
    Pending,
    Paid,
    Confirmed,
    Shipping,
    Delivered,
    ReturnRequested,
    Returned,
    Rejected,
    Cancelled
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string PaymentMethod { get; set; } = "COD";
    public string? PaymentRef { get; set; }
    public string? ShippingName { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ReturnReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ComicId { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }

    public Order Order { get; set; } = null!;
    public Comic Comic { get; set; } = null!;
}
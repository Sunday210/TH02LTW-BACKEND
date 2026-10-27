namespace ComicStore.Api.Models;

public enum ComicStatus
{
    Draft,
    Pending,
    Approved,
    Rejected
}

public class Comic
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Issue { get; set; }
    public int Price { get; set; }
    public int? OldPrice { get; set; }
    public string? Description { get; set; }
    public string? Preview { get; set; }
    public string? ImageUrl { get; set; }
    public string? Color { get; set; }
    public string? Accent { get; set; }
    public string? Mark { get; set; }
    public string? Badge { get; set; }
    public int Stock { get; set; }
    public int? PublishYear { get; set; }
    public ComicStatus Status { get; set; } = ComicStatus.Draft;
    public string? RejectReason { get; set; }

    public int? CreatedById { get; set; }
    public int? ApprovedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? CreatedBy { get; set; }
    public User? ApprovedBy { get; set; }
    public ICollection<ComicCharacter> Characters { get; set; } = new List<ComicCharacter>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
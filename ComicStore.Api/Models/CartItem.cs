namespace ComicStore.Api.Models;

public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ComicId { get; set; }
    public int Quantity { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Comic Comic { get; set; } = null!;
}
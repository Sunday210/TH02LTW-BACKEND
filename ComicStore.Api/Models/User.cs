namespace ComicStore.Api.Models;

public enum UserRole
{
    SystemAdmin,
    Manager,
    Publisher,
    Buyer
}

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.Buyer;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Avatar { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Comic> AuthoredComics { get; set; } = new List<Comic>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
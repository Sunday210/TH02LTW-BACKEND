using ComicStore.Api.Models;

namespace ComicStore.Api.Dtos;

// ============ COMICS ============
public record ComicResponse(
    int Id,
    string Title,
    string? Issue,
    int Price,
    int? OldPrice,
    string? Description,
    string? Preview,
    string? ImageUrl,
    string? Color,
    string? Accent,
    string? Mark,
    string? Badge,
    int Stock,
    int? PublishYear,
    string Status,
    string? RejectReason,
    int? CreatedById,
    int? ApprovedById,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<string> Characters);

public record ComicCreateRequest(
    string Title,
    string? Issue,
    int Price,
    int? OldPrice,
    string? Description,
    string? Preview,
    string? ImageUrl,
    string? Color,
    string? Accent,
    string? Mark,
    string? Badge,
    int Stock,
    int? PublishYear,
    int? CreatedById,
    List<string>? Characters);

public record ComicUpdateRequest(
    string Title,
    string? Issue,
    int Price,
    int? OldPrice,
    string? Description,
    string? Preview,
    string? ImageUrl,
    string? Color,
    string? Accent,
    string? Mark,
    string? Badge,
    int Stock,
    int? PublishYear,
    string Status,
    string? RejectReason,
    List<string>? Characters);

public static class ComicMapping
{
    public static ComicResponse ToResponse(this Comic c) => new(
        c.Id, c.Title, c.Issue, c.Price, c.OldPrice, c.Description, c.Preview, c.ImageUrl,
        c.Color, c.Accent, c.Mark, c.Badge, c.Stock, c.PublishYear,
        c.Status.ToString(), c.RejectReason, c.CreatedById, c.ApprovedById,
        c.CreatedAt, c.UpdatedAt,
        c.Characters.Select(ch => ch.CharacterName).ToList());
}

// ============ USERS ============
public record UserResponse(
    int Id,
    string Email,
    string Name,
    string Role,
    string? Phone,
    string? Address,
    string? Avatar,
    bool IsVerified,
    DateTime CreatedAt);

public record UserCreateRequest(
    string Email,
    string Name,
    string? Password,
    string Role,
    string? Phone,
    string? Address);

public record UserUpdateRequest(
    string Email,
    string Name,
    string Role,
    string? Phone,
    string? Address,
    string? Avatar,
    bool IsVerified);

public static class UserMapping
{
    public static UserResponse ToResponse(this User u) => new(
        u.Id, u.Email, u.Name, u.Role.ToString(), u.Phone, u.Address,
        u.Avatar, u.IsVerified, u.CreatedAt);
}

// ============ ORDERS ============
public record OrderItemResponse(int Id, int ComicId, string ComicTitle, int Quantity, int Price);

public record OrderResponse(
    int Id,
    int UserId,
    int Total,
    string Status,
    string PaymentMethod,
    string? PaymentRef,
    string? ShippingName,
    string? ShippingPhone,
    string? ShippingAddress,
    string? ReturnReason,
    DateTime CreatedAt,
    List<OrderItemResponse> Items);

public record OrderItemRequest(int ComicId, int Quantity);

public record OrderCreateRequest(
    int UserId,
    string PaymentMethod,
    string? ShippingName,
    string? ShippingPhone,
    string? ShippingAddress,
    List<OrderItemRequest> Items);

public record OrderUpdateRequest(
    string Status,
    string? ShippingName,
    string? ShippingPhone,
    string? ShippingAddress,
    string? ReturnReason);

public static class OrderMapping
{
    public static OrderResponse ToResponse(this Order o) => new(
        o.Id, o.UserId, o.Total, o.Status.ToString(), o.PaymentMethod, o.PaymentRef,
        o.ShippingName, o.ShippingPhone, o.ShippingAddress, o.ReturnReason, o.CreatedAt,
        o.Items.Select(i => new OrderItemResponse(i.Id, i.ComicId, i.Comic.Title, i.Quantity, i.Price)).ToList());
}

// ============ CART ============
public record CartItemResponse(
    int Id,
    int UserId,
    int ComicId,
    string ComicTitle,
    string? ImageUrl,
    int UnitPrice,
    int Quantity,
    int Subtotal);

public record CartAddRequest(int UserId, int ComicId, int Quantity);

public record CartUpdateRequest(int Quantity);

public static class CartMapping
{
    public static CartItemResponse ToResponse(this CartItem ci) => new(
        ci.Id, ci.UserId, ci.ComicId, ci.Comic.Title, ci.Comic.ImageUrl,
        ci.Comic.Price, ci.Quantity, ci.Comic.Price * ci.Quantity);
}

// ============ TAGS ============
public record TagResponse(int Id, string Name, string? WikiUrl, string Status, int? CreatedById, DateTime CreatedAt);

public record TagCreateRequest(string Name, string? WikiUrl, int? CreatedById);

public record TagUpdateRequest(string Name, string? WikiUrl, string Status);

public static class TagMapping
{
    public static TagResponse ToResponse(this CharacterTag t) =>
        new(t.Id, t.Name, t.WikiUrl, t.Status.ToString(), t.CreatedById, t.CreatedAt);
}
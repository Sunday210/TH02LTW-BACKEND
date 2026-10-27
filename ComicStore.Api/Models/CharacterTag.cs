namespace ComicStore.Api.Models;

/// <summary>Nhân vật Marvel (thẻ tag) dùng để tra cứu Wiki.</summary>
public class CharacterTag
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? WikiUrl { get; set; }
    public ComicStatus Status { get; set; } = ComicStatus.Approved;
    public int? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? CreatedBy { get; set; }
}

/// <summary>Bảng quan hệ n-n giữa Comic và nhân vật.</summary>
public class ComicCharacter
{
    public int Id { get; set; }
    public int ComicId { get; set; }
    public string CharacterName { get; set; } = null!;
    public string? WikiUrl { get; set; }

    public Comic Comic { get; set; } = null!;
}
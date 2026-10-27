using ComicStore.Api.Models;

namespace ComicStore.Api.Data;

/// <summary>Seed dữ liệu mẫu (chạy khi Database:SeedData = true).</summary>
public static class DbInitializer
{
    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var admin = new User
        {
            Email = "admin@comicstore.vn",
            Name = "System Admin",
            PasswordHash = "admin123",
            Role = UserRole.SystemAdmin,
            IsVerified = true
        };
        var publisher = new User
        {
            Email = "publisher@comicstore.vn",
            Name = "Publisher",
            PasswordHash = "publisher123",
            Role = UserRole.Publisher,
            IsVerified = true
        };
        var buyer = new User
        {
            Email = "buyer@comicstore.vn",
            Name = "Buyer",
            PasswordHash = "buyer123",
            Role = UserRole.Buyer,
            IsVerified = true
        };

        db.Users.AddRange(admin, publisher, buyer);

        db.CharacterTags.AddRange(
            new CharacterTag { Name = "Iron Man", WikiUrl = "https://en.wikipedia.org/wiki/Iron_Man" },
            new CharacterTag { Name = "Spider-Man", WikiUrl = "https://en.wikipedia.org/wiki/Spider-Man" },
            new CharacterTag { Name = "Thor", WikiUrl = "https://en.wikipedia.org/wiki/Thor_(Marvel_Comics)" });

        db.Comics.AddRange(
            new Comic
            {
                Title = "Iron Man #1",
                Issue = "Vol. 1",
                Price = 120000,
                OldPrice = 150000,
                Description = "Khởi đầu hành trình của Tony Stark.",
                Stock = 20,
                PublishYear = 2024,
                Status = ComicStatus.Approved,
                CreatedBy = publisher,
                Color = "#111111",
                Accent = "#e51c2a"
            },
            new Comic
            {
                Title = "The Amazing Spider-Man #1",
                Issue = "Vol. 6",
                Price = 95000,
                Description = "Spider-Man quay lại đường phố New York.",
                Stock = 15,
                PublishYear = 2025,
                Status = ComicStatus.Approved,
                CreatedBy = publisher,
                Color = "#0b2a5b",
                Accent = "#e51c2a"
            });

        db.SaveChanges();
    }
}
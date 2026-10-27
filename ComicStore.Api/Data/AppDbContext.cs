using ComicStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ComicStore.Api.Data;

/// <summary>
/// DbContext kết nối PostgreSQL qua Npgsql — nơi khai báo DbSet và cấu hình
/// quan hệ (Fluent API) cho EF Core.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Comic> Comics => Set<Comic>();
    public DbSet<CharacterTag> CharacterTags => Set<CharacterTag>();
    public DbSet<ComicCharacter> ComicCharacters => Set<ComicCharacter>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Email).HasMaxLength(255).IsRequired();
            e.Property(u => u.Name).HasMaxLength(255).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(255);
            e.Property(u => u.Phone).HasMaxLength(50);
            e.Property(u => u.Avatar).HasMaxLength(512);
            e.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Comic>(e =>
        {
            e.Property(c => c.Title).HasMaxLength(255).IsRequired();
            e.Property(c => c.Issue).HasMaxLength(255);
            e.Property(c => c.Color).HasMaxLength(20);
            e.Property(c => c.Accent).HasMaxLength(20);
            e.Property(c => c.Mark).HasMaxLength(50);
            e.Property(c => c.Badge).HasMaxLength(50);
            e.Property(c => c.ImageUrl).HasMaxLength(512);
            e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(c => c.Status);
            e.HasIndex(c => c.CreatedById);

            e.HasOne(c => c.CreatedBy)
                .WithMany(u => u.AuthoredComics)
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.ApprovedBy)
                .WithMany()
                .HasForeignKey(c => c.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CharacterTag>(e =>
        {
            e.Property(t => t.Name).HasMaxLength(100).IsRequired();
            e.Property(t => t.WikiUrl).HasMaxLength(512);
            e.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(t => t.Name).IsUnique();

            e.HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ComicCharacter>(e =>
        {
            e.Property(cc => cc.CharacterName).HasMaxLength(100).IsRequired();
            e.Property(cc => cc.WikiUrl).HasMaxLength(512);

            e.HasOne(cc => cc.Comic)
                .WithMany(c => c.Characters)
                .HasForeignKey(cc => cc.ComicId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(cc => new { cc.ComicId, cc.CharacterName }).IsUnique();
        });

        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasOne(ci => ci.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ci => ci.Comic)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.ComicId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(ci => new { ci.UserId, ci.ComicId }).IsUnique();
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.PaymentMethod).HasMaxLength(20);
            e.Property(o => o.PaymentRef).HasMaxLength(255);
            e.Property(o => o.ShippingName).HasMaxLength(255);
            e.Property(o => o.ShippingPhone).HasMaxLength(20);
            e.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
            e.HasIndex(o => o.UserId);

            e.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(oi => oi.Comic)
                .WithMany(c => c.OrderItems)
                .HasForeignKey(oi => oi.ComicId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
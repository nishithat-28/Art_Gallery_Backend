using ArtGallery.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtGallery.API.Data
{
    public class ArtGalleryContext : DbContext
    {
        public ArtGalleryContext(DbContextOptions<ArtGalleryContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ArtWork> ArtWorks { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision for monetary values
            modelBuilder.Entity<ArtWork>()
                .Property(a => a.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

            // Configure required fields
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordSalt)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.FirstName)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.LastName)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .Property(c => c.Description)
                .IsRequired();

            modelBuilder.Entity<ArtWork>()
                .Property(a => a.Title)
                .IsRequired();

            modelBuilder.Entity<ArtWork>()
                .Property(a => a.Artist)
                .IsRequired();

            modelBuilder.Entity<ArtWork>()
                .Property(a => a.Description)
                .IsRequired();

            modelBuilder.Entity<ArtWork>()
                .Property(a => a.Medium)
                .IsRequired();

            modelBuilder.Entity<ArtWork>()
                .Property(a => a.Dimensions)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingAddress)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentMethod)
                .IsRequired();

            modelBuilder.Entity<Order>()
                .Property(o => o.InvoiceNumber)
                .IsRequired();

            // Configure relationships
            modelBuilder.Entity<ArtWork>()
                .HasOne(a => a.Category)
                .WithMany(c => c.ArtWorks)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ArtWork)
                .WithMany(a => a.OrderItems)
                .HasForeignKey(oi => oi.ArtWorkId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
using Kenbar.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Kenbar.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<OtpLog> OtpLogs => Set<OtpLog>();
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Phone).HasMaxLength(20).IsRequired();
                entity.Property(x => x.FullName).HasMaxLength(255);
                entity.HasIndex(x => x.Phone).IsUnique();
            });

            modelBuilder.Entity<OtpLog>(entity =>
            {
                entity.ToTable("otp_logs");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Phone).HasMaxLength(20).IsRequired();
                entity.Property(x => x.OtpCode).HasMaxLength(10).IsRequired();
                entity.Property(x => x.Purpose).HasMaxLength(50).IsRequired();
            });




            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("user_profiles");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FullName).HasMaxLength(255);

                entity.Property(x => x.Email).HasMaxLength(255);

                entity.Property(x => x.AvatarUrl).HasMaxLength(500);

                entity.HasOne(x => x.User)
                      .WithOne()
                      .HasForeignKey<UserProfile>(x => x.UserId);
            });


            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.ToTable("user_sessions");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.RefreshToken).HasMaxLength(500).IsRequired();
                entity.Property(x => x.DeviceId).HasMaxLength(255);
                entity.Property(x => x.DeviceName).HasMaxLength(255);

                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId);
            });

            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.ToTable("user_addresses");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ReceiverName).HasMaxLength(255).IsRequired();
                entity.Property(x => x.ReceiverPhone).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Province).HasMaxLength(100).IsRequired();
                entity.Property(x => x.District).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Ward).HasMaxLength(100).IsRequired();
                entity.Property(x => x.AddressLine).HasMaxLength(500).IsRequired();

                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId);
            });


            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name).HasMaxLength(255).IsRequired();
                entity.Property(x => x.Slug).HasMaxLength(255).IsRequired();

                entity.HasIndex(x => x.Slug).IsUnique();
            });


            modelBuilder.Entity<Unit>(entity =>
            {
                entity.ToTable("units");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Name).HasMaxLength(255).IsRequired();

                entity.HasIndex(x => x.Code).IsUnique();
            });


            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name).HasMaxLength(255).IsRequired();
                entity.Property(x => x.Slug).HasMaxLength(255).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(2000);
                entity.Property(x => x.Brand).HasMaxLength(255);

                entity.HasIndex(x => x.Slug).IsUnique();

                entity.HasOne(x => x.Category)
                      .WithMany()
                      .HasForeignKey(x => x.CategoryId);
            });



            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("product_images");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ImageUrl).HasMaxLength(1000).IsRequired();

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId);
            });

            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("product_variants");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.VariantName).HasMaxLength(255).IsRequired();
                entity.Property(x => x.SKU).HasMaxLength(100).IsRequired();

                entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
                entity.Property(x => x.SalePrice).HasColumnType("decimal(18,2)");
                entity.Property(x => x.UnitValue).HasColumnType("decimal(18,2)");

                entity.HasIndex(x => x.SKU).IsUnique();

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId);

                entity.HasOne(x => x.Unit)
                      .WithMany()
                      .HasForeignKey(x => x.UnitId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("carts");
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.UserId).IsUnique();

                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId);
            });



            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("cart_items");
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Cart)
                      .WithMany()
                      .HasForeignKey(x => x.CartId);

                entity.HasOne(x => x.ProductVariant)
                      .WithMany()
                      .HasForeignKey(x => x.ProductVariantId);

                entity.HasIndex(x => new { x.CartId, x.ProductVariantId }).IsUnique();
            });


            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
                entity.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");

                entity.HasOne(x => x.Order)
                      .WithMany()
                      .HasForeignKey(x => x.OrderId);
            });

            
        }
    }
}
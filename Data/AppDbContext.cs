// Data/AppDbContext.cs
using CoffeePos.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeePos.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            });

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); //  Add this line!
                entity.Property(e => e.Token).IsRequired();

                entity.HasOne(e => e.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- DEFAULT USER SEEDING ---
            // Password yahan "SecretPassword123" ka BCrypt hashed version hai
            var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var cashierId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = adminId,
                    Username = "admin",
                    PasswordHash = "$2a$11$vK3i..drdNMr9zlENu4pwu/mB0Ea9ZuxHDCEbRlf3cKK5qoQtCPTC", // SecretPassword123
                    Role = "Admin"
                },
                new ApplicationUser
                {
                    Id = cashierId,
                    Username = "cashier_ali",
                    PasswordHash = "$2a$11$vK3i..drdNMr9zlENu4pwu/mB0Ea9ZuxHDCEbRlf3cKK5qoQtCPTC", // SecretPassword123
                    Role = "Cashier"
                }
            );
        }
    }
}
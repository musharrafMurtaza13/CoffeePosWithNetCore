

namespace CoffeePos.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key to ApplicationUser
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        // Helper property to check if token is valid
        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
    }
}
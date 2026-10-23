namespace CoffeePos.Models
{
    public class ApplicationUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Cashier"; // Default role ('Admin' ya 'Cashier')

        // Navigation property (One-to-Many relationship with RefreshToken)
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
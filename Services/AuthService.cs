using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CoffeePos.Data;

using CoffeePos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CoffeePos.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            // Database se user find karo aur uske refresh tokens include karo
            var user = await _db.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            
            
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword("SecretPassword123");
                 Console.WriteLine(hashedPassword);
            

            // Password verify karo (BCrypt ke zariye)
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            // Naye tokens generate karo
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(user.Id);

            user.RefreshTokens.Add(refreshToken);
            _db.Entry(refreshToken).State = Microsoft.EntityFrameworkCore.EntityState.Added;

            await _db.SaveChangesAsync();

            return new AuthResponseDto(accessToken, refreshToken.Token, refreshToken.ExpiresAt);
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string token)
        {
            // Check karo ke refresh token database mein exist karta hai ya nahi aur active hai ya nahi
            var existingToken = await _db.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token);

            if (existingToken == null || !existingToken.IsActive)
                return null;

            // Purane token ko revoke kar do (Token Rotation Security)
            existingToken.IsRevoked = true;

            // Naya Access Token aur Refresh Token pair generate karo
            var newAccessToken = GenerateAccessToken(existingToken.User);
            var newRefreshToken = GenerateRefreshToken(existingToken.UserId);

            _db.RefreshTokens.Add(newRefreshToken);
            await _db.SaveChangesAsync();

            return new AuthResponseDto(newAccessToken, newRefreshToken.Token, newRefreshToken.ExpiresAt);
        }

        private string GenerateAccessToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), // Short-lived Access Token (15 mins)
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private RefreshToken GenerateRefreshToken(Guid userId)

        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return new RefreshToken
            {
                Id = Guid.NewGuid(), //  Yeh line add karna lazmi hai agar Id Guid hai!
                Token = Convert.ToBase64String(randomNumber),
                ExpiresAt = DateTime.UtcNow.AddDays(7), // Long-lived Refresh Token (7 days)
                UserId = userId
            };
        }
    }
}
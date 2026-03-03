namespace EComAPI.Services
{
    using BCrypt.Net;
    using EComAPI.DTOs.Auth;
    using EComAPI.Entities;
    using EComAPI.Repositories.Interfaces;
    using EComAPI.Services.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;

    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var exists = await _unitOfWork.Users
                .Query()
                .AnyAsync(x => x.Email == request.Email);

            if (exists)
                throw new Exception("Email already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = BCrypt.HashPassword(request.Password),
                Role = "User"
            };

            await _unitOfWork.Users.AddAsync(user);

            var accessToken = GenerateJwtToken(user);
            var rawRefreshToken = GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                TokenHash = BCrypt.HashPassword(rawRefreshToken),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();
            

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _unitOfWork.Users
                .Query()
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null || !BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("Invalid email or password");

            var accessToken = GenerateJwtToken(user);
            var rawRefreshToken = GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                TokenHash = BCrypt.HashPassword(rawRefreshToken),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task<AuthResponse?> RefreshTokenAsync(string token)
        {
            var refreshTokens = await _unitOfWork.RefreshTokens
                .Query()
                .Where(x => !x.IsRevoked && x.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();

            var refreshToken = refreshTokens
                .FirstOrDefault(x => BCrypt.Verify(token, x.TokenHash));

            if (refreshToken == null)
                return null;

            refreshToken.IsRevoked = true;

            var rawNewRefreshToken = GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                TokenHash = BCrypt.HashPassword(rawNewRefreshToken),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                UserId = refreshToken.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);

            var user = await _unitOfWork.Users.GetByIdAsync(refreshToken.UserId);
            var newAccessToken = GenerateJwtToken(user);

            await _unitOfWork.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = rawNewRefreshToken,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var tokens = await _unitOfWork.RefreshTokens
                .Query()
                .Where(x => !x.IsRevoked)
                .ToListAsync();

            var tokenEntity = tokens
                .FirstOrDefault(x => BCrypt.Verify(refreshToken, x.TokenHash));

            if (tokenEntity != null)
            {
                tokenEntity.IsRevoked = true;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
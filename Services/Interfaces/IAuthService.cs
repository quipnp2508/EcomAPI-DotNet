using EComAPI.DTOs.Auth;

namespace EComAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        Task<AuthResponse> LoginAsync(LoginRequest request);

        Task<AuthResponse?> RefreshTokenAsync(string token);

        Task LogoutAsync(string refreshToken);
    }

}

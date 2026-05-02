using ExpenseTracker.API.DTOs.Auth;

namespace ExpenseTracker.API.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}

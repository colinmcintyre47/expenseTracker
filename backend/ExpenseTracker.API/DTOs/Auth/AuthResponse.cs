namespace ExpenseTracker.API.DTOs.Auth;

/// <summary>
/// Response body for both /register and /login.
/// Returns a JWT token and basic user info so the frontend can
/// display the user's name without an extra API call.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

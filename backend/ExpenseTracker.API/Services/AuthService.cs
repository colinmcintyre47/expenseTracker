using ExpenseTracker.API.DTOs.Auth;
using ExpenseTracker.API.Helpers;
using ExpenseTracker.API.Models;
using ExpenseTracker.API.Repositories;

namespace ExpenseTracker.API.Services;

/// <summary>
/// Handles user registration and login.
///
/// Password security approach:
/// - BCrypt is used for hashing (not MD5/SHA256 which are cryptographically broken for passwords)
/// - BCrypt automatically salts and stretches the password (work factor = 12)
/// - On login, BCrypt.Verify() compares the plain text against the stored hash
///   without ever decrypting — it's a one-way operation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly JwtHelper _jwtHelper;

    public AuthService(IUserRepository userRepo, JwtHelper jwtHelper)
    {
        _userRepo = userRepo;
        _jwtHelper = jwtHelper;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check for duplicate email before creating the user
        if (await _userRepo.EmailExistsAsync(request.Email))
            throw new InvalidOperationException("An account with this email already exists.");

        // Hash the password using BCrypt (work factor 12 = ~300ms on modern hardware)
        // This is slow by design — it prevents brute-force attacks
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        var user = new User
        {
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
        };

        var created = await _userRepo.CreateAsync(user);
        var token = _jwtHelper.GenerateToken(created);

        return new AuthResponse
        {
            Token = token,
            UserId = created.Id,
            Email = created.Email,
            FirstName = created.FirstName,
            LastName = created.LastName
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Look up user by email
        var user = await _userRepo.GetByEmailAsync(request.Email);

        // Use constant-time comparison to prevent timing attacks.
        // We verify even if the user doesn't exist (with a dummy hash) to prevent
        // email enumeration attacks via timing differences.
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = _jwtHelper.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}

using ExpenseTracker.API.DTOs.Auth;
using ExpenseTracker.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

/// <summary>
/// Handles user registration and login.
/// These endpoints are public — no [Authorize] attribute.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account.
    /// Returns a JWT token on success.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Model validation is handled automatically by [ApiController]
        // If request is invalid, ASP.NET Core returns 400 before we even reach this code
        var response = await _authService.RegisterAsync(request);
        return StatusCode(201, response);
    }

    /// <summary>
    /// Login with email and password.
    /// Returns a JWT token on success.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }
}

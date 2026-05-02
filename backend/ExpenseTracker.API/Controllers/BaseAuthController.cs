using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

/// <summary>
/// Base controller for all authenticated endpoints.
/// Provides the UserId property that reads the current user's ID from the JWT claims.
///
/// All controllers that require authentication extend this class.
/// The [Authorize] attribute on this class means every endpoint in child controllers
/// requires a valid JWT token unless overridden with [AllowAnonymous].
/// </summary>
[ApiController]
[Authorize]
public abstract class BaseAuthController : ControllerBase
{
    /// <summary>
    /// Extracts the authenticated user's ID from the JWT token claims.
    /// The ID was embedded in the token at login time by JwtHelper.GenerateToken().
    /// Throws if the claim is missing (shouldn't happen with valid tokens).
    /// </summary>
    protected int UserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User ID claim not found in token.");
            return int.Parse(claim);
        }
    }
}

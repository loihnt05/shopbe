using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var claims = User.Claims
            .Select(claim => new
            {
                claim.Type,
                claim.Value
            });

        return Ok(new
        {
            User.Identity?.Name,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            Subject = User.FindFirstValue("sub"),
            PreferredUsername = User.FindFirstValue("preferred_username"),
            Email = User.FindFirstValue("email"),
            Roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value),
            Claims = claims
        });
    }
}
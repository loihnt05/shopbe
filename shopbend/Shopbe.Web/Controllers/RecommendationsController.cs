using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/recommendations")]
public sealed class RecommendationsController(
    IRecommendationService recommendations,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ControllerBase
{
    private async Task<Guid> GetAppUserIdAsync()
    {
        var keycloakId = currentUser.KeycloakId;
        if (string.IsNullOrWhiteSpace(keycloakId))
            throw new UnauthorizedAccessException("Missing user identity");

        var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
        if (user is null)
            throw new UnauthorizedAccessException("User not found");

        return user.Id;
    }

    /// <summary>
    /// Homepage recommendations: top selling products.
    /// </summary>
    [HttpGet("top-selling")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTopSelling([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var result = await recommendations.GetTopSellingAsync(count);
        return Ok(result);
    }

    /// <summary>
    /// Similar products by category.
    /// </summary>
    [HttpGet("products/{productId:guid}/similar")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSimilar(Guid productId, [FromQuery] int count = 8, CancellationToken cancellationToken = default)
    {
        var result = await recommendations.GetSimilarProductsAsync(productId, count);
        return Ok(result);
    }

    /// <summary>
    /// Personalized recommendations for current authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetPersonalized([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var userId = await GetAppUserIdAsync();
        var result = await recommendations.GetPersonalizedAsync(userId, count);
        return Ok(result);
    }
}



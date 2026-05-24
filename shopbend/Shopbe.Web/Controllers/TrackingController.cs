using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Application.Common.Interfaces;
using Shopbe.Domain.Enums;

namespace Shopbe.Web.Controllers;

[ApiController]
[Route("api/tracking")]
public sealed class TrackingController(
    IBehaviorTrackingService tracking,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("track")]
    [AllowAnonymous]
    public async Task<IActionResult> Track([FromBody] TrackRequestDto request)
    {
        Guid? userId = null;
        var keycloakId = currentUser.KeycloakId;
        if (!string.IsNullOrWhiteSpace(keycloakId))
        {
            var user = await unitOfWork.Users.GetUserByKeycloakIdAsync(keycloakId);
            userId = user?.Id;
        }

        await tracking.TrackAsync(
            userId, 
            request.SessionId, 
            request.CorrelationId, 
            request.BehaviorType, 
            request.BehaviorType.ToString(), 
            request.ProductId, 
            request.CategoryId,
            request.OrderId,
            request.Quantity,
            request.Value,
            request.Currency,
            "Web",
            request.Device,
            request.Referrer,
            request.UserAgent,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            null, null,
            request.Metadata);

        return Ok();
    }
}

public class TrackRequestDto
{
    public BehaviorType BehaviorType { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? OrderId { get; set; }
    public string? SessionId { get; set; }
    public string? CorrelationId { get; set; }
    public int? Quantity { get; set; }
    public decimal? Value { get; set; }
    public string? Currency { get; set; }
    public string? Device { get; set; }
    public string? Referrer { get; set; }
    public string? UserAgent { get; set; }
    public string? Metadata { get; set; }
}

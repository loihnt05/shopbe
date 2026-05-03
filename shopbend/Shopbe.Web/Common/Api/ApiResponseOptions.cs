namespace Shopbe.Web.Common.Api;

public sealed class ApiResponseOptions
{
    public const string SectionName = "Api";

    /// <summary>
    /// When true, successful (2xx) JSON responses are wrapped in ApiResponse envelopes.
    /// Default false for backward compatibility; can be opted-in per request via X-Response-Envelope: 1.
    /// </summary>
    public bool UseResponseEnvelope { get; init; } = false;
}


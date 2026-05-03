namespace Shopbe.Web.Common.Api;

public sealed class ApiResponse<T>
{
    public required bool Success { get; init; }
    public required T Data { get; init; }
    public ApiMeta? Meta { get; init; }
}

public sealed class ApiMeta
{
    public string? TraceId { get; init; }
    public int? Count { get; init; }
}


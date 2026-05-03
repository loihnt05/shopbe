using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Shopbe.Web.Common.Api;

public sealed class ApiResponseEnvelopeFilter(IOptions<Shopbe.Web.Common.Api.ApiResponseOptions> options) : IAsyncResultFilter
{
    private const string OptInHeader = "X-Response-Envelope";
    private const string OptInHeaderValue = "1";

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (!ShouldEnvelope(context.HttpContext))
        {
            await next();
            return;
        }

        // Only wrap 2xx JSON-like object results.
        if (context.Result is ObjectResult objectResult)
        {
            // Don't wrap errors / rfc7807 responses.
            if (objectResult.Value is ProblemDetails)
            {
                await next();
                return;
            }

            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            if (statusCode < 200 || statusCode >= 300)
            {
                await next();
                return;
            }

            // Don't double wrap.
            if (objectResult.Value is not null)
            {
                var valueType = objectResult.Value.GetType();
                if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(ApiResponse<>))
                {
                    await next();
                    return;
                }
            }

            var meta = BuildMeta(context.HttpContext, objectResult.Value);

            context.Result = new ObjectResult(new ApiResponse<object?>
                {
                    Success = true,
                    Data = objectResult.Value,
                    Meta = meta
                })
            {
                StatusCode = statusCode,
                DeclaredType = typeof(ApiResponse<object?>)
            };
        }

        await next();
    }

    private bool ShouldEnvelope(HttpContext httpContext)
    {
        if (options.Value.UseResponseEnvelope)
        {
            return true;
        }

        // Per-request opt-in for backward compatibility.
        if (httpContext.Request.Headers.TryGetValue(OptInHeader, out var header)
            && string.Equals(header.ToString(), OptInHeaderValue, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static ApiMeta BuildMeta(HttpContext httpContext, object? value)
    {
        int? count = value switch
        {
            Array a => a.Length,
            System.Collections.ICollection c => c.Count,
            _ => null
        };

        return new ApiMeta
        {
            TraceId = httpContext.TraceIdentifier,
            Count = count
        };
    }
}



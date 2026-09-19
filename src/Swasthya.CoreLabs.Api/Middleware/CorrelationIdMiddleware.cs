using Swasthya.CoreLabs.Application.Abstractions;

namespace Swasthya.CoreLabs.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = ReadOrGenerate(context);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (AmbientCorrelationIdProvider.Scope(correlationId))
        {
            await next(context);
        }
    }

    private static string ReadOrGenerate(HttpContext context)
    {
        string? header = context.Request.Headers[HeaderName].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(header) && IsValid(header))
        {
            return header;
        }

        return Guid.NewGuid().ToString("N");
    }

    private static bool IsValid(string value) =>
        value.Length <= 100
        && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.');
}

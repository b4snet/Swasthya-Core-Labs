using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Swasthya.CoreLabs.Api.Middleware;
using Swasthya.CoreLabs.Application.Exceptions;

namespace Swasthya.CoreLabs.Api.Tests;

public sealed class ErrorHandlingMiddlewareTests
{
    [Fact]
    public async Task UnknownException_ReturnsGeneric500_WithoutInternalDetails()
    {
        HttpContext context = NewContext();
        const string secret = "password=supersecret-connection-detail";
        var middleware = new ErrorHandlingMiddleware(
            _ => throw new InvalidOperationException(secret));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains(
            "application/problem+json",
            context.Response.ContentType ?? string.Empty);
        string body = await ReadBodyAsync(context);
        Assert.DoesNotContain(secret, body);
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("InvalidOperationException", body);
    }

    [Fact]
    public async Task NotFoundException_Returns404ProblemDetails()
    {
        HttpContext context = NewContext();
        var middleware = new ErrorHandlingMiddleware(
            _ => throw new NotFoundException("Organization 'x' was not found."));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        string body = await ReadBodyAsync(context);
        Assert.Contains("Not Found", body);
    }

    [Fact]
    public async Task PermissionDeniedException_Returns403ProblemDetails()
    {
        HttpContext context = NewContext();
        var middleware = new ErrorHandlingMiddleware(
            _ => throw new PermissionDeniedException("Principal has no grant."));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        string body = await ReadBodyAsync(context);
        Assert.Contains("Forbidden", body);
    }

    [Fact]
    public async Task ValidationException_Returns400ProblemDetails()
    {
        HttpContext context = NewContext();
        var middleware = new ErrorHandlingMiddleware(
            _ => throw new ValidationException("Code must be between 1 and 100 characters."));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        string body = await ReadBodyAsync(context);
        Assert.Contains("Bad Request", body);
    }

    [Fact]
    public async Task ConflictException_Returns409ProblemDetails()
    {
        HttpContext context = NewContext();
        var middleware = new ErrorHandlingMiddleware(
            _ => throw new ConflictException("A test with code 'cbc' already exists."));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        string body = await ReadBodyAsync(context);
        Assert.Contains("Conflict", body);
    }

    [Fact]
    public async Task DbUpdateConcurrencyException_Returns409WithoutInternalDetails()
    {
        HttpContext context = NewContext();
        var middleware = new ErrorHandlingMiddleware(
            _ => throw new DbUpdateConcurrencyException("xmin conflict detail"));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        string body = await ReadBodyAsync(context);
        Assert.DoesNotContain("xmin", body);
    }

    private static DefaultHttpContext NewContext()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
        };
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}

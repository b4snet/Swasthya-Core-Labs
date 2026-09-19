using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Swasthya.CoreLabs.Api.Options;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Models;

namespace Swasthya.CoreLabs.Api.Security;

public sealed class CoreLabsIdentityRequirement : IAuthorizationRequirement
{
}

public static class AuthContextStorage
{
    public static string HttpContextKey => "Swasthya.CoreLabs.AuthContext";
}

public sealed class CoreLabsIdentityAuthorizationHandler
    : AuthorizationHandler<CoreLabsIdentityRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CoreLabsIdentityRequirement requirement)
    {
        if (context.Resource is not HttpContext httpContext
            || httpContext.User.Identity is not { IsAuthenticated: true })
        {
            context.Fail();
            return;
        }

        AuthOptions options = httpContext.RequestServices
            .GetRequiredService<IOptions<AuthOptions>>().Value;

        if (!httpContext.User.TryGetPrincipalIdentity(options, out PrincipalIdentity? identity)
            || identity is null)
        {
            context.Fail();
            return;
        }

        IAuthContextProvider provider = httpContext.RequestServices
            .GetRequiredService<IAuthContextProvider>();

        AuthContextResolution resolution = await provider.ResolveAsync(
            identity, httpContext.RequestAborted);

        if (resolution.Context is null)
        {
            context.Fail();
            return;
        }

        httpContext.Items[AuthContextStorage.HttpContextKey] = resolution.Context;
        context.Succeed(requirement);
    }
}

public static class HttpContextAuthContextExtensions
{
    public static AuthContext GetAuthContext(this HttpContext context) =>
        context.Items[AuthContextStorage.HttpContextKey] as AuthContext
        ?? throw new InvalidOperationException(
            "Auth context was not resolved for this request.");
}

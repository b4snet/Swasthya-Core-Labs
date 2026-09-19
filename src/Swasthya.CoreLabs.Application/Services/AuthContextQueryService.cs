using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class AuthContextQueryService(OrganizationQueryService organizationQueryService)
{
    public async Task<AuthContextDto> GetAsync(
        AuthContext context,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<OrganizationDto> organizations;
        try
        {
            organizations = await organizationQueryService.ListScopedAsync(context, cancellationToken);
        }
        catch (PermissionDeniedException)
        {
            organizations = Array.Empty<OrganizationDto>();
        }

        return new AuthContextDto(
            context.PrincipalId,
            context.Issuer,
            context.ExternalSubject,
            context.PrincipalType,
            organizations);
    }
}

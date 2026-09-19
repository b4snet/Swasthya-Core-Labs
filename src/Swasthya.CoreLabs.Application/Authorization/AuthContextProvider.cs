using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Application.Authorization;

public sealed class AuthContextProvider(
    IPrincipalRepository principalRepository,
    IAuthorizationRepository authorizationRepository)
    : IAuthContextProvider
{
    public async Task<AuthContextResolution> ResolveAsync(
        PrincipalIdentity identity,
        CancellationToken cancellationToken)
    {
        Principal? principal = await principalRepository.FindByIdentityAsync(
            identity.Issuer,
            identity.ExternalSubject,
            identity.PrincipalType,
            cancellationToken);

        if (principal is null)
        {
            return new AuthContextResolution(null, AuthContextFailure.UnknownPrincipal);
        }

        if (!principal.IsActive)
        {
            return new AuthContextResolution(null, AuthContextFailure.InactivePrincipal);
        }

        IReadOnlyCollection<AuthContextGrant> grants =
            await authorizationRepository.LoadGrantsAsync(principal.Id, cancellationToken);

        AuthContext context = AuthContext.Resolved(
            principal.Id,
            principal.Issuer,
            principal.ExternalSubject,
            principal.Type,
            grants);

        return new AuthContextResolution(context, AuthContextFailure.None);
    }
}

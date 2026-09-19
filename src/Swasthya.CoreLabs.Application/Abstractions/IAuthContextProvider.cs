using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Application.Abstractions;

public enum AuthContextFailure
{
    None = 0,
    UnknownPrincipal = 1,
    InactivePrincipal = 2,
}

public sealed record AuthContextResolution(AuthContext? Context, AuthContextFailure Failure);

public interface IAuthContextProvider
{
    Task<AuthContextResolution> ResolveAsync(
        PrincipalIdentity identity,
        CancellationToken cancellationToken);
}

public interface ICorrelationIdProvider
{
    string Current { get; }
}

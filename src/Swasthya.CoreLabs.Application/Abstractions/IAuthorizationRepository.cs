using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Models;

namespace Swasthya.CoreLabs.Application.Abstractions;

public interface IAuthorizationRepository
{
    Task<IReadOnlyCollection<AuthContextGrant>> LoadGrantsAsync(
        Guid principalId,
        CancellationToken cancellationToken);
}

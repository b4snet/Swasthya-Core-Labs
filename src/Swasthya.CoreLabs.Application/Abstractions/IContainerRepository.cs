using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Application.Abstractions;

public interface IContainerRepository
{
    Task<Container?> GetAsync(Guid containerId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Container>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Container container, CancellationToken cancellationToken);
}

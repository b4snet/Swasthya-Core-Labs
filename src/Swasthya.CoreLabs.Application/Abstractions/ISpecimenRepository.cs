using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Application.Abstractions;

public interface ISpecimenRepository
{
    Task<Specimen?> GetByIdAsync(Guid specimenId, CancellationToken cancellationToken);

    Task<Specimen?> GetBySpecimenNumberAsync(string specimenNumber, CancellationToken cancellationToken);

    Task<OrderItem?> GetOrderItemAsync(Guid orderItemId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Specimen>> ListAsync(
        Guid organizationId,
        Guid? facilityId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Specimen specimen, CancellationToken cancellationToken);
}

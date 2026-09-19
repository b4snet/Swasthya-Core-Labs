using Swasthya.CoreLabs.Domain.Audit;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Laboratory;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Application.Abstractions;

public interface IPrincipalRepository
{
    Task<Principal?> FindByIdentityAsync(
        string issuer,
        string externalSubject,
        PrincipalType type,
        CancellationToken cancellationToken);
}

public interface IOrganizationRepository
{
    Task<IReadOnlyCollection<Organization>> ListByIdsAsync(
        IReadOnlyCollection<Guid> organizationIds,
        CancellationToken cancellationToken);
}

public interface IFacilityRepository
{
    Task<IReadOnlyCollection<Facility>> ListByOrganizationIdsAsync(
        IReadOnlyCollection<Guid> organizationIds,
        CancellationToken cancellationToken);
}

public interface IAuditRepository
{
    /// <summary>
    /// Lists audit records visible to the caller. Records are included when their organization is
    /// in <paramref name="organizationIds"/> (organization-wide grant) or when their facility is
    /// in <paramref name="facilityIds"/> (facility-restricted grant).
    /// </summary>
    Task<IReadOnlyCollection<AuditRecord>> ListAsync(
        IReadOnlyCollection<Guid> organizationIds,
        IReadOnlyCollection<Guid>? facilityIds,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task AddAsync(AuditRecord record, CancellationToken cancellationToken);
}

public interface IAccessionRepository
{
    Task<Accession?> GetAsync(Guid accessionId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Accession>> ListAsync(
        Guid organizationId,
        Guid? facilityId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Accession accession, CancellationToken cancellationToken);
}

public interface IOrderItemRepository
{
    Task<OrderItem?> GetAsync(Guid orderItemId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OrderItem>> ListAsync(
        Guid organizationId,
        Guid? facilityId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(OrderItem orderItem, CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken);

    Task CommitTransactionAsync(CancellationToken cancellationToken);

    Task RollbackTransactionAsync(CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

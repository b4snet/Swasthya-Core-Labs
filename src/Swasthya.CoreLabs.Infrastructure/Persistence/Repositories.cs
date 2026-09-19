using Microsoft.EntityFrameworkCore;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Audit;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Infrastructure.Persistence;

public sealed class PrincipalRepository(CoreLabDbContext dbContext) : IPrincipalRepository
{
    public Task<Principal?> FindByIdentityAsync(
        string issuer,
        string externalSubject,
        PrincipalType type,
        CancellationToken cancellationToken) =>
        dbContext.Principals.AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Issuer == issuer
                    && p.ExternalSubject == externalSubject
                    && p.Type == type,
                cancellationToken);
}

public sealed class AuthorizationRepository(CoreLabDbContext dbContext) : IAuthorizationRepository
{
    public async Task<IReadOnlyCollection<AuthContextGrant>> LoadGrantsAsync(
        Guid principalId,
        CancellationToken cancellationToken)
    {
        var grants = await
            (from assignment in dbContext.PrincipalRoleAssignments.AsNoTracking()
             join role in dbContext.Roles.AsNoTracking()
                 on assignment.RoleId equals role.Id
             join organization in dbContext.Organizations.AsNoTracking()
                 on assignment.OrganizationId equals organization.Id
             join rolePermission in dbContext.RolePermissions.AsNoTracking()
                 on role.Id equals rolePermission.RoleId
             join permission in dbContext.Permissions.AsNoTracking()
                 on rolePermission.PermissionId equals permission.Id
             where assignment.PrincipalId == principalId
                 && role.Status == EntityStatus.Active
                 && organization.Status == EntityStatus.Active
                 && (assignment.FacilityId == null
                     || dbContext.Facilities.Any(f =>
                         f.Id == assignment.FacilityId && f.Status == EntityStatus.Active))
             select new AuthContextGrant(
                 permission.Code,
                 assignment.OrganizationId,
                 assignment.FacilityId))
            .Distinct()
            .ToListAsync(cancellationToken);

        return grants;
    }
}

public sealed class OrganizationRepository(CoreLabDbContext dbContext) : IOrganizationRepository
{
    public async Task<IReadOnlyCollection<Organization>> ListByIdsAsync(
        IReadOnlyCollection<Guid> organizationIds,
        CancellationToken cancellationToken)
    {
        if (organizationIds.Count == 0)
        {
            return Array.Empty<Organization>();
        }

        return await dbContext.Organizations.AsNoTracking()
            .Where(o => organizationIds.Contains(o.Id))
            .ToListAsync(cancellationToken);
    }
}

public sealed class FacilityRepository(CoreLabDbContext dbContext) : IFacilityRepository
{
    public async Task<IReadOnlyCollection<Facility>> ListByOrganizationIdsAsync(
        IReadOnlyCollection<Guid> organizationIds,
        CancellationToken cancellationToken)
    {
        if (organizationIds.Count == 0)
        {
            return Array.Empty<Facility>();
        }

        return await dbContext.Facilities.AsNoTracking()
            .Where(f => organizationIds.Contains(f.OrganizationId))
            .ToListAsync(cancellationToken);
    }
}

public sealed class AuditRepository(CoreLabDbContext dbContext) : IAuditRepository
{
    public async Task<IReadOnlyCollection<AuditRecord>> ListAsync(
        IReadOnlyCollection<Guid> organizationIds,
        IReadOnlyCollection<Guid>? facilityIds,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<AuditRecord> query = dbContext.AuditRecords.AsNoTracking()
            .Where(a => dbContext.Organizations.Any(o =>
                o.Id == a.OrganizationId && o.Status == EntityStatus.Active));

        bool hasOrgScope = organizationIds.Count > 0;
        IReadOnlyCollection<Guid> restrictedFacilities = facilityIds ?? Array.Empty<Guid>();

        if (hasOrgScope && restrictedFacilities.Count > 0)
        {
            query = query.Where(a =>
                organizationIds.Contains(a.OrganizationId)
                || (a.FacilityId != null && restrictedFacilities.Contains(a.FacilityId.Value)));
        }
        else if (hasOrgScope)
        {
            query = query.Where(a => organizationIds.Contains(a.OrganizationId));
        }
        else
        {
            query = query.Where(a =>
                a.FacilityId != null && restrictedFacilities.Contains(a.FacilityId.Value));
        }

        return await query
            .OrderByDescending(a => a.OccurredAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(AuditRecord record, CancellationToken cancellationToken)
    {
        dbContext.AuditRecords.Add(record);
        return Task.CompletedTask;
    }
}

public sealed class UnitOfWork(CoreLabDbContext dbContext) : IUnitOfWork
{
    public Task BeginTransactionAsync(CancellationToken cancellationToken) =>
        dbContext.Database.BeginTransactionAsync(cancellationToken);

    public Task CommitTransactionAsync(CancellationToken cancellationToken) =>
        dbContext.Database.CommitTransactionAsync(cancellationToken);

    public Task RollbackTransactionAsync(CancellationToken cancellationToken) =>
        dbContext.Database.RollbackTransactionAsync(cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

public sealed class AccessionRepository(CoreLabDbContext dbContext) : IAccessionRepository
{
    public Task<Accession?> GetAsync(Guid accessionId, CancellationToken cancellationToken) =>
        dbContext.Accessions.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accessionId, cancellationToken);

    public Task<IReadOnlyCollection<Accession>> ListAsync(
        Guid organizationId,
        Guid? facilityId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Accession> query = dbContext.Accessions.AsNoTracking();

        // Organization scoping: Accessions have FacilityId, and Facilities belong to Organizations
        if (facilityId.HasValue)
        {
            query = query.Where(a => a.FacilityId == facilityId.Value);
        }
        else
        {
            // When no facility filter, scope by organization through facilities
            query = query.Where(a =>
                dbContext.Facilities.Any(f => f.Id == a.FacilityId && f.OrganizationId == organizationId));
        }

        return Task.FromResult<IReadOnlyCollection<Accession>>(query.ToList());
    }

    public Task AddAsync(Accession accession, CancellationToken cancellationToken)
    {
        dbContext.Accessions.Add(accession);
        return Task.CompletedTask;
    }
}

public sealed class OrderItemRepository(CoreLabDbContext dbContext) : IOrderItemRepository
{
    public Task<OrderItem?> GetAsync(Guid orderItemId, CancellationToken cancellationToken) =>
        dbContext.OrderItems.AsNoTracking()
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId, cancellationToken);

    public Task<IReadOnlyCollection<OrderItem>> ListAsync(
        Guid organizationId,
        Guid? facilityId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<OrderItem> query = dbContext.OrderItems.AsNoTracking();

        // OrderItems have OrderId directly, Orders have OrganizationId
        query = query.Where(oi => oi.OrderId == organizationId);

        if (facilityId.HasValue)
        {
            // When facility filter, also check the order's facility
            query = query.Where(oi =>
                dbContext.Orders.Any(o => o.Id == oi.OrderId && o.FacilityId == facilityId.Value));
        }

        return Task.FromResult<IReadOnlyCollection<OrderItem>>(query.ToList());
    }

    public Task AddAsync(OrderItem orderItem, CancellationToken cancellationToken)
    {
        dbContext.OrderItems.Add(orderItem);
        return Task.CompletedTask;
    }
}

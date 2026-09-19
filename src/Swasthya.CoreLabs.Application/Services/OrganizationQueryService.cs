using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class OrganizationQueryService(
    IOrganizationRepository organizationRepository,
    IFacilityRepository facilityRepository,
    IAuditRepository auditRepository,
    ICorrelationIdProvider correlationIdProvider)
{
    public async Task<IReadOnlyCollection<OrganizationDto>> ListScopedAsync(
        AuthContext context,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.OrganizationList);

        IReadOnlyCollection<Organization> organizations = await organizationRepository
            .ListByIdsAsync(context.OrganizationIds, cancellationToken);

        if (organizations.Count == 0)
        {
            return Array.Empty<OrganizationDto>();
        }

        IReadOnlyCollection<Facility> facilities = await facilityRepository
            .ListByOrganizationIdsAsync(
                organizations.Select(o => o.Id).ToArray(), cancellationToken);

        List<OrganizationDto> result = [];
        foreach (Organization organization in organizations
                     .Where(o => o.Status == EntityStatus.Active))
        {
            if (!context.IsGrantedAtOrg(Permissions.OrganizationList, organization.Id))
            {
                continue;
            }

            IReadOnlyCollection<Guid> accessibleFacilityIds = ResolveFacilityIds(
                context, organization.Id, fullAccess => fullAccess
                    ? facilities.Where(f => f.OrganizationId == organization.Id)
                        .Select(f => f.Id)
                        .ToArray()
                    : null);

            result.Add(new OrganizationDto(
                organization.Id,
                organization.Name,
                organization.Status,
                HasFullOrgAccess(context, organization.Id),
                accessibleFacilityIds));
        }

        await WriteAuditAsync(
            context,
            context.OrganizationIds,
            AuditActions.OrganizationListed,
            AuditResourceTypes.Organization,
            AuditOutcome.Success,
            cancellationToken);

        return result;
    }

    public async Task<OrganizationDto> GetScopedAsync(
        AuthContext context,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.OrganizationList, organizationId);

        Organization? organization = (await organizationRepository
            .ListByIdsAsync([organizationId], cancellationToken)).FirstOrDefault();

        if (organization is null || organization.Status != EntityStatus.Active)
        {
            throw new NotFoundException(
                $"Organization '{organizationId}' was not found or is not active.");
        }

        IReadOnlyCollection<Facility> facilities = await facilityRepository
            .ListByOrganizationIdsAsync([organization.Id], cancellationToken);

        IReadOnlyCollection<Guid> accessibleFacilityIds = ResolveFacilityIds(
            context, organization.Id, fullAccess => fullAccess
                ? facilities.Select(f => f.Id).ToArray()
                : null);

        await WriteAuditAsync(
            context,
            [organization.Id],
            AuditActions.OrganizationListed,
            AuditResourceTypes.Organization,
            AuditOutcome.Success,
            cancellationToken,
            resourceId: organization.Id.ToString());

        return new OrganizationDto(
            organization.Id,
            organization.Name,
            organization.Status,
            HasFullOrgAccess(context, organization.Id),
            accessibleFacilityIds);
    }

    private static bool HasFullOrgAccess(AuthContext context, Guid organizationId) =>
        context.Grants.Any(g =>
            g.PermissionCode == Permissions.OrganizationList
            && g.OrganizationId == organizationId
            && g.FacilityId is null);

    private static IReadOnlyCollection<Guid> ResolveFacilityIds(
        AuthContext context,
        Guid organizationId,
        Func<bool, IReadOnlyCollection<Guid>?> fullAccessResolver)
    {
        IReadOnlyCollection<Guid>? all = fullAccessResolver(HasFullOrgAccess(context, organizationId));
        if (all is not null)
        {
            return all;
        }

        return context.Grants
            .Where(g => g.PermissionCode == Permissions.OrganizationList
                && g.OrganizationId == organizationId
                && g.FacilityId is not null)
            .Select(g => g.FacilityId!.Value)
            .Distinct()
            .ToArray();
    }

    private async Task WriteAuditAsync(
        AuthContext context,
        IReadOnlyCollection<Guid> organizationIds,
        string action,
        string resourceType,
        AuditOutcome outcome,
        CancellationToken cancellationToken,
        string? resourceId = null)
    {
        foreach (Guid organizationId in organizationIds)
        {
            await auditRepository.AddAsync(
                AuditRecordFactory.Create(
                    context,
                    correlationIdProvider.Current,
                    organizationId,
                    null,
                    action,
                    resourceType,
                    resourceId,
                    outcome,
                    null),
                cancellationToken);
        }
    }
}

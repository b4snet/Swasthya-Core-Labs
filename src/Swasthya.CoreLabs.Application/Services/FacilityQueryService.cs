using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class FacilityQueryService(
    IOrganizationRepository organizationRepository,
    IFacilityRepository facilityRepository,
    IAuditRepository auditRepository,
    ICorrelationIdProvider correlationIdProvider)
{
    public async Task<IReadOnlyCollection<FacilityDto>> ListScopedAsync(
        AuthContext context,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.FacilityList, organizationId);

        Organization? organization = (await organizationRepository
            .ListByIdsAsync([organizationId], cancellationToken)).FirstOrDefault();

        if (organization is null || organization.Status != EntityStatus.Active)
        {
            throw new NotFoundException(
                $"Organization '{organizationId}' was not found or is not active.");
        }

        IReadOnlyCollection<Facility> facilities = await facilityRepository
            .ListByOrganizationIdsAsync([organizationId], cancellationToken);

        bool fullOrgAccess = context.Grants.Any(g =>
            g.PermissionCode == Permissions.FacilityList
            && g.OrganizationId == organizationId
            && g.FacilityId is null);

        IReadOnlyCollection<Guid> accessibleFacilityIds = fullOrgAccess
            ? facilities.Select(f => f.Id).ToArray()
            : context.Grants
                .Where(g => g.PermissionCode == Permissions.FacilityList
                    && g.OrganizationId == organizationId
                    && g.FacilityId is not null)
                .Select(g => g.FacilityId!.Value)
                .Distinct()
                .ToArray();

        List<FacilityDto> result = facilities
            .Where(f => f.Status == EntityStatus.Active && accessibleFacilityIds.Contains(f.Id))
            .Select(f => new FacilityDto(f.Id, f.OrganizationId, f.Code, f.Name, f.Status))
            .ToList();

        await auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                null,
                AuditActions.FacilityListed,
                AuditResourceTypes.Facility,
                null,
                AuditOutcome.Success,
                null),
            cancellationToken);

        return result;
    }
}

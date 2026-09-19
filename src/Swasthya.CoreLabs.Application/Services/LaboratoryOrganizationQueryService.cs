using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class LaboratoryOrganizationQueryService(
    ILaboratoryCatalogRepository catalogRepository,
    IAuditRepository auditRepository,
    ICorrelationIdProvider correlationIdProvider)
{
    public async Task<IReadOnlyCollection<DisciplineDto>> ListDisciplinesAsync(
        AuthContext context,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);

        IReadOnlyCollection<Discipline> disciplines = await catalogRepository
            .ListDisciplinesAsync(organizationId, cancellationToken);

        await LaboratoryAudit.WriteForResourceAsync(
            auditRepository,
            correlationIdProvider,
            context,
            organizationId,
            AuditResourceTypes.Discipline,
            cancellationToken);

        return disciplines.Select(d => d.ToDto()).ToArray();
    }

    public async Task<IReadOnlyCollection<LaboratorySectionDto>> ListSectionsAsync(
        AuthContext context,
        Guid organizationId,
        Guid? facilityId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);

        IReadOnlyCollection<Guid>? facilityFilter = LaboratoryScope.ResolveFacilityFilter(
            context, Permissions.LaboratoryCatalogRead, organizationId, facilityId);

        IReadOnlyCollection<LaboratorySection> sections = await catalogRepository
            .ListSectionsAsync(organizationId, facilityFilter, cancellationToken);

        await LaboratoryAudit.WriteForResourceAsync(
            auditRepository,
            correlationIdProvider,
            context,
            organizationId,
            AuditResourceTypes.LaboratorySection,
            cancellationToken);

        return sections.Select(s => s.ToDto()).ToArray();
    }
}

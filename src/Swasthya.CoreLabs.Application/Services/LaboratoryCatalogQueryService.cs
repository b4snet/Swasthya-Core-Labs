using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class LaboratoryCatalogQueryService(
    ILaboratoryCatalogRepository catalogRepository,
    IAuditRepository auditRepository,
    ICorrelationIdProvider correlationIdProvider)
{
    public async Task<IReadOnlyCollection<ResultDataTypeDto>> ListResultDataTypesAsync(
        AuthContext context,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);
        IReadOnlyCollection<ResultDataType> items = await catalogRepository
            .ListResultDataTypesAsync(organizationId, cancellationToken);
        await AuditAsync(context, organizationId, AuditResourceTypes.ResultDataType, cancellationToken);
        return items.Select(i => i.ToDto()).ToArray();
    }

    public async Task<IReadOnlyCollection<UnitDto>> ListUnitsAsync(
        AuthContext context,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);
        IReadOnlyCollection<Unit> items = await catalogRepository
            .ListUnitsAsync(organizationId, cancellationToken);
        await AuditAsync(context, organizationId, AuditResourceTypes.Unit, cancellationToken);
        return items.Select(i => i.ToDto()).ToArray();
    }

    public async Task<IReadOnlyCollection<CodeSystemDto>> ListCodeSystemsAsync(
        AuthContext context,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);
        IReadOnlyCollection<CodeSystem> items = await catalogRepository
            .ListCodeSystemsAsync(organizationId, cancellationToken);
        await AuditAsync(context, organizationId, AuditResourceTypes.CodeSystem, cancellationToken);
        return items.Select(i => i.ToDto()).ToArray();
    }

    public async Task<IReadOnlyCollection<SpecimenTypeDto>> ListSpecimenTypesAsync(
        AuthContext context,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);
        IReadOnlyCollection<SpecimenType> items = await catalogRepository
            .ListSpecimenTypesAsync(organizationId, cancellationToken);
        await AuditAsync(context, organizationId, AuditResourceTypes.SpecimenType, cancellationToken);
        return items.Select(i => i.ToDto()).ToArray();
    }

    public async Task<IReadOnlyCollection<SpecimenSourceDto>> ListSpecimenSourcesAsync(
        AuthContext context,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);
        IReadOnlyCollection<SpecimenSource> items = await catalogRepository
            .ListSpecimenSourcesAsync(organizationId, cancellationToken);
        await AuditAsync(context, organizationId, AuditResourceTypes.SpecimenSource, cancellationToken);
        return items.Select(i => i.ToDto()).ToArray();
    }

    public async Task<IReadOnlyCollection<ContainerTypeDto>> ListContainerTypesAsync(
        AuthContext context,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);
        IReadOnlyCollection<ContainerType> items = await catalogRepository
            .ListContainerTypesAsync(organizationId, cancellationToken);
        await AuditAsync(context, organizationId, AuditResourceTypes.ContainerType, cancellationToken);
        return items.Select(i => i.ToDto()).ToArray();
    }

    public async Task<IReadOnlyCollection<ResultStatusDto>> ListResultStatusesAsync(
        AuthContext context,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);
        IReadOnlyCollection<ResultStatus> items = await catalogRepository
            .ListResultStatusesAsync(organizationId, cancellationToken);
        await AuditAsync(context, organizationId, AuditResourceTypes.ResultStatus, cancellationToken);
        return items.Select(i => i.ToDto()).ToArray();
    }

    public async Task<IReadOnlyCollection<AnalyzerDto>> ListAnalyzersAsync(
        AuthContext context,
        Guid organizationId,
        Guid? facilityId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);
        IReadOnlyCollection<Guid>? facilityFilter = LaboratoryScope.ResolveFacilityFilter(
            context, Permissions.LaboratoryCatalogRead, organizationId, facilityId);
        IReadOnlyCollection<Analyzer> items = await catalogRepository
            .ListAnalyzersAsync(organizationId, facilityFilter, cancellationToken);
        await AuditAsync(context, organizationId, AuditResourceTypes.Analyzer, cancellationToken);
        return items.Select(i => i.ToDto()).ToArray();
    }

    public async Task<IReadOnlyCollection<ConfigurationItemDto>> ListConfigurationItemsAsync(
        AuthContext context,
        Guid organizationId,
        Guid? facilityId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);
        IReadOnlyCollection<Guid>? facilityFilter = LaboratoryScope.ResolveFacilityFilter(
            context, Permissions.LaboratoryCatalogRead, organizationId, facilityId);
        IReadOnlyCollection<ConfigurationItem> items = await catalogRepository
            .ListConfigurationItemsAsync(organizationId, facilityFilter, cancellationToken);
        await AuditAsync(context, organizationId, AuditResourceTypes.ConfigurationItem, cancellationToken);
        return items.Select(i => i.ToDto()).ToArray();
    }

    private Task AuditAsync(
        AuthContext context,
        Guid organizationId,
        string resourceType,
        CancellationToken cancellationToken) =>
        LaboratoryAudit.WriteForResourceAsync(
            auditRepository,
            correlationIdProvider,
            context,
            organizationId,
            resourceType,
            cancellationToken);
}

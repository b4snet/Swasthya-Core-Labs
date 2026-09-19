using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Application.Services;

internal static class LaboratoryScope
{
    /// <summary>
    /// Resolves the facility filter for a facility-scoped query: null means all
    /// facilities in the organization (organization-wide grant); otherwise the
    /// caller's granted facilities.
    /// </summary>
    public static IReadOnlyCollection<Guid>? ResolveFacilityFilter(
        AuthContext context,
        string permissionCode,
        Guid organizationId,
        Guid? requestedFacilityId)
    {
        if (requestedFacilityId is { } facilityId)
        {
            LaboratoryPermissionGuard.RequireFacilityScoped(
                context, permissionCode, organizationId, facilityId);
            return [facilityId];
        }

        bool organizationWide = context.Grants.Any(g =>
            string.Equals(g.PermissionCode, permissionCode, StringComparison.Ordinal)
            && g.OrganizationId == organizationId
            && g.FacilityId is null);

        if (organizationWide)
        {
            return null;
        }

        Guid[] grantedFacilities = context.Grants
            .Where(g => string.Equals(g.PermissionCode, permissionCode, StringComparison.Ordinal)
                && g.OrganizationId == organizationId
                && g.FacilityId is not null)
            .Select(g => g.FacilityId!.Value)
            .Distinct()
            .ToArray();

        if (grantedFacilities.Length == 0)
        {
            throw new Exceptions.PermissionDeniedException(
                $"Principal has no facility-scoped grant for permission '{permissionCode}'.");
        }

        return grantedFacilities;
    }
}

internal static class LaboratoryAudit
{
    public static Task WriteForResourceAsync(
        IAuditRepository auditRepository,
        ICorrelationIdProvider correlationIdProvider,
        AuthContext context,
        Guid organizationId,
        string resourceType,
        CancellationToken cancellationToken) =>
        auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                null,
                AuditActions.LaboratoryListed,
                resourceType,
                null,
                Domain.Common.AuditOutcome.Success,
                null),
            cancellationToken);

    public static Task WriteAsync(
        IAuditRepository auditRepository,
        ICorrelationIdProvider correlationIdProvider,
        AuthContext context,
        Guid organizationId,
        string action,
        string resourceType,
        string? resourceId,
        CancellationToken cancellationToken) =>
        auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                null,
                action,
                resourceType,
                resourceId,
                Domain.Common.AuditOutcome.Success,
                null),
            cancellationToken);
}

internal static class LaboratoryMappings
{
    public static DisciplineDto ToDto(this Discipline discipline) =>
        new(
            discipline.Id,
            discipline.OrganizationId,
            discipline.Code,
            discipline.Name,
            discipline.Description,
            discipline.Status);

    public static LaboratorySectionDto ToDto(this LaboratorySection section) =>
        new(
            section.Id,
            section.OrganizationId,
            section.FacilityId,
            section.Code,
            section.Name,
            section.Description,
            section.Status);

    public static ResultDataTypeDto ToDto(this ResultDataType dataType) =>
        new(
            dataType.Id,
            dataType.OrganizationId,
            dataType.Code,
            dataType.Name,
            dataType.Kind,
            dataType.Description,
            dataType.Status);

    public static UnitDto ToDto(this Unit unit) =>
        new(
            unit.Id,
            unit.OrganizationId,
            unit.Code,
            unit.Name,
            unit.UcumCode,
            unit.Description,
            unit.Status);

    public static CodeSystemDto ToDto(this CodeSystem codeSystem) =>
        new(
            codeSystem.Id,
            codeSystem.OrganizationId,
            codeSystem.Code,
            codeSystem.Name,
            codeSystem.CanonicalUrl,
            codeSystem.VersionRequired,
            codeSystem.Description,
            codeSystem.Status);

    public static SpecimenTypeDto ToDto(this SpecimenType specimenType) =>
        new(
            specimenType.Id,
            specimenType.OrganizationId,
            specimenType.Code,
            specimenType.Name,
            specimenType.Description,
            specimenType.Status);

    public static SpecimenSourceDto ToDto(this SpecimenSource specimenSource) =>
        new(
            specimenSource.Id,
            specimenSource.OrganizationId,
            specimenSource.Code,
            specimenSource.Name,
            specimenSource.Description,
            specimenSource.Status);

    public static ContainerTypeDto ToDto(this ContainerType containerType) =>
        new(
            containerType.Id,
            containerType.OrganizationId,
            containerType.Code,
            containerType.Name,
            containerType.Description,
            containerType.HandlingNotes,
            containerType.Status);

    public static ResultStatusDto ToDto(this ResultStatus resultStatus) =>
        new(
            resultStatus.Id,
            resultStatus.OrganizationId,
            resultStatus.Code,
            resultStatus.Name,
            resultStatus.Description,
            resultStatus.Status);

    public static AnalyzerDto ToDto(this Analyzer analyzer) =>
        new(
            analyzer.Id,
            analyzer.OrganizationId,
            analyzer.FacilityId,
            analyzer.Code,
            analyzer.Name,
            analyzer.Manufacturer,
            analyzer.Model,
            analyzer.Description,
            analyzer.Status);

    public static ConfigurationItemDto ToDto(this ConfigurationItem item) =>
        new(
            item.Id,
            item.OrganizationId,
            item.FacilityId,
            item.Key,
            item.Value,
            item.ValueType,
            item.Description,
            item.Status);

    public static TestVersionDto ToDto(this TestVersion version) =>
        new(
            version.Id,
            version.VersionNumber,
            version.Name,
            version.Description,
            version.DisciplineId,
            version.ResultDataTypeId,
            version.UnitId,
            version.EffectiveFromUtc,
            version.EffectiveToUtc,
            version.CreatedAtUtc);

    public static TestSpecimenRequirementDto ToDto(this TestSpecimenRequirement requirement) =>
        new(
            requirement.Id,
            requirement.SpecimenTypeId,
            requirement.SpecimenSourceId,
            requirement.ContainerTypeId,
            requirement.RequiredQuantity,
            requirement.QuantityUnitUcumCode,
            requirement.Notes);

    public static TestTerminologyMappingDto ToDto(this TestTerminologyMapping mapping) =>
        new(
            mapping.Id,
            mapping.CodeSystemId,
            mapping.ExternalCode,
            mapping.ExternalVersion,
            mapping.Status,
            mapping.Notes,
            mapping.EffectiveFromUtc,
            mapping.EffectiveToUtc);

    public static ReferenceRangeDto ToDto(this ReferenceRange range) =>
        new(
            range.Id,
            range.OrganizationId,
            range.FacilityId,
            range.TestId,
            range.Source,
            range.Population,
            range.Sex,
            range.AgeMinimum,
            range.AgeMaximum,
            range.AgeUnitUcumCode,
            range.SpecimenContext,
            range.LowValue,
            range.HighValue,
            range.TextualReference,
            range.UnitId,
            range.EffectiveFromUtc,
            range.EffectiveToUtc,
            range.Status);

    public static PanelVersionDto ToDto(this PanelVersion version) =>
        new(
            version.Id,
            version.VersionNumber,
            version.Name,
            version.Description,
            version.EffectiveFromUtc,
            version.EffectiveToUtc,
            version.CreatedAtUtc);

    public static PanelMembershipDto ToDto(this PanelTestMembership membership, string testCode) =>
        new(
            membership.Id,
            membership.TestId,
            testCode,
            membership.SequenceNumber,
            membership.DisplayText);
}

using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record DisciplineDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string Description,
    MasterDataStatus Status);

public sealed record LaboratorySectionDto(
    Guid Id,
    Guid OrganizationId,
    Guid FacilityId,
    string Code,
    string Name,
    string Description,
    MasterDataStatus Status);

public sealed record ResultDataTypeDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    ResultDataTypeKind Kind,
    string Description,
    MasterDataStatus Status);

public sealed record UnitDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string UcumCode,
    string Description,
    MasterDataStatus Status);

public sealed record CodeSystemDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string CanonicalUrl,
    bool VersionRequired,
    string Description,
    MasterDataStatus Status);

public sealed record SpecimenTypeDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string Description,
    MasterDataStatus Status);

public sealed record SpecimenSourceDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string Description,
    MasterDataStatus Status);

public sealed record ContainerTypeDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string Description,
    string HandlingNotes,
    MasterDataStatus Status);

public sealed record ResultStatusDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string Description,
    MasterDataStatus Status);

public sealed record AnalyzerDto(
    Guid Id,
    Guid OrganizationId,
    Guid FacilityId,
    string Code,
    string Name,
    string Manufacturer,
    string Model,
    string Description,
    MasterDataStatus Status);

public sealed record ConfigurationItemDto(
    Guid Id,
    Guid OrganizationId,
    Guid? FacilityId,
    string Key,
    string Value,
    ConfigurationValueType ValueType,
    string Description,
    MasterDataStatus Status);

public sealed record TestVersionDto(
    Guid Id,
    int VersionNumber,
    string Name,
    string Description,
    Guid? DisciplineId,
    Guid ResultDataTypeId,
    Guid? UnitId,
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    DateTimeOffset CreatedAtUtc);

public sealed record TestSpecimenRequirementDto(
    Guid Id,
    Guid SpecimenTypeId,
    Guid? SpecimenSourceId,
    Guid? ContainerTypeId,
    decimal? RequiredQuantity,
    string? QuantityUnitUcumCode,
    string Notes);

public sealed record TestTerminologyMappingDto(
    Guid Id,
    Guid CodeSystemId,
    string ExternalCode,
    string ExternalVersion,
    TerminologyMappingStatus Status,
    string Notes,
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc);

public sealed record ReferenceRangeDto(
    Guid Id,
    Guid OrganizationId,
    Guid? FacilityId,
    Guid TestId,
    string Source,
    string Population,
    ReferenceRangeSex? Sex,
    decimal? AgeMinimum,
    decimal? AgeMaximum,
    string? AgeUnitUcumCode,
    string SpecimenContext,
    decimal? LowValue,
    decimal? HighValue,
    string TextualReference,
    Guid? UnitId,
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    MasterDataStatus Status);

public sealed record TestSummaryDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    MasterDataStatus Status,
    int CurrentVersionNumber,
    string LatestName);

public sealed record TestDetailDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    MasterDataStatus Status,
    int CurrentVersionNumber,
    IReadOnlyCollection<TestVersionDto> Versions,
    IReadOnlyCollection<TestSpecimenRequirementDto> SpecimenRequirements,
    IReadOnlyCollection<TestTerminologyMappingDto> TerminologyMappings,
    IReadOnlyCollection<ReferenceRangeDto> ReferenceRanges,
    IReadOnlyCollection<Guid> ApplicableFacilityIds);

public sealed record PanelVersionDto(
    Guid Id,
    int VersionNumber,
    string Name,
    string Description,
    DateTimeOffset EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    DateTimeOffset CreatedAtUtc);

public sealed record PanelMembershipDto(
    Guid Id,
    Guid TestId,
    string TestCode,
    int SequenceNumber,
    string DisplayText);

public sealed record PanelSummaryDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    MasterDataStatus Status,
    int CurrentVersionNumber,
    string LatestName);

public sealed record PanelDetailDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    MasterDataStatus Status,
    int CurrentVersionNumber,
    IReadOnlyCollection<PanelVersionDto> Versions,
    IReadOnlyCollection<PanelMembershipDto> Memberships);

public sealed record CreateTestRequest(
    string Code,
    string Name,
    Guid ResultDataTypeId,
    Guid? DisciplineId = null,
    Guid? UnitId = null,
    string? Description = null,
    DateTimeOffset? EffectiveFromUtc = null,
    DateTimeOffset? EffectiveToUtc = null,
    IReadOnlyCollection<Guid>? ApplicableFacilityIds = null);

public sealed record CreateTestVersionRequest(
    string Name,
    Guid ResultDataTypeId,
    Guid? DisciplineId = null,
    Guid? UnitId = null,
    string? Description = null,
    DateTimeOffset? EffectiveFromUtc = null,
    DateTimeOffset? EffectiveToUtc = null);

public sealed record TestSpecimenRequirementInput(
    Guid SpecimenTypeId,
    Guid? SpecimenSourceId = null,
    Guid? ContainerTypeId = null,
    decimal? RequiredQuantity = null,
    string? QuantityUnitUcumCode = null,
    string? Notes = null);

public sealed record ReplaceSpecimenRequirementsRequest(
    IReadOnlyCollection<TestSpecimenRequirementInput> Items);

public sealed record TestTerminologyMappingInput(
    Guid CodeSystemId,
    string ExternalCode,
    string? ExternalVersion = null,
    string? Notes = null,
    DateTimeOffset? EffectiveFromUtc = null,
    DateTimeOffset? EffectiveToUtc = null);

public sealed record ReplaceTerminologyMappingsRequest(
    IReadOnlyCollection<TestTerminologyMappingInput> Items);

public sealed record ReplaceTestFacilitiesRequest(
    IReadOnlyCollection<Guid> FacilityIds);

public sealed record PanelMembershipInput(
    string TestCode,
    int SequenceNumber = 0,
    string? DisplayText = null);

public sealed record CreatePanelRequest(
    string Code,
    string Name,
    IReadOnlyCollection<PanelMembershipInput>? Tests = null,
    string? Description = null,
    DateTimeOffset? EffectiveFromUtc = null,
    DateTimeOffset? EffectiveToUtc = null);

public sealed record CreatePanelVersionRequest(
    string Name,
    string? Description = null,
    DateTimeOffset? EffectiveFromUtc = null,
    DateTimeOffset? EffectiveToUtc = null);

public sealed record ReplacePanelMembershipsRequest(
    IReadOnlyCollection<PanelMembershipInput> Items);

public sealed record CreateReferenceRangeRequest(
    string Source,
    Guid? FacilityId = null,
    string? Population = null,
    ReferenceRangeSex? Sex = null,
    decimal? AgeMinimum = null,
    decimal? AgeMaximum = null,
    string? AgeUnitUcumCode = null,
    string? SpecimenContext = null,
    decimal? LowValue = null,
    decimal? HighValue = null,
    string? TextualReference = null,
    Guid? UnitId = null,
    DateTimeOffset? EffectiveFromUtc = null,
    DateTimeOffset? EffectiveToUtc = null);

public sealed record CreateConfigurationItemRequest(
    string Key,
    string Value,
    ConfigurationValueType ValueType,
    Guid? FacilityId = null,
    string? Description = null);

public sealed record UpdateConfigurationItemRequest(
    string Value,
    ConfigurationValueType ValueType,
    string? Description = null);

using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Application.Abstractions;

public interface ILaboratoryCatalogRepository
{
    Task<IReadOnlyCollection<Discipline>> ListDisciplinesAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<Discipline?> FindDisciplineAsync(
        Guid organizationId,
        Guid disciplineId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<LaboratorySection>> ListSectionsAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid>? facilityIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ResultDataType>> ListResultDataTypesAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<ResultDataType?> FindResultDataTypeAsync(
        Guid organizationId,
        Guid resultDataTypeId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Unit>> ListUnitsAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<Unit?> FindUnitAsync(
        Guid organizationId,
        Guid unitId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CodeSystem>> ListCodeSystemsAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<CodeSystem?> FindCodeSystemAsync(
        Guid organizationId,
        Guid codeSystemId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SpecimenType>> ListSpecimenTypesAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<SpecimenType?> FindSpecimenTypeAsync(
        Guid organizationId,
        Guid specimenTypeId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SpecimenSource>> ListSpecimenSourcesAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<SpecimenSource?> FindSpecimenSourceAsync(
        Guid organizationId,
        Guid specimenSourceId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ContainerType>> ListContainerTypesAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<ContainerType?> FindContainerTypeAsync(
        Guid organizationId,
        Guid containerTypeId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ResultStatus>> ListResultStatusesAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Analyzer>> ListAnalyzersAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid>? facilityIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ConfigurationItem>> ListConfigurationItemsAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid>? facilityIds,
        CancellationToken cancellationToken);

    Task<ConfigurationItem?> FindConfigurationItemAsync(
        Guid organizationId,
        Guid? facilityId,
        string key,
        CancellationToken cancellationToken);

    Task AddConfigurationItemAsync(
        ConfigurationItem item,
        CancellationToken cancellationToken);
}

public interface ITestCatalogRepository
{
    Task<IReadOnlyCollection<Test>> ListTestsAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<Test?> FindTestAsync(
        Guid organizationId,
        Guid testId,
        CancellationToken cancellationToken);

    Task<Test?> FindTestByCodeAsync(
        Guid organizationId,
        string code,
        CancellationToken cancellationToken);

    Task AddTestAsync(Test test, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TestVersion>> ListTestVersionsAsync(
        IReadOnlyCollection<Guid> testIds,
        CancellationToken cancellationToken);

    Task AddTestVersionAsync(TestVersion version, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TestSpecimenRequirement>> ListSpecimenRequirementsAsync(
        IReadOnlyCollection<Guid> testIds,
        CancellationToken cancellationToken);

    Task AddSpecimenRequirementAsync(
        TestSpecimenRequirement requirement,
        CancellationToken cancellationToken);

    void RemoveSpecimenRequirements(
        IReadOnlyCollection<TestSpecimenRequirement> requirements);

    Task<IReadOnlyCollection<TestTerminologyMapping>> ListTerminologyMappingsAsync(
        IReadOnlyCollection<Guid> testIds,
        CancellationToken cancellationToken);

    Task AddTerminologyMappingAsync(
        TestTerminologyMapping mapping,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TestFacility>> ListTestFacilitiesAsync(
        IReadOnlyCollection<Guid> testIds,
        CancellationToken cancellationToken);

    Task AddTestFacilityAsync(TestFacility facility, CancellationToken cancellationToken);

    void RemoveTestFacilities(IReadOnlyCollection<TestFacility> facilities);

    Task<IReadOnlyCollection<ReferenceRange>> ListReferenceRangesAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid>? facilityIds,
        IReadOnlyCollection<Guid>? testIds,
        CancellationToken cancellationToken);

    Task<ReferenceRange?> FindReferenceRangeAsync(
        Guid organizationId,
        Guid referenceRangeId,
        CancellationToken cancellationToken);

    Task AddReferenceRangeAsync(ReferenceRange range, CancellationToken cancellationToken);
}

public interface IPanelRepository
{
    Task<IReadOnlyCollection<Panel>> ListPanelsAsync(
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<Panel?> FindPanelAsync(
        Guid organizationId,
        Guid panelId,
        CancellationToken cancellationToken);

    Task<Panel?> FindPanelByCodeAsync(
        Guid organizationId,
        string code,
        CancellationToken cancellationToken);

    Task AddPanelAsync(Panel panel, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PanelVersion>> ListPanelVersionsAsync(
        IReadOnlyCollection<Guid> panelIds,
        CancellationToken cancellationToken);

    Task AddPanelVersionAsync(PanelVersion version, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PanelTestMembership>> ListPanelMembershipsAsync(
        IReadOnlyCollection<Guid> panelIds,
        CancellationToken cancellationToken);

    Task AddPanelMembershipAsync(
        PanelTestMembership membership,
        CancellationToken cancellationToken);

    void RemovePanelMemberships(
        IReadOnlyCollection<PanelTestMembership> memberships);
}

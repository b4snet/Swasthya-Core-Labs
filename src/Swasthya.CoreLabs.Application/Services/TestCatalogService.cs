using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.Laboratory;
using Swasthya.CoreLabs.Domain.IdentityTenancy;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class TestCatalogService(
    ITestCatalogRepository testCatalogRepository,
    ILaboratoryCatalogRepository catalogRepository,
    IFacilityRepository facilityRepository,
    IAuditRepository auditRepository,
    ICorrelationIdProvider correlationIdProvider)
{
    public async Task<IReadOnlyCollection<TestSummaryDto>> ListTestsAsync(
        AuthContext context,
        Guid organizationId,
        Guid? facilityId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);

        IReadOnlyCollection<Guid>? facilityFilter = LaboratoryScope.ResolveFacilityFilter(
            context, Permissions.LaboratoryCatalogRead, organizationId, facilityId);

        IReadOnlyCollection<Test> tests = await testCatalogRepository
            .ListTestsAsync(organizationId, cancellationToken);

        Guid[] testIds = tests.Select(t => t.Id).ToArray();
        IReadOnlyCollection<TestVersion> versions = await testCatalogRepository
            .ListTestVersionsAsync(testIds, cancellationToken);

        if (facilityFilter is not null && tests.Count > 0)
        {
            IReadOnlyCollection<TestFacility> facilities = await testCatalogRepository
                .ListTestFacilitiesAsync(testIds, cancellationToken);
            HashSet<Guid> applicable = tests
                .Where(t => !facilities.Any(f => f.TestId == t.Id)
                    || facilities.Any(f => f.TestId == t.Id && facilityFilter.Contains(f.FacilityId)))
                .Select(t => t.Id)
                .ToHashSet();
            tests = tests.Where(t => applicable.Contains(t.Id)).ToArray();
        }

        await LaboratoryAudit.WriteForResourceAsync(
            auditRepository,
            correlationIdProvider,
            context,
            organizationId,
            AuditResourceTypes.Test,
            cancellationToken);

        return tests
            .Select(t => new TestSummaryDto(
                t.Id,
                t.OrganizationId,
                t.Code,
                t.Status,
                t.CurrentVersionNumber,
                LatestVersionName(versions, t.Id)))
            .OrderBy(t => t.Code, StringComparer.Ordinal)
            .ToArray();
    }

    public async Task<TestDetailDto> GetTestAsync(
        AuthContext context,
        Guid organizationId,
        string code,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);

        Test test = await RequireTestAsync(organizationId, code, cancellationToken);
        return await BuildDetailAsync(organizationId, test, cancellationToken);
    }

    public async Task<TestDetailDto> CreateTestAsync(
        AuthContext context,
        Guid organizationId,
        CreateTestRequest request,
        CancellationToken cancellationToken)
    {
        LaboratoryPermissionGuard.RequireOrganizationWide(
            context, Permissions.LaboratoryCatalogWrite, organizationId);

        if (request is null)
        {
            throw new ValidationException("A request body is required.");
        }

        if (await testCatalogRepository.FindTestByCodeAsync(organizationId, request.Code ?? string.Empty, cancellationToken)
            is not null)
        {
            throw new ConflictException($"A test with code '{request.Code}' already exists.");
        }

        await RequireResultDataTypeAsync(organizationId, request.ResultDataTypeId, cancellationToken);
        await RequireDisciplineIfProvidedAsync(organizationId, request.DisciplineId, cancellationToken);
        await RequireUnitIfProvidedAsync(organizationId, request.UnitId, cancellationToken);
        Guid[] facilities = await ResolveFacilitiesAsync(
            organizationId, request.ApplicableFacilityIds, cancellationToken);

        Test test;
        TestVersion version;
        try
        {
            test = new Test(organizationId, request.Code ?? string.Empty);
            int versionNumber = test.StartNextVersion();
            version = new TestVersion(
                test.Id,
                versionNumber,
                request.Name ?? string.Empty,
                request.ResultDataTypeId,
                request.DisciplineId,
                request.UnitId,
                request.Description,
                request.EffectiveFromUtc,
                request.EffectiveToUtc);
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }

        await testCatalogRepository.AddTestAsync(test, cancellationToken);
        await testCatalogRepository.AddTestVersionAsync(version, cancellationToken);
        foreach (Guid facilityId in facilities)
        {
            await testCatalogRepository.AddTestFacilityAsync(
                new TestFacility(test.Id, facilityId), cancellationToken);
        }

        await auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                null,
                AuditActions.LaboratoryCreated,
                AuditResourceTypes.Test,
                test.Code,
                AuditOutcome.Success,
                null),
            cancellationToken);

        return await BuildDetailAsync(organizationId, test, cancellationToken);
    }

    public async Task<TestDetailDto> PublishVersionAsync(
        AuthContext context,
        Guid organizationId,
        string code,
        CreateTestVersionRequest request,
        CancellationToken cancellationToken)
    {
        LaboratoryPermissionGuard.RequireOrganizationWide(
            context, Permissions.LaboratoryCatalogWrite, organizationId);

        if (request is null)
        {
            throw new ValidationException("A request body is required.");
        }

        Test test = await RequireTestAsync(organizationId, code, cancellationToken);
        if (test.Status == MasterDataStatus.Retired)
        {
            throw new ValidationException("A retired test cannot receive new versions.");
        }

        await RequireResultDataTypeAsync(organizationId, request.ResultDataTypeId, cancellationToken);
        await RequireDisciplineIfProvidedAsync(organizationId, request.DisciplineId, cancellationToken);
        await RequireUnitIfProvidedAsync(organizationId, request.UnitId, cancellationToken);

        TestVersion version;
        try
        {
            int versionNumber = test.StartNextVersion();
            version = new TestVersion(
                test.Id,
                versionNumber,
                request.Name ?? string.Empty,
                request.ResultDataTypeId,
                request.DisciplineId,
                request.UnitId,
                request.Description,
                request.EffectiveFromUtc,
                request.EffectiveToUtc);
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }

        await testCatalogRepository.AddTestVersionAsync(version, cancellationToken);
        await auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                null,
                AuditActions.LaboratoryVersioned,
                AuditResourceTypes.TestVersion,
                $"{test.Code}:v{version.VersionNumber}",
                AuditOutcome.Success,
                null),
            cancellationToken);

        return await BuildDetailAsync(organizationId, test, cancellationToken);
    }

    public async Task<TestDetailDto> TransitionAsync(
        AuthContext context,
        Guid organizationId,
        string code,
        string transition,
        CancellationToken cancellationToken)
    {
        LaboratoryPermissionGuard.RequireOrganizationWide(
            context, Permissions.LaboratoryCatalogWrite, organizationId);

        MasterDataStatus target = transition switch
        {
            "activate" => MasterDataStatus.Active,
            "deactivate" => MasterDataStatus.Inactive,
            "retire" => MasterDataStatus.Retired,
            _ => throw new ValidationException(
                $"Unknown transition '{transition}'. Expected 'activate', 'deactivate' or 'retire'."),
        };

        Test test = await RequireTestAsync(organizationId, code, cancellationToken);
        if (test.Status == target)
        {
            throw new ValidationException($"Test is already '{target}'.");
        }

        if (!MasterDataLifecycle.CanTransition(test.Status, target))
        {
            throw new ValidationException(
                $"Test cannot transition from '{test.Status}' to '{target}'.");
        }

        test.SetStatus(target);

        string action = target switch
        {
            MasterDataStatus.Active => AuditActions.LaboratoryActivated,
            MasterDataStatus.Inactive => AuditActions.LaboratoryDeactivated,
            _ => AuditActions.LaboratoryRetired,
        };

        await auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                null,
                action,
                AuditResourceTypes.Test,
                test.Code,
                AuditOutcome.Success,
                null),
            cancellationToken);

        return await BuildDetailAsync(organizationId, test, cancellationToken);
    }

    public async Task<TestDetailDto> ReplaceSpecimenRequirementsAsync(
        AuthContext context,
        Guid organizationId,
        string code,
        ReplaceSpecimenRequirementsRequest request,
        CancellationToken cancellationToken)
    {
        LaboratoryPermissionGuard.RequireOrganizationWide(
            context, Permissions.LaboratoryCatalogWrite, organizationId);

        if (request?.Items is null)
        {
            throw new ValidationException("A request body with items is required.");
        }

        Test test = await RequireTestAsync(organizationId, code, cancellationToken);
        RequireEditable(test);

        HashSet<Guid> specimenTypes = new();
        List<TestSpecimenRequirement> requirements = [];
        foreach (TestSpecimenRequirementInput item in request.Items)
        {
            if (!specimenTypes.Add(item.SpecimenTypeId))
            {
                throw new ValidationException(
                    "The same specimen type cannot be listed more than once.");
            }

            await RequireSpecimenTypeAsync(organizationId, item.SpecimenTypeId, cancellationToken);
            await RequireSpecimenSourceIfProvidedAsync(organizationId, item.SpecimenSourceId, cancellationToken);
            await RequireContainerTypeIfProvidedAsync(organizationId, item.ContainerTypeId, cancellationToken);

            try
            {
                requirements.Add(new TestSpecimenRequirement(
                    test.Id,
                    item.SpecimenTypeId,
                    item.SpecimenSourceId,
                    item.ContainerTypeId,
                    item.RequiredQuantity,
                    item.QuantityUnitUcumCode,
                    item.Notes));
            }
            catch (ArgumentException exception)
            {
                throw new ValidationException(exception.Message);
            }
        }

        IReadOnlyCollection<TestSpecimenRequirement> existing = await testCatalogRepository
            .ListSpecimenRequirementsAsync([test.Id], cancellationToken);
        testCatalogRepository.RemoveSpecimenRequirements(existing);
        foreach (TestSpecimenRequirement requirement in requirements)
        {
            await testCatalogRepository.AddSpecimenRequirementAsync(requirement, cancellationToken);
        }

        await auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                null,
                AuditActions.LaboratoryUpdated,
                AuditResourceTypes.TestSpecimenRequirement,
                test.Code,
                AuditOutcome.Success,
                null),
            cancellationToken);

        return await BuildDetailAsync(organizationId, test, cancellationToken);
    }

    public async Task<TestDetailDto> ReplaceTerminologyMappingsAsync(
        AuthContext context,
        Guid organizationId,
        string code,
        ReplaceTerminologyMappingsRequest request,
        CancellationToken cancellationToken)
    {
        LaboratoryPermissionGuard.RequireOrganizationWide(
            context, Permissions.LaboratoryCatalogWrite, organizationId);

        if (request?.Items is null)
        {
            throw new ValidationException("A request body with items is required.");
        }

        Test test = await RequireTestAsync(organizationId, code, cancellationToken);
        RequireEditable(test);

        IReadOnlyCollection<TestTerminologyMapping> existing = await testCatalogRepository
            .ListTerminologyMappingsAsync([test.Id], cancellationToken);

        HashSet<(Guid CodeSystemId, string ExternalCode)> desired = [];
        List<TestTerminologyMapping> toAdd = [];
        foreach (TestTerminologyMappingInput item in request.Items)
        {
            await RequireCodeSystemAsync(organizationId, item.CodeSystemId, cancellationToken);

            TestTerminologyMapping? candidate = null;
            try
            {
                candidate = new TestTerminologyMapping(
                    test.Id,
                    item.CodeSystemId,
                    item.ExternalCode ?? string.Empty,
                    item.ExternalVersion,
                    item.Notes,
                    item.EffectiveFromUtc,
                    item.EffectiveToUtc);
            }
            catch (ArgumentException exception)
            {
                throw new ValidationException(exception.Message);
            }

            if (!desired.Add((candidate.CodeSystemId, candidate.ExternalCode)))
            {
                throw new ValidationException(
                    "The same external code cannot be mapped more than once per code system.");
            }

            if (!existing.Any(m =>
                    m.CodeSystemId == candidate.CodeSystemId
                    && string.Equals(m.ExternalCode, candidate.ExternalCode, StringComparison.Ordinal)))
            {
                toAdd.Add(candidate);
            }
        }

        foreach (TestTerminologyMapping mapping in existing)
        {
            if (mapping.Status == TerminologyMappingStatus.Superseded)
            {
                continue;
            }

            bool stillPresent = desired.Contains((mapping.CodeSystemId, mapping.ExternalCode));
            if (!stillPresent)
            {
                mapping.SetStatus(TerminologyMappingStatus.Superseded);
            }
        }

        foreach (TestTerminologyMapping mapping in toAdd)
        {
            await testCatalogRepository.AddTerminologyMappingAsync(mapping, cancellationToken);
        }

        await auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                null,
                AuditActions.LaboratoryUpdated,
                AuditResourceTypes.TestTerminologyMapping,
                test.Code,
                AuditOutcome.Success,
                null),
            cancellationToken);

        return await BuildDetailAsync(organizationId, test, cancellationToken);
    }

    public async Task<TestDetailDto> ReplaceFacilitiesAsync(
        AuthContext context,
        Guid organizationId,
        string code,
        ReplaceTestFacilitiesRequest request,
        CancellationToken cancellationToken)
    {
        LaboratoryPermissionGuard.RequireOrganizationWide(
            context, Permissions.LaboratoryCatalogWrite, organizationId);

        if (request?.FacilityIds is null)
        {
            throw new ValidationException("A request body with facilityIds is required.");
        }

        Test test = await RequireTestAsync(organizationId, code, cancellationToken);
        RequireEditable(test);

        Guid[] facilities = await ResolveFacilitiesAsync(
            organizationId, request.FacilityIds, cancellationToken);

        IReadOnlyCollection<TestFacility> existing = await testCatalogRepository
            .ListTestFacilitiesAsync([test.Id], cancellationToken);
        testCatalogRepository.RemoveTestFacilities(existing);
        foreach (Guid facilityId in facilities)
        {
            await testCatalogRepository.AddTestFacilityAsync(
                new TestFacility(test.Id, facilityId), cancellationToken);
        }

        await auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                null,
                AuditActions.LaboratoryUpdated,
                AuditResourceTypes.TestFacility,
                test.Code,
                AuditOutcome.Success,
                null),
            cancellationToken);

        return await BuildDetailAsync(organizationId, test, cancellationToken);
    }

    private async Task<TestDetailDto> BuildDetailAsync(
        Guid organizationId,
        Test test,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TestVersion> versions = await testCatalogRepository
            .ListTestVersionsAsync([test.Id], cancellationToken);
        IReadOnlyCollection<TestSpecimenRequirement> requirements = await testCatalogRepository
            .ListSpecimenRequirementsAsync([test.Id], cancellationToken);
        IReadOnlyCollection<TestTerminologyMapping> mappings = await testCatalogRepository
            .ListTerminologyMappingsAsync([test.Id], cancellationToken);
        IReadOnlyCollection<ReferenceRange> ranges = await testCatalogRepository
            .ListReferenceRangesAsync(organizationId, null, [test.Id], cancellationToken);
        IReadOnlyCollection<TestFacility> facilities = await testCatalogRepository
            .ListTestFacilitiesAsync([test.Id], cancellationToken);

        return new TestDetailDto(
            test.Id,
            test.OrganizationId,
            test.Code,
            test.Status,
            test.CurrentVersionNumber,
            versions.OrderBy(v => v.VersionNumber).Select(v => v.ToDto()).ToArray(),
            requirements.Select(r => r.ToDto()).ToArray(),
            mappings.OrderBy(m => m.ExternalCode, StringComparer.Ordinal).Select(m => m.ToDto()).ToArray(),
            ranges.OrderBy(r => r.EffectiveFromUtc).Select(r => r.ToDto()).ToArray(),
            facilities.Select(f => f.FacilityId).OrderBy(id => id).ToArray());
    }

    private async Task<Test> RequireTestAsync(
        Guid organizationId,
        string code,
        CancellationToken cancellationToken) =>
        await testCatalogRepository.FindTestByCodeAsync(organizationId, code, cancellationToken)
        ?? throw new NotFoundException($"Test '{code}' was not found.");

    private static void RequireEditable(Test test)
    {
        if (test.Status == MasterDataStatus.Retired)
        {
            throw new ValidationException("A retired test cannot be modified.");
        }
    }

    private static string LatestVersionName(
        IReadOnlyCollection<TestVersion> versions,
        Guid testId)
    {
        TestVersion? latest = versions
            .Where(v => v.TestId == testId)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefault();
        return latest?.Name ?? string.Empty;
    }

    private async Task RequireResultDataTypeAsync(
        Guid organizationId,
        Guid resultDataTypeId,
        CancellationToken cancellationToken)
    {
        ResultDataType? dataType = await catalogRepository
            .FindResultDataTypeAsync(organizationId, resultDataTypeId, cancellationToken);
        if (dataType is null)
        {
            throw new ValidationException($"Result data type '{resultDataTypeId}' was not found.");
        }

        if (dataType.Status == MasterDataStatus.Retired)
        {
            throw new ValidationException($"Result data type '{dataType.Code}' is retired.");
        }
    }

    private async Task RequireDisciplineIfProvidedAsync(
        Guid organizationId,
        Guid? disciplineId,
        CancellationToken cancellationToken)
    {
        if (disciplineId is not { } id)
        {
            return;
        }

        Discipline? discipline = await catalogRepository
            .FindDisciplineAsync(organizationId, id, cancellationToken);
        if (discipline is null)
        {
            throw new ValidationException($"Discipline '{id}' was not found.");
        }

        if (discipline.Status == MasterDataStatus.Retired)
        {
            throw new ValidationException($"Discipline '{discipline.Code}' is retired.");
        }
    }

    private async Task RequireUnitIfProvidedAsync(
        Guid organizationId,
        Guid? unitId,
        CancellationToken cancellationToken)
    {
        if (unitId is not { } id)
        {
            return;
        }

        Unit? unit = await catalogRepository.FindUnitAsync(organizationId, id, cancellationToken);
        if (unit is null)
        {
            throw new ValidationException($"Unit '{id}' was not found.");
        }

        if (unit.Status == MasterDataStatus.Retired)
        {
            throw new ValidationException($"Unit '{unit.Code}' is retired.");
        }
    }

    private async Task RequireSpecimenTypeAsync(
        Guid organizationId,
        Guid specimenTypeId,
        CancellationToken cancellationToken)
    {
        SpecimenType? specimenType = await catalogRepository
            .FindSpecimenTypeAsync(organizationId, specimenTypeId, cancellationToken);
        if (specimenType is null)
        {
            throw new ValidationException($"Specimen type '{specimenTypeId}' was not found.");
        }

        if (specimenType.Status == MasterDataStatus.Retired)
        {
            throw new ValidationException($"Specimen type '{specimenType.Code}' is retired.");
        }
    }

    private async Task RequireSpecimenSourceIfProvidedAsync(
        Guid organizationId,
        Guid? specimenSourceId,
        CancellationToken cancellationToken)
    {
        if (specimenSourceId is not { } id)
        {
            return;
        }

        SpecimenSource? source = await catalogRepository
            .FindSpecimenSourceAsync(organizationId, id, cancellationToken);
        if (source is null)
        {
            throw new ValidationException($"Specimen source '{id}' was not found.");
        }
    }

    private async Task RequireContainerTypeIfProvidedAsync(
        Guid organizationId,
        Guid? containerTypeId,
        CancellationToken cancellationToken)
    {
        if (containerTypeId is not { } id)
        {
            return;
        }

        ContainerType? container = await catalogRepository
            .FindContainerTypeAsync(organizationId, id, cancellationToken);
        if (container is null)
        {
            throw new ValidationException($"Container type '{id}' was not found.");
        }
    }

    private async Task RequireCodeSystemAsync(
        Guid organizationId,
        Guid codeSystemId,
        CancellationToken cancellationToken)
    {
        CodeSystem? codeSystem = await catalogRepository
            .FindCodeSystemAsync(organizationId, codeSystemId, cancellationToken);
        if (codeSystem is null)
        {
            throw new ValidationException($"Code system '{codeSystemId}' was not found.");
        }

        if (codeSystem.Status == MasterDataStatus.Retired)
        {
            throw new ValidationException($"Code system '{codeSystem.Code}' is retired.");
        }
    }

    private async Task<Guid[]> ResolveFacilitiesAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid>? facilityIds,
        CancellationToken cancellationToken)
    {
        if (facilityIds is null || facilityIds.Count == 0)
        {
            return [];
        }

        Guid[] requested = facilityIds.Distinct().ToArray();
        IReadOnlyCollection<Facility> facilities = await facilityRepository
            .ListByOrganizationIdsAsync([organizationId], cancellationToken);
        HashSet<Guid> known = facilities.Select(f => f.Id).ToHashSet();

        foreach (Guid facilityId in requested)
        {
            if (!known.Contains(facilityId))
            {
                throw new ValidationException(
                    $"Facility '{facilityId}' does not belong to organization '{organizationId}'.");
            }
        }

        return requested;
    }
}

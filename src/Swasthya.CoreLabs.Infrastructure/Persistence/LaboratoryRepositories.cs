using Microsoft.EntityFrameworkCore;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Infrastructure.Persistence;

public sealed class LaboratoryCatalogRepository(CoreLabDbContext dbContext)
    : ILaboratoryCatalogRepository
{
    public async Task<IReadOnlyCollection<Discipline>> ListDisciplinesAsync(
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.Disciplines.AsNoTracking()
            .Where(d => d.OrganizationId == organizationId)
            .OrderBy(d => d.Code)
            .ToListAsync(cancellationToken);

    public Task<Discipline?> FindDisciplineAsync(
        Guid organizationId,
        Guid disciplineId,
        CancellationToken cancellationToken) =>
        dbContext.Disciplines.AsNoTracking()
            .FirstOrDefaultAsync(
                d => d.OrganizationId == organizationId && d.Id == disciplineId,
                cancellationToken);

    public async Task<IReadOnlyCollection<LaboratorySection>> ListSectionsAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid>? facilityIds,
        CancellationToken cancellationToken)
    {
        IQueryable<LaboratorySection> query = dbContext.LaboratorySections.AsNoTracking()
            .Where(s => s.OrganizationId == organizationId);

        if (facilityIds is not null)
        {
            query = query.Where(s => facilityIds.Contains(s.FacilityId));
        }

        return await query.OrderBy(s => s.Code).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ResultDataType>> ListResultDataTypesAsync(
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.ResultDataTypes.AsNoTracking()
            .Where(r => r.OrganizationId == organizationId)
            .OrderBy(r => r.Code)
            .ToListAsync(cancellationToken);

    public Task<ResultDataType?> FindResultDataTypeAsync(
        Guid organizationId,
        Guid resultDataTypeId,
        CancellationToken cancellationToken) =>
        dbContext.ResultDataTypes.AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.OrganizationId == organizationId && r.Id == resultDataTypeId,
                cancellationToken);

    public async Task<IReadOnlyCollection<Unit>> ListUnitsAsync(
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.Units.AsNoTracking()
            .Where(u => u.OrganizationId == organizationId)
            .OrderBy(u => u.Code)
            .ToListAsync(cancellationToken);

    public Task<Unit?> FindUnitAsync(
        Guid organizationId,
        Guid unitId,
        CancellationToken cancellationToken) =>
        dbContext.Units.AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.OrganizationId == organizationId && u.Id == unitId,
                cancellationToken);

    public async Task<IReadOnlyCollection<CodeSystem>> ListCodeSystemsAsync(
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.CodeSystems.AsNoTracking()
            .Where(c => c.OrganizationId == organizationId)
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);

    public Task<CodeSystem?> FindCodeSystemAsync(
        Guid organizationId,
        Guid codeSystemId,
        CancellationToken cancellationToken) =>
        dbContext.CodeSystems.AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.OrganizationId == organizationId && c.Id == codeSystemId,
                cancellationToken);

    public async Task<IReadOnlyCollection<SpecimenType>> ListSpecimenTypesAsync(
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.SpecimenTypes.AsNoTracking()
            .Where(s => s.OrganizationId == organizationId)
            .OrderBy(s => s.Code)
            .ToListAsync(cancellationToken);

    public Task<SpecimenType?> FindSpecimenTypeAsync(
        Guid organizationId,
        Guid specimenTypeId,
        CancellationToken cancellationToken) =>
        dbContext.SpecimenTypes.AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.OrganizationId == organizationId && s.Id == specimenTypeId,
                cancellationToken);

    public async Task<IReadOnlyCollection<SpecimenSource>> ListSpecimenSourcesAsync(
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.SpecimenSources.AsNoTracking()
            .Where(s => s.OrganizationId == organizationId)
            .OrderBy(s => s.Code)
            .ToListAsync(cancellationToken);

    public Task<SpecimenSource?> FindSpecimenSourceAsync(
        Guid organizationId,
        Guid specimenSourceId,
        CancellationToken cancellationToken) =>
        dbContext.SpecimenSources.AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.OrganizationId == organizationId && s.Id == specimenSourceId,
                cancellationToken);

    public async Task<IReadOnlyCollection<ContainerType>> ListContainerTypesAsync(
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.ContainerTypes.AsNoTracking()
            .Where(c => c.OrganizationId == organizationId)
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);

    public Task<ContainerType?> FindContainerTypeAsync(
        Guid organizationId,
        Guid containerTypeId,
        CancellationToken cancellationToken) =>
        dbContext.ContainerTypes.AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.OrganizationId == organizationId && c.Id == containerTypeId,
                cancellationToken);

    public async Task<IReadOnlyCollection<ResultStatus>> ListResultStatusesAsync(
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.ResultStatuses.AsNoTracking()
            .Where(r => r.OrganizationId == organizationId)
            .OrderBy(r => r.Code)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Analyzer>> ListAnalyzersAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid>? facilityIds,
        CancellationToken cancellationToken)
    {
        IQueryable<Analyzer> query = dbContext.Analyzers.AsNoTracking()
            .Where(a => a.OrganizationId == organizationId);

        if (facilityIds is not null)
        {
            query = query.Where(a => facilityIds.Contains(a.FacilityId));
        }

        return await query.OrderBy(a => a.Code).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ConfigurationItem>> ListConfigurationItemsAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid>? facilityIds,
        CancellationToken cancellationToken)
    {
        IQueryable<ConfigurationItem> query = dbContext.ConfigurationItems.AsNoTracking()
            .Where(c => c.OrganizationId == organizationId);

        if (facilityIds is not null)
        {
            query = query.Where(c =>
                c.FacilityId == null || facilityIds.Contains(c.FacilityId.Value));
        }

        return await query
            .OrderBy(c => c.Key)
            .ThenBy(c => c.FacilityId)
            .ToListAsync(cancellationToken);
    }

    public Task<ConfigurationItem?> FindConfigurationItemAsync(
        Guid organizationId,
        Guid? facilityId,
        string key,
        CancellationToken cancellationToken) =>
        dbContext.ConfigurationItems
            .FirstOrDefaultAsync(
                c => c.OrganizationId == organizationId
                    && c.FacilityId == facilityId
                    && c.Key == key,
                cancellationToken);

    public Task AddConfigurationItemAsync(
        ConfigurationItem item,
        CancellationToken cancellationToken)
    {
        dbContext.ConfigurationItems.Add(item);
        return Task.CompletedTask;
    }
}

public sealed class TestCatalogRepository(CoreLabDbContext dbContext) : ITestCatalogRepository
{
    public async Task<IReadOnlyCollection<Test>> ListTestsAsync(
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.Tests.AsNoTracking()
            .Where(t => t.OrganizationId == organizationId)
            .OrderBy(t => t.Code)
            .ToListAsync(cancellationToken);

    public Task<Test?> FindTestAsync(
        Guid organizationId,
        Guid testId,
        CancellationToken cancellationToken) =>
        dbContext.Tests
            .FirstOrDefaultAsync(
                t => t.OrganizationId == organizationId && t.Id == testId,
                cancellationToken);

    public Task<Test?> FindTestByCodeAsync(
        Guid organizationId,
        string code,
        CancellationToken cancellationToken) =>
        dbContext.Tests
            .FirstOrDefaultAsync(
                t => t.OrganizationId == organizationId && t.Code == code,
                cancellationToken);

    public Task AddTestAsync(Test test, CancellationToken cancellationToken)
    {
        dbContext.Tests.Add(test);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyCollection<TestVersion>> ListTestVersionsAsync(
        IReadOnlyCollection<Guid> testIds,
        CancellationToken cancellationToken)
    {
        if (testIds.Count == 0)
        {
            return Array.Empty<TestVersion>();
        }

        return await dbContext.TestVersions.AsNoTracking()
            .Where(v => testIds.Contains(v.TestId))
            .OrderBy(v => v.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public Task AddTestVersionAsync(TestVersion version, CancellationToken cancellationToken)
    {
        dbContext.TestVersions.Add(version);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyCollection<TestSpecimenRequirement>> ListSpecimenRequirementsAsync(
        IReadOnlyCollection<Guid> testIds,
        CancellationToken cancellationToken)
    {
        if (testIds.Count == 0)
        {
            return Array.Empty<TestSpecimenRequirement>();
        }

        return await dbContext.TestSpecimenRequirements
            .Where(r => testIds.Contains(r.TestId))
            .ToListAsync(cancellationToken);
    }

    public Task AddSpecimenRequirementAsync(
        TestSpecimenRequirement requirement,
        CancellationToken cancellationToken)
    {
        dbContext.TestSpecimenRequirements.Add(requirement);
        return Task.CompletedTask;
    }

    public void RemoveSpecimenRequirements(
        IReadOnlyCollection<TestSpecimenRequirement> requirements)
    {
        if (requirements.Count > 0)
        {
            dbContext.TestSpecimenRequirements.RemoveRange(requirements);
        }
    }

    public async Task<IReadOnlyCollection<TestTerminologyMapping>> ListTerminologyMappingsAsync(
        IReadOnlyCollection<Guid> testIds,
        CancellationToken cancellationToken)
    {
        if (testIds.Count == 0)
        {
            return Array.Empty<TestTerminologyMapping>();
        }

        return await dbContext.TestTerminologyMappings
            .Where(m => testIds.Contains(m.TestId))
            .ToListAsync(cancellationToken);
    }

    public Task AddTerminologyMappingAsync(
        TestTerminologyMapping mapping,
        CancellationToken cancellationToken)
    {
        dbContext.TestTerminologyMappings.Add(mapping);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyCollection<TestFacility>> ListTestFacilitiesAsync(
        IReadOnlyCollection<Guid> testIds,
        CancellationToken cancellationToken)
    {
        if (testIds.Count == 0)
        {
            return Array.Empty<TestFacility>();
        }

        return await dbContext.TestFacilities.AsNoTracking()
            .Where(f => testIds.Contains(f.TestId))
            .ToListAsync(cancellationToken);
    }

    public Task AddTestFacilityAsync(TestFacility facility, CancellationToken cancellationToken)
    {
        dbContext.TestFacilities.Add(facility);
        return Task.CompletedTask;
    }

    public void RemoveTestFacilities(IReadOnlyCollection<TestFacility> facilities)
    {
        if (facilities.Count > 0)
        {
            dbContext.TestFacilities.RemoveRange(facilities);
        }
    }

    public async Task<IReadOnlyCollection<ReferenceRange>> ListReferenceRangesAsync(
        Guid organizationId,
        IReadOnlyCollection<Guid>? facilityIds,
        IReadOnlyCollection<Guid>? testIds,
        CancellationToken cancellationToken)
    {
        IQueryable<ReferenceRange> query = dbContext.ReferenceRanges.AsNoTracking()
            .Where(r => r.OrganizationId == organizationId);

        if (facilityIds is not null)
        {
            query = query.Where(r =>
                r.FacilityId == null || facilityIds.Contains(r.FacilityId.Value));
        }

        if (testIds is not null)
        {
            if (testIds.Count == 0)
            {
                return Array.Empty<ReferenceRange>();
            }

            query = query.Where(r => testIds.Contains(r.TestId));
        }

        return await query
            .OrderBy(r => r.TestId)
            .ThenBy(r => r.EffectiveFromUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<ReferenceRange?> FindReferenceRangeAsync(
        Guid organizationId,
        Guid referenceRangeId,
        CancellationToken cancellationToken) =>
        dbContext.ReferenceRanges
            .FirstOrDefaultAsync(
                r => r.OrganizationId == organizationId && r.Id == referenceRangeId,
                cancellationToken);

    public Task AddReferenceRangeAsync(ReferenceRange range, CancellationToken cancellationToken)
    {
        dbContext.ReferenceRanges.Add(range);
        return Task.CompletedTask;
    }
}

public sealed class PanelRepository(CoreLabDbContext dbContext) : IPanelRepository
{
    public async Task<IReadOnlyCollection<Panel>> ListPanelsAsync(
        Guid organizationId,
        CancellationToken cancellationToken) =>
        await dbContext.Panels.AsNoTracking()
            .Where(p => p.OrganizationId == organizationId)
            .OrderBy(p => p.Code)
            .ToListAsync(cancellationToken);

    public Task<Panel?> FindPanelAsync(
        Guid organizationId,
        Guid panelId,
        CancellationToken cancellationToken) =>
        dbContext.Panels
            .FirstOrDefaultAsync(
                p => p.OrganizationId == organizationId && p.Id == panelId,
                cancellationToken);

    public Task<Panel?> FindPanelByCodeAsync(
        Guid organizationId,
        string code,
        CancellationToken cancellationToken) =>
        dbContext.Panels
            .FirstOrDefaultAsync(
                p => p.OrganizationId == organizationId && p.Code == code,
                cancellationToken);

    public Task AddPanelAsync(Panel panel, CancellationToken cancellationToken)
    {
        dbContext.Panels.Add(panel);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyCollection<PanelVersion>> ListPanelVersionsAsync(
        IReadOnlyCollection<Guid> panelIds,
        CancellationToken cancellationToken)
    {
        if (panelIds.Count == 0)
        {
            return Array.Empty<PanelVersion>();
        }

        return await dbContext.PanelVersions.AsNoTracking()
            .Where(v => panelIds.Contains(v.PanelId))
            .OrderBy(v => v.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public Task AddPanelVersionAsync(PanelVersion version, CancellationToken cancellationToken)
    {
        dbContext.PanelVersions.Add(version);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyCollection<PanelTestMembership>> ListPanelMembershipsAsync(
        IReadOnlyCollection<Guid> panelIds,
        CancellationToken cancellationToken)
    {
        if (panelIds.Count == 0)
        {
            return Array.Empty<PanelTestMembership>();
        }

        return await dbContext.PanelTestMemberships
            .Where(m => panelIds.Contains(m.PanelId))
            .ToListAsync(cancellationToken);
    }

    public Task AddPanelMembershipAsync(
        PanelTestMembership membership,
        CancellationToken cancellationToken)
    {
        dbContext.PanelTestMemberships.Add(membership);
        return Task.CompletedTask;
    }

    public void RemovePanelMemberships(
        IReadOnlyCollection<PanelTestMembership> memberships)
    {
        if (memberships.Count > 0)
        {
            dbContext.PanelTestMemberships.RemoveRange(memberships);
        }
    }
}

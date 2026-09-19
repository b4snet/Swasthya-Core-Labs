using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class ReferenceRangeService(
    ITestCatalogRepository testCatalogRepository,
    ILaboratoryCatalogRepository catalogRepository,
    IFacilityRepository facilityRepository,
    IAuditRepository auditRepository,
    ICorrelationIdProvider correlationIdProvider)
{
    public async Task<IReadOnlyCollection<ReferenceRangeDto>> ListAsync(
        AuthContext context,
        Guid organizationId,
        Guid? facilityId,
        string? testCode,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);

        IReadOnlyCollection<Guid>? facilityFilter = LaboratoryScope.ResolveFacilityFilter(
            context, Permissions.LaboratoryCatalogRead, organizationId, facilityId);

        IReadOnlyCollection<Guid>? testFilter = null;
        if (!string.IsNullOrWhiteSpace(testCode))
        {
            Test test = await testCatalogRepository.FindTestByCodeAsync(
                    organizationId, testCode, cancellationToken)
                ?? throw new NotFoundException($"Test '{testCode}' was not found.");
            testFilter = [test.Id];
        }

        IReadOnlyCollection<ReferenceRange> ranges = await testCatalogRepository
            .ListReferenceRangesAsync(organizationId, facilityFilter, testFilter, cancellationToken);

        await LaboratoryAudit.WriteForResourceAsync(
            auditRepository,
            correlationIdProvider,
            context,
            organizationId,
            AuditResourceTypes.ReferenceRange,
            cancellationToken);

        return ranges
            .OrderBy(r => r.EffectiveFromUtc)
            .Select(r => r.ToDto())
            .ToArray();
    }

    public async Task<ReferenceRangeDto> CreateAsync(
        AuthContext context,
        Guid organizationId,
        string testCode,
        CreateReferenceRangeRequest request,
        CancellationToken cancellationToken)
    {
        LaboratoryPermissionGuard.RequireOrganizationWide(
            context, Permissions.LaboratoryCatalogWrite, organizationId);

        if (request is null)
        {
            throw new ValidationException("A request body is required.");
        }

        Test test = await testCatalogRepository.FindTestByCodeAsync(
                organizationId, testCode, cancellationToken)
            ?? throw new NotFoundException($"Test '{testCode}' was not found.");

        if (test.Status == MasterDataStatus.Retired)
        {
            throw new ValidationException("A reference range cannot be added to a retired test.");
        }

        if (request.UnitId is { } unitId)
        {
            Unit? unit = await catalogRepository.FindUnitAsync(organizationId, unitId, cancellationToken);
            if (unit is null)
            {
                throw new ValidationException($"Unit '{unitId}' was not found.");
            }
        }

        if (request.FacilityId is { } facilityId)
        {
            IReadOnlyCollection<Facility> facilities = await facilityRepository
                .ListByOrganizationIdsAsync([organizationId], cancellationToken);
            if (facilities.All(f => f.Id != facilityId))
            {
                throw new ValidationException(
                    $"Facility '{facilityId}' does not belong to organization '{organizationId}'.");
            }
        }

        ReferenceRange range;
        try
        {
            range = new ReferenceRange(
                organizationId,
                request.FacilityId,
                test.Id,
                request.Source ?? string.Empty,
                request.Population,
                request.Sex,
                request.AgeMinimum,
                request.AgeMaximum,
                request.AgeUnitUcumCode,
                request.SpecimenContext,
                request.LowValue,
                request.HighValue,
                request.TextualReference,
                request.UnitId,
                request.EffectiveFromUtc,
                request.EffectiveToUtc);
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }

        await testCatalogRepository.AddReferenceRangeAsync(range, cancellationToken);
        await auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                range.FacilityId,
                AuditActions.LaboratoryCreated,
                AuditResourceTypes.ReferenceRange,
                test.Code,
                AuditOutcome.Success,
                null),
            cancellationToken);

        return range.ToDto();
    }

    public async Task<ReferenceRangeDto> TransitionAsync(
        AuthContext context,
        Guid organizationId,
        Guid referenceRangeId,
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

        ReferenceRange range = await testCatalogRepository
            .FindReferenceRangeAsync(organizationId, referenceRangeId, cancellationToken)
            ?? throw new NotFoundException($"Reference range '{referenceRangeId}' was not found.");

        if (range.Status == target)
        {
            throw new ValidationException($"Reference range is already '{target}'.");
        }

        if (!MasterDataLifecycle.CanTransition(range.Status, target))
        {
            throw new ValidationException(
                $"Reference range cannot transition from '{range.Status}' to '{target}'.");
        }

        range.SetStatus(target);

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
                range.FacilityId,
                action,
                AuditResourceTypes.ReferenceRange,
                range.Id.ToString(),
                AuditOutcome.Success,
                null),
            cancellationToken);

        return range.ToDto();
    }
}

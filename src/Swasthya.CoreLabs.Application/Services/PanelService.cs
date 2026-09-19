using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class PanelService(
    IPanelRepository panelRepository,
    ITestCatalogRepository testCatalogRepository,
    IAuditRepository auditRepository,
    ICorrelationIdProvider correlationIdProvider)
{
    public async Task<IReadOnlyCollection<PanelSummaryDto>> ListPanelsAsync(
        AuthContext context,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);

        IReadOnlyCollection<Panel> panels = await panelRepository
            .ListPanelsAsync(organizationId, cancellationToken);
        IReadOnlyCollection<PanelVersion> versions = await panelRepository
            .ListPanelVersionsAsync(panels.Select(p => p.Id).ToArray(), cancellationToken);

        await LaboratoryAudit.WriteForResourceAsync(
            auditRepository,
            correlationIdProvider,
            context,
            organizationId,
            AuditResourceTypes.Panel,
            cancellationToken);

        return panels
            .Select(p => new PanelSummaryDto(
                p.Id,
                p.OrganizationId,
                p.Code,
                p.Status,
                p.CurrentVersionNumber,
                versions
                    .Where(v => v.PanelId == p.Id)
                    .OrderByDescending(v => v.VersionNumber)
                    .Select(v => v.Name)
                    .FirstOrDefault() ?? string.Empty))
            .OrderBy(p => p.Code, StringComparer.Ordinal)
            .ToArray();
    }

    public async Task<PanelDetailDto> GetPanelAsync(
        AuthContext context,
        Guid organizationId,
        string code,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.LaboratoryCatalogRead, organizationId);

        Panel panel = await RequirePanelAsync(organizationId, code, cancellationToken);
        return await BuildDetailAsync(organizationId, panel, cancellationToken);
    }

    public async Task<PanelDetailDto> CreatePanelAsync(
        AuthContext context,
        Guid organizationId,
        CreatePanelRequest request,
        CancellationToken cancellationToken)
    {
        LaboratoryPermissionGuard.RequireOrganizationWide(
            context, Permissions.LaboratoryCatalogWrite, organizationId);

        if (request is null)
        {
            throw new ValidationException("A request body is required.");
        }

        if (await panelRepository.FindPanelByCodeAsync(
                organizationId, request.Code ?? string.Empty, cancellationToken) is not null)
        {
            throw new ConflictException($"A panel with code '{request.Code}' already exists.");
        }

        Panel panel = new(organizationId, request.Code ?? string.Empty);
        PanelVersion version;
        try
        {
            int versionNumber = panel.StartNextVersion();
            version = new PanelVersion(
                panel.Id,
                versionNumber,
                request.Name ?? string.Empty,
                request.Description,
                request.EffectiveFromUtc,
                request.EffectiveToUtc);
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }

        IReadOnlyCollection<PanelTestMembership> memberships = await BuildMembershipsAsync(
            organizationId, panel.Id, request.Tests, cancellationToken);

        await panelRepository.AddPanelAsync(panel, cancellationToken);
        await panelRepository.AddPanelVersionAsync(version, cancellationToken);
        foreach (PanelTestMembership membership in memberships)
        {
            await panelRepository.AddPanelMembershipAsync(membership, cancellationToken);
        }

        await auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                null,
                AuditActions.LaboratoryCreated,
                AuditResourceTypes.Panel,
                panel.Code,
                AuditOutcome.Success,
                null),
            cancellationToken);

        return await BuildDetailAsync(organizationId, panel, cancellationToken);
    }

    public async Task<PanelDetailDto> PublishVersionAsync(
        AuthContext context,
        Guid organizationId,
        string code,
        CreatePanelVersionRequest request,
        CancellationToken cancellationToken)
    {
        LaboratoryPermissionGuard.RequireOrganizationWide(
            context, Permissions.LaboratoryCatalogWrite, organizationId);

        if (request is null)
        {
            throw new ValidationException("A request body is required.");
        }

        Panel panel = await RequirePanelAsync(organizationId, code, cancellationToken);
        if (panel.Status == MasterDataStatus.Retired)
        {
            throw new ValidationException("A retired panel cannot receive new versions.");
        }

        PanelVersion version;
        try
        {
            int versionNumber = panel.StartNextVersion();
            version = new PanelVersion(
                panel.Id,
                versionNumber,
                request.Name ?? string.Empty,
                request.Description,
                request.EffectiveFromUtc,
                request.EffectiveToUtc);
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }

        await panelRepository.AddPanelVersionAsync(version, cancellationToken);
        await auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                null,
                AuditActions.LaboratoryVersioned,
                AuditResourceTypes.PanelVersion,
                $"{panel.Code}:v{version.VersionNumber}",
                AuditOutcome.Success,
                null),
            cancellationToken);

        return await BuildDetailAsync(organizationId, panel, cancellationToken);
    }

    public async Task<PanelDetailDto> TransitionAsync(
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

        Panel panel = await RequirePanelAsync(organizationId, code, cancellationToken);
        if (panel.Status == target)
        {
            throw new ValidationException($"Panel is already '{target}'.");
        }

        if (!MasterDataLifecycle.CanTransition(panel.Status, target))
        {
            throw new ValidationException(
                $"Panel cannot transition from '{panel.Status}' to '{target}'.");
        }

        panel.SetStatus(target);

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
                AuditResourceTypes.Panel,
                panel.Code,
                AuditOutcome.Success,
                null),
            cancellationToken);

        return await BuildDetailAsync(organizationId, panel, cancellationToken);
    }

    public async Task<PanelDetailDto> ReplaceMembershipsAsync(
        AuthContext context,
        Guid organizationId,
        string code,
        ReplacePanelMembershipsRequest request,
        CancellationToken cancellationToken)
    {
        LaboratoryPermissionGuard.RequireOrganizationWide(
            context, Permissions.LaboratoryCatalogWrite, organizationId);

        if (request?.Items is null)
        {
            throw new ValidationException("A request body with items is required.");
        }

        Panel panel = await RequirePanelAsync(organizationId, code, cancellationToken);
        if (panel.Status == MasterDataStatus.Retired)
        {
            throw new ValidationException("A retired panel cannot be modified.");
        }

        IReadOnlyCollection<PanelTestMembership> memberships = await BuildMembershipsAsync(
            organizationId, panel.Id, request.Items, cancellationToken);

        IReadOnlyCollection<PanelTestMembership> existing = await panelRepository
            .ListPanelMembershipsAsync([panel.Id], cancellationToken);
        panelRepository.RemovePanelMemberships(existing);
        foreach (PanelTestMembership membership in memberships)
        {
            await panelRepository.AddPanelMembershipAsync(membership, cancellationToken);
        }

        await auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                null,
                AuditActions.LaboratoryUpdated,
                AuditResourceTypes.PanelMembership,
                panel.Code,
                AuditOutcome.Success,
                null),
            cancellationToken);

        return await BuildDetailAsync(organizationId, panel, cancellationToken);
    }

    private async Task<PanelDetailDto> BuildDetailAsync(
        Guid organizationId,
        Panel panel,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<PanelVersion> versions = await panelRepository
            .ListPanelVersionsAsync([panel.Id], cancellationToken);
        IReadOnlyCollection<PanelTestMembership> memberships = await panelRepository
            .ListPanelMembershipsAsync([panel.Id], cancellationToken);

        IReadOnlyCollection<Test> tests = await testCatalogRepository
            .ListTestsAsync(organizationId, cancellationToken);
        Dictionary<Guid, string> testCodes = tests.ToDictionary(t => t.Id, t => t.Code);

        return new PanelDetailDto(
            panel.Id,
            panel.OrganizationId,
            panel.Code,
            panel.Status,
            panel.CurrentVersionNumber,
            versions.OrderBy(v => v.VersionNumber).Select(v => v.ToDto()).ToArray(),
            memberships
                .OrderBy(m => m.SequenceNumber)
                .ThenBy(m => m.CreatedAtUtc)
                .Select(m => m.ToDto(testCodes.TryGetValue(m.TestId, out string? testCode)
                    ? testCode
                    : string.Empty))
                .ToArray());
    }

    private async Task<IReadOnlyCollection<PanelTestMembership>> BuildMembershipsAsync(
        Guid organizationId,
        Guid panelId,
        IReadOnlyCollection<PanelMembershipInput>? inputs,
        CancellationToken cancellationToken)
    {
        if (inputs is null || inputs.Count == 0)
        {
            return [];
        }

        IReadOnlyCollection<Test> tests = await testCatalogRepository
            .ListTestsAsync(organizationId, cancellationToken);
        Dictionary<string, Test> testsByCode = tests.ToDictionary(t => t.Code, StringComparer.Ordinal);

        HashSet<Guid> seen = [];
        List<PanelTestMembership> memberships = [];
        foreach (PanelMembershipInput input in inputs)
        {
            string testCode = (input.TestCode ?? string.Empty).Trim();
            if (!testsByCode.TryGetValue(testCode, out Test? test))
            {
                throw new ValidationException($"Test '{testCode}' was not found.");
            }

            if (test.Status == MasterDataStatus.Retired)
            {
                throw new ValidationException($"Test '{testCode}' is retired.");
            }

            if (!seen.Add(test.Id))
            {
                throw new ValidationException($"Test '{testCode}' is listed more than once.");
            }

            try
            {
                memberships.Add(new PanelTestMembership(
                    panelId, test.Id, input.SequenceNumber, input.DisplayText));
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ValidationException(exception.Message);
            }
        }

        return memberships;
    }

    private async Task<Panel> RequirePanelAsync(
        Guid organizationId,
        string code,
        CancellationToken cancellationToken) =>
        await panelRepository.FindPanelByCodeAsync(organizationId, code, cancellationToken)
        ?? throw new NotFoundException($"Panel '{code}' was not found.");
}

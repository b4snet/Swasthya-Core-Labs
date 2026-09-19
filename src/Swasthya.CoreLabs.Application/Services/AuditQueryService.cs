using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class AuditQueryService(
    IAuditRepository auditRepository,
    ICorrelationIdProvider correlationIdProvider)
{
    public async Task<IReadOnlyCollection<AuditRecordDto>> ListScopedAsync(
        AuthContext context,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.AuditRead);
        if (skip < 0)
        {
            skip = 0;
        }

        take = Math.Clamp(take, 1, 500);

        Guid[] orgWideGrants = context.Grants
            .Where(g => g.PermissionCode == Permissions.AuditRead && g.FacilityId is null)
            .Select(g => g.OrganizationId)
            .Distinct()
            .ToArray();

        Guid[] facilityRestrictedGrants = context.Grants
            .Where(g => g.PermissionCode == Permissions.AuditRead && g.FacilityId is not null)
            .Select(g => g.FacilityId!.Value)
            .Distinct()
            .ToArray();

        if (orgWideGrants.Length == 0 && facilityRestrictedGrants.Length == 0)
        {
            throw new PermissionDeniedException(
                $"Principal has no grant for permission '{Permissions.AuditRead}'.");
        }

        IReadOnlyCollection<AuditRecordDto> records = (await auditRepository.ListAsync(
                orgWideGrants,
                facilityRestrictedGrants,
                skip,
                take,
                cancellationToken))
            .Select(r => new AuditRecordDto(
                r.Id,
                r.OrganizationId,
                r.FacilityId,
                r.ActorId,
                r.ActorType,
                r.Action,
                r.ResourceType,
                r.ResourceId,
                r.OccurredAtUtc,
                r.CorrelationId,
                r.Outcome,
                r.ContextMetadata))
            .ToArray();

        foreach (Guid organizationId in orgWideGrants.Concat(
                     context.Grants
                         .Where(g => g.PermissionCode == Permissions.AuditRead && g.FacilityId is not null)
                         .Select(g => g.OrganizationId))
                 .Distinct())
        {
            await auditRepository.AddAsync(
                AuditRecordFactory.Create(
                    context,
                    correlationIdProvider.Current,
                    organizationId,
                    null,
                    AuditActions.AuditRead,
                    AuditResourceTypes.AuditRecord,
                    null,
                    AuditOutcome.Success,
                    new Dictionary<string, string>
                    {
                        ["request_method"] = "GET",
                        ["request_path"] = "/v1/audit",
                    }),
                cancellationToken);
        }

        return records;
    }
}

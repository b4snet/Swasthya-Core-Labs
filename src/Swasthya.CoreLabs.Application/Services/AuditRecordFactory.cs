using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Audit;
using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Application.Services;

public static class AuditRecordFactory
{
    public static AuditRecord Create(
        AuthContext context,
        string correlationId,
        Guid organizationId,
        Guid? facilityId,
        string action,
        string resourceType,
        string? resourceId,
        AuditOutcome outcome,
        IReadOnlyDictionary<string, string>? metadata) =>
        new(
            organizationId,
            action,
            resourceType,
            resourceId,
            outcome,
            correlationId,
            context.PrincipalId,
            context.PrincipalType,
            facilityId,
            metadata is null ? null : AuditMetadata.Serialize(metadata));
}

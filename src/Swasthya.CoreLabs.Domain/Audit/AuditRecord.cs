using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Domain.Audit;

public sealed class AuditRecord
{
    private AuditRecord()
    {
    }

    public AuditRecord(
        Guid organizationId,
        string action,
        string resourceType,
        string? resourceId,
        AuditOutcome outcome,
        string correlationId,
        Guid? actorId,
        PrincipalType? actorType,
        Guid? facilityId = null,
        string? contextMetadata = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        FacilityId = facilityId;
        ActorId = actorId;
        ActorType = actorType;
        Action = string.IsNullOrWhiteSpace(action)
            ? throw new ArgumentException("Audit action is required.", nameof(action))
            : action;
        ResourceType = string.IsNullOrWhiteSpace(resourceType)
            ? throw new ArgumentException("Audit resource type is required.", nameof(resourceType))
            : resourceType;
        ResourceId = resourceId;
        OccurredAtUtc = DateTimeOffset.UtcNow;
        CorrelationId = string.IsNullOrWhiteSpace(correlationId)
            ? throw new ArgumentException("Audit correlation id is required.", nameof(correlationId))
            : correlationId;
        Outcome = outcome;
        ContextMetadata = contextMetadata;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public Guid? FacilityId { get; private set; }

    public Guid? ActorId { get; private set; }

    public PrincipalType? ActorType { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string ResourceType { get; private set; } = string.Empty;

    public string? ResourceId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string CorrelationId { get; private set; } = string.Empty;

    public AuditOutcome Outcome { get; private set; }

    public string? ContextMetadata { get; private set; }
}

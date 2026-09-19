using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record OrganizationDto(
    Guid Id,
    string Name,
    EntityStatus Status,
    bool HasFullOrganizationAccess,
    IReadOnlyCollection<Guid> AccessibleFacilityIds);

public sealed record FacilityDto(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    EntityStatus Status);

public sealed record AuditRecordDto(
    Guid Id,
    Guid OrganizationId,
    Guid? FacilityId,
    Guid? ActorId,
    PrincipalType? ActorType,
    string Action,
    string ResourceType,
    string? ResourceId,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId,
    AuditOutcome Outcome,
    string? ContextMetadata);

public sealed record AuthContextDto(
    Guid PrincipalId,
    string Issuer,
    string ExternalSubject,
    PrincipalType PrincipalType,
    IReadOnlyCollection<OrganizationDto> Organizations);

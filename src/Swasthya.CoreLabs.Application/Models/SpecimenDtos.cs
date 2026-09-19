using Swasthya.CoreLabs.Domain.Orders;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record SpecimenDto(
    Guid Id,
    Guid OrderItemId,
    Guid ContainerId,
    Swasthya.CoreLabs.Domain.Specimen.SpecimenType SpecimenType,
    string? AccessionNumber,
    Swasthya.CoreLabs.Domain.Specimen.SpecimenStatus Status,
    string? CollectorIdentity,
    Guid? FacilityId,
    string? CollectionNotes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record ContainerDto(
    Guid Id,
    Guid FacilityId,
    Swasthya.CoreLabs.Domain.Specimen.SpecimenType SpecimenType,
    string? Label,
    string? Barcode,
    Swasthya.CoreLabs.Domain.Specimen.SpecimenStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record SpecimenStatusDto(
    Swasthya.CoreLabs.Domain.Specimen.SpecimenStatus Status,
    bool IsTerminal);

public sealed record AccessionDto(
    Guid Id,
    Guid FacilityId,
    string AccessionNumber,
    string? Description,
    Swasthya.CoreLabs.Domain.Specimen.AccessionStatus Status,
    Guid? ReceivingActorId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record CreateSpecimenRequest(
    Guid OrderItemId,
    Guid ContainerId,
    Swasthya.CoreLabs.Domain.Specimen.SpecimenType SpecimenType,
    string? AccessionNumber = null,
    Swasthya.CoreLabs.Domain.Specimen.SpecimenStatus Status = Swasthya.CoreLabs.Domain.Specimen.SpecimenStatus.Requested,
    string? CollectorIdentity = null,
    Guid? FacilityId = null,
    string? CollectionNotes = null);

public sealed record UpdateSpecimenRequest(
    Swasthya.CoreLabs.Domain.Specimen.SpecimenStatus? Status = null,
    string? AccessionNumber = null);

public sealed record CreateCollectionRequest(
    Swasthya.CoreLabs.Domain.Specimen.SpecimenStatus NewStatus,
    string? CollectorIdentity = null,
    string? CollectionNotes = null);

public sealed record CreateAccessionRequest(
    Swasthya.CoreLabs.Domain.Specimen.AccessionStatus NewStatus,
    string? Description = null,
    Guid? ReceivingActorId = null);

public sealed record SpecimenCollectionEventDto(
    Guid SpecimenId,
    Swasthya.CoreLabs.Domain.Specimen.SpecimenStatus Status,
    string? CollectorIdentity,
    string? CollectionNotes,
    DateTimeOffset? CollectedAtUtc);

public sealed record SpecimenAccessionDto(
    Guid SpecimenId,
    string AccessionNumber,
    Swasthya.CoreLabs.Domain.Specimen.AccessionStatus Status,
    DateTimeOffset? ReceivedAtUtc,
    DateTimeOffset? AccessionedAtUtc);

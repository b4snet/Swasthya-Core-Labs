using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record SpecimenListItemDto(
    Guid Id,
    Guid OrderItemId,
    Guid ContainerId,
    SpecimenType SpecimenType,
    string? AccessionNumber,
    SpecimenStatus Status,
    Guid? FacilityId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

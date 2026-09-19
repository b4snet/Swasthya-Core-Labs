using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record AccessionListItemDto(
    Guid Id,
    Guid? FacilityId,
    string? AccessionNumber,
    string? Description,
    AccessionStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

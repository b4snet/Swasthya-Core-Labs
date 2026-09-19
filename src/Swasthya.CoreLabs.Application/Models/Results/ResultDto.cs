using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Results;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record ResultDto(
    Guid Id,
    Guid OrderItemId,
    Guid SpecimenId,
    Guid TestVersionId,
    int ValueTypeCode,
    decimal? NumericValue,
    string? TextualValue,
    string? CodedValue,
    string? CodedSystem,
    string? UnitUcumCode,
    DateTimeOffset ObservedAtUtc,
    DateTimeOffset EnteredAtUtc,
    string EnteredByPrincipalIdentity,
    string StatusCode,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

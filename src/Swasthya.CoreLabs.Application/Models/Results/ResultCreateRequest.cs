using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Results;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record ResultCreateRequest(
    Guid OrderItemId,
    Guid SpecimenId,
    Guid TestVersionId,
    int ValueTypeCode, // maps to ResultDataTypeKind
    decimal? NumericValue,
    string? TextualValue,
    string? CodedValue,
    string? CodedSystem,
    string? UnitUcumCode,
    DateTimeOffset ObservedAtUtc,
    Guid EnteredByPrincipalId);

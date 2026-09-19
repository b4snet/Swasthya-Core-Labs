using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Results;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record ResultListItemDto(
    Guid Id,
    Guid OrderItemId,
    Guid SpecimenId,
    Guid TestVersionId,
    int ValueTypeCode,
    string StatusCode,
    DateTimeOffset ObservedAtUtc,
    DateTimeOffset EnteredAtUtc,
    string EnteredByPrincipalIdentity,
    DateTimeOffset CreatedAtUtc);

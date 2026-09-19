using Swasthya.CoreLabs.Domain.Results;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record UpdateResultRequest(
    Guid ResultId,
    string? StatusCode,
    decimal? NumericValue,
    string? TextualValue,
    string? CodedValue,
    string? CodedSystem,
    string? UnitUcumCode);

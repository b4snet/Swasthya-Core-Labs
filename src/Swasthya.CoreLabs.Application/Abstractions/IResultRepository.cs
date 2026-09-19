using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Results;
using Swasthya.CoreLabs.Domain.Specimen;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Swasthya.CoreLabs.Application.Abstractions;

public interface IResultRepository
{
    Task<ResultDto?> GetByIdAsync(Guid resultId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ResultListItemDto>> ListByOrderItemAsync(
        Guid orderItemId,
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ResultListItemDto>> ListBySpecimenAsync(
        Guid specimenId,
        Guid organizationId,
        CancellationToken cancellationToken);

    Task<ResultDto> CreateAsync(
        Guid orderItemId,
        Guid specimenId,
        Guid testVersionId,
        int valueTypeCode,
        decimal? numericValue,
        string? textualValue,
        string? codedValue,
        string? codedSystem,
        string? unitUcumCode,
        DateTimeOffset observedAtUtc,
        Guid enteredByPrincipalId,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        Guid orderItemId,
        Guid specimenId,
        Guid testVersionId,
        CancellationToken cancellationToken);
}

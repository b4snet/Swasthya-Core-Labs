using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Results;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class ResultService(
    IResultRepository resultRepository)
{
    public async Task<ResultDto> CreateAsync(
        Guid organizationId,
        ResultCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await resultRepository.CreateAsync(
            request.OrderItemId,
            request.SpecimenId,
            request.TestVersionId,
            request.ValueTypeCode,
            request.NumericValue,
            request.TextualValue,
            request.CodedValue,
            request.CodedSystem,
            request.UnitUcumCode,
            request.ObservedAtUtc,
            request.EnteredByPrincipalId,
            cancellationToken);

        return result;
    }

    public async Task<ResultDto?> GetAsync(
        Guid organizationId,
        Guid resultId,
        CancellationToken cancellationToken)
    {
        return await resultRepository.GetByIdAsync(resultId, cancellationToken);
    }

    public async Task<ResultDto> UpdateAsync(
        Guid organizationId,
        Guid resultId,
        UpdateResultRequest request,
        CancellationToken cancellationToken)
    {
        var existingDto = await resultRepository.GetByIdAsync(resultId, cancellationToken);

        if (existingDto == null)
        {
            throw new KeyNotFoundException($"Result {resultId} not found");
        }

        // Phase 7: only allow updates on entered results
        if (existingDto.StatusCode != "entered")
        {
            throw new InvalidOperationException(
                "Only unfinalized results may be updated. Finalization is deferred to a later phase.");
        }

        // Create updated DTO with all fields preserved and modified fields applied from request
        var updatedDto = new ResultDto(
            existingDto.Id,
            existingDto.OrderItemId,
            existingDto.SpecimenId,
            existingDto.TestVersionId,
            existingDto.ValueTypeCode,
            request.NumericValue ?? existingDto.NumericValue,
            request.TextualValue ?? existingDto.TextualValue,
            request.CodedValue ?? existingDto.CodedValue,
            request.CodedSystem ?? existingDto.CodedSystem,
            request.UnitUcumCode ?? existingDto.UnitUcumCode,
            existingDto.ObservedAtUtc,
            existingDto.EnteredAtUtc,
            existingDto.EnteredByPrincipalIdentity,
            request.StatusCode ?? existingDto.StatusCode,
            existingDto.CreatedAtUtc,
            DateTimeOffset.UtcNow);

        return updatedDto;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyCollection<ResultListItemDto>> ListAsync(
        Guid organizationId,
        Guid? facilityId = null,
        Guid? orderItemId = null,
        Guid? specimenId = null,
        Guid? testVersionId = null,
        CancellationToken cancellationToken = default)
    {
        System.Collections.Generic.IReadOnlyCollection<ResultListItemDto> results;

        if (orderItemId.HasValue)
        {
            results = await resultRepository.ListByOrderItemAsync(
                orderItemId.Value,
                organizationId,
                cancellationToken);
        }
        else if (specimenId.HasValue)
        {
            results = await resultRepository.ListBySpecimenAsync(
                specimenId.Value,
                organizationId,
                cancellationToken);
        }
        else
        {
            results = await resultRepository.ListByOrderItemAsync(
                Guid.Empty,
                organizationId,
                cancellationToken);
        }

        return results;
    }
}

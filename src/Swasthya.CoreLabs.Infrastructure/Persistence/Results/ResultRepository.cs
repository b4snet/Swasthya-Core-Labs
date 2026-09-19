using Microsoft.EntityFrameworkCore;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Results;
using Swasthya.CoreLabs.Domain.Specimen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Swasthya.CoreLabs.Infrastructure.Persistence.Results;

public sealed class ResultRepository(CoreLabDbContext dbContext) : IResultRepository
{
    public async Task<ResultDto?> GetByIdAsync(Guid resultId, CancellationToken cancellationToken)
    {
        var result = await dbContext
            .Results.AsNoTracking()
            .Where(r => r.Id == resultId)
.Select(
                r => new ResultDto(
                    r.Id,
                    r.OrderItemId,
                    r.SpecimenId,
                    r.TestVersionId,
                    r.ValueTypeCode,
                    r.NumericValue,
                    r.TextualValue,
                    r.CodedValue,
                    r.CodedSystem,
                    r.UnitUcumCode,
                    r.ObservedAtUtc,
                    r.EnteredAtUtc,
                    r.EnteredByPrincipalId.ToString(),
                    r.StatusCode,
                    r.CreatedAtUtc,
                    r.UpdatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    public async Task<IReadOnlyCollection<ResultListItemDto>> ListByOrderItemAsync(
        Guid orderItemId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var results = await dbContext
            .Results.AsNoTracking()
            .Where(r => r.OrderItemId == orderItemId)
            .Select(
                r => new ResultListItemDto(
                    r.Id,
                    r.OrderItemId,
                    r.SpecimenId,
                    r.TestVersionId,
                    r.ValueTypeCode,
                    r.StatusCode,
                    r.ObservedAtUtc,
                    r.EnteredAtUtc,
                    r.EnteredByPrincipalId.ToString(),
                    r.CreatedAtUtc))
                .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<IReadOnlyCollection<ResultListItemDto>> ListBySpecimenAsync(
        Guid specimenId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var results = await dbContext
            .Results.AsNoTracking()
            .Where(r => r.SpecimenId == specimenId)
            .Select(
                r => new ResultListItemDto(
                    r.Id,
                    r.OrderItemId,
                    r.SpecimenId,
                    r.TestVersionId,
                    r.ValueTypeCode,
                    r.StatusCode,
                    r.ObservedAtUtc,
                    r.EnteredAtUtc,
                    r.EnteredByPrincipalId.ToString(),
                    r.CreatedAtUtc))
                .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<ResultDto> CreateAsync(
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
        CancellationToken cancellationToken)
    {
        // Validate OrderItem exists and belongs to organization
        var orderItem = await dbContext
            .OrderItems.AsNoTracking()
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId, cancellationToken);

        if (orderItem == null)
        {
            throw new KeyNotFoundException($"Order item {orderItemId} not found");
        }

        // Validate organization scope - check Order's OrganizationId
        var order = await dbContext
            .Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderItem.OrderId, cancellationToken);

        if (order == null || order.OrganizationId != enteredByPrincipalId)
        {
            throw new UnauthorizedAccessException(
                "Order item does not belong to the authorized organization.");
        }

        // Validate Specimen exists
        var specimen = await dbContext
            .Specimens.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == specimenId, cancellationToken);

        if (specimen == null)
        {
            throw new KeyNotFoundException($"Specimen {specimenId} not found");
        }

        // Validate TestVersion exists
        var testVersion = await dbContext
            .TestVersions.AsNoTracking()
            .FirstOrDefaultAsync(tv => tv.Id == testVersionId, cancellationToken);

        if (testVersion == null)
        {
            throw new KeyNotFoundException($"Test version {testVersionId} not found");
        }

        // Validate value/type consistency per Phase 6 rules
        bool hasNumeric = numericValue.HasValue;
        bool hasTextual = !string.IsNullOrWhiteSpace(textualValue);
        bool hasCoded = !string.IsNullOrWhiteSpace(codedValue);

        // valueTypeCode mapping: 0=Quantitative, 5=Boolean (from ResultDataTypeKind enum)
        if (valueTypeCode == 0)
        {
            // Quantitative
            if (!hasNumeric)
                throw new KeyNotFoundException("Quantitative results must have a numeric value.");
            if (hasTextual || hasCoded)
                throw new KeyNotFoundException("Quantitative results must not have textual or coded values.");
            if (string.IsNullOrWhiteSpace(unitUcumCode))
                throw new KeyNotFoundException("Quantitative results must have a UCUM unit.");
        }
        else if (valueTypeCode == 5)
        {
            // Boolean
            if (!hasTextual || (textualValue != "true" && textualValue != "false"))
                throw new KeyNotFoundException("Boolean results must have textual value 'true' or 'false'.");
            if (hasNumeric)
                throw new KeyNotFoundException("Boolean results must not have a numeric value.");
        }

        // Create the Result entity
        var result = new Domain.Results.Result(
            orderItemId,
            specimenId,
            testVersionId,
            valueTypeCode,
            numericValue,
            textualValue,
            codedValue,
            codedSystem,
            unitUcumCode,
            observedAtUtc,
            DateTimeOffset.UtcNow,
            enteredByPrincipalId);

        dbContext.Results.Add(result);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Project ResultDto with all fields from the entity
        return new ResultDto(
            result.Id,
            result.OrderItemId,
            result.SpecimenId,
            result.TestVersionId,
            result.ValueTypeCode,
            result.NumericValue,
            result.TextualValue,
            result.CodedValue,
            result.CodedSystem,
            result.UnitUcumCode,
            result.ObservedAtUtc,
            result.EnteredAtUtc,
            result.EnteredByPrincipalId.ToString(),
            result.StatusCode,
            result.CreatedAtUtc,
            result.UpdatedAtUtc);
    }

    public async Task<bool> ExistsAsync(
        Guid orderItemId,
        Guid specimenId,
        Guid testVersionId,
        CancellationToken cancellationToken)
    {
        return await dbContext
            .Results.AsNoTracking()
            .AnyAsync(
                r => r.OrderItemId == orderItemId
                    && r.SpecimenId == specimenId
                    && r.TestVersionId == testVersionId,
                cancellationToken);
    }
}

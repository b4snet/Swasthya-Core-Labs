using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class WorklistService(
    ISpecimenRepository specimenRepository,
    IAccessionRepository accessionRepository,
    IOrderItemRepository orderItemRepository,
    IAuthorizationService authorizationService)
{
    public async Task<List<SpecimenListItemDto>> ListSpecimensAsWorklistAsync(
        AuthContext authContext,
        Guid organizationId,
        Guid? facilityId = null,
        string? status = null,
        string? accessionNumber = null,
        Guid? orderItemId = null,
        CancellationToken cancellationToken = default)
    {
        await authorizationService.RequireOrganizationAccess(
            authContext, Permissions.LaboratoryCatalogRead, organizationId);

        var specimens = await specimenRepository.ListAsync(
            organizationId, facilityId, cancellationToken);

        var query = specimens.AsQueryable();

        if (status != null)
        {
            query = query.Where(s => s.Status.ToString().Equals(status, System.StringComparison.OrdinalIgnoreCase));
        }

        if (accessionNumber != null)
        {
            query = query.Where(s => s.AccessionNumber != null && s.AccessionNumber.Contains(accessionNumber));
        }

        if (orderItemId.HasValue)
        {
            query = query.Where(s => s.OrderItemId == orderItemId.Value);
        }

        var result = query
            .Select(s => new SpecimenListItemDto(
                s.Id,
                s.OrderItemId,
                s.ContainerId,
                s.SpecimenType,
                s.AccessionNumber,
                s.Status,
                s.FacilityId,
                s.CreatedAtUtc,
                s.UpdatedAtUtc))
            .ToList();

        return result;
    }

    public async Task<List<AccessionListItemDto>> ListAccessionsAsWorklistAsync(
        AuthContext authContext,
        Guid organizationId,
        Guid? facilityId = null,
        string? status = null,
        string? accessionNumber = null,
        CancellationToken cancellationToken = default)
    {
        await authorizationService.RequireOrganizationAccess(
            authContext, Permissions.LaboratoryCatalogRead, organizationId);

        var accessions = await accessionRepository.ListAsync(
            organizationId, facilityId, cancellationToken);

        var query = accessions.AsQueryable();

        if (status != null)
        {
            query = query.Where(a => a.Status.ToString().Equals(status, System.StringComparison.OrdinalIgnoreCase));
        }

        if (accessionNumber != null)
        {
            query = query.Where(a => a.AccessionNumber != null && a.AccessionNumber.Contains(accessionNumber));
        }

        var result = query
            .Select(a => new AccessionListItemDto(
                a.Id,
                a.FacilityId,
                a.AccessionNumber,
                a.Description,
                a.Status,
                a.CreatedAtUtc,
                a.UpdatedAtUtc))
            .ToList();

        return result;
    }

    public async Task<List<OrderItemListItemDto>> ListOrderItemsAsWorklistAsync(
        AuthContext authContext,
        Guid organizationId,
        Guid? facilityId = null,
        string? status = null,
        Guid? orderId = null,
        CancellationToken cancellationToken = default)
    {
        await authorizationService.RequireOrganizationAccess(
            authContext, Permissions.LaboratoryCatalogRead, organizationId);

        var orderItems = await orderItemRepository.ListAsync(
            organizationId, facilityId, cancellationToken);

        var query = orderItems.AsQueryable();

        if (status != null)
        {
            query = query.Where(i => i.Status.ToString().Equals(status, System.StringComparison.OrdinalIgnoreCase));
        }

        if (orderId.HasValue)
        {
            query = query.Where(i => i.OrderId == orderId.Value);
        }

        var result = query
            .Select(i => new OrderItemListItemDto(
                i.Id,
                i.OrderId,
                i.TestCode,
                i.TestName,
                i.PanelCode,
                i.PanelName,
                i.RequestedQuantity,
                i.QuantityUnitUcumCode,
                i.Status,
                i.CreatedAtUtc,
                i.UpdatedAtUtc))
            .ToList();

        return result;
    }

    public async Task<List<SpecimenType>> GetAvailableSpecimenTypesAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var specimens = await specimenRepository.ListAsync(
            organizationId, cancellationToken: cancellationToken);

        return specimens
            .Select(s => s.SpecimenType)
            .Distinct()
            .ToList();
    }
}

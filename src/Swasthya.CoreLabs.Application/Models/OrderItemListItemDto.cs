using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record OrderItemListItemDto(
    Guid Id,
    Guid OrderId,
    string? TestCode,
    string? TestName,
    string? PanelCode,
    string? PanelName,
    decimal? RequestedQuantity,
    string? QuantityUnitUcumCode,
    OrderItemStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

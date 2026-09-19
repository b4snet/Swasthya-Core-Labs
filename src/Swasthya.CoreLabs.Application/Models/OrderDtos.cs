using Swasthya.CoreLabs.Domain.Orders;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record OrderItemDto(
    Guid Id,
    Guid OrderId,
    int SequenceNumber,
    OrderItemStatus Status,
    OrderPriority? Priority,
    Guid? TestId,
    Guid? PanelId,
    Guid? TestVersionId,
    Guid? PanelVersionId,
    string? TestCode,
    string? TestName,
    string? PanelCode,
    string? PanelName,
    decimal? RequestedQuantity,
    string? QuantityUnitUcumCode,
    DateTimeOffset? CancelledAtUtc,
    DateTimeOffset CreatedAtUtc);

public sealed record OrderDetailDto(
    Guid Id,
    Guid OrganizationId,
    Guid FacilityId,
    string OrderNumber,
    string? ExternalOrderId,
    string PatientExternalSystem,
    string PatientExternalIdentifier,
    string? EncounterExternalSystem,
    string? EncounterExternalIdentifier,
    OrderPriority Priority,
    OrderStatus Status,
    Guid? RequestedByPrincipalId,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<OrderItemDto> Items);

public sealed record OrderItemInput(
    string? TestCode = null,
    string? PanelCode = null,
    OrderPriority? Priority = null,
    decimal? RequestedQuantity = null,
    string? QuantityUnitUcumCode = null);

public sealed record CreateOrderRequest(
    string OrderNumber,
    Guid FacilityId,
    string PatientExternalSystem,
    string PatientExternalIdentifier,
    string? ExternalOrderId = null,
    string? EncounterExternalSystem = null,
    string? EncounterExternalIdentifier = null,
    OrderPriority Priority = OrderPriority.Routine,
    IReadOnlyCollection<OrderItemInput>? Items = null);

public sealed record AddOrderItemRequest(
    string? TestCode = null,
    string? PanelCode = null,
    OrderPriority? Priority = null,
    decimal? RequestedQuantity = null,
    string? QuantityUnitUcumCode = null);

/// <summary>
/// Null values mean "unchanged"; an empty string clears an optional field.
/// Required patient fields reject blank values.
/// </summary>
public sealed record UpdateOrderRequest(
    OrderPriority? Priority = null,
    string? PatientExternalSystem = null,
    string? PatientExternalIdentifier = null,
    string? EncounterExternalSystem = null,
    string? EncounterExternalIdentifier = null);

public sealed record CreateOrderResultDto(OrderDetailDto Order, bool WasReplayed);

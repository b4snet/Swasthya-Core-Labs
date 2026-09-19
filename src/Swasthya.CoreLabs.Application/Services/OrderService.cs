using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Laboratory;
using Swasthya.CoreLabs.Domain.Orders;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class OrderService(
    IOrderRepository orderRepository,
    ITestCatalogRepository testCatalogRepository,
    IPanelRepository panelRepository,
    IAuditRepository auditRepository,
    ICorrelationIdProvider correlationIdProvider)
{
    public async Task<CreateOrderResultDto> CreateOrderAsync(
        AuthContext context,
        Guid organizationId,
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ValidationException("A request body is required.");
        }

        LaboratoryPermissionGuard.RequireFacilityScoped(
            context, Permissions.OrderWrite, organizationId, request.FacilityId);

        if (request.ExternalOrderId is { } externalOrderId)
        {
            Order? existing = await orderRepository.FindOrderByExternalIdAsync(
                organizationId, externalOrderId, cancellationToken);
            if (existing is not null)
            {
                LaboratoryPermissionGuard.RequireFacilityScoped(
                    context, Permissions.OrderWrite, organizationId, existing.FacilityId);
                return new CreateOrderResultDto(
                    await BuildDetailAsync(organizationId, existing, cancellationToken),
                    WasReplayed: true);
            }
        }

        if (await orderRepository.FindOrderByNumberAsync(
                organizationId, request.OrderNumber ?? string.Empty, cancellationToken) is not null)
        {
            throw new ConflictException(
                $"An order with number '{request.OrderNumber}' already exists.");
        }

        Order order;
        try
        {
            order = new Order(
                organizationId,
                request.FacilityId,
                request.OrderNumber ?? string.Empty,
                request.ExternalOrderId,
                request.PatientExternalSystem ?? string.Empty,
                request.PatientExternalIdentifier ?? string.Empty,
                request.EncounterExternalSystem,
                request.EncounterExternalIdentifier,
                request.Priority,
                context.PrincipalId);
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }

        await orderRepository.AddOrderAsync(order, cancellationToken);

        var createdItems = new List<OrderItem>();
        int sequence = 0;
        if (request.Items is not null)
        {
            foreach (OrderItemInput input in request.Items)
            {
                OrderItem item = await BuildOrderItemAsync(
                    organizationId,
                    order.Id,
                    order.FacilityId,
                    sequence,
                    input,
                    cancellationToken);
                await orderRepository.AddOrderItemAsync(item, cancellationToken);
                createdItems.Add(item);
                sequence++;
            }
        }

        await WriteOrderAuditAsync(
            context,
            organizationId,
            order.FacilityId,
            AuditActions.OrderCreated,
            AuditResourceTypes.Order,
            order.OrderNumber,
            cancellationToken);

        return new CreateOrderResultDto(
            ToDetail(order, createdItems),
            WasReplayed: false);
    }

    public async Task<IReadOnlyCollection<OrderDetailDto>> ListOrdersAsync(
        AuthContext context,
        Guid organizationId,
        Guid? facilityId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid>? facilityFilter = LaboratoryScope.ResolveFacilityFilter(
            context, Permissions.OrderRead, organizationId, facilityId);

        IReadOnlyCollection<Order> orders = await orderRepository
            .ListOrdersAsync(organizationId, facilityFilter, cancellationToken);

        Guid[] orderIds = orders.Select(o => o.Id).ToArray();
        IReadOnlyCollection<OrderItem> items = await orderRepository
            .ListOrderItemsAsync(orderIds, cancellationToken);

        await WriteOrderAuditAsync(
            context,
            organizationId,
            null,
            AuditActions.OrderListed,
            AuditResourceTypes.Order,
            null,
            cancellationToken);

        return orders
            .OrderBy(o => o.OrderNumber, StringComparer.Ordinal)
            .Select(o => ToDetail(o, items))
            .ToArray();
    }

    public async Task<OrderDetailDto> GetOrderAsync(
        AuthContext context,
        Guid organizationId,
        string orderNumber,
        CancellationToken cancellationToken)
    {
        PermissionGuard.Require(context, Permissions.OrderRead, organizationId);

        Order order = await RequireOrderAsync(organizationId, orderNumber, cancellationToken);
        RequireFacilityRead(context, organizationId, order.FacilityId);

        await WriteOrderAuditAsync(
            context,
            organizationId,
            order.FacilityId,
            AuditActions.OrderListed,
            AuditResourceTypes.Order,
            order.OrderNumber,
            cancellationToken);

        return await BuildDetailAsync(organizationId, order, cancellationToken);
    }

    public async Task<OrderDetailDto> UpdateOrderAsync(
        AuthContext context,
        Guid organizationId,
        string orderNumber,
        UpdateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ValidationException("A request body is required.");
        }

        Order order = await RequireOrderAsync(organizationId, orderNumber, cancellationToken);
        LaboratoryPermissionGuard.RequireFacilityScoped(
            context, Permissions.OrderWrite, organizationId, order.FacilityId);

        RequireRequested(order);

        OrderPriority priority = request.Priority ?? order.Priority;
        string patientSystem = request.PatientExternalSystem ?? order.PatientExternalSystem;
        string patientIdentifier = request.PatientExternalIdentifier ?? order.PatientExternalIdentifier;
        string? encounterSystem = request.EncounterExternalSystem ?? order.EncounterExternalSystem;
        string? encounterIdentifier = request.EncounterExternalIdentifier ?? order.EncounterExternalIdentifier;

        try
        {
            order.Update(
                priority,
                patientSystem,
                patientIdentifier,
                encounterSystem,
                encounterIdentifier);
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }

        await WriteOrderAuditAsync(
            context,
            organizationId,
            order.FacilityId,
            AuditActions.OrderUpdated,
            AuditResourceTypes.Order,
            order.OrderNumber,
            cancellationToken);

        return await BuildDetailAsync(organizationId, order, cancellationToken);
    }

    public async Task<OrderDetailDto> AddOrderItemAsync(
        AuthContext context,
        Guid organizationId,
        string orderNumber,
        AddOrderItemRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ValidationException("A request body is required.");
        }

        Order order = await RequireOrderAsync(organizationId, orderNumber, cancellationToken);
        LaboratoryPermissionGuard.RequireFacilityScoped(
            context, Permissions.OrderWrite, organizationId, order.FacilityId);

        RequireRequested(order);

        IReadOnlyCollection<OrderItem> existingItems = await orderRepository
            .ListOrderItemsAsync([order.Id], cancellationToken);
        int sequence = existingItems.Count == 0
            ? 0
            : existingItems.Max(i => i.SequenceNumber) + 1;

        OrderItem item = await BuildOrderItemAsync(
            organizationId,
            order.Id,
            order.FacilityId,
            sequence,
            new OrderItemInput(
                request.TestCode,
                request.PanelCode,
                request.Priority,
                request.RequestedQuantity,
                request.QuantityUnitUcumCode),
            cancellationToken);
        await orderRepository.AddOrderItemAsync(item, cancellationToken);

        await WriteOrderAuditAsync(
            context,
            organizationId,
            order.FacilityId,
            AuditActions.OrderUpdated,
            AuditResourceTypes.Order,
            order.OrderNumber,
            cancellationToken);

        return ToDetail(order, existingItems.Append(item).ToArray());
    }

    public async Task<OrderDetailDto> CancelOrderItemAsync(
        AuthContext context,
        Guid organizationId,
        string orderNumber,
        Guid orderItemId,
        CancellationToken cancellationToken)
    {
        Order order = await RequireOrderAsync(organizationId, orderNumber, cancellationToken);
        LaboratoryPermissionGuard.RequireFacilityScoped(
            context, Permissions.OrderWrite, organizationId, order.FacilityId);

        RequireRequested(order);

        IReadOnlyCollection<OrderItem> items = await orderRepository
            .ListOrderItemsAsync([order.Id], cancellationToken);
        OrderItem item = items.FirstOrDefault(i => i.Id == orderItemId)
            ?? throw new NotFoundException("The order item was not found.");

        CancelItem(item);
        await WriteOrderAuditAsync(
            context,
            organizationId,
            order.FacilityId,
            AuditActions.OrderItemCancelled,
            AuditResourceTypes.OrderItem,
            $"{order.OrderNumber}:{item.SequenceNumber}",
            cancellationToken);

        return ToDetail(order, items);
    }

    public async Task<OrderDetailDto> CancelOrderAsync(
        AuthContext context,
        Guid organizationId,
        string orderNumber,
        CancellationToken cancellationToken)
    {
        Order order = await RequireOrderAsync(organizationId, orderNumber, cancellationToken);
        LaboratoryPermissionGuard.RequireFacilityScoped(
            context, Permissions.OrderWrite, organizationId, order.FacilityId);

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new ValidationException("The order is already cancelled.");
        }

        IReadOnlyCollection<OrderItem> items = await orderRepository
            .ListOrderItemsAsync([order.Id], cancellationToken);

        order.Cancel();
        foreach (OrderItem item in items.Where(i => i.Status == OrderItemStatus.Requested))
        {
            CancelItem(item);
        }

        await WriteOrderAuditAsync(
            context,
            organizationId,
            order.FacilityId,
            AuditActions.OrderCancelled,
            AuditResourceTypes.Order,
            order.OrderNumber,
            cancellationToken);
        foreach (OrderItem item in items.Where(i => i.Status == OrderItemStatus.Cancelled))
        {
            await WriteOrderAuditAsync(
                context,
                organizationId,
                order.FacilityId,
                AuditActions.OrderItemCancelled,
                AuditResourceTypes.OrderItem,
                $"{order.OrderNumber}:{item.SequenceNumber}",
                cancellationToken);
        }

        return ToDetail(order, items);
    }

    private async Task<OrderItem> BuildOrderItemAsync(
        Guid organizationId,
        Guid orderId,
        Guid facilityId,
        int sequence,
        OrderItemInput input,
        CancellationToken cancellationToken)
    {
        bool hasTest = !string.IsNullOrWhiteSpace(input.TestCode);
        bool hasPanel = !string.IsNullOrWhiteSpace(input.PanelCode);
        if (hasTest == hasPanel)
        {
            throw new ValidationException(
                "Each order item must reference exactly one test or one panel.");
        }

        try
        {
            if (hasTest)
            {
                return await BuildTestItemAsync(
                    organizationId,
                    orderId,
                    facilityId,
                    sequence,
                    input,
                    cancellationToken);
            }

            return await BuildPanelItemAsync(
                organizationId,
                orderId,
                sequence,
                input,
                cancellationToken);
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }
    }

    private async Task<OrderItem> BuildTestItemAsync(
        Guid organizationId,
        Guid orderId,
        Guid facilityId,
        int sequence,
        OrderItemInput input,
        CancellationToken cancellationToken)
    {
        Test test = await testCatalogRepository.FindTestByCodeAsync(
                organizationId, input.TestCode!, cancellationToken)
            ?? throw new ValidationException($"Test '{input.TestCode}' was not found.");

        RequireActive(test);
        TestVersion version = await RequireCurrentVersionAsync(
            test, cancellationToken);

        IReadOnlyCollection<TestFacility> facilities = await testCatalogRepository
            .ListTestFacilitiesAsync([test.Id], cancellationToken);
        if (facilities.Count > 0
            && !facilities.Any(f => f.FacilityId == facilityId))
        {
            throw new ValidationException(
                $"Test '{test.Code}' is not available at the selected facility.");
        }

        return OrderItem.ForTest(
            orderId,
            sequence,
            test.Id,
            version.Id,
            test.Code,
            version.Name,
            input.Priority,
            input.RequestedQuantity,
            input.QuantityUnitUcumCode);
    }

    private async Task<OrderItem> BuildPanelItemAsync(
        Guid organizationId,
        Guid orderId,
        int sequence,
        OrderItemInput input,
        CancellationToken cancellationToken)
    {
        Panel panel = await panelRepository.FindPanelByCodeAsync(
                organizationId, input.PanelCode!, cancellationToken)
            ?? throw new ValidationException($"Panel '{input.PanelCode}' was not found.");

        RequireActive(panel);
        PanelVersion version = await RequireCurrentVersionAsync(
            panel, cancellationToken);

        return OrderItem.ForPanel(
            orderId,
            sequence,
            panel.Id,
            version.Id,
            panel.Code,
            version.Name,
            input.Priority,
            input.RequestedQuantity,
            input.QuantityUnitUcumCode);
    }

    private static void RequireActive(Test test)
    {
        if (test.Status != MasterDataStatus.Active)
        {
            throw new ValidationException(
                $"Test '{test.Code}' must be active to be ordered.");
        }
    }

    private static void RequireActive(Panel panel)
    {
        if (panel.Status != MasterDataStatus.Active)
        {
            throw new ValidationException(
                $"Panel '{panel.Code}' must be active to be ordered.");
        }
    }

    private async Task<TestVersion> RequireCurrentVersionAsync(
        Test test,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TestVersion> versions = await testCatalogRepository
            .ListTestVersionsAsync([test.Id], cancellationToken);
        return versions.FirstOrDefault(v => v.VersionNumber == test.CurrentVersionNumber)
            ?? throw new NotFoundException(
                $"Test '{test.Code}' has no published definition.");
    }

    private async Task<PanelVersion> RequireCurrentVersionAsync(
        Panel panel,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<PanelVersion> versions = await panelRepository
            .ListPanelVersionsAsync([panel.Id], cancellationToken);
        return versions.FirstOrDefault(v => v.VersionNumber == panel.CurrentVersionNumber)
            ?? throw new NotFoundException(
                $"Panel '{panel.Code}' has no published definition.");
    }

    private async Task<Order> RequireOrderAsync(
        Guid organizationId,
        string orderNumber,
        CancellationToken cancellationToken) =>
        await orderRepository.FindOrderByNumberAsync(organizationId, orderNumber, cancellationToken)
        ?? throw new NotFoundException($"Order '{orderNumber}' was not found.");

    private static void RequireRequested(Order order)
    {
        if (order.Status == OrderStatus.Cancelled)
        {
            throw new ValidationException("A cancelled order cannot be modified.");
        }
    }

    private static void CancelItem(OrderItem item)
    {
        try
        {
            item.Cancel();
        }
        catch (InvalidOperationException exception)
        {
            throw new ValidationException(exception.Message);
        }
    }

    private static void RequireFacilityRead(
        AuthContext context,
        Guid organizationId,
        Guid facilityId)
    {
        IReadOnlyCollection<Guid>? facilityFilter = LaboratoryScope.ResolveFacilityFilter(
            context, Permissions.OrderRead, organizationId, null);
        if (facilityFilter is not null && !facilityFilter.Contains(facilityId))
        {
            throw new PermissionDeniedException(
                "Principal has no order read access for the order's facility.");
        }
    }

    private async Task<OrderDetailDto> BuildDetailAsync(
        Guid organizationId,
        Order order,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<OrderItem> items = await orderRepository
            .ListOrderItemsAsync([order.Id], cancellationToken);
        return ToDetail(order, items);
    }

    private static OrderDetailDto ToDetail(
        Order order,
        IReadOnlyCollection<OrderItem> items) =>
        new(
            order.Id,
            order.OrganizationId,
            order.FacilityId,
            order.OrderNumber,
            order.ExternalOrderId,
            order.PatientExternalSystem,
            order.PatientExternalIdentifier,
            order.EncounterExternalSystem,
            order.EncounterExternalIdentifier,
            order.Priority,
            order.Status,
            order.RequestedByPrincipalId,
            order.RequestedAtUtc,
            order.CancelledAtUtc,
            order.CreatedAtUtc,
            order.UpdatedAtUtc,
            items
                .Where(i => i.OrderId == order.Id)
                .OrderBy(i => i.SequenceNumber)
                .Select(i => new OrderItemDto(
                    i.Id,
                    i.OrderId,
                    i.SequenceNumber,
                    i.Status,
                    i.Priority,
                    i.TestId,
                    i.PanelId,
                    i.TestVersionId,
                    i.PanelVersionId,
                    i.TestCode,
                    i.TestName,
                    i.PanelCode,
                    i.PanelName,
                    i.RequestedQuantity,
                    i.QuantityUnitUcumCode,
                    i.CancelledAtUtc,
                    i.CreatedAtUtc))
                .ToArray());

    private Task WriteOrderAuditAsync(
        AuthContext context,
        Guid organizationId,
        Guid? facilityId,
        string action,
        string resourceType,
        string? resourceId,
        CancellationToken cancellationToken) =>
        auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                facilityId,
                action,
                resourceType,
                resourceId,
                Domain.Common.AuditOutcome.Success,
                null),
            cancellationToken);
}

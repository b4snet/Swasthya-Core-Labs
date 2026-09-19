using Swasthya.CoreLabs.Domain.Orders;

namespace Swasthya.CoreLabs.Tests;

public class OrderDomainTests
{
    [Theory]
    [InlineData(OrderStatus.Requested, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Requested, false)]
    [InlineData(OrderStatus.Requested, OrderStatus.Requested, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Cancelled, false)]
    public void OrderLifecycle_Transitions_MatchPolicy(
        OrderStatus from,
        OrderStatus to,
        bool expected)
    {
        Assert.Equal(expected, OrderLifecycle.CanTransition(from, to));
    }

    [Fact]
    public void OrderLifecycle_RequireTransition_CancelledIsTerminal()
    {
        Assert.Throws<InvalidOperationException>(
            () => OrderLifecycle.RequireTransition(
                OrderStatus.Cancelled, OrderStatus.Requested));
    }

    [Theory]
    [InlineData(OrderItemStatus.Requested, OrderItemStatus.Cancelled, true)]
    [InlineData(OrderItemStatus.Cancelled, OrderItemStatus.Requested, false)]
    [InlineData(OrderItemStatus.Requested, OrderItemStatus.Requested, false)]
    public void OrderItemLifecycle_Transitions_MatchPolicy(
        OrderItemStatus from,
        OrderItemStatus to,
        bool expected)
    {
        Assert.Equal(expected, OrderItemLifecycle.CanTransition(from, to));
    }

    [Fact]
    public void Order_NewOrder_StartsRequestedAndPinsDefaults()
    {
        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1",
            null,
            "emr.test",
            "subject-42");

        Assert.Equal(OrderStatus.Requested, order.Status);
        Assert.Equal(OrderPriority.Routine, order.Priority);
        Assert.False(string.IsNullOrWhiteSpace(order.OrderNumber));
        Assert.Null(order.EncounterExternalSystem);
        Assert.Null(order.EncounterExternalIdentifier);
    }

    [Fact]
    public void Order_EmptyOrganization_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Order(
            Guid.Empty, Guid.NewGuid(), "ORD-1", null, "emr.test", "subject-42"));
    }

    [Fact]
    public void Order_EmptyFacility_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Order(
            Guid.NewGuid(), Guid.Empty, "ORD-1", null, "emr.test", "subject-42"));
    }

    [Fact]
    public void Order_BlankOrderNumber_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Order(
            Guid.NewGuid(), Guid.NewGuid(), " ", null, "emr.test", "subject-42"));
    }

    [Fact]
    public void Order_BlankPatientReference_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Order(
            Guid.NewGuid(), Guid.NewGuid(), "ORD-1", null, " ", "subject-42"));
        Assert.Throws<ArgumentException>(() => new Order(
            Guid.NewGuid(), Guid.NewGuid(), "ORD-1", null, "emr.test", " "));
    }

    [Fact]
    public void Order_EncounterSystemWithoutIdentifier_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1",
            null,
            "emr.test",
            "subject-42",
            encounterExternalSystem: "emr.test"));
    }

    [Fact]
    public void Order_UnknownPriority_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1",
            null,
            "emr.test",
            "subject-42",
            priority: (OrderPriority)99));
    }

    [Fact]
    public void Order_Update_ReplacesRequestDetails()
    {
        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1",
            null,
            "emr.test",
            "subject-42");

        order.Update(
            OrderPriority.Urgent,
            "ehr.next",
            "subject-99",
            "emr.test",
            "encounter-7");

        Assert.Equal(OrderPriority.Urgent, order.Priority);
        Assert.Equal("ehr.next", order.PatientExternalSystem);
        Assert.Equal("subject-99", order.PatientExternalIdentifier);
        Assert.Equal("encounter-7", order.EncounterExternalIdentifier);
    }

    [Fact]
    public void Order_UpdateEncounterSystemWithoutIdentifier_Throws()
    {
        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1",
            null,
            "emr.test",
            "subject-42");

        Assert.Throws<ArgumentException>(() => order.Update(
            OrderPriority.Routine, "emr.test", "subject-42", "emr.test", null));
    }

    [Fact]
    public void Order_Cancel_SetsCancelledState()
    {
        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1",
            null,
            "emr.test",
            "subject-42");

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.NotNull(order.CancelledAtUtc);
    }

    [Fact]
    public void Order_CancelTwice_Throws()
    {
        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1",
            null,
            "emr.test",
            "subject-42");
        order.Cancel();

        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void Order_UpdateAfterCancel_Throws()
    {
        var order = new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ORD-1",
            null,
            "emr.test",
            "subject-42");
        order.Cancel();

        Assert.Throws<InvalidOperationException>(() => order.Update(
            OrderPriority.Routine, "emr.test", "subject-42", null, null));
    }

    [Fact]
    public void OrderItem_ForTest_CreatesRequestedItemWithVersionPin()
    {
        Guid orderId = Guid.NewGuid();
        Guid testId = Guid.NewGuid();
        Guid versionId = Guid.NewGuid();

        OrderItem item = OrderItem.ForTest(
            orderId, 0, testId, versionId, "cbc", "Complete blood count");

        Assert.Equal(OrderItemStatus.Requested, item.Status);
        Assert.Equal(testId, item.TestId);
        Assert.Equal(versionId, item.TestVersionId);
        Assert.Null(item.PanelId);
        Assert.Equal("cbc", item.TestCode);
        Assert.Equal("Complete blood count", item.TestName);
        Assert.Null(item.RequestedQuantity);
    }

    [Fact]
    public void OrderItem_ForPanel_CreatesRequestedItemWithVersionPin()
    {
        Guid orderId = Guid.NewGuid();
        Guid panelId = Guid.NewGuid();
        Guid versionId = Guid.NewGuid();

        OrderItem item = OrderItem.ForPanel(
            orderId, 1, panelId, versionId, "met", "Metabolic panel");

        Assert.Equal(OrderItemStatus.Requested, item.Status);
        Assert.Equal(panelId, item.PanelId);
        Assert.Equal(versionId, item.PanelVersionId);
        Assert.Null(item.TestId);
        Assert.Equal("met", item.PanelCode);
        Assert.Equal("Metabolic panel", item.PanelName);
    }

    [Fact]
    public void OrderItem_CancelTwice_Throws()
    {
        OrderItem item = OrderItem.ForTest(
            Guid.NewGuid(), 0, Guid.NewGuid(), Guid.NewGuid(), "cbc", "CBC");
        item.Cancel();

        Assert.Throws<InvalidOperationException>(() => item.Cancel());
    }

    [Fact]
    public void OrderItem_QuantityWithoutUnit_Throws()
    {
        Assert.Throws<ArgumentException>(() => OrderItem.ForTest(
            Guid.NewGuid(),
            0,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "cbc",
            "CBC",
            requestedQuantity: 2m));
    }

    [Fact]
    public void OrderItem_NegativeQuantity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderItem.ForTest(
            Guid.NewGuid(),
            0,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "glu",
            "Glucose",
            requestedQuantity: -1m,
            quantityUnitUcumCode: "mg/dL"));
    }

    [Fact]
    public void OrderItem_InvalidUcumCode_Throws()
    {
        Assert.Throws<ArgumentException>(() => OrderItem.ForTest(
            Guid.NewGuid(),
            0,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "glu",
            "Glucose",
            requestedQuantity: 1m,
            quantityUnitUcumCode: "bad unit!"));
    }

    [Fact]
    public void OrderItem_EmptySnapshotCode_Throws()
    {
        Assert.Throws<ArgumentException>(() => OrderItem.ForTest(
            Guid.NewGuid(), 0, Guid.NewGuid(), Guid.NewGuid(), " ", "CBC"));
    }

    [Fact]
    public void OrderItem_UnknownPriority_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderItem.ForTest(
            Guid.NewGuid(),
            0,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "cbc",
            "CBC",
            priority: (OrderPriority)99));
    }
}

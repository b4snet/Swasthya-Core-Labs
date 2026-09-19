using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Tests;

public class SpecimenDomainTests
{
    [Theory]
    [InlineData(SpecimenStatus.Requested, SpecimenStatus.Collected, true)]
    [InlineData(SpecimenStatus.Collected, SpecimenStatus.Requested, false)]
    [InlineData(SpecimenStatus.Requested, SpecimenStatus.Requested, false)]
    [InlineData(SpecimenStatus.Collected, SpecimenStatus.Collected, false)]
    public void SpecimenStatus_Transitions_MatchPolicy(
        SpecimenStatus from,
        SpecimenStatus to,
        bool expected)
    {
        Assert.Equal(expected, SpecimenLifecycle.CanTransition(from, to));
    }

    [Fact]
    public void SpecimenStatus_RequireTransition_TerminalStates()
    {
        Assert.Throws<InvalidOperationException>(
            () => SpecimenLifecycle.RequireTransition(
                SpecimenStatus.Stored, SpecimenStatus.Requested));

        Assert.Throws<InvalidOperationException>(
            () => SpecimenLifecycle.RequireTransition(
                SpecimenStatus.Rejected, SpecimenStatus.Requested));
    }

    [Theory]
    [InlineData(SpecimenType.Blood, 0)]
    [InlineData(SpecimenType.Plasma, 2)]
    [InlineData(SpecimenType.Other, 8)]
    public void SpecimenType_Values_AreValid(SpecimenType type, int expectedCode)
    {
        Assert.Equal(expectedCode, (int)type);
    }

    [Fact]
    public void Specimen_NewSpecimen_HasCorrectInitialState()
    {
        var orderItemId = Guid.NewGuid();
        var containerId = Guid.NewGuid();

        var specimen = new Specimen(
            orderItemId,
            containerId,
            SpecimenType.Blood);

        Assert.Equal(orderItemId, specimen.OrderItemId);
        Assert.Equal(containerId, specimen.ContainerId);
        Assert.Equal(SpecimenType.Blood, specimen.SpecimenType);
        Assert.Equal(SpecimenStatus.Requested, specimen.Status);
        Assert.Null(specimen.AccessionNumber);
        Assert.Null(specimen.CollectorIdentity);
        Assert.Null(specimen.FacilityId);
        Assert.Null(specimen.CollectionNotes);
        Assert.True(specimen.CreatedAtUtc > DateTimeOffset.MinValue);
        Assert.True(specimen.UpdatedAtUtc > DateTimeOffset.MinValue);
    }

    [Fact]
    public void Specimen_SetStatus_ValidTransition()
    {
        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood);

        specimen.SetStatus(SpecimenStatus.Collected);
        Assert.Equal(SpecimenStatus.Collected, specimen.Status);
        Assert.NotEqual(default, specimen.UpdatedAtUtc);
    }

    [Fact]
    public void Specimen_SetStatus_InvalidTransition_Throws()
    {
        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood);

        Assert.Throws<InvalidOperationException>(
            () => specimen.SetStatus(SpecimenStatus.Requested));
    }

    [Fact]
    public void Specimen_SetAccessionNumber_UpdatesAccessionNumber()
    {
        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood);

        specimen.SetAccessionNumber("ACC-12345");

        Assert.Equal("ACC-12345", specimen.AccessionNumber);
        Assert.NotEqual(default, specimen.UpdatedAtUtc);
    }

    [Fact]
    public void Specimen_SetCollectorIdentity_UpdatesIdentity()
    {
        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood);

        specimen.SetCollectorIdentity("Dr. Smith");

        Assert.Equal("Dr. Smith", specimen.CollectorIdentity);
        Assert.NotEqual(default, specimen.UpdatedAtUtc);
    }

    [Fact]
    public void Specimen_SetCollectionNotes_UpdatesNotes()
    {
        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood);

        specimen.SetCollectionNotes("Rush sample - hemolysis suspected");

        Assert.Equal("Rush sample - hemolysis suspected", specimen.CollectionNotes);
        Assert.NotEqual(default, specimen.UpdatedAtUtc);
    }

    [Fact]
    public void Specimen_AssignContainer_UpdatesContainer()
    {
        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood);

        var newContainerId = Guid.NewGuid();

        specimen.AssignContainer(newContainerId);

        Assert.Equal(newContainerId, specimen.ContainerId);
        Assert.NotEqual(default, specimen.UpdatedAtUtc);
    }

    [Fact]
    public void Specimen_ReassignOrderItem_UpdatesOrderItem()
    {
        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood);

        var newOrderItemId = Guid.NewGuid();

        specimen.ReassignOrderItem(newOrderItemId);

        Assert.Equal(newOrderItemId, specimen.OrderItemId);
        Assert.NotEqual(default, specimen.UpdatedAtUtc);
    }

    [Fact]
    public void Specimen_Constructor_EmptyAccessionNumber_IsValid()
    {
        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood,
            accessionNumber: null);

        Assert.Null(specimen.AccessionNumber);
    }

    [Fact]
    public void Specimen_Constructor_WithAccessionNumber_SetsIt()
    {
        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood,
            accessionNumber: "ACC-999");

        Assert.Equal("ACC-999", specimen.AccessionNumber);
    }

    [Fact]
    public void Specimen_Constructor_EmptyFacilityId_IsValid()
    {
        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood,
            facilityId: null);

        Assert.Null(specimen.FacilityId);
    }

    [Fact]
    public void Specimen_Constructor_ValidFacilityId_SetsIt()
    {
        var facilityId = Guid.NewGuid();

        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood,
            facilityId: facilityId);

        Assert.Equal(facilityId, specimen.FacilityId);
    }

    [Fact]
    public void Specimen_Constructor_EmptyCollectionNotes_IsValid()
    {
        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood,
            collectionNotes: null);

        Assert.Null(specimen.CollectionNotes);
    }

    [Fact]
    public void Specimen_Constructor_WithCollectionNotes_SetsIt()
    {
        var specimen = new Specimen(
            Guid.NewGuid(),
            Guid.NewGuid(),
            SpecimenType.Blood,
            collectionNotes: "Rush sample");

        Assert.Equal("Rush sample", specimen.CollectionNotes);
    }
}

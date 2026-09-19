using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Tests;

public class LaboratoryDomainTests
{
    [Theory]
    [InlineData(MasterDataStatus.Draft, MasterDataStatus.Active, true)]
    [InlineData(MasterDataStatus.Draft, MasterDataStatus.Inactive, true)]
    [InlineData(MasterDataStatus.Draft, MasterDataStatus.Retired, true)]
    [InlineData(MasterDataStatus.Active, MasterDataStatus.Inactive, true)]
    [InlineData(MasterDataStatus.Active, MasterDataStatus.Retired, true)]
    [InlineData(MasterDataStatus.Inactive, MasterDataStatus.Active, true)]
    [InlineData(MasterDataStatus.Inactive, MasterDataStatus.Retired, true)]
    [InlineData(MasterDataStatus.Active, MasterDataStatus.Draft, false)]
    [InlineData(MasterDataStatus.Retired, MasterDataStatus.Active, false)]
    [InlineData(MasterDataStatus.Retired, MasterDataStatus.Retired, false)]
    [InlineData(MasterDataStatus.Active, MasterDataStatus.Active, false)]
    public void Lifecycle_Transitions_MatchPolicy(
        MasterDataStatus from,
        MasterDataStatus to,
        bool expected)
    {
        Assert.Equal(expected, MasterDataLifecycle.CanTransition(from, to));
    }

    [Fact]
    public void Lifecycle_RequireTransition_RetiredIsTerminal()
    {
        Assert.Throws<InvalidOperationException>(
            () => MasterDataLifecycle.RequireTransition(
                MasterDataStatus.Retired, MasterDataStatus.Active));
    }

    [Fact]
    public void Test_StartNextVersion_IncrementsVersionPointer()
    {
        var test = new Test(Guid.NewGuid(), "cbc");
        Assert.Equal(0, test.CurrentVersionNumber);

        Assert.Equal(1, test.StartNextVersion());
        Assert.Equal(2, test.StartNextVersion());
        Assert.Equal(2, test.CurrentVersionNumber);
    }

    [Fact]
    public void Test_RetireThenReactivate_Throws()
    {
        var test = new Test(Guid.NewGuid(), "cbc");
        test.SetStatus(MasterDataStatus.Retired);

        Assert.Throws<InvalidOperationException>(
            () => test.SetStatus(MasterDataStatus.Active));
    }

    [Fact]
    public void Test_InvalidCode_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Test(Guid.NewGuid(), "bad code!"));
    }

    [Fact]
    public void TestVersion_ZeroVersionNumber_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TestVersion(
            Guid.NewGuid(), 0, "Name", Guid.NewGuid()));
    }

    [Fact]
    public void TestVersion_EmptyResultDataType_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TestVersion(
            Guid.NewGuid(), 1, "Name", Guid.Empty));
    }

    [Fact]
    public void TestVersion_EffectiveToBeforeFrom_Throws()
    {
        DateTimeOffset from = DateTimeOffset.UtcNow;
        Assert.Throws<ArgumentException>(() => new TestVersion(
            Guid.NewGuid(),
            1,
            "Name",
            Guid.NewGuid(),
            effectiveFromUtc: from,
            effectiveToUtc: from.AddDays(-1)));
    }

    [Fact]
    public void ReferenceRange_BlankSource_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ReferenceRange(
            Guid.NewGuid(), null, Guid.NewGuid(), " ", textualReference: "see note"));
    }

    [Fact]
    public void ReferenceRange_NoBoundsOrText_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ReferenceRange(
            Guid.NewGuid(), null, Guid.NewGuid(), "source"));
    }

    [Fact]
    public void ReferenceRange_PartialQuantitativeBounds_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ReferenceRange(
            Guid.NewGuid(), null, Guid.NewGuid(), "source", lowValue: 1m));
    }

    [Fact]
    public void ReferenceRange_LowAboveHigh_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ReferenceRange(
            Guid.NewGuid(), null, Guid.NewGuid(), "source", lowValue: 5m, highValue: 1m));
    }

    [Fact]
    public void ReferenceRange_AgeWithoutUnit_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ReferenceRange(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            "source",
            ageMinimum: 0m,
            highValue: null,
            textualReference: "see note"));
    }

    [Fact]
    public void ReferenceRange_TextualOnly_IsAllowed()
    {
        var range = new ReferenceRange(
            Guid.NewGuid(), null, Guid.NewGuid(), "source", textualReference: "negative");

        Assert.Equal("negative", range.TextualReference);
        Assert.Null(range.LowValue);
    }

    [Fact]
    public void ConfigurationItem_UpperKey_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ConfigurationItem(
            Guid.NewGuid(),
            null,
            "Lab.Mode",
            "value",
            ConfigurationValueType.Text));
    }

    [Fact]
    public void TestSpecimenRequirement_EmptySpecimenType_Throws()
    {
        Assert.Throws<ArgumentException>(() => new TestSpecimenRequirement(
            Guid.NewGuid(), Guid.Empty));
    }

    [Fact]
    public void PanelTestMembership_NegativeSequence_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PanelTestMembership(
            Guid.NewGuid(), Guid.NewGuid(), -1));
    }

    [Theory]
    [InlineData("mg/dL", true)]
    [InlineData("10*3/uL", true)]
    [InlineData("", false)]
    [InlineData("mg dl", false)]
    public void UcumCode_Validates(string code, bool valid)
    {
        if (valid)
        {
            Assert.Equal(code, CatalogCodes.RequireUcumCode(code, nameof(code)));
        }
        else
        {
            Assert.Throws<ArgumentException>(
                () => CatalogCodes.RequireUcumCode(code, nameof(code)));
        }
    }

    [Fact]
    public void ExternalCode_DisallowedCharacter_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => CatalogCodes.RequireExternalCode("LOINC 1234", nameof(CatalogCodes)));
    }

    [Fact]
    public void Name_OverLength_Throws()
    {
        string tooLong = new('a', 151);
        Assert.Throws<ArgumentException>(
            () => CatalogCodes.RequireName(tooLong, nameof(CatalogCodes)));
    }
}

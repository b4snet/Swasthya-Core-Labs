using Swasthya.CoreLabs.Domain.Audit;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.IdentityTenancy;

namespace Swasthya.CoreLabs.Tests;

public class EntityInvariantTests
{
    [Fact]
    public void Organization_BlankName_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Organization(" "));
    }

    [Fact]
    public void Facility_BlankCode_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Facility(Guid.NewGuid(), " ", "Name"));
    }

    [Fact]
    public void Facility_BlankName_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Facility(Guid.NewGuid(), "code", " "));
    }

    [Fact]
    public void Principal_BlankExternalSubject_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new Principal(PrincipalType.Human, "issuer", " ", "John"));
    }

    [Fact]
    public void Principal_SetActive_TogglesState()
    {
        var principal = new Principal(
            PrincipalType.Human, "issuer", "sub-1", "John");
        Assert.True(principal.IsActive);

        principal.SetActive(false);
        Assert.False(principal.IsActive);
    }

    [Fact]
    public void Role_BlankCode_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new Role(Guid.NewGuid(), " ", "Name"));
    }

    [Fact]
    public void Permission_BlankCode_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new Permission(" ", "Name"));
    }

    [Fact]
    public void AuditRecord_BlankAction_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new AuditRecord(
                Guid.NewGuid(),
                " ",
                "organization",
                null,
                AuditOutcome.Success,
                "corr-1",
                null,
                null));
    }

    [Fact]
    public void AuditRecord_BlankCorrelation_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new AuditRecord(
                Guid.NewGuid(),
                "org.created",
                "organization",
                null,
                AuditOutcome.Success,
                " ",
                null,
                null));
    }
}

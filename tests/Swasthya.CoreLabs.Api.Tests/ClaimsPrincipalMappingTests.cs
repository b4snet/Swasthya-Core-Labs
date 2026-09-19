using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Swasthya.CoreLabs.Api.Options;
using Swasthya.CoreLabs.Api.Security;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Api.Tests;

public sealed class ClaimsPrincipalMappingTests
{
    private static readonly AuthOptions Options = new()
    {
        Issuer = TestJwtDefaults.Issuer,
        Audience = TestJwtDefaults.Audience,
        PrincipalTypeClaim = "principal_type",
        ServicePrincipalTypeValue = "service",
    };

    [Fact]
    public void ValidatedToken_MapsSubjectAndIssuer()
    {
        ClaimsPrincipal principal = PrincipalFrom(TestJwtDefaults.CreateToken("sub-1"));

        bool mapped = principal.TryGetPrincipalIdentity(Options, out var identity);

        Assert.True(mapped);
        Assert.Equal("sub-1", identity!.ExternalSubject);
        Assert.Equal(TestJwtDefaults.Issuer, identity.Issuer);
        Assert.Equal(PrincipalType.Human, identity.PrincipalType);
    }

    [Fact]
    public void ServiceTypeClaim_MapsToServicePrincipal()
    {
        ClaimsPrincipal principal = PrincipalFrom(
            TestJwtDefaults.CreateToken("svc-1", principalType: "service"));

        bool mapped = principal.TryGetPrincipalIdentity(Options, out var identity);

        Assert.True(mapped);
        Assert.Equal(PrincipalType.Service, identity!.PrincipalType);
    }

    [Fact]
    public void UnknownPrincipalType_DefaultsToHuman()
    {
        ClaimsPrincipal principal = PrincipalFrom(
            TestJwtDefaults.CreateToken("sub-2", principalType: "machine"));

        bool mapped = principal.TryGetPrincipalIdentity(Options, out var identity);

        Assert.True(mapped);
        Assert.Equal(PrincipalType.Human, identity!.PrincipalType);
    }

    [Fact]
    public void ClientIdClaim_DoesNotBecomeIdentity()
    {
        ClaimsPrincipal principal = PrincipalFrom(TestJwtDefaults.CreateToken(
            "sub-3",
            additionalClaims: new Dictionary<string, string>
            {
                ["client_id"] = "some-client",
            }));

        bool mapped = principal.TryGetPrincipalIdentity(Options, out var identity);

        Assert.True(mapped);
        Assert.Equal("sub-3", identity!.ExternalSubject);
        Assert.Equal(TestJwtDefaults.Issuer, identity.Issuer);
    }

    [Fact]
    public void MissingSubject_FailsMapping()
    {
        ClaimsPrincipal principal = PrincipalFrom(TestJwtDefaults.CreateToken(string.Empty));

        bool mapped = principal.TryGetPrincipalIdentity(Options, out var identity);

        Assert.False(mapped);
        Assert.Null(identity);
    }

    private static ClaimsPrincipal PrincipalFrom(string token)
    {
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        return handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = TestJwtDefaults.SigningKey,
            ValidateIssuer = true,
            ValidIssuer = TestJwtDefaults.Issuer,
            ValidateAudience = true,
            ValidAudience = TestJwtDefaults.Audience,
            ValidateLifetime = true,
        }, out _);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Swasthya.CoreLabs.TestSupport;

public static class TestJwtDefaults
{
    public const string Issuer = "https://identity.test/swasthya/core";
    public const string Audience = "swasthya-core-labs-api";
    public const string SigningKeyBase64 = "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY=";
    public const string SubjectHuman = "subject-human-admin";
    public const string SubjectService = "subject-service-auditor";
    public const string SubjectUnknown = "subject-unknown";

    public static SymmetricSecurityKey SigningKey { get; } =
        new SymmetricSecurityKey(Convert.FromBase64String(SigningKeyBase64));

    public static string AltSigningKeyBase64 { get; } =
        Convert.ToBase64String(Enumerable.Repeat((byte)0x41, 32).ToArray());

    public static string CreateToken(
        string subject,
        string? principalType = null,
        string? issuer = null,
        string? audience = null,
        string signingKeyBase64 = SigningKeyBase64,
        IReadOnlyDictionary<string, string>? additionalClaims = null)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Convert.FromBase64String(signingKeyBase64)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("sub", subject),
        };

        if (principalType is not null)
        {
            claims.Add(new("principal_type", principalType));
        }

        if (additionalClaims is not null)
        {
            claims.AddRange(additionalClaims.Select(c => new Claim(c.Key, c.Value)));
        }

        var token = new JwtSecurityToken(
            issuer: issuer ?? Issuer,
            audience: audience ?? Audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GetConnectionString() => TestScratch.GetConnectionString()
        ?? throw new InvalidOperationException(
            "Set SCL_PG_TEST_CONNECTION or run scripts/pg-test.ps1 to run DB-backed tests.");
}

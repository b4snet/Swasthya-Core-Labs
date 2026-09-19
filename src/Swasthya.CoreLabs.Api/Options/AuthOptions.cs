namespace Swasthya.CoreLabs.Api.Options;

public sealed class AuthOptions
{
    public const string SectionName = "Authentication";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string? SigningKeyBase64 { get; init; }

    public string? JwksUrl { get; init; }

    public string PrincipalTypeClaim { get; init; } = "principal_type";

    public string ServicePrincipalTypeValue { get; init; } = "service";
}

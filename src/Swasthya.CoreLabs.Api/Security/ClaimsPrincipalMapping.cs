using System.Security.Claims;
using Swasthya.CoreLabs.Api.Options;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Api.Security;

public static class ClaimsPrincipalMapping
{
    private const string SubClaimType = "sub";
    private const string IssClaimType = "iss";

    /// <summary>
    /// Maps an already VALIDATED (signature, issuer, audience, lifetime) JWT principal to the
    /// internal principal identity used for authorization. The <c>iss</c> and <c>sub</c> claims
    /// are taken only after JwtBearer has validated them against the configured contract; the
    /// principal type is derived from the configured type claim, never from arbitrary client_id
    /// claims.
    /// </summary>
    public static bool TryGetPrincipalIdentity(
        this ClaimsPrincipal principal,
        AuthOptions options,
        out PrincipalIdentity? identity)
    {
        identity = null;

        string? subject = principal.FindFirst(SubClaimType)?.Value;
        string? issuer = principal.FindFirst(IssClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(issuer))
        {
            return false;
        }

        PrincipalType type = PrincipalType.Human;
        if (!string.IsNullOrWhiteSpace(options.PrincipalTypeClaim))
        {
            string? typeValue = principal.FindFirst(options.PrincipalTypeClaim)?.Value;
            if (typeValue is not null)
            {
                type = string.Equals(
                    typeValue,
                    options.ServicePrincipalTypeValue,
                    StringComparison.OrdinalIgnoreCase)
                    ? PrincipalType.Service
                    : PrincipalType.Human;
            }
        }

        identity = new PrincipalIdentity(issuer, subject, type);
        return true;
    }
}

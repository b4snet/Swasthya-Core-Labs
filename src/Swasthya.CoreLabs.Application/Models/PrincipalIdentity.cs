using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Application.Models;

public sealed record PrincipalIdentity(string Issuer, string ExternalSubject, PrincipalType PrincipalType)
{
    public bool Matches(string issuer, string subject, PrincipalType type) =>
        string.Equals(Issuer, issuer, StringComparison.OrdinalIgnoreCase)
        && string.Equals(ExternalSubject, subject, StringComparison.Ordinal)
        && PrincipalType == type;
}

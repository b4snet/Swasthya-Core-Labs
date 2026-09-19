using Swasthya.CoreLabs.Domain.Common;

namespace Swasthya.CoreLabs.Domain.IdentityTenancy;

public sealed class Principal
{
    private Principal()
    {
    }

    public Principal(PrincipalType type, string issuer, string externalSubject, string displayName)
    {
        Id = Guid.NewGuid();
        Type = type;
        Issuer = !string.IsNullOrWhiteSpace(issuer)
            ? issuer
            : throw new ArgumentException("Principal issuer is required.", nameof(issuer));
        ExternalSubject = !string.IsNullOrWhiteSpace(externalSubject)
            ? externalSubject
            : throw new ArgumentException("Principal external subject is required.", nameof(externalSubject));
        DisplayName = !string.IsNullOrWhiteSpace(displayName)
            ? displayName
            : throw new ArgumentException("Principal display name is required.", nameof(displayName));
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public PrincipalType Type { get; private set; }

    public string Issuer { get; private set; } = string.Empty;

    public string ExternalSubject { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}

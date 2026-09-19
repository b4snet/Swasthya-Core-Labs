using System.Diagnostics.CodeAnalysis;

namespace Swasthya.CoreLabs.Domain.IdentityTenancy;

[SuppressMessage("Design", "CA1711", Justification = "'Permission' is the intentional domain term for an access right.")]
public sealed class Permission
{
    private Permission()
    {
    }

    public Permission(string code, string name, string? description = null)
    {
        Id = Guid.NewGuid();
        Code = !string.IsNullOrWhiteSpace(code)
            ? code
            : throw new ArgumentException("Permission code is required.", nameof(code));
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Permission name is required.", nameof(name));
        Description = description;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public uint Version { get; private set; }
}

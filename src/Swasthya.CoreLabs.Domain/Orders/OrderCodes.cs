namespace Swasthya.CoreLabs.Domain.Orders;

/// <summary>
/// Validation helpers for diagnostic-order fields. Patient and encounter
/// references are always external identifiers owned by an ordering system;
/// this system never stores patient identity data itself.
/// </summary>
public static class OrderCodes
{
    public static string RequireOrderNumber(string orderNumber, string paramName)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new ArgumentException("An order number is required.", paramName);
        }

        orderNumber = orderNumber.Trim();
        if (orderNumber.Length > 100)
        {
            throw new ArgumentException(
                "Order number must not exceed 100 characters.",
                paramName);
        }

        return orderNumber;
    }

    /// <summary>
    /// Normalizes an optional external reference value to null when blank.
    /// </summary>
    public static string? NormalizeOptional(string? value, int maxLength, string paramName)
    {
        value = (value ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            return null;
        }

        if (value.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value must not exceed {maxLength} characters.",
                paramName);
        }

        return value;
    }

    public static string RequireExternalSystem(string system, string paramName)
    {
        if (string.IsNullOrWhiteSpace(system))
        {
            throw new ArgumentException("A reference system is required.", paramName);
        }

        system = system.Trim();
        if (system.Length > 100)
        {
            throw new ArgumentException(
                "Reference system must not exceed 100 characters.",
                paramName);
        }

        return system;
    }

    public static string RequireExternalIdentifier(string identifier, string paramName)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("An external identifier is required.", paramName);
        }

        identifier = identifier.Trim();
        if (identifier.Length > 200)
        {
            throw new ArgumentException(
                "External identifier must not exceed 200 characters.",
                paramName);
        }

        return identifier;
    }

    public static string RequireSnapshotCode(string code, string paramName)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("A snapshot code is required.", paramName);
        }

        code = code.Trim();
        if (code.Length > 100)
        {
            throw new ArgumentException(
                "Snapshot code must not exceed 100 characters.",
                paramName);
        }

        return code;
    }

    public static string RequireSnapshotName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A snapshot name is required.", paramName);
        }

        name = name.Trim();
        if (name.Length > 150)
        {
            throw new ArgumentException(
                "Snapshot name must not exceed 150 characters.",
                paramName);
        }

        return name;
    }
}

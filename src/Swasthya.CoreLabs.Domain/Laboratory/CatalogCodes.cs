using System.Text.RegularExpressions;

namespace Swasthya.CoreLabs.Domain.Laboratory;

public static partial class CatalogCodes
{
    private const int MaxNameLength = 150;
    private const int MaxDescriptionLength = 500;
    private const int MaxNotesLength = 500;

    public static string RequireCode(string code, string paramName)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("A code is required.", paramName);
        }

        code = code.Trim();
        if (code.Length > 100 || !CodePattern().IsMatch(code))
        {
            throw new ArgumentException(
                "Code must be between 1 and 100 characters using letters, digits, '.', '_' or '-'.",
                paramName);
        }

        return code;
    }

    public static string RequireConfigurationKey(string key, string paramName)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("A configuration key is required.", paramName);
        }

        key = key.Trim();
        if (key.Length > 128 || !ConfigurationKeyPattern().IsMatch(key))
        {
            throw new ArgumentException(
                "Configuration keys must start with a lowercase letter and use lowercase letters, digits, '.', '_' or '-' (max 128).",
                paramName);
        }

        return key;
    }

    public static string RequireUcumCode(string code, string paramName)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("A UCUM code is required.", paramName);
        }

        code = code.Trim();
        if (code.Length > 80
            || !UcumPattern().IsMatch(code)
            || !code.Any(char.IsLetterOrDigit))
        {
            throw new ArgumentException(
                "A UCUM code must be a supported UCUM expression (letters, digits, '.', '/', '%', '_', '^', '(', ')', '*', '-' up to 80 characters).",
                paramName);
        }

        return code;
    }

    public static string RequireExternalCode(string code, string paramName)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("An external code is required.", paramName);
        }

        code = code.Trim();
        if (code.Length > 100 || !ExternalCodePattern().IsMatch(code))
        {
            throw new ArgumentException(
                "An external code must be between 1 and 100 characters using letters, digits, '.', '_', ':', '/' or '-'.",
                paramName);
        }

        return code;
    }

    public static string RequireName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A name is required.", paramName);
        }

        name = name.Trim();
        if (name.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Name must not exceed {MaxNameLength} characters.",
                paramName);
        }

        return name;
    }

    public static string RequireDescription(string? description, string paramName)
    {
        description = (description ?? string.Empty).Trim();
        if (description.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"Description must not exceed {MaxDescriptionLength} characters.",
                paramName);
        }

        return description;
    }

    public static string RequireNotes(string? notes, string paramName)
    {
        notes = (notes ?? string.Empty).Trim();
        if (notes.Length > MaxNotesLength)
        {
            throw new ArgumentException(
                $"Notes must not exceed {MaxNotesLength} characters.",
                paramName);
        }

        return notes;
    }

    [GeneratedRegex("^[A-Za-z0-9._-]{1,100}$")]
    private static partial Regex CodePattern();

    [GeneratedRegex("^[a-z][a-z0-9._-]{0,127}$")]
    private static partial Regex ConfigurationKeyPattern();

    [GeneratedRegex("^[A-Za-z0-9.*%_/^()-]{1,80}$")]
    private static partial Regex UcumPattern();

    [GeneratedRegex("^[A-Za-z0-9._:/\\\\-]{1,100}$")]
    private static partial Regex ExternalCodePattern();
}

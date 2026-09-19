using System.Collections.ObjectModel;

namespace Swasthya.CoreLabs.Application.Authorization;

public static class AuditMetadata
{
    private static readonly HashSet<string> AllowlistedKeys = new(
    [
        "request_method",
        "request_path",
        "http_status",
        "reason",
        "failure",
    ], StringComparer.Ordinal);

    public static void TryAdd(IDictionary<string, string> metadata, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!AllowlistedKeys.Contains(key))
        {
            throw new ArgumentException(
                $"Audit metadata key '{key}' is not on the allowlist.", nameof(key));
        }

        if (value.Length > 200)
        {
            throw new ArgumentException(
                $"Audit metadata value for '{key}' exceeds 200 characters.", nameof(value));
        }

        metadata[key] = value;
    }

    public static string Serialize(IReadOnlyDictionary<string, string> metadata)
    {
        if (metadata.Count == 0)
        {
            return string.Empty;
        }

        return System.Text.Json.JsonSerializer.Serialize(
            new ReadOnlyDictionary<string, string>((IDictionary<string, string>)metadata));
    }
}

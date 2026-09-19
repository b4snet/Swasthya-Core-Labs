using System.Text.Json;
using Swasthya.CoreLabs.Application.Authorization;

namespace Swasthya.CoreLabs.Tests;

public class AuditMetadataTests
{
    [Fact]
    public void AllowlistedKey_IsAccepted()
    {
        var metadata = new Dictionary<string, string>();

        AuditMetadata.TryAdd(metadata, "request_method", "GET");
        AuditMetadata.TryAdd(metadata, "request_path", "/v1/audit");
        AuditMetadata.TryAdd(metadata, "http_status", "200");
        AuditMetadata.TryAdd(metadata, "reason", "policy decision");
        AuditMetadata.TryAdd(metadata, "failure", "unknown_principal");

        Assert.Equal(5, metadata.Count);
    }

    [Fact]
    public void UnknownKey_Throws()
    {
        var metadata = new Dictionary<string, string>();

        Assert.Throws<ArgumentException>(
            () => AuditMetadata.TryAdd(metadata, "patient_name", "John"));
    }

    [Fact]
    public void OversizedValue_Throws()
    {
        var metadata = new Dictionary<string, string>();

        Assert.Throws<ArgumentException>(
            () => AuditMetadata.TryAdd(metadata, "request_path", new string('x', 201)));
    }

    [Fact]
    public void BlankValue_IsIgnored()
    {
        var metadata = new Dictionary<string, string>();

        AuditMetadata.TryAdd(metadata, "reason", "   ");

        Assert.Empty(metadata);
    }

    [Fact]
    public void Serialize_Empty_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, AuditMetadata.Serialize(new Dictionary<string, string>()));
    }

    [Fact]
    public void Serialize_ProducesJsonWithOnlyAllowlistedKeys()
    {
        var metadata = new Dictionary<string, string>
        {
            ["request_method"] = "GET",
            ["request_path"] = "/v1/audit",
        };

        using JsonDocument document = JsonDocument.Parse(AuditMetadata.Serialize(metadata));

        Assert.Equal("GET", document.RootElement.GetProperty("request_method").GetString());
        Assert.Equal("/v1/audit", document.RootElement.GetProperty("request_path").GetString());
        Assert.Equal(2, document.RootElement.EnumerateObject().Count());
    }

    [Fact]
    public void Serialize_BoundaryValue_IsAccepted()
    {
        var metadata = new Dictionary<string, string>();

        AuditMetadata.TryAdd(metadata, "reason", new string('r', 200));

        string serialized = AuditMetadata.Serialize(metadata);
        Assert.Contains(new string('r', 200), serialized);
    }
}

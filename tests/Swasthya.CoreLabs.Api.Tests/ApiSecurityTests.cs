using System.Net;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Api.Tests;

[Trait("Category", TestCategory.Db)]
public sealed class ApiSecurityDbTests : IClassFixture<SwasthyaDbApiFactory>
{
    private readonly SwasthyaDbApiFactory _factory;

    public ApiSecurityDbTests(SwasthyaDbApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Anonymous_AuthContext_IsUnauthorized()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/v1/auth/context");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_Organizations_IsUnauthorized()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/v1/organizations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_Audit_IsUnauthorized()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/v1/audit");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TokenSignedWithWrongKey_IsUnauthorized()
    {
        using HttpClient client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/v1/organizations");
        request.Headers.TryAddWithoutValidation(
            "Authorization",
            "Bearer " + TestJwtDefaults.CreateToken(
                "subject", signingKeyBase64: TestJwtDefaults.AltSigningKeyBase64));

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TokenForUnknownIssuer_IsUnauthorized()
    {
        using HttpClient client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/v1/organizations");
        request.Headers.TryAddWithoutValidation(
            "Authorization",
            "Bearer " + TestJwtDefaults.CreateToken(
                "subject", issuer: "https://untrusted-issuer.test"));

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UnauthorizedResponse_DoesNotExposeInternalDetails()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/v1/organizations");
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("InnerException", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SigningKey", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ForbiddenResponse_DoesNotExposeInternalDetails()
    {
        using HttpClient client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/v1/audit?take=10");
        request.Headers.TryAddWithoutValidation(
            "Authorization",
            "Bearer " + TestJwtDefaults.CreateToken(TestJwtDefaults.SubjectUnknown));

        HttpResponseMessage response = await client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(string.Empty, body);
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("InnerException", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SigningKey", body, StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class ApiSecurityTests : IClassFixture<SwasthyaApiFactory>
{
    private readonly SwasthyaApiFactory _factory;

    public ApiSecurityTests(SwasthyaApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CorrelationId_Provided_IsEchoed()
    {
        const string correlation = "client-supplied-correlation-id";
        using HttpClient client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.TryAddWithoutValidation(
            CorrelationIdMiddlewareHeaderName, correlation);

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            correlation,
            response.Headers.GetValues(CorrelationIdMiddlewareHeaderName).Single());
    }

    [Fact]
    public async Task CorrelationId_Missing_IsGenerated()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string value = response.Headers.GetValues(CorrelationIdMiddlewareHeaderName).Single();
        Assert.True(Guid.TryParse(value, out _));
    }

    [Fact]
    public async Task CorrelationId_Invalid_IsReplaced()
    {
        using HttpClient client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.TryAddWithoutValidation(CorrelationIdMiddlewareHeaderName, "not valid !!");

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string value = response.Headers.GetValues(CorrelationIdMiddlewareHeaderName).Single();
        Assert.True(Guid.TryParse(value, out _));
        Assert.NotEqual("not valid !!", value);
    }

    [Fact]
    public async Task OpenApi_DevelopmentDocument_DescribesBearerScheme()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("securitySchemes", content);
        Assert.Contains("bearer", content, StringComparison.OrdinalIgnoreCase);
    }

    private const string CorrelationIdMiddlewareHeaderName = "X-Correlation-Id";
}

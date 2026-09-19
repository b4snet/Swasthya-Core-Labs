using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Swasthya.CoreLabs.Infrastructure.Persistence;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Api.Tests;

[Trait("Category", TestCategory.Db)]
public sealed class LaboratoryApiTests : IClassFixture<SwasthyaDbApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly SwasthyaDbApiFactory _factory;

    public LaboratoryApiTests(SwasthyaDbApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Catalog_Admin_ReturnsSeededMasterData()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/units");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UnitJson[] units = Deserialize<UnitJson[]>(content);
        Assert.Contains(units, u => u.Id == lab.UnitId);
    }

    [Fact]
    public async Task Tests_Admin_ListsAndGetsDetail()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage list = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/tests");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        TestSummaryJson[] tests = Deserialize<TestSummaryJson[]>(
            await list.Content.ReadAsStringAsync());
        Assert.Contains(tests, t => t.Code == lab.TestCode);

        HttpResponseMessage detail = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/tests/{lab.TestCode}");
        string detailContent = await detail.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        TestDetailJson test = Deserialize<TestDetailJson>(detailContent);
        Assert.Equal(lab.TestCode, test.Code);
        TestVersionJson version = Assert.Single(test.Versions);
        Assert.Equal(1, version.VersionNumber);
        Assert.DoesNotContain("xmin", detailContent);
        Assert.DoesNotContain("rowVersion", detailContent);
    }

    [Fact]
    public async Task Tests_Create_AsAdmin_ReturnsCreated()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        string code = $"api_{Guid.NewGuid():N}";
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/tests",
            new
            {
                code,
                name = "API-created test",
                resultDataTypeId = lab.ResultDataTypeId,
                disciplineId = lab.DisciplineId,
                unitId = lab.UnitId,
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        HttpResponseMessage detail = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/tests/{code}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
    }

    [Fact]
    public async Task Tests_Create_DuplicateCode_ReturnsConflict()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/tests",
            new
            {
                code = lab.TestCode,
                name = "Duplicate",
                resultDataTypeId = lab.ResultDataTypeId,
            });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Tests_Create_WithInvalidCode_ReturnsBadRequest()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/tests",
            new
            {
                code = "invalid code!",
                name = "Invalid",
                resultDataTypeId = lab.ResultDataTypeId,
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Tests_Create_AsFacilityScopedStaff_IsForbidden()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.StaffSubject));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/tests",
            new
            {
                code = $"staff_{Guid.NewGuid():N}",
                name = "Staff attempt",
                resultDataTypeId = lab.ResultDataTypeId,
            });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Tests_UnknownCode_ReturnsNotFound()
    {
        SeededTenant tenant = Seed();
        SeedLab(tenant);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/tests/no_such_code");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Panels_CreateWithMembership_ReusesTestConcept()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        string panelCode = $"panel_{Guid.NewGuid():N}";
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage created = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/panels",
            new
            {
                code = panelCode,
                name = "Metabolic panel",
                tests = new[]
                {
                    new { testCode = lab.TestCode, sequenceNumber = 1, displayText = "First" },
                },
            });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        HttpResponseMessage detail = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/panels/{panelCode}");
        string content = await detail.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        PanelDetailJson panel = Deserialize<PanelDetailJson>(content);
        PanelMembershipJson membership = Assert.Single(panel.Memberships);
        Assert.Equal(lab.TestCode, membership.TestCode);
        Assert.Equal(1, membership.SequenceNumber);
    }

    [Fact]
    public async Task ReferenceRanges_Create_ThenListedByTest()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage created = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/tests/{lab.TestCode}/reference-ranges",
            new
            {
                source = "provider verified worksheet",
                textualReference = "no reference interval configured",
            });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        HttpResponseMessage list = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/reference-ranges?testCode={lab.TestCode}");
        string content = await list.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        ReferenceRangeJson[] ranges = Deserialize<ReferenceRangeJson[]>(content);
        Assert.Contains(ranges, r => r.TestId == lab.TestId);
    }

    [Fact]
    public async Task ConfigurationItems_OrgWideVisibleToFacilityScopedStaff_FacilityScopedIsolated()
    {
        SeededTenant tenant = Seed();
        string key = $"lab.{Guid.NewGuid():N}.mode";
        using HttpClient admin = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage orgWide = await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/configuration-items",
            new { key, value = "standard", valueType = 0 });
        Assert.Equal(HttpStatusCode.Created, orgWide.StatusCode);

        HttpResponseMessage facilityB = await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/configuration-items",
            new
            {
                key = $"{key}.b",
                value = "facility-b",
                valueType = 0,
                facilityId = tenant.FacilityBId,
            });
        Assert.Equal(HttpStatusCode.Created, facilityB.StatusCode);

        using HttpClient staff = Authorized(TestJwtDefaults.CreateToken(tenant.StaffSubject));
        HttpResponseMessage list = await staff.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/configuration-items");
        string content = await list.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        ConfigurationItemJson[] items = Deserialize<ConfigurationItemJson[]>(content);
        Assert.Contains(items, i => i.Key == key);
        Assert.DoesNotContain(items, i => i.FacilityId == tenant.FacilityBId);
    }

    [Fact]
    public async Task ConfigurationItems_StaffWritingOtherFacility_IsForbidden()
    {
        SeededTenant tenant = Seed();
        using HttpClient staff = Authorized(TestJwtDefaults.CreateToken(tenant.StaffSubject));

        HttpResponseMessage response = await staff.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/configuration-items",
            new
            {
                key = $"lab.{Guid.NewGuid():N}.mode",
                value = "x",
                valueType = 0,
                facilityId = tenant.FacilityBId,
            });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ConfigurationItems_InvalidKey_ReturnsBadRequest()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/configuration-items",
            new { key = "Invalid.Key", value = "x", valueType = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Laboratory_CrossTenantAccess_IsForbidden()
    {
        SeededTenant first = Seed();
        SeededTenant second = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(first.AdminSubject));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{second.OrganizationId}/laboratory/units");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Laboratory_ForgedScopeClaims_DoNotExpandAccess()
    {
        SeededTenant tenant = Seed();
        SeededTenant other = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(
            tenant.StaffSubject,
            additionalClaims: new Dictionary<string, string>
            {
                ["organization_id"] = other.OrganizationId.ToString(),
                ["facility_id"] = other.FacilityBId.ToString(),
            }));

        HttpResponseMessage units = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/units");
        Assert.Equal(HttpStatusCode.OK, units.StatusCode);

        HttpResponseMessage crossTenant = await client.GetAsync(
            $"/v1/organizations/{other.OrganizationId}/laboratory/units");
        Assert.Equal(HttpStatusCode.Forbidden, crossTenant.StatusCode);

        HttpResponseMessage createForOtherTenant = await client.PostAsJsonAsync(
            $"/v1/organizations/{other.OrganizationId}/laboratory/configuration-items",
            new
            {
                key = $"lab.{Guid.NewGuid():N}.mode",
                value = "x",
                valueType = 0,
                facilityId = other.FacilityBId,
            });
        Assert.Equal(HttpStatusCode.Forbidden, createForOtherTenant.StatusCode);
    }

    [Fact]
    public async Task Laboratory_UnknownSubject_IsForbidden()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(
            TestJwtDefaults.CreateToken($"no-such-subject-{Guid.NewGuid():N}"));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/units");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Laboratory_WithoutToken_IsUnauthorized()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/units");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Laboratory_CatalogsAreEmptyForOrgWithoutMasterData()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/units");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", content.Trim());
    }

    [Fact]
    public async Task ConfigurationItems_Create_GeneratesAllowlistedAuditRecord()
    {
        SeededTenant tenant = Seed();
        using HttpClient admin = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string key = $"lab.{Guid.NewGuid():N}.mode";
        string value = $"value-{Guid.NewGuid():N}";

        HttpResponseMessage created = await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/laboratory/configuration-items",
            new { key, value, valueType = 0 });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        HttpResponseMessage audit = await admin.GetAsync("/v1/audit?take=100");
        string content = await audit.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, audit.StatusCode);
        Assert.Contains("\"action\":\"laboratory.created\"", content);
        Assert.Contains("\"resourceType\":\"laboratory.configuration_item\"", content);
        Assert.Contains($"\"resourceId\":\"{key}\"", content);
        Assert.DoesNotContain(value, content);
    }

    private SeededTenant Seed()
    {
        using CoreLabDbContext db = OpenContext();
        SeededTenant tenant = SeedSamples.Seed(db, $"{Guid.NewGuid():N}");
        SeedSamples.GrantLaboratoryPermissions(db, tenant);
        return tenant;
    }

    private SeededLaboratory SeedLab(SeededTenant tenant)
    {
        using CoreLabDbContext db = OpenContext();
        return SeedSamples.SeedLaboratory(db, tenant);
    }

    private CoreLabDbContext OpenContext() =>
        new DbTestContextFactory(_factory.ConnectionString).Create();

    private HttpClient Authorized(string token)
    {
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static T Deserialize<T>(string content) =>
        JsonSerializer.Deserialize<T>(content, JsonOptions)!;

    private sealed record UnitJson(Guid Id, string Code, string UcumCode);

    private sealed record TestSummaryJson(Guid Id, string Code, int CurrentVersionNumber);

    private sealed record TestVersionJson(int VersionNumber, Guid ResultDataTypeId);

    private sealed record TestDetailJson(
        Guid Id,
        string Code,
        IReadOnlyList<TestVersionJson> Versions);

    private sealed record PanelMembershipJson(
        Guid TestId,
        string TestCode,
        int SequenceNumber);

    private sealed record PanelDetailJson(
        Guid Id,
        string Code,
        IReadOnlyList<PanelMembershipJson> Memberships);

    private sealed record ReferenceRangeJson(Guid Id, Guid TestId, string Source);

    private sealed record ConfigurationItemJson(Guid Id, Guid? FacilityId, string Key);
}

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Infrastructure.Persistence;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Api.Tests;

[Trait("Category", TestCategory.Db)]
public sealed class IdentityTenancyApiTests : IClassFixture<SwasthyaDbApiFactory>
{
    private const string SeedRecordFacilityA = "seed.record.fa";
    private const string SeedRecordFacilityB = "seed.record.fb";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly SwasthyaDbApiFactory _factory;

    public IdentityTenancyApiTests(SwasthyaDbApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuthContext_Admin_ReportsFullOrgAccess()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.GetAsync("/v1/auth/context");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AuthContextJson context = Deserialize<AuthContextJson>(content);
        OrganizationJson organization = Assert.Single(context.Organizations);
        Assert.Equal(tenant.OrganizationId, organization.Id);
        Assert.True(organization.HasFullOrganizationAccess);
        Assert.Contains(tenant.FacilityAId, organization.AccessibleFacilityIds);
        Assert.Contains(tenant.FacilityBId, organization.AccessibleFacilityIds);
    }

    [Fact]
    public async Task AuthContext_Service_ReportsEmptyOrganizations()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(
            TestJwtDefaults.CreateToken(tenant.ServiceAuditorSubject, principalType: "service"));

        HttpResponseMessage response = await client.GetAsync("/v1/auth/context");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AuthContextJson context = Deserialize<AuthContextJson>(content);
        Assert.Empty(context.Organizations);
    }

    [Fact]
    public async Task Organizations_Admin_ReturnsGrantedOrganization()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.GetAsync("/v1/organizations");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<OrganizationJson> organizations = Deserialize<List<OrganizationJson>>(content);
        Assert.Contains(organizations, o => o.Id == tenant.OrganizationId);
    }

    [Fact]
    public async Task Organizations_Staff_ReportsFacilityScopedAccess()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.StaffSubject));

        HttpResponseMessage response = await client.GetAsync("/v1/organizations");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        OrganizationJson organization = Assert.Single(Deserialize<List<OrganizationJson>>(content));
        Assert.Equal(tenant.OrganizationId, organization.Id);
        Assert.False(organization.HasFullOrganizationAccess);
        Assert.Equal(tenant.FacilityAId, Assert.Single(organization.AccessibleFacilityIds));
        Assert.DoesNotContain(tenant.FacilityBId, organization.AccessibleFacilityIds);
    }

    [Fact]
    public async Task Facilities_Admin_ReturnsAllActiveFacilities()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/facilities");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<FacilityJson> facilities = Deserialize<List<FacilityJson>>(content);
        Assert.Equal(2, facilities.Count);
        Assert.Contains(facilities, f => f.Id == tenant.FacilityAId);
        Assert.Contains(facilities, f => f.Id == tenant.FacilityBId);
    }

    [Fact]
    public async Task Facilities_Staff_ReturnsOnlyGrantedFacility()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.StaffSubject));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/facilities");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        FacilityJson facility = Assert.Single(Deserialize<List<FacilityJson>>(content));
        Assert.Equal(tenant.FacilityAId, facility.Id);
    }

    [Fact]
    public async Task Facilities_CrossTenantOrganization_IsForbidden()
    {
        SeededTenant firstTenant = Seed();
        SeededTenant secondTenant = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(firstTenant.StaffSubject));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{secondTenant.OrganizationId}/facilities");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Audit_Admin_SeesRecordsAcrossFacilities()
    {
        SeededTenant tenant = Seed();
        SeedAuditRecords(tenant);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.GetAsync("/v1/audit?take=100");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AuditJson[] records = Deserialize<AuditJson[]>(content);
        Assert.Contains(records, r => r.Action == SeedRecordFacilityA);
        Assert.Contains(records, r => r.Action == SeedRecordFacilityB);
    }

    [Fact]
    public async Task Audit_Staff_SeesOnlyFacilityARecords()
    {
        SeededTenant tenant = Seed();
        SeedAuditRecords(tenant);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.StaffSubject));

        HttpResponseMessage response = await client.GetAsync("/v1/audit?take=100");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<AuditJson> records = Deserialize<List<AuditJson>>(content);
        Assert.Contains(records, r => r.Action == SeedRecordFacilityA);
        Assert.DoesNotContain(records, r => r.Action == SeedRecordFacilityB);
    }

    [Fact]
    public async Task Audit_UnknownSubject_IsForbidden()
    {
        using HttpClient client = Authorized(
            TestJwtDefaults.CreateToken("no-such-subject"));

        HttpResponseMessage response = await client.GetAsync("/v1/audit?take=10");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organizations_UnassignedPrincipal_IsForbidden()
    {
        string suffix = Guid.NewGuid().ToString("N");
        using (CoreLabDbContext db = OpenContext())
        {
            SeedSamples.CreateUnassignedPrincipal(db, suffix);
        }

        using HttpClient client = Authorized(
            TestJwtDefaults.CreateToken($"unassigned.{suffix}"));

        HttpResponseMessage response = await client.GetAsync("/v1/organizations");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ForgedTenantFacilityClaims_CannotExpandAccess()
    {
        SeededTenant tenant = Seed();
        SeededTenant otherTenant = Seed();
        SeedAuditRecords(tenant);
        string token = TestJwtDefaults.CreateToken(
            tenant.StaffSubject,
            additionalClaims: new Dictionary<string, string>
            {
                ["organization_id"] = otherTenant.OrganizationId.ToString(),
                ["facility_id"] = otherTenant.FacilityBId.ToString(),
                ["org"] = otherTenant.OrganizationId.ToString(),
                ["facility"] = tenant.FacilityBId.ToString(),
            });
        using HttpClient client = Authorized(token);

        HttpResponseMessage organizations = await client.GetAsync("/v1/organizations");
        string organizationsContent = await organizations.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, organizations.StatusCode);
        OrganizationJson organization = Assert.Single(
            Deserialize<List<OrganizationJson>>(organizationsContent));
        Assert.Equal(tenant.OrganizationId, organization.Id);
        Assert.Equal(tenant.FacilityAId, Assert.Single(organization.AccessibleFacilityIds));
        Assert.DoesNotContain(tenant.FacilityBId, organization.AccessibleFacilityIds);

        HttpResponseMessage crossTenant = await client.GetAsync(
            $"/v1/organizations/{otherTenant.OrganizationId}/facilities");
        Assert.Equal(HttpStatusCode.Forbidden, crossTenant.StatusCode);

        HttpResponseMessage audit = await client.GetAsync("/v1/audit?take=100");
        string auditContent = await audit.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, audit.StatusCode);
        List<AuditJson> records = Deserialize<List<AuditJson>>(auditContent);
        Assert.Contains(records, r => r.Action == SeedRecordFacilityA);
        Assert.DoesNotContain(records, r => r.Action == SeedRecordFacilityB);
    }

    [Fact]
    public async Task ServicePrincipal_FacilityScopedGrant_EnforcesSameScope()
    {
        SeededTenant tenant = Seed();
        string subject = $"svc-scope.{Guid.NewGuid():N}";
        using (CoreLabDbContext db = OpenContext())
        {
            var service = new Principal(
                PrincipalType.Service, TestJwtDefaults.Issuer, subject, "Scope-check service");
            var role = new Role(
                tenant.OrganizationId, $"svc-role-{Guid.NewGuid():N}", "Service role");
            db.Principals.Add(service);
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            db.RolePermissions.Add(new RolePermission(
                role.Id, PermissionCatalog.Ensure(db)[Permissions.FacilityList]));
            db.PrincipalRoleAssignments.Add(new PrincipalRoleAssignment(
                service.Id, role.Id, tenant.OrganizationId, tenant.FacilityAId));
            await db.SaveChangesAsync();
        }

        using HttpClient client = Authorized(
            TestJwtDefaults.CreateToken(subject, principalType: "service"));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/facilities");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        FacilityJson facility = Assert.Single(Deserialize<List<FacilityJson>>(content));
        Assert.Equal(tenant.FacilityAId, facility.Id);
    }

    [Fact]
    public async Task Facilities_NonGuidRouteSuffix_ReturnsNotFound()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/not-a-guid/facilities");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Facilities_UnknownOrganizationGuid_IsForbidden()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{Guid.NewGuid()}/facilities");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TokenMissingSubject_IsDenied()
    {
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(string.Empty));

        HttpResponseMessage response = await client.GetAsync("/v1/organizations");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Audit_MalformedPagination_IsClampedRatherThanErroring()
    {
        SeededTenant tenant = Seed();
        SeedAuditRecords(tenant);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.GetAsync("/v1/audit?skip=-5&take=0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private SeededTenant Seed()
    {
        using CoreLabDbContext db = OpenContext();
        return SeedSamples.Seed(db, $"{Guid.NewGuid():N}");
    }

    private void SeedAuditRecords(SeededTenant tenant)
    {
        using CoreLabDbContext db = OpenContext();
        SeedSamples.AddAuditRecord(
            db, tenant.OrganizationId, tenant.FacilityAId,
            tenant.AdminPrincipalId, null, SeedRecordFacilityA, $"corr-{Guid.NewGuid():N}");
        SeedSamples.AddAuditRecord(
            db, tenant.OrganizationId, tenant.FacilityBId,
            tenant.AdminPrincipalId, null, SeedRecordFacilityB, $"corr-{Guid.NewGuid():N}");
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

    private sealed record AuthContextJson(
        Guid PrincipalId,
        string ExternalSubject,
        IReadOnlyList<OrganizationJson> Organizations);

    private sealed record OrganizationJson(
        Guid Id,
        string Name,
        bool HasFullOrganizationAccess,
        IReadOnlyList<Guid> AccessibleFacilityIds);

    private sealed record FacilityJson(Guid Id, Guid OrganizationId, string Code, string Name);

    private sealed record AuditJson(Guid Id, Guid? FacilityId, string Action);
}

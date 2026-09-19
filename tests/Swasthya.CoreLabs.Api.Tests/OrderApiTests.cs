using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Swasthya.CoreLabs.Domain.Laboratory;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Infrastructure.Persistence;
using Swasthya.CoreLabs.TestSupport;

namespace Swasthya.CoreLabs.Api.Tests;

[Trait("Category", TestCategory.Db)]
public sealed class OrderApiTests : IClassFixture<SwasthyaDbApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly SwasthyaDbApiFactory _factory;

    public OrderApiTests(SwasthyaDbApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Orders_Create_AsAdmin_ReturnsCreatedWithItems()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string orderNumber = $"ORD-{Guid.NewGuid():N}";

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber,
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
                priority = 1,
                items = new[]
                {
                    new { testCode = lab.TestCode },
                },
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        OrderJson order = Deserialize<OrderJson>(await response.Content.ReadAsStringAsync());
        Assert.Equal(orderNumber, order.OrderNumber);
        Assert.Equal(OrderPriority.Urgent, order.Priority);
        Assert.Equal(OrderStatus.Requested, order.Status);
        OrderItemJson item = Assert.Single(order.Items);
        Assert.Equal(lab.TestCode, item.TestCode);
        Assert.NotNull(item.TestVersionId);
        Assert.Null(item.PanelId);
    }

    [Fact]
    public async Task Orders_Create_ReplaySameExternalOrderId_ReturnsOk()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string externalId = $"ext-{Guid.NewGuid():N}";

        object body = new
        {
            orderNumber = $"ORD-{Guid.NewGuid():N}",
            facilityId = tenant.FacilityAId,
            patientExternalSystem = "emr.test",
            patientExternalIdentifier = "subject-42",
            externalOrderId = externalId,
            items = new[] { new { testCode = lab.TestCode } },
        };

        HttpResponseMessage first = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders", body);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        HttpResponseMessage replay = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders", body);
        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
    }

    [Fact]
    public async Task Orders_Create_DuplicateOrderNumber_ReturnsConflict()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string orderNumber = $"ORD-{Guid.NewGuid():N}";

        await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber,
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
                items = new[] { new { testCode = lab.TestCode } },
            });

        HttpResponseMessage duplicate = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber,
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
                items = new[] { new { testCode = lab.TestCode } },
            });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Orders_Create_ByStaffForOwnFacility_ReturnsCreated()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.StaffSubject));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber = $"ORD-{Guid.NewGuid():N}",
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
                items = new[] { new { testCode = lab.TestCode } },
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Orders_Create_ByStaffForOtherFacility_IsForbidden()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.StaffSubject));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber = $"ORD-{Guid.NewGuid():N}",
                facilityId = tenant.FacilityBId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
                items = new[] { new { testCode = lab.TestCode } },
            });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Orders_Create_WithBlankPatientReference_ReturnsBadRequest()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber = $"ORD-{Guid.NewGuid():N}",
                facilityId = tenant.FacilityAId,
                patientExternalSystem = " ",
                patientExternalIdentifier = "subject-42",
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Orders_Create_WithInactiveTest_ReturnsBadRequest()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber = $"ORD-{Guid.NewGuid():N}",
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
                items = new[] { new { testCode = lab.TestCode } },
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Orders_Create_WithUnknownTest_ReturnsBadRequest()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber = $"ORD-{Guid.NewGuid():N}",
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
                items = new[] { new { testCode = "no_such_test" } },
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Orders_Create_WithTestNotOfferedAtFacility_ReturnsBadRequest()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using (CoreLabDbContext db = OpenContext())
        {
            db.TestFacilities.Add(new TestFacility(lab.TestId, tenant.FacilityBId));
            db.SaveChanges();
        }

        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber = $"ORD-{Guid.NewGuid():N}",
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
                items = new[] { new { testCode = lab.TestCode } },
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Orders_Create_WithPanel_ReturnsCreated()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        SeededPanel panel = SeedPanel(tenant, lab);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber = $"ORD-{Guid.NewGuid():N}",
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
                items = new[] { new { panelCode = panel.PanelCode } },
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        OrderJson order = Deserialize<OrderJson>(await response.Content.ReadAsStringAsync());
        OrderItemJson item = Assert.Single(order.Items);
        Assert.Equal(panel.PanelCode, item.PanelCode);
        Assert.NotNull(item.PanelVersionId);
        Assert.Null(item.TestId);
    }

    [Fact]
    public async Task Orders_Create_ItemWithBothTestAndPanel_ReturnsBadRequest()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        SeededPanel panel = SeedPanel(tenant, lab);
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber = $"ORD-{Guid.NewGuid():N}",
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
                items = new[]
                {
                    new { testCode = lab.TestCode, panelCode = panel.PanelCode },
                },
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Orders_List_AsAdmin_ReturnsAllFacilities()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient admin = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string orderNumber = $"ORD-{Guid.NewGuid():N}";

        await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber,
                facilityId = tenant.FacilityBId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
            });

        HttpResponseMessage list = await admin.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders");
        string content = await list.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        OrderJson[] orders = Deserialize<OrderJson[]>(content);
        Assert.Contains(orders, o => o.OrderNumber == orderNumber);
    }

    [Fact]
    public async Task Orders_List_AsStaff_OnlySeesOwnFacility()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient admin = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string facilityAOrder = $"ORD-{Guid.NewGuid():N}";
        string facilityBOrder = $"ORD-{Guid.NewGuid():N}";

        await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber = facilityAOrder,
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
            });
        await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber = facilityBOrder,
                facilityId = tenant.FacilityBId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
            });

        using HttpClient staff = Authorized(TestJwtDefaults.CreateToken(tenant.StaffSubject));
        HttpResponseMessage list = await staff.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders");
        string content = await list.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        OrderJson[] orders = Deserialize<OrderJson[]>(content);
        Assert.Contains(orders, o => o.OrderNumber == facilityAOrder);
        Assert.DoesNotContain(orders, o => o.OrderNumber == facilityBOrder);
    }

    [Fact]
    public async Task Orders_Get_ReturnsDetailForOwnFacility()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient admin = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string orderNumber = $"ORD-{Guid.NewGuid():N}";
        string patientIdentifier = $"subject-{Guid.NewGuid():N}";

        HttpResponseMessage created = await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber,
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = patientIdentifier,
                items = new[] { new { testCode = lab.TestCode } },
            });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        HttpResponseMessage detail = await admin.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders/{orderNumber}");
        string content = await detail.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        OrderJson order = Deserialize<OrderJson>(content);
        Assert.Equal(patientIdentifier, order.PatientExternalIdentifier);
        Assert.DoesNotContain("xmin", content);
    }

    [Fact]
    public async Task Orders_Get_UnknownNumber_ReturnsNotFound()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders/NO_SUCH_ORD");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Orders_Get_AsStaffForOtherFacility_IsForbidden()
    {
        SeededTenant tenant = Seed();
        using HttpClient admin = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string orderNumber = $"ORD-{Guid.NewGuid():N}";

        await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber,
                facilityId = tenant.FacilityBId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
            });

        using HttpClient staff = Authorized(TestJwtDefaults.CreateToken(tenant.StaffSubject));
        HttpResponseMessage response = await staff.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders/{orderNumber}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Orders_Update_ChangesRequestDetails()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient admin = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string orderNumber = $"ORD-{Guid.NewGuid():N}";

        HttpResponseMessage created = await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber,
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
            });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        HttpResponseMessage updated = await admin.PutAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders/{orderNumber}",
            new
            {
                priority = 2,
                encounterExternalSystem = "emr.test",
                encounterExternalIdentifier = "encounter-7",
            });
        string content = await updated.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        OrderJson order = Deserialize<OrderJson>(content);
        Assert.Equal(OrderPriority.Stat, order.Priority);
        Assert.Equal("encounter-7", order.EncounterExternalIdentifier);
    }

    [Fact]
    public async Task Orders_Update_AfterCancel_ReturnsBadRequest()
    {
        SeededTenant tenant = Seed();
        using HttpClient admin = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string orderNumber = $"ORD-{Guid.NewGuid():N}";

        HttpResponseMessage created = await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber,
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
            });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        HttpResponseMessage cancelled = await admin.PostAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders/{orderNumber}/transitions/cancel",
            null);
        Assert.Equal(HttpStatusCode.OK, cancelled.StatusCode);

        HttpResponseMessage updated = await admin.PutAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders/{orderNumber}",
            new { priority = 2 });

        Assert.Equal(HttpStatusCode.BadRequest, updated.StatusCode);
    }

    [Fact]
    public async Task Orders_CancelItem_MarksLineItemCancelled()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient admin = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string orderNumber = $"ORD-{Guid.NewGuid():N}";

        HttpResponseMessage created = await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber,
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
                items = new[]
                {
                    new { testCode = lab.TestCode },
                    new { testCode = lab.TestCode },
                },
            });
        string createdContent = await created.Content.ReadAsStringAsync();
        OrderJson order = Deserialize<OrderJson>(createdContent);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(2, order.Items.Count);
        Guid firstItem = order.Items[0].Id;

        HttpResponseMessage cancelled = await admin.PostAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders/{orderNumber}/items/{firstItem}/transitions/cancel",
            null);
        string content = await cancelled.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, cancelled.StatusCode);
        OrderJson after = Deserialize<OrderJson>(content);
        Assert.Contains(after.Items, i => i.Id == firstItem && i.Status == OrderItemStatus.Cancelled);
        Assert.Equal(OrderStatus.Requested, after.Status);
    }

    [Fact]
    public async Task Orders_Cancel_CancelsOrderAndRequestedItems()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient admin = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string orderNumber = $"ORD-{Guid.NewGuid():N}";

        HttpResponseMessage created = await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber,
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = "subject-42",
                items = new[]
                {
                    new { testCode = lab.TestCode },
                    new { testCode = lab.TestCode },
                },
            });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        HttpResponseMessage cancelled = await admin.PostAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders/{orderNumber}/transitions/cancel",
            null);
        string content = await cancelled.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, cancelled.StatusCode);
        OrderJson order = Deserialize<OrderJson>(content);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.All(order.Items, i => Assert.Equal(OrderItemStatus.Cancelled, i.Status));
    }

    [Fact]
    public async Task Orders_CrossTenant_IsForbidden()
    {
        SeededTenant first = Seed();
        SeededTenant second = Seed();
        using HttpClient client = Authorized(TestJwtDefaults.CreateToken(first.AdminSubject));

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{second.OrganizationId}/orders");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Orders_WithoutToken_IsUnauthorized()
    {
        SeededTenant tenant = Seed();
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Orders_Create_GeneratesAllowlistedAuditWithoutPatientReference()
    {
        SeededTenant tenant = Seed();
        SeededLaboratory lab = SeedLab(tenant);
        ActivateTest(tenant, lab);
        using HttpClient admin = Authorized(TestJwtDefaults.CreateToken(tenant.AdminSubject));
        string patientIdentifier = $"subject-{Guid.NewGuid():N}";

        HttpResponseMessage created = await admin.PostAsJsonAsync(
            $"/v1/organizations/{tenant.OrganizationId}/orders",
            new
            {
                orderNumber = $"ORD-{Guid.NewGuid():N}",
                facilityId = tenant.FacilityAId,
                patientExternalSystem = "emr.test",
                patientExternalIdentifier = patientIdentifier,
                items = new[] { new { testCode = lab.TestCode } },
            });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        HttpResponseMessage audit = await admin.GetAsync("/v1/audit?take=100");
        string content = await audit.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, audit.StatusCode);
        Assert.Contains("\"action\":\"laboratory.order.created\"", content);
        Assert.Contains("\"resourceType\":\"laboratory.order\"", content);
        Assert.DoesNotContain(patientIdentifier, content);
        Assert.DoesNotContain("subject-42", content);
    }

    private SeededTenant Seed()
    {
        using CoreLabDbContext db = OpenContext();
        SeededTenant tenant = SeedSamples.Seed(db, $"{Guid.NewGuid():N}");
        SeedSamples.GrantLaboratoryPermissions(db, tenant);
        SeedSamples.GrantOrderPermissions(db, tenant);
        return tenant;
    }

    private SeededLaboratory SeedLab(SeededTenant tenant)
    {
        using CoreLabDbContext db = OpenContext();
        return SeedSamples.SeedLaboratory(db, tenant);
    }

    private void ActivateTest(SeededTenant tenant, SeededLaboratory lab)
    {
        using CoreLabDbContext db = OpenContext();
        SeedSamples.ActivateSeededTest(db, lab);
    }

    private SeededPanel SeedPanel(SeededTenant tenant, SeededLaboratory lab)
    {
        using CoreLabDbContext db = OpenContext();
        return SeedSamples.SeedActivePanel(db, tenant, lab);
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

    private sealed record OrderItemJson(
        Guid Id,
        int SequenceNumber,
        OrderItemStatus Status,
        OrderPriority? Priority,
        Guid? TestId,
        Guid? PanelId,
        Guid? TestVersionId,
        Guid? PanelVersionId,
        string? TestCode,
        string? TestName,
        string? PanelCode,
        string? PanelName,
        decimal? RequestedQuantity,
        string? QuantityUnitUcumCode);

    private sealed record OrderJson(
        Guid Id,
        string OrderNumber,
        Guid FacilityId,
        string PatientExternalSystem,
        string PatientExternalIdentifier,
        string? EncounterExternalIdentifier,
        OrderPriority Priority,
        OrderStatus Status,
        IReadOnlyList<OrderItemJson> Items);
}

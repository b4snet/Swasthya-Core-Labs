using Microsoft.AspNetCore.Http;
using Swasthya.CoreLabs.Api.Security;
using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Application.Services;

namespace Swasthya.CoreLabs.Api.Endpoints;

public static class LaboratoryEndpoints
{
    public static IEndpointRouteBuilder MapLaboratoryEndpoints(
        this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup("/v1/organizations/{organizationId:guid}/laboratory")
            .WithTags("Laboratory");

        group.AddEndpointFilter(async (context, next) =>
        {
            object? result = await next(context);
            var unitOfWork = context.HttpContext.RequestServices
                .GetRequiredService<IUnitOfWork>();
            await unitOfWork.SaveChangesAsync(context.HttpContext.RequestAborted);
            return result;
        });

        group.RequireAuthorization(IdentityTenancyEndpoints.Policy);

        MapOrganizationReads(group);
        MapCatalogReads(group);
        MapTestEndpoints(group);
        MapPanelEndpoints(group);
        MapReferenceRangeEndpoints(group);
        MapConfigurationItemEndpoints(group);
        MapResultEndpoints(group);

        return app;
    }

    private static void MapOrganizationReads(RouteGroupBuilder group)
    {
        group.MapGet("/disciplines", async (
                HttpContext httpContext,
                Guid organizationId,
                LaboratoryOrganizationQueryService service,
                CancellationToken cancellationToken) =>
                await service.ListDisciplinesAsync(
                    httpContext.GetAuthContext(), organizationId, cancellationToken));

        group.MapGet("/sections", async (
                HttpContext httpContext,
                Guid organizationId,
                LaboratoryOrganizationQueryService service,
                CancellationToken cancellationToken,
                Guid? facilityId = null) =>
                await service.ListSectionsAsync(
                    httpContext.GetAuthContext(), organizationId, facilityId, cancellationToken));
    }

    private static void MapCatalogReads(RouteGroupBuilder group)
    {
        group.MapGet("/result-data-types", async (
                HttpContext httpContext,
                Guid organizationId,
                LaboratoryCatalogQueryService service,
                CancellationToken cancellationToken) =>
                await service.ListResultDataTypesAsync(
                    httpContext.GetAuthContext(), organizationId, cancellationToken));

        group.MapGet("/units", async (
                HttpContext httpContext,
                Guid organizationId,
                LaboratoryCatalogQueryService service,
                CancellationToken cancellationToken) =>
                await service.ListUnitsAsync(
                    httpContext.GetAuthContext(), organizationId, cancellationToken));

        group.MapGet("/code-systems", async (
                HttpContext httpContext,
                Guid organizationId,
                LaboratoryCatalogQueryService service,
                CancellationToken cancellationToken) =>
                await service.ListCodeSystemsAsync(
                    httpContext.GetAuthContext(), organizationId, cancellationToken));

        group.MapGet("/specimen-types", async (
                HttpContext httpContext,
                Guid organizationId,
                LaboratoryCatalogQueryService service,
                CancellationToken cancellationToken) =>
                await service.ListSpecimenTypesAsync(
                    httpContext.GetAuthContext(), organizationId, cancellationToken));

        group.MapGet("/specimen-sources", async (
                HttpContext httpContext,
                Guid organizationId,
                LaboratoryCatalogQueryService service,
                CancellationToken cancellationToken) =>
                await service.ListSpecimenSourcesAsync(
                    httpContext.GetAuthContext(), organizationId, cancellationToken));

        group.MapGet("/container-types", async (
                HttpContext httpContext,
                Guid organizationId,
                LaboratoryCatalogQueryService service,
                CancellationToken cancellationToken) =>
                await service.ListContainerTypesAsync(
                    httpContext.GetAuthContext(), organizationId, cancellationToken));

        group.MapGet("/result-statuses", async (
                HttpContext httpContext,
                Guid organizationId,
                LaboratoryCatalogQueryService service,
                CancellationToken cancellationToken) =>
                await service.ListResultStatusesAsync(
                    httpContext.GetAuthContext(), organizationId, cancellationToken));

        group.MapGet("/analyzers", async (
                HttpContext httpContext,
                Guid organizationId,
                LaboratoryCatalogQueryService service,
                CancellationToken cancellationToken,
                Guid? facilityId = null) =>
                await service.ListAnalyzersAsync(
                    httpContext.GetAuthContext(), organizationId, facilityId, cancellationToken));

        group.MapGet("/configuration-items", async (
                HttpContext httpContext,
                Guid organizationId,
                LaboratoryCatalogQueryService service,
                CancellationToken cancellationToken,
                Guid? facilityId = null) =>
                await service.ListConfigurationItemsAsync(
                    httpContext.GetAuthContext(), organizationId, facilityId, cancellationToken));
    }

    private static void MapTestEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/tests", async (
                HttpContext httpContext,
                Guid organizationId,
                TestCatalogService service,
                CancellationToken cancellationToken,
                Guid? facilityId = null) =>
                await service.ListTestsAsync(
                    httpContext.GetAuthContext(), organizationId, facilityId, cancellationToken));

        group.MapGet("/tests/{code}", async (
                HttpContext httpContext,
                Guid organizationId,
                string code,
                TestCatalogService service,
                CancellationToken cancellationToken) =>
                await service.GetTestAsync(
                    httpContext.GetAuthContext(), organizationId, code, cancellationToken));

        group.MapPost("/tests", async (
                HttpContext httpContext,
                Guid organizationId,
                CreateTestRequest request,
                TestCatalogService service,
                CancellationToken cancellationToken) =>
                Results.Created(
                    $"/v1/organizations/{organizationId}/laboratory/tests/{request?.Code}",
                    await service.CreateTestAsync(
                        httpContext.GetAuthContext(), organizationId, request!, cancellationToken)));

        group.MapPost("/tests/{code}/versions", async (
                HttpContext httpContext,
                Guid organizationId,
                string code,
                CreateTestVersionRequest request,
                TestCatalogService service,
                CancellationToken cancellationToken) =>
                await service.PublishVersionAsync(
                    httpContext.GetAuthContext(), organizationId, code, request!, cancellationToken));

        group.MapPost("/tests/{code}/transitions/{transition}", async (
                HttpContext httpContext,
                Guid organizationId,
                string code,
                string transition,
                TestCatalogService service,
                CancellationToken cancellationToken) =>
                await service.TransitionAsync(
                    httpContext.GetAuthContext(), organizationId, code, transition, cancellationToken));

        group.MapPut("/tests/{code}/specimen-requirements", async (
                HttpContext httpContext,
                Guid organizationId,
                string code,
                ReplaceSpecimenRequirementsRequest request,
                TestCatalogService service,
                CancellationToken cancellationToken) =>
                await service.ReplaceSpecimenRequirementsAsync(
                    httpContext.GetAuthContext(), organizationId, code, request!, cancellationToken));

        group.MapPut("/tests/{code}/terminology-mappings", async (
                HttpContext httpContext,
                Guid organizationId,
                string code,
                ReplaceTerminologyMappingsRequest request,
                TestCatalogService service,
                CancellationToken cancellationToken) =>
                await service.ReplaceTerminologyMappingsAsync(
                    httpContext.GetAuthContext(), organizationId, code, request!, cancellationToken));

        group.MapPut("/tests/{code}/facilities", async (
                HttpContext httpContext,
                Guid organizationId,
                string code,
                ReplaceTestFacilitiesRequest request,
                TestCatalogService service,
                CancellationToken cancellationToken) =>
                await service.ReplaceFacilitiesAsync(
                    httpContext.GetAuthContext(), organizationId, code, request!, cancellationToken));
    }

    private static void MapPanelEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/panels", async (
                HttpContext httpContext,
                Guid organizationId,
                PanelService service,
                CancellationToken cancellationToken) =>
                await service.ListPanelsAsync(
                    httpContext.GetAuthContext(), organizationId, cancellationToken));

        group.MapGet("/panels/{code}", async (
                HttpContext httpContext,
                Guid organizationId,
                string code,
                PanelService service,
                CancellationToken cancellationToken) =>
                await service.GetPanelAsync(
                    httpContext.GetAuthContext(), organizationId, code, cancellationToken));

        group.MapPost("/panels", async (
                HttpContext httpContext,
                Guid organizationId,
                CreatePanelRequest request,
                PanelService service,
                CancellationToken cancellationToken) =>
                Results.Created(
                    $"/v1/organizations/{organizationId}/laboratory/panels/{request?.Code}",
                    await service.CreatePanelAsync(
                        httpContext.GetAuthContext(), organizationId, request!, cancellationToken)));

        group.MapPost("/panels/{code}/versions", async (
                HttpContext httpContext,
                Guid organizationId,
                string code,
                CreatePanelVersionRequest request,
                PanelService service,
                CancellationToken cancellationToken) =>
                await service.PublishVersionAsync(
                    httpContext.GetAuthContext(), organizationId, code, request!, cancellationToken));

        group.MapPost("/panels/{code}/transitions/{transition}", async (
                HttpContext httpContext,
                Guid organizationId,
                string code,
                string transition,
                PanelService service,
                CancellationToken cancellationToken) =>
                await service.TransitionAsync(
                    httpContext.GetAuthContext(), organizationId, code, transition, cancellationToken));

        group.MapPut("/panels/{code}/memberships", async (
                HttpContext httpContext,
                Guid organizationId,
                string code,
                ReplacePanelMembershipsRequest request,
                PanelService service,
                CancellationToken cancellationToken) =>
                await service.ReplaceMembershipsAsync(
                    httpContext.GetAuthContext(), organizationId, code, request!, cancellationToken));
    }

    private static void MapReferenceRangeEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/reference-ranges", async (
                HttpContext httpContext,
                Guid organizationId,
                ReferenceRangeService service,
                CancellationToken cancellationToken,
                Guid? facilityId = null,
                string? testCode = null) =>
                await service.ListAsync(
                    httpContext.GetAuthContext(),
                    organizationId,
                    facilityId,
                    testCode,
                    cancellationToken));

        group.MapPost("/tests/{code}/reference-ranges", async (
                HttpContext httpContext,
                Guid organizationId,
                string code,
                CreateReferenceRangeRequest request,
                ReferenceRangeService service,
                CancellationToken cancellationToken) =>
                Results.Created(
                    $"/v1/organizations/{organizationId}/laboratory/tests/{code}/reference-ranges",
                    await service.CreateAsync(
                        httpContext.GetAuthContext(), organizationId, code, request!, cancellationToken)));

        group.MapPost("/reference-ranges/{referenceRangeId:guid}/transitions/{transition}", async (
                HttpContext httpContext,
                Guid organizationId,
                Guid referenceRangeId,
                string transition,
                ReferenceRangeService service,
                CancellationToken cancellationToken) =>
                await service.TransitionAsync(
                    httpContext.GetAuthContext(),
                    organizationId,
                    referenceRangeId,
                    transition,
                    cancellationToken));
    }

    private static void MapConfigurationItemEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/configuration-items", async (
                HttpContext httpContext,
                Guid organizationId,
                CreateConfigurationItemRequest request,
                ConfigurationItemService service,
                CancellationToken cancellationToken) =>
                Results.Created(
                    $"/v1/organizations/{organizationId}/laboratory/configuration-items/{request?.Key}",
                    await service.CreateAsync(
                        httpContext.GetAuthContext(), organizationId, request!, cancellationToken)));

        group.MapPut("/configuration-items/{key}", async (
                HttpContext httpContext,
                Guid organizationId,
                string key,
                UpdateConfigurationItemRequest request,
                ConfigurationItemService service,
                CancellationToken cancellationToken,
                Guid? facilityId = null) =>
                await service.UpdateAsync(
                    httpContext.GetAuthContext(),
                    organizationId,
                    facilityId,
                    key,
                    request!,
                    cancellationToken));

        group.MapPost("/configuration-items/{key}/transitions/{transition}", async (
                HttpContext httpContext,
                Guid organizationId,
                string key,
                string transition,
                ConfigurationItemService service,
                CancellationToken cancellationToken,
                Guid? facilityId = null) =>
                await service.TransitionAsync(
                    httpContext.GetAuthContext(),
                    organizationId,
                    facilityId,
                    key,
                    transition,
                    cancellationToken));
    }

    private static void MapResultEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/results", async (
                HttpContext httpContext,
                Guid organizationId,
                ResultCreateRequest request,
                ResultService service,
                CancellationToken cancellationToken) =>
                Results.Created(
                    $"/v1/organizations/{organizationId}/laboratory/results",
                    await service.CreateAsync(organizationId, request!, cancellationToken)));

        group.MapGet("/results", async (
                HttpContext httpContext,
                Guid organizationId,
                ResultService service,
                CancellationToken cancellationToken,
                Guid? facilityId = null,
                Guid? orderItemId = null,
                Guid? specimenId = null,
                Guid? testVersionId = null) =>
                await service.ListAsync(
                    organizationId,
                    facilityId,
                    orderItemId,
                    specimenId,
                    testVersionId,
                    cancellationToken));

        group.MapGet("/results/{resultId:guid}", async (
                HttpContext httpContext,
                Guid organizationId,
                Guid resultId,
                ResultService service,
                CancellationToken cancellationToken) =>
                await service.GetAsync(organizationId, resultId, cancellationToken));

        group.MapPut("/results/{resultId:guid}", async (
                HttpContext httpContext,
                Guid organizationId,
                Guid resultId,
                UpdateResultRequest request,
                ResultService service,
                CancellationToken cancellationToken) =>
                await service.UpdateAsync(organizationId, resultId, request!, cancellationToken));
    }
}

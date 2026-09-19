using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Authorization;
using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.IdentityTenancy;
using Swasthya.CoreLabs.Domain.Laboratory;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class ConfigurationItemService(
    ILaboratoryCatalogRepository catalogRepository,
    IFacilityRepository facilityRepository,
    IAuditRepository auditRepository,
    ICorrelationIdProvider correlationIdProvider)
{
    public async Task<ConfigurationItemDto> CreateAsync(
        AuthContext context,
        Guid organizationId,
        CreateConfigurationItemRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ValidationException("A request body is required.");
        }

        RequireWriteAccess(context, organizationId, request.FacilityId);
        await RequireFacilityAsync(organizationId, request.FacilityId, cancellationToken);

        ConfigurationItem item;
        try
        {
            item = new ConfigurationItem(
                organizationId,
                request.FacilityId,
                request.Key ?? string.Empty,
                request.Value ?? string.Empty,
                request.ValueType,
                request.Description);
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message);
        }

        if (await catalogRepository.FindConfigurationItemAsync(
                organizationId, item.FacilityId, item.Key, cancellationToken) is not null)
        {
            throw new ConflictException(
                $"Configuration item '{item.Key}' already exists for this scope.");
        }

        await catalogRepository.AddConfigurationItemAsync(item, cancellationToken);
        await WriteAuditAsync(
            context,
            organizationId,
            item.FacilityId,
            AuditActions.LaboratoryCreated,
            item.Key,
            cancellationToken);

        return item.ToDto();
    }

    public async Task<ConfigurationItemDto> UpdateAsync(
        AuthContext context,
        Guid organizationId,
        Guid? facilityId,
        string key,
        UpdateConfigurationItemRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ValidationException("A request body is required.");
        }

        RequireWriteAccess(context, organizationId, facilityId);

        ConfigurationItem item = await RequireItemAsync(
            organizationId, facilityId, key, cancellationToken);

        item.Update(request.Value ?? string.Empty, request.ValueType, request.Description);
        await WriteAuditAsync(
            context,
            organizationId,
            item.FacilityId,
            AuditActions.LaboratoryUpdated,
            item.Key,
            cancellationToken);

        return item.ToDto();
    }

    public async Task<ConfigurationItemDto> TransitionAsync(
        AuthContext context,
        Guid organizationId,
        Guid? facilityId,
        string key,
        string transition,
        CancellationToken cancellationToken)
    {
        RequireWriteAccess(context, organizationId, facilityId);

        MasterDataStatus target = transition switch
        {
            "activate" => MasterDataStatus.Active,
            "deactivate" => MasterDataStatus.Inactive,
            "retire" => MasterDataStatus.Retired,
            _ => throw new ValidationException(
                $"Unknown transition '{transition}'. Expected 'activate', 'deactivate' or 'retire'."),
        };

        ConfigurationItem item = await RequireItemAsync(
            organizationId, facilityId, key, cancellationToken);

        if (item.Status == target)
        {
            throw new ValidationException($"Configuration item is already '{target}'.");
        }

        if (!MasterDataLifecycle.CanTransition(item.Status, target))
        {
            throw new ValidationException(
                $"Configuration item cannot transition from '{item.Status}' to '{target}'.");
        }

        item.SetStatus(target);

        string action = target switch
        {
            MasterDataStatus.Active => AuditActions.LaboratoryActivated,
            MasterDataStatus.Inactive => AuditActions.LaboratoryDeactivated,
            _ => AuditActions.LaboratoryRetired,
        };

        await WriteAuditAsync(
            context,
            organizationId,
            item.FacilityId,
            action,
            item.Key,
            cancellationToken);

        return item.ToDto();
    }

    private static void RequireWriteAccess(
        AuthContext context,
        Guid organizationId,
        Guid? facilityId)
    {
        if (facilityId is { } facility)
        {
            LaboratoryPermissionGuard.RequireFacilityScoped(
                context, Permissions.LaboratoryConfigWrite, organizationId, facility);
            return;
        }

        LaboratoryPermissionGuard.RequireOrganizationWide(
            context, Permissions.LaboratoryConfigWrite, organizationId);
    }

    private async Task RequireFacilityAsync(
        Guid organizationId,
        Guid? facilityId,
        CancellationToken cancellationToken)
    {
        if (facilityId is not { } id)
        {
            return;
        }

        IReadOnlyCollection<Facility> facilities = await facilityRepository
            .ListByOrganizationIdsAsync([organizationId], cancellationToken);
        if (facilities.All(f => f.Id != id))
        {
            throw new ValidationException(
                $"Facility '{id}' does not belong to organization '{organizationId}'.");
        }
    }

    private async Task<ConfigurationItem> RequireItemAsync(
        Guid organizationId,
        Guid? facilityId,
        string key,
        CancellationToken cancellationToken) =>
        await catalogRepository.FindConfigurationItemAsync(
            organizationId, facilityId, key, cancellationToken)
        ?? throw new NotFoundException($"Configuration item '{key}' was not found.");

    private Task WriteAuditAsync(
        AuthContext context,
        Guid organizationId,
        Guid? facilityId,
        string action,
        string resourceId,
        CancellationToken cancellationToken) =>
        auditRepository.AddAsync(
            AuditRecordFactory.Create(
                context,
                correlationIdProvider.Current,
                organizationId,
                facilityId,
                action,
                AuditResourceTypes.ConfigurationItem,
                resourceId,
                AuditOutcome.Success,
                null),
            cancellationToken);
}

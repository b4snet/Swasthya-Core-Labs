using Swasthya.CoreLabs.Application.Abstractions;
using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;
using Swasthya.CoreLabs.Domain.Common;
using Swasthya.CoreLabs.Domain.Laboratory;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Application.Services;

public sealed class SpecimenService(
    ISpecimenRepository specimenRepository,
    IContainerRepository containerRepository,
    IUnitOfWork unitOfWork,
    IAuthorizationService authorizationService)
{
    public async Task<SpecimenDto> CreateSpecimenAsync(
        AuthContext authContext,
        Guid organizationId,
        CreateSpecimenRequest request,
        CancellationToken cancellationToken)
    {
        await authorizationService.RequireOrganizationAccess(
            authContext, Permissions.SpecimenWrite, organizationId);

        var orderItem = await specimenRepository
            .GetOrderItemAsync(request.OrderItemId, cancellationToken)
            ?? throw new NotFoundException($"Order item {request.OrderItemId} not found");

        var container = await containerRepository
            .GetAsync(request.ContainerId, cancellationToken)
            ?? throw new NotFoundException($"Container {request.ContainerId} not found");

        var specimen = new Specimen(
            request.OrderItemId,
            request.ContainerId,
            request.SpecimenType,
            request.AccessionNumber,
            request.Status,
            request.CollectorIdentity,
            request.FacilityId,
            request.CollectionNotes);

        await specimenRepository.AddAsync(specimen, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return SpecimenDtoFromEntity(specimen);
    }

    public async Task<List<SpecimenDto>> ListSpecimensAsync(
        AuthContext authContext,
        Guid organizationId,
        Guid? facilityId = null,
        CancellationToken cancellationToken = default)
    {
        var specimens = await specimenRepository
            .ListAsync(organizationId, facilityId, cancellationToken);

        return specimens.Select(SpecimenDtoFromEntity).ToList();
    }

    public async Task<SpecimenDto?> GetSpecimenAsync(
        string specimenNumber,
        CancellationToken cancellationToken = default)
    {
        var specimen = await specimenRepository
            .GetBySpecimenNumberAsync(specimenNumber, cancellationToken);

        if (specimen is null)
        {
            return null;
        }

        return SpecimenDtoFromEntity(specimen);
    }

    public async Task<SpecimenDto> UpdateSpecimenAsync(
        string specimenNumber,
        UpdateSpecimenRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetSpecimenEntityAsync(specimenNumber, cancellationToken)
            ?? throw new NotFoundException($"Specimen {specimenNumber} not found");

        if (request.Status.HasValue)
        {
            entity.SetStatus(request.Status.Value);
        }

        if (!string.IsNullOrEmpty(request.AccessionNumber))
        {
            entity.SetAccessionNumber(request.AccessionNumber);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return SpecimenDtoFromEntity(entity);
    }

    public async Task RecordCollectionAsync(
        string specimenNumber,
        CreateCollectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetSpecimenEntityAsync(specimenNumber, cancellationToken)
            ?? throw new NotFoundException($"Specimen {specimenNumber} not found");

        entity.SetStatus(request.NewStatus);

        if (!string.IsNullOrEmpty(request.CollectorIdentity))
        {
            entity.SetCollectorIdentity(request.CollectorIdentity);
        }

        if (!string.IsNullOrEmpty(request.CollectionNotes))
        {
            entity.SetCollectionNotes(request.CollectionNotes);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordAccessionAsync(
        string specimenNumber,
        CreateAccessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetSpecimenEntityAsync(specimenNumber, cancellationToken)
            ?? throw new NotFoundException($"Specimen {specimenNumber} not found");

        if (string.IsNullOrEmpty(entity.AccessionNumber))
        {
            throw new ValidationException("Specimen must have an accession number before recording accession.");
        }

        var facilityId = entity.FacilityId.GetValueOrDefault();

        var accession = new Accession(
            facilityId,
            entity.AccessionNumber,
            request.Description,
            request.ReceivingActorId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Specimen?> GetSpecimenEntityAsync(
        string specimenNumber,
        CancellationToken cancellationToken = default)
    {
        return await specimenRepository
            .GetBySpecimenNumberAsync(specimenNumber, cancellationToken);
    }

    private static SpecimenDto SpecimenDtoFromEntity(Specimen entity)
    {
        return new SpecimenDto(
            entity.Id,
            entity.OrderItemId,
            entity.ContainerId,
            entity.SpecimenType,
            entity.AccessionNumber,
            entity.Status,
            entity.CollectorIdentity,
            entity.FacilityId,
            entity.CollectionNotes,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc);
    }
}

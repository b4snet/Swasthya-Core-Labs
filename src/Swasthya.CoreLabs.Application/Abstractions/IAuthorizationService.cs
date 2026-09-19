using Swasthya.CoreLabs.Application.Models;

namespace Swasthya.CoreLabs.Application.Abstractions;

public interface IAuthorizationService
{
    Task RequireOrganizationAccess(AuthContext authContext, string permissionCode, Guid organizationId);

    Task RequireFacilityAccess(AuthContext authContext, string permissionCode, Guid organizationId, Guid facilityId);
}

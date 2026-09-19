using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;

namespace Swasthya.CoreLabs.Application.Authorization;

public static class PermissionGuard
{
    public static void Require(AuthContext context, string permissionCode)
    {
        if (!context.IsGranted(permissionCode))
        {
            throw new PermissionDeniedException(
                $"Principal has no grant for permission '{permissionCode}'.");
        }
    }

    public static void Require(AuthContext context, string permissionCode, Guid organizationId)
    {
        if (!context.IsGrantedAtOrg(permissionCode, organizationId))
        {
            throw new PermissionDeniedException(
                $"Principal has no grant for permission '{permissionCode}' at organization '{organizationId}'.");
        }
    }

    public static void Require(AuthContext context, string permissionCode, Guid organizationId, Guid facilityId)
    {
        if (!context.IsGranted(permissionCode, organizationId, facilityId)
            && !context.IsGrantedAtOrg(permissionCode, organizationId))
        {
            throw new PermissionDeniedException(
                $"Principal has no grant for permission '{permissionCode}' at facility '{facilityId}'.");
        }
    }
}

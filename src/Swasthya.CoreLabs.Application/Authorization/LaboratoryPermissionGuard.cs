using Swasthya.CoreLabs.Application.Exceptions;
using Swasthya.CoreLabs.Application.Models;

namespace Swasthya.CoreLabs.Application.Authorization;

/// <summary>
/// Authorization guards for Phase 2 laboratory master data. Facility-scoped
/// operations require a grant at the target facility OR an organization-wide
/// grant; a facility-only grant never authorizes another facility. These are
/// Phase 2 semantics and deliberately separate from the frozen Phase 1
/// <see cref="PermissionGuard"/>.
/// </summary>
public static class LaboratoryPermissionGuard
{
    public static void RequireFacilityScoped(
        AuthContext context,
        string permissionCode,
        Guid organizationId,
        Guid facilityId)
    {
        if (context.IsGranted(permissionCode, organizationId, facilityId))
        {
            return;
        }

        RequireOrganizationWide(context, permissionCode, organizationId);
    }

    /// <summary>
    /// Requires a grant that is not restricted to a single facility. Used for
    /// creating organization-scoped master data.
    /// </summary>
    public static void RequireOrganizationWide(
        AuthContext context,
        string permissionCode,
        Guid organizationId)
    {
        if (!context.Grants.Any(g =>
                string.Equals(g.PermissionCode, permissionCode, StringComparison.Ordinal)
                && g.OrganizationId == organizationId
                && g.FacilityId is null))
        {
            throw new PermissionDeniedException(
                $"Principal has no organization-wide grant for permission '{permissionCode}' at organization '{organizationId}'.");
        }
    }
}

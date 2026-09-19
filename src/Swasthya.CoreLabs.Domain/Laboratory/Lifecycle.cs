namespace Swasthya.CoreLabs.Domain.Laboratory;

/// <summary>
/// Valid transitions for master-data lifecycle states.
/// </summary>
public static class MasterDataLifecycle
{
    public static bool CanTransition(MasterDataStatus from, MasterDataStatus to)
    {
        if (from == to)
        {
            return false;
        }

        if (from == MasterDataStatus.Retired)
        {
            return false;
        }

        return to switch
        {
            MasterDataStatus.Draft => false,
            _ => true,
        };
    }

    public static void RequireTransition(MasterDataStatus from, MasterDataStatus to)
    {
        if (!CanTransition(from, to))
        {
            throw new InvalidOperationException(
                $"Master data cannot transition from '{from}' to '{to}'.");
        }
    }
}

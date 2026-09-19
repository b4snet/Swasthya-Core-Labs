namespace Swasthya.CoreLabs.Domain.Specimen;

/// <summary>
/// Valid transitions for the Phase 4 accession lifecycle. An accession is
/// Created → Received → Processing → terminal states (Completed/Cancelled).
/// </summary>
public static class AccessionLifecycle
{
    public static bool CanTransition(AccessionStatus from, AccessionStatus to)
    {
        if (from == to)
        {
            return false;
        }

        return (from == AccessionStatus.Created && to == AccessionStatus.Received)
            || (from == AccessionStatus.Received && to == AccessionStatus.Processing)
            || (from == AccessionStatus.Processing && to == AccessionStatus.Completed)
            || (from == AccessionStatus.Created && to == AccessionStatus.Cancelled)
            || (from == AccessionStatus.Cancelled && to == AccessionStatus.Cancelled);
    }

    public static void RequireTransition(AccessionStatus from, AccessionStatus to)
    {
        if (!CanTransition(from, to))
        {
            throw new InvalidOperationException(
                $"Accession cannot transition from '{from}' to '{to}'.");
        }
    }
}

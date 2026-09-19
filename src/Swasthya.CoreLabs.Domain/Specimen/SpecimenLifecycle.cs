namespace Swasthya.CoreLabs.Domain.Specimen;

/// <summary>
/// Valid transitions for the Phase 4 specimen lifecycle. A specimen is
/// Created/Requested → Collected → Received → Accessioned → terminal
/// states (Rejected/Cancelled/Stored).
/// </summary>
public static class SpecimenLifecycle
{
    public static bool CanTransition(SpecimenStatus from, SpecimenStatus to)
    {
        if (from == to)
        {
            return false;
        }

        return (from == SpecimenStatus.Requested && to == SpecimenStatus.Collected)
            || (from == SpecimenStatus.Collected && to == SpecimenStatus.Received)
            || (from == SpecimenStatus.Received && to == SpecimenStatus.Accessioned)
            || (from == SpecimenStatus.Accessioned && to == SpecimenStatus.Rejected)
            || (from == SpecimenStatus.Accessioned && to == SpecimenStatus.Cancelled)
            || (from == SpecimenStatus.Accessioned && to == SpecimenStatus.Stored)
            || (from == SpecimenStatus.Rejected && to == SpecimenStatus.Cancelled)
            || (from == SpecimenStatus.Cancelled && to == SpecimenStatus.Rejected);
    }

    public static void RequireTransition(SpecimenStatus from, SpecimenStatus to)
    {
        if (!CanTransition(from, to))
        {
            throw new InvalidOperationException(
                $"Specimen cannot transition from '{from}' to '{to}'.");
        }
    }
}

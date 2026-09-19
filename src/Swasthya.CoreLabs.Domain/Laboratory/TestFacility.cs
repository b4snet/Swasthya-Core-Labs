namespace Swasthya.CoreLabs.Domain.Laboratory;

/// <summary>
/// Facility applicability for a test concept. When a test has no rows it is
/// applicable to every facility in its organization.
/// </summary>
public sealed class TestFacility
{
    private TestFacility()
    {
    }

    public TestFacility(Guid testId, Guid facilityId)
    {
        Id = Guid.NewGuid();
        TestId = testId;
        FacilityId = facilityId;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TestId { get; private set; }

    public Guid FacilityId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
}

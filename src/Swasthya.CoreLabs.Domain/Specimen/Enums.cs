namespace Swasthya.CoreLabs.Domain.Specimen;

/// <summary>
/// The type of biological specimen required or collected.
/// </summary>
public enum SpecimenType
{
    Blood = 0,
    Serum = 1,
    Plasma = 2,
    Urine = 3,
    Tissue = 4,
    Swab = 5,
    Stool = 6,
    Csf = 7,
    Other = 8,
}

/// <summary>
/// The type of container used to hold a specimen.
/// </summary>
public enum ContainerType
{
    Tube = 0,
    Vial = 1,
    Cup = 2,
    SwabTube = 3,
    Other = 4,
}

/// <summary>
/// The current lifecycle state of a specimen.
/// </summary>
public enum SpecimenStatus
{
    Requested = 0,
    Collected = 1,
    Received = 2,
    Accessioned = 3,
    Rejected = 4,
    Cancelled = 5,
    Stored = 6,
}

/// <summary>
/// The current lifecycle state of an accession.
/// </summary>
public enum AccessionStatus
{
    Created = 0,
    Received = 1,
    Processing = 2,
    Completed = 3,
    Cancelled = 4,
}

/// <summary>
/// The current lifecycle state of a collection event.
/// </summary>
public enum CollectionStatus
{
    Requested = 0,
    Collected = 1,
    Received = 2,
}

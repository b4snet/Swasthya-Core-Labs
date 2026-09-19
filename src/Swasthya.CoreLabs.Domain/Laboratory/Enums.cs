namespace Swasthya.CoreLabs.Domain.Laboratory;

public enum MasterDataStatus
{
    Draft = 0,
    Active = 1,
    Inactive = 2,
    Retired = 3,
}

public enum ResultDataTypeKind
{
    Quantitative = 0,
    Qualitative = 1,
    Coded = 2,
    Textual = 3,
    Ordinal = 4,
    Boolean = 5,
    Structured = 6,
    PanelComponent = 7,
}

public enum TerminologyMappingStatus
{
    Proposed = 0,
    Active = 1,
    Superseded = 2,
}

public enum ConfigurationValueType
{
    Text = 0,
    WholeNumber = 1,
    Number = 2,
    Boolean = 3,
    Json = 4,
}

public enum ReferenceRangeSex
{
    Any = 0,
    Female = 1,
    Male = 2,
}

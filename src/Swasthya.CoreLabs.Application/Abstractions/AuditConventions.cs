namespace Swasthya.CoreLabs.Application.Abstractions;

public static class AuditActions
{
    public const string AuthContextResolved = "authc.context_resolved";
    public const string OrganizationListed = "organization.listed";
    public const string FacilityListed = "facility.listed";
    public const string AuditRead = "audit.read";
    public const string LaboratoryListed = "laboratory.listed";
    public const string LaboratoryCreated = "laboratory.created";
    public const string LaboratoryUpdated = "laboratory.updated";
    public const string LaboratoryVersioned = "laboratory.versioned";
    public const string LaboratoryActivated = "laboratory.activated";
    public const string LaboratoryDeactivated = "laboratory.deactivated";
    public const string LaboratoryRetired = "laboratory.retired";
    public const string OrderListed = "laboratory.order.listed";
    public const string OrderCreated = "laboratory.order.created";
    public const string OrderUpdated = "laboratory.order.updated";
    public const string OrderCancelled = "laboratory.order.cancelled";
    public const string OrderItemCancelled = "laboratory.order_item.cancelled";
    public const string ResultCreated = "laboratory.result.created";
    public const string ResultVerified = "laboratory.result.verified";
    public const string ResultFinalized = "laboratory.result.finalized";
    public const string ResultCorrected = "laboratory.result.corrected";
}

public static class AuditResourceTypes
{
    public const string Organization = "organization";
    public const string Facility = "facility";
    public const string AuditRecord = "audit_record";
    public const string Principal = "principal";
    public const string Discipline = "laboratory.discipline";
    public const string LaboratorySection = "laboratory.section";
    public const string ResultDataType = "laboratory.result_data_type";
    public const string Unit = "laboratory.unit";
    public const string CodeSystem = "laboratory.code_system";
    public const string SpecimenType = "laboratory.specimen_type";
    public const string SpecimenSource = "laboratory.specimen_source";
    public const string ContainerType = "laboratory.container_type";
    public const string ResultStatus = "laboratory.result_status";
    public const string Analyzer = "laboratory.analyzer";
    public const string ConfigurationItem = "laboratory.configuration_item";
    public const string Test = "laboratory.test";
    public const string TestVersion = "laboratory.test_version";
    public const string TestSpecimenRequirement = "laboratory.test_specimen_requirement";
    public const string TestTerminologyMapping = "laboratory.test_terminology_mapping";
    public const string TestFacility = "laboratory.test_facility";
    public const string Panel = "laboratory.panel";
    public const string PanelVersion = "laboratory.panel_version";
    public const string PanelMembership = "laboratory.panel_membership";
    public const string ReferenceRange = "laboratory.reference_range";
    public const string Order = "laboratory.order";
    public const string OrderItem = "laboratory.order_item";
    public const string Result = "laboratory.result";
}
    public const string Test = "laboratory.test";
    public const string TestVersion = "laboratory.test_version";
    public const string TestSpecimenRequirement = "laboratory.test_specimen_requirement";
    public const string TestTerminologyMapping = "laboratory.test_terminology_mapping";
    public const string TestFacility = "laboratory.test_facility";
    public const string Panel = "laboratory.panel";
    public const string PanelVersion = "laboratory.panel_version";
    public const string PanelMembership = "laboratory.panel_membership";
    public const string ReferenceRange = "laboratory.reference_range";
    public const string Order = "laboratory.order";
    public const string OrderItem = "laboratory.order_item";
}

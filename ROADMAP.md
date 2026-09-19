# SWASTHYA CORE LABS — MASTER ARCHITECTURAL ROADMAP

> Baseline decision: **.NET 10 (LTS)** throughout. This document is the
> **long-term architectural north star** and the authoritative implementation
> sequence. The target architecture is defined in
> [`docs/architecture/master-blueprint.md`](docs/architecture/master-blueprint.md).

You are building **Swasthya Core Labs**, an enterprise-grade, standalone **Diagnostic Information System (DIS)** with a strong Laboratory Information System (LIS) foundation.

This document is the **long-term architectural north star** for the project.

It defines where the system is going, how the major domains fit together, the intended implementation sequence, and the boundaries that must remain stable.

Do not blindly implement future phases from this roadmap. Each phase will receive its own execution specification based on the repository state and the actual domain requirements at that point.

Phase boundaries may be refined when architectural reviews demonstrate that a domain needs to be split, merged, reordered, or redesigned. Such changes must be documented rather than made silently.

---

# 1. PRODUCT VISION

Swasthya Core Labs is intended to become a **standalone diagnostic platform for healthcare organizations**.

It should be capable of supporting diagnostic services across a hospital or healthcare network, including but not limited to:

* clinical laboratory;
* hematology;
* clinical chemistry;
* immunology;
* serology;
* microbiology;
* bacteriology;
* virology;
* parasitology;
* mycology;
* molecular diagnostics;
* genetics/molecular diagnostics;
* coagulation;
* urinalysis;
* clinical microscopy;
* toxicology;
* endocrinology;
* special chemistry;
* blood bank/transfusion medicine;
* pathology/cytology interfaces;
* point-of-care testing;
* ECG;
* EEG;
* EMG/NCV;
* pulmonary function diagnostics;
* cardiac diagnostics;
* neurophysiology;
* other physiological diagnostic services;
* radiography/X-ray;
* CT;
* MRI;
* ultrasound;
* mammography;
* nuclear medicine where applicable;
* future diagnostic modalities.

The architecture must therefore **not assume that every diagnostic investigation is a conventional laboratory test**.

The system must be capable of representing different diagnostic domains while maintaining a common enterprise platform.

---

# 2. PRODUCT BOUNDARY

Swasthya Core Labs is a diagnostic information platform.

It is not intended to replace every healthcare system.

External systems may include:

* HMS;
* EHR/EMR;
* hospital billing;
* registration/ADT;
* external identity providers;
* PACS/VNA;
* RIS;
* pharmacy;
* clinical decision support;
* external terminology services;
* analyzer/device systems;
* national health-information infrastructure.

The platform must expose standards-based integration boundaries rather than attempting to absorb unrelated responsibilities.

Where an external system remains the system of record for a domain, Swasthya Core Labs should integrate with it rather than silently duplicating authority.

---

# 3. HIGH-LEVEL ARCHITECTURE

The intended architecture is a modular, enterprise-grade diagnostic platform.

Conceptually:

```text
                       SWASTHYA CORE LABS
                                │
           ┌────────────────────┼────────────────────┐
           │                    │                    │
     Shared Platform      Diagnostic Domains    Integration Layer
           │                    │                    │
     Identity/Auth         Laboratory             HL7 v2
     Tenancy               Microbiology            FHIR
     Organizations         Molecular               DICOM
     Master Data           Blood Bank              DICOMweb
     Audit                 Pathology               IHE
     Provenance            Cardiac                 Terminology
     Security              Neurophysiology         External APIs
     Notifications         Pulmonary
     Observability         Imaging
     Quality               Future domains
           │                    │
           └────────────────────┼────────────────────┘
                                │
                       Data / Persistence
```

The implementation may remain a modular monolith initially.

Do not introduce distributed services merely for architectural appearance.

Separate modules/services only when justified by:

* domain boundaries;
* scaling characteristics;
* security isolation;
* deployment requirements;
* interoperability requirements;
* operational requirements;
* or proven architectural need.

---

# 4. SHARED PLATFORM FOUNDATION

The following capabilities should become reusable platform infrastructure rather than being independently reinvented by each diagnostic domain.

## Identity and access

* authentication;
* OAuth2/OIDC-compatible identity boundary;
* service identities;
* RBAC;
* resource/facility scope;
* extensible authorization;
* deny-by-default;
* tenant isolation;
* facility isolation;
* auditability.

## Organization and tenancy

```text
Organization
  └── Facility
       └── Diagnostic Unit / Department / Section
```

The exact hierarchy must remain configurable.

A facility may contain multiple diagnostic departments and services.

## Master data

Shared master-data infrastructure should eventually support:

* identifiers;
* terminology;
* organizations;
* facilities;
* departments;
* diagnostic disciplines;
* services;
* procedures;
* test definitions;
* panels/profiles;
* specimen concepts;
* units;
* reference intervals;
* device definitions;
* configuration;
* lifecycle/versioning.

Do not force every diagnostic domain into an identical data model where the domain semantics genuinely differ.

---

# 5. DIAGNOSTIC DOMAIN MODEL

The platform should eventually contain independent but interoperable diagnostic domains.

## Domain A — Core Laboratory

Foundation for:

* hematology;
* chemistry;
* immunology;
* serology;
* coagulation;
* urinalysis;
* microscopy;
* endocrinology;
* toxicology;
* special chemistry.

Core workflows:

```text
Order
→ Specimen
→ Accession
→ Worklist
→ Processing
→ Measurement
→ Result
→ Validation
→ Finalization
→ Report
```

---

## Domain B — Microbiology

Support domain-specific workflows for:

* bacteriology;
* virology;
* mycology;
* parasitology;
* culture;
* organism identification;
* susceptibility testing;
* antimicrobial susceptibility;
* microbiology observations;
* organism/result relationships.

Do not force microbiology into a numeric-result-only model.

---

## Domain C — Molecular Diagnostics

Support:

* molecular assays;
* amplification-based testing;
* molecular observations;
* targets;
* qualitative/quantitative molecular results;
* genetic/molecular metadata;
* specimen relationships;
* assay versioning;
* provenance.

Avoid prematurely implementing specialized genetic interpretation without domain requirements.

---

## Domain D — Blood Bank / Transfusion Medicine

Support eventual:

* donor concepts;
* blood components;
* blood grouping;
* compatibility testing;
* antibody screening;
* crossmatching;
* inventory;
* transfusion-related records;
* traceability;
* expiration;
* chain of custody.

This domain requires its own workflow and safety model.

Do not treat it as ordinary laboratory chemistry.

---

## Domain E — Pathology / Cytology Interface

Support integration boundaries for:

* pathology;
* histopathology;
* cytology;
* anatomic pathology;
* digital pathology references where applicable.

Do not assume pathology data is equivalent to ordinary laboratory observations.

---

# 6. PHYSIOLOGICAL DIAGNOSTICS

Create a separate architectural family for diagnostic services that generate waveforms, measurements, structured findings, images, or reports rather than conventional laboratory results.

Potential domains:

* ECG;
* EEG;
* EMG;
* NCV;
* PFT;
* spirometry;
* cardiac diagnostics;
* neurophysiology;
* other physiological testing.

These domains may contain:

* procedure metadata;
* acquisition metadata;
* device metadata;
* waveform/file references;
* measurements;
* structured findings;
* interpretation;
* final reports;
* attachments;
* provenance.

Do not force waveform or diagnostic-study data into the laboratory `Observation` model merely because both are "results."

Reuse common infrastructure where appropriate while preserving domain-specific semantics.

---

# 7. MEDICAL IMAGING

Create an imaging domain capable of integrating with:

* X-ray/radiography;
* CT;
* MRI;
* ultrasound;
* mammography;
* nuclear medicine;
* other applicable imaging modalities.

The architecture must account for:

* imaging studies;
* procedures;
* series;
* instances;
* acquisition metadata;
* modality;
* device identity;
* study status;
* report association;
* image references;
* PACS/VNA integration.

Use **DICOM/DICOMweb** where applicable.

Do not build a proprietary replacement for DICOM.

Do not attempt to become a PACS unless that becomes an explicitly approved product requirement.

The diagnostic platform should be able to reference and exchange imaging studies while respecting the external imaging system boundary.

---

# 8. COMMON DIAGNOSTIC WORKFLOW ABSTRACTION

Different diagnostic domains may eventually share a conceptual lifecycle:

```text
Request
  ↓
Scheduling / Work Assignment
  ↓
Subject / Patient Association
  ↓
Specimen / Study / Acquisition
  ↓
Processing / Measurement
  ↓
Observation / Finding
  ↓
Validation
  ↓
Finalization
  ↓
Report
  ↓
Distribution / Interoperability
```

This is an architectural concept, not permission to create one universal workflow table.

Each domain must preserve its own semantics.

Examples:

```text
Laboratory:
Specimen → Analyzer → Result

ECG:
Procedure → Waveform → Measurements → Interpretation

MRI:
Imaging Study → Series → Instances → Findings → Report

Microbiology:
Specimen → Culture → Organism → Susceptibility → Result

Pathology:
Specimen → Processing → Examination → Findings → Report
```

The platform should share infrastructure without erasing these distinctions.

---

# 9. RESULTS AND OBSERVATIONS

Eventually establish a generalized diagnostic-result architecture capable of representing:

* quantitative;
* qualitative;
* coded;
* categorical;
* ordinal;
* boolean;
* textual;
* structured;
* component-based;
* waveform-derived;
* image-derived;
* microbiological;
* molecular;
* pathology-related;
* device-generated;
* manually entered;
* calculated where explicitly justified.

Every result must preserve:

* source;
* provenance;
* timestamp;
* status;
* units where applicable;
* reference context where applicable;
* subject/patient association;
* procedure/order association;
* specimen/study association;
* author/operator/device provenance;
* validation status;
* version;
* amendment history.

Finalized clinical results must not be silently overwritten.

Corrections and amendments must remain traceable.

---

# 10. DEVICE AND ANALYZER INTEGRATION

Device integration is a first-class architectural domain.

Eventually support vendor-neutral integration boundaries for:

* laboratory analyzers;
* ECG devices;
* EEG systems;
* EMG/NCV devices;
* imaging modalities;
* physiological measurement devices;
* POCT devices;
* other diagnostic equipment.

The integration model should account for:

* device identity;
* manufacturer;
* model;
* firmware/software version;
* protocol;
* connection state;
* interface configuration;
* mapping;
* raw input;
* normalized data;
* timestamps;
* units;
* quality/status;
* specimen/study association;
* duplicate detection;
* replay protection;
* malformed input;
* quarantine;
* acknowledgements;
* failures;
* provenance.

Conceptually:

```text
Device
 ↓
Raw Input
 ↓
Validation / Normalization
 ↓
Diagnostic Observation
 ↓
Laboratory/Clinical Validation
 ↓
Final Result
```

Never represent unvalidated device data as a finalized clinical result.

No real analyzer/device interoperability may be claimed without actual runtime validation.

---

# 11. INTEROPERABILITY ROADMAP

Interoperability is a core product capability.

## HL7 v2

Support where applicable:

* messaging;
* orders;
* results;
* acknowledgements;
* validation;
* correlation;
* idempotency;
* replay protection;
* error handling.

## FHIR

Support applicable resources and workflows, potentially including:

* Patient;
* ServiceRequest;
* Specimen;
* Observation;
* DiagnosticReport;
* Task;
* Device;
* ImagingStudy;
* Procedure;
* Provenance;
* AuditEvent;
* CapabilityStatement;
* other resources as required.

Use standards rather than inventing proprietary equivalents.

## DICOM

Use for imaging interoperability.

## DICOMweb

Use where applicable for web-based imaging exchange.

## IHE

Use applicable profiles where they provide meaningful interoperability patterns.

## Terminology

Provide controlled terminology integration for applicable:

* LOINC;
* SNOMED CT;
* UCUM;
* ICD;
* other relevant systems.

External terminology licensing and version management must be respected.

Never invent external codes.

---

# 12. QUALITY MANAGEMENT

Eventually establish diagnostic quality capabilities including:

* internal quality control;
* control materials;
* calibration;
* reagent/lot tracking;
* analyzer maintenance;
* quality events;
* corrective/preventive actions where applicable;
* proficiency testing;
* result flags;
* specimen rejection;
* quality exceptions;
* auditability.

Quality data must remain distinct from ordinary clinical observations.

---

# 13. AUDIT, PROVENANCE AND DATA INTEGRITY

The system must preserve trustworthy diagnostic history.

Important principles:

* append-only audit where appropriate;
* immutable finalized clinical records;
* complete amendment history;
* actor provenance;
* device provenance;
* timestamp provenance;
* correlation IDs;
* source-system provenance;
* version history;
* secure audit payloads;
* no silent destructive mutation.

The audit system must never become a mechanism for storing unnecessary PHI, secrets, credentials, or complete clinical payloads.

---

# 14. SECURITY ARCHITECTURE

Security must remain cross-cutting and centralized.

Maintain:

* authentication;
* authorization;
* least privilege;
* deny-by-default;
* tenant isolation;
* facility isolation;
* service identity validation;
* secure token validation;
* secure secret management;
* encryption in transit;
* encryption at rest where applicable;
* safe error handling;
* security logging;
* rate limiting;
* input validation;
* API security;
* dependency security;
* secure configuration;
* backup protection;
* disaster recovery;
* incident evidence.

Never trust client-supplied tenant/facility scope.

Never weaken authorization to make a test pass.

---

# 15. REGULATORY AND STANDARDS STRATEGY

The project must distinguish:

* technical standards;
* interoperability standards;
* terminology standards;
* security standards;
* quality standards;
* regulations;
* accreditation frameworks;
* certification requirements.

Potential areas include:

* HL7;
* FHIR;
* DICOM;
* IHE;
* LOINC;
* SNOMED CT;
* UCUM;
* ISO 15189;
* ISO 17025 where applicable;
* CLSI guidance where applicable;
* ISO/IEC 27001;
* ISO/IEC 27701;
* OWASP ASVS;
* OWASP API Security;
* HIPAA where applicable;
* FDA requirements where applicable;
* other jurisdiction-specific medical-device/software requirements;
* Nepalese healthcare, privacy, electronic-record, and interoperability requirements.

The project must never claim:

> "compliant"

or

> "certified"

merely because supporting technical structures have been implemented.

Applicability must be evaluated by jurisdiction, product function, deployment model, and legal/regulatory requirements.

---

# 16. IMPLEMENTATION ROADMAP

The following is the initial implementation envelope.

Phase boundaries are adjustable after architectural review.

## Foundation

### Phase 0

Repository, architecture, engineering foundation, standards inventory, security principles, documentation.

### Phase 1

Identity, tenancy, authorization, persistence, audit foundation.

### Phase 2

Laboratory organization, facilities, departments, disciplines, master data, test catalog foundation.

---

## Core Laboratory

### Phase 3

Laboratory service catalog and diagnostic request/order foundation.

### Phase 4

Specimen architecture, specimen lifecycle, collection concepts, accessioning foundation.

### Phase 5

Accessioning and specimen tracking.

### Phase 6

Laboratory worklists and operational workflow.

### Phase 7

Result-entry and observation architecture.

### Phase 8

Result validation, authorization and finalization.

### Phase 9

Reference intervals, flags and result-status engine.

### Phase 10

Diagnostic reporting and report lifecycle.

---

## Laboratory Specialties

### Phase 11

Hematology.

### Phase 12

Clinical chemistry.

### Phase 13

Immunology and serology.

### Phase 14

Coagulation, urinalysis and clinical microscopy.

### Phase 15

Microbiology foundation.

### Phase 16

Bacteriology and culture workflows.

### Phase 17

Antimicrobial susceptibility and microbiology reporting.

### Phase 18

Virology, parasitology and mycology.

### Phase 19

Molecular diagnostics.

### Phase 20

Genetics/molecular diagnostic extensions.

### Phase 21

Blood bank/transfusion medicine.

### Phase 22

Pathology/cytology integration.

---

## Device Integration

### Phase 23

Vendor-neutral device/analyzer integration foundation.

### Phase 24

Laboratory analyzer integration.

### Phase 25

POCT/device integration.

### Phase 26

Physiological-device integration framework.

---

## Physiological Diagnostics

### Phase 27

ECG.

### Phase 28

EEG.

### Phase 29

EMG/NCV.

### Phase 30

Pulmonary function diagnostics.

### Phase 31

Cardiac and other physiological diagnostics.

---

## Imaging

### Phase 32

Imaging domain foundation.

### Phase 33

X-ray/radiography.

### Phase 34

Ultrasound.

### Phase 35

CT.

### Phase 36

MRI.

### Phase 37

Mammography and additional imaging modalities.

### Phase 38

DICOM/DICOMweb integration.

### Phase 39

Imaging reporting and PACS/VNA integration boundary.

---

## Enterprise Interoperability

### Phase 40

HL7 v2 interoperability framework.

### Phase 41

FHIR interoperability framework.

### Phase 42

IHE and enterprise interoperability profiles.

### Phase 43

Terminology services and terminology lifecycle.

### Phase 44

External HMS/EHR/EMR integration framework.

---

## Quality and Enterprise Operations

### Phase 45

Laboratory quality management.

### Phase 46

Device quality, calibration, maintenance and QC.

### Phase 47

Advanced audit, provenance and data governance.

### Phase 48

Operational analytics and diagnostic dashboards.

### Phase 49

Notifications, subscriptions and enterprise workflow integrations.

---

## Final Hardening

### Phase 50

Performance, scalability, resilience and disaster recovery.

### Phase 51

Security hardening and threat-model validation.

### Phase 52

Interoperability conformance and contract validation.

### Phase 53

Clinical data integrity and workflow validation.

### Phase 54

Regulatory/accreditation readiness assessment.

### Phase 55

Production-readiness, deployment, observability and operational validation.

The final phase number is not sacred. The roadmap may be expanded if new diagnostic domains or architectural requirements justify additional phases.

---

# 17. ARCHITECTURAL REVIEW CYCLES

Do not wait until the end of the project to review architecture.

Perform a formal architectural review approximately after:

* Phase 5;
* Phase 10;
* Phase 15;
* Phase 22;
* Phase 31;
* Phase 39;
* Phase 44;
* Phase 49;
* and before final hardening.

Reviews should examine:

* domain boundaries;
* database architecture;
* API architecture;
* security;
* tenancy;
* authorization;
* audit;
* provenance;
* interoperability;
* terminology;
* device integration;
* clinical data integrity;
* performance;
* maintainability;
* technical debt;
* duplicated concepts;
* accidental coupling;
* future extensibility.

If the architecture needs correction, correct it deliberately and document the decision.

Do not preserve a bad design merely because an earlier phase implemented it.

Do not redesign stable foundations without evidence.

---

# 18. FINAL HARDENING STRATEGY

The final hardening program must be substantially more than ordinary bug fixing.

It should include:

* architecture review;
* threat modeling;
* penetration/security testing;
* authorization testing;
* tenant/facility isolation testing;
* concurrency testing;
* database integrity testing;
* migration testing;
* backup/restore testing;
* disaster-recovery testing;
* failure-injection testing;
* load testing;
* performance testing;
* API contract testing;
* HL7 validation;
* FHIR validation;
* DICOM validation;
* terminology validation;
* device integration testing;
* clinical workflow validation;
* audit/provenance validation;
* immutable-result validation;
* amendment/correction validation;
* observability validation;
* dependency/security scanning;
* deployment validation;
* operational runbook validation.

Any claim of compliance, certification, accreditation, or real device interoperability must be supported by the appropriate evidence and external validation process.

---

# 19. WHAT MUST NEVER HAPPEN

Never:

* invent clinical requirements;
* invent medical reference ranges;
* invent terminology codes;
* copy proprietary standards text;
* claim certification without certification;
* claim regulatory compliance without assessment;
* trust client-provided tenant scope;
* weaken authorization;
* silently overwrite finalized clinical data;
* delete clinical history without an approved retention/legal basis;
* expose internal database fields through public APIs;
* put secrets into logs;
* put unnecessary PHI into audit logs;
* introduce RLS without architectural justification;
* introduce microservices without justification;
* access production/staging systems during development;
* use real patient data for development/testing;
* commit or push unless explicitly authorized;
* implement future clinical domains merely because they appear on this roadmap.

---

# 20. PHASE EXECUTION PRINCIPLE

Each phase must follow this general engineering pattern:

```text
Understand repository
       ↓
Understand actual domain contract
       ↓
Implement minimally
       ↓
Test behavior
       ↓
Test security/isolation
       ↓
Test persistence
       ↓
Review architecture
       ↓
Document decisions
       ↓
Run full regression
       ↓
Freeze accepted phase
```

The exact execution prompt for each phase will be supplied separately.

Do not infer detailed implementation requirements solely from this roadmap.

---

# 21. VALIDATION TIERS

Every major capability must clearly state whether it is:

### PROVEN LOCALLY

Actually executed and verified against the relevant runtime/infrastructure.

### DOCUMENTED / ARCHITECTURALLY DEFINED

The architecture and contract are established, but the complete runtime behavior has not yet been proven.

### SIMULATED / CONTRACT-TESTED

Behavior has been tested using controlled substitutes, mocks, fixtures, or simulations rather than the real external system.

### REQUIRES FUTURE IMPLEMENTATION / VALIDATION

The capability is planned but not yet implemented or validated.

Never represent one tier as another.

---

# 22. ROADMAP GOVERNANCE

This roadmap is authoritative for the **direction of the product**, but not a substitute for phase-level engineering analysis.

When a future phase encounters a conflict between:

1. this roadmap;
2. the actual repository;
3. an established security invariant;
4. an actual clinical/domain requirement;
5. a verified external standard;

do not silently guess.

Investigate the conflict, document it, and update the roadmap or phase architecture deliberately.

The project should evolve without losing its architectural direction.

---

# 23. CURRENT POSITION

Current known state:

```text
Phase 0  → ACCEPTED
Phase 1  → ACCEPTED / GREEN / FROZEN
Phase 2  → ACCEPTED / GREEN / FROZEN
Phase 3  → ACCEPTED / GREEN / FROZEN
Phase 4  → ACCEPTED / GREEN / FROZEN
Phase 5  → ACCEPTED / GREEN / FROZEN
Phase 6  → ACCEPTED / GREEN / FROZEN
Phase 7  → ACCEPTED / GREEN / FROZEN   (result entry & observation completion)
Phase 8  → NOT STARTED  (validation / verification / finalization — NOT authorized)
```

Phase 7 is the current frozen milestone: laboratory result entry and observation
completion, per [ADR-0021](docs/decisions/ADR-0021.md).

Phase 8 (result validation, verification, finalization and controlled
correction) has **not** been started and must not be begun without explicit
authorization.

The immediate objective is to complete each phase correctly according to its own approved specification.

After each phase is accepted, the next phase must be explicitly authorized.

Do not automatically advance through the roadmap.

---

# 24. NORTH STAR

The final system should be understood as:

> **A secure, interoperable, vendor-neutral diagnostic information platform capable of supporting laboratory, physiological diagnostic, imaging, and other diagnostic services across hospitals and healthcare organizations, while preserving domain-specific clinical semantics and integrating cleanly with external HMS/EHR/EMR/PACS/device ecosystems.**

Build toward that architecture incrementally.

Do not attempt to build the entire platform at once.

Do not lose the long-term architecture while implementing individual phases.

Do not over-engineer future domains before their actual requirements are known.

Every phase should move the repository toward this destination while preserving security, clinical data integrity, interoperability, maintainability, and evidence-based engineering.
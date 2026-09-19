# SWASTHYA CORE LABS — MASTER SYSTEM BLUEPRINT

This document defines the **target architecture and system blueprint** for Swasthya Core Labs.

It is the architectural map for the entire project. Use it to understand **what we are building, how the pieces relate, what belongs in each layer, and where the project is going**.

This is not a request to implement the entire blueprint now.

Implement only the currently authorized phase. Use this blueprint to prevent architectural drift and incorrect assumptions about the final product.

The implementation sequence is governed by the [master roadmap](../../ROADMAP.md).

---

# 1. WHAT SWASTHYA CORE LABS IS

Swasthya Core Labs is intended to become a **standalone Diagnostic Information System (DIS)** for hospitals and healthcare organizations.

It begins with a strong **Laboratory Information System (LIS)** foundation but is not limited to conventional laboratory testing.

The eventual platform should support diagnostic services such as:

```text
LABORATORY
├── Hematology
├── Clinical Chemistry
├── Immunology
├── Serology
├── Coagulation
├── Urinalysis
├── Clinical Microscopy
├── Endocrinology
├── Toxicology
└── Special Chemistry

MICROBIOLOGY
├── Bacteriology
├── Virology
├── Parasitology
└── Mycology

MOLECULAR
├── Molecular Diagnostics
├── Genetics
└── Molecular Assays

TRANSFUSION
└── Blood Bank / Transfusion Medicine

PATHOLOGY
├── Histopathology
└── Cytology Interfaces

PHYSIOLOGICAL DIAGNOSTICS
├── ECG
├── EEG
├── EMG / NCV
├── Pulmonary Function
├── Cardiac Diagnostics
└── Other Physiological Testing

IMAGING
├── X-Ray / Radiography
├── Ultrasound
├── CT
├── MRI
├── Mammography
├── Nuclear Medicine
└── Other Imaging Modalities

POINT OF CARE
└── POCT / Near-Patient Testing
```

The platform must therefore be designed as a **diagnostic platform with independent diagnostic domains**, not as one giant laboratory workflow.

---

# 2. CORE ARCHITECTURAL IDEA

The platform has three major architectural areas:

```text
                         SWASTHYA CORE LABS
                                │
        ┌───────────────────────┼───────────────────────┐
        │                       │                       │
        ▼                       ▼                       ▼
 SHARED PLATFORM          DIAGNOSTIC DOMAINS      INTEGRATION
        │                       │                       │
        │                 ┌─────┼─────┐                 │
        │                 │     │     │                 │
        │               LAB   PHYSIO IMAGE              │
        │                 │     │     │                 │
        │                 └─────┼─────┘                 │
        │                       │                       │
        └───────────────────────┼───────────────────────┘
                                │
                         DATA / PERSISTENCE
```

The three areas have different responsibilities.

---

# 3. SHARED PLATFORM

Shared infrastructure should provide capabilities used by multiple diagnostic domains.

## Identity

```text
Authentication
Authorization
RBAC
Service Identity
Permission Model
Tenant Context
Facility Context
```

## Organization

```text
Organization
   │
   └── Facility
        │
        ├── Laboratory
        ├── Radiology
        ├── Cardiology
        ├── Neurology
        ├── Pathology
        └── Other Diagnostic Units
```

Do not force this hierarchy into unnecessary entities.

Model only what actual domain requirements justify.

---

# 4. MASTER DATA

Master data is shared infrastructure, but domain-specific semantics must remain possible.

```text
MASTER DATA
│
├── Organizations
├── Facilities
├── Departments
├── Sections
├── Disciplines
├── Diagnostic Services
├── Procedures
├── Test Definitions
├── Panels / Profiles
├── Specimen Types
├── Containers
├── Units
├── Terminology Mappings
├── Reference Intervals
├── Devices
└── Configuration
```

Master data must support:

* identifiers;
* lifecycle;
* effective dates;
* versioning;
* scope;
* provenance;
* concurrency;
* auditability.

Do not put patient-specific clinical data into master-data tables.

---

# 5. PATIENT BOUNDARY

The diagnostic system will eventually need to associate diagnostic activity with patients/subjects.

However:

**Patient identity does not necessarily mean Swasthya Core Labs owns the authoritative patient record.**

In an integrated deployment:

```text
HMS / EHR
    │
    │ Patient identity
    ▼
Swasthya Core Labs
    │
    └── Diagnostic activity
```

The system must support external patient identity/reference integration.

Do not duplicate the entire HMS/EHR patient domain without an explicit requirement.

---

# 6. COMMON DIAGNOSTIC REQUEST MODEL

Eventually diagnostic services will generally originate from a request/order.

Conceptually:

```text
Patient
   │
   ▼
Diagnostic Request
   │
   ├── Laboratory Test
   ├── Microbiology Test
   ├── Molecular Test
   ├── ECG
   ├── EEG
   ├── EMG
   ├── X-Ray
   ├── CT
   ├── MRI
   └── Other Diagnostic Procedure
```

Do not assume every diagnostic service is represented by the same internal workflow.

A common request abstraction may exist, while domain-specific workflows remain separate.

---

# 7. LABORATORY FLOW

The eventual laboratory workflow should conceptually become:

```text
Order
  ↓
Test / Panel
  ↓
Specimen Requirement
  ↓
Collection
  ↓
Accession
  ↓
Worklist
  ↓
Processing
  ↓
Analyzer / Manual Measurement
  ↓
Observation
  ↓
Validation
  ↓
Finalization
  ↓
Diagnostic Report
  ↓
Distribution
```

Each stage should preserve provenance and state.

Finalized results must not be silently overwritten.

---

# 8. MICROBIOLOGY FLOW

Microbiology requires a different domain model.

Conceptually:

```text
Request
  ↓
Specimen
  ↓
Accession
  ↓
Culture / Examination
  ↓
Organism Identification
  ↓
Susceptibility Testing
  ↓
Interpretation
  ↓
Validation
  ↓
Report
```

Do not force microbiology into a numeric-result-only architecture.

---

# 9. MOLECULAR DIAGNOSTICS FLOW

Conceptually:

```text
Request
  ↓
Specimen
  ↓
Assay
  ↓
Processing
  ↓
Target / Measurement
  ↓
Molecular Result
  ↓
Validation
  ↓
Report
```

Support future quantitative and qualitative molecular results.

Do not prematurely build complex genomic interpretation systems unless specifically required.

---

# 10. BLOOD BANK FLOW

Blood bank/transfusion medicine is its own safety-critical domain.

Conceptually:

```text
Donor / Source
      ↓
Collection
      ↓
Component
      ↓
Testing
      ↓
Typing
      ↓
Compatibility
      ↓
Inventory
      ↓
Issue
      ↓
Transfusion Traceability
```

Blood components, compatibility, inventory, expiration, and traceability must not be treated as ordinary laboratory tests.

---

# 11. PHYSIOLOGICAL DIAGNOSTICS

Physiological diagnostics should have their own domain family.

Examples:

```text
ECG
EEG
EMG / NCV
PFT
Cardiac Diagnostics
Neurophysiology
```

Typical flow:

```text
Request
  ↓
Procedure
  ↓
Device Acquisition
  ↓
Raw/Waveform Data
  ↓
Measurements
  ↓
Clinical Interpretation
  ↓
Validation
  ↓
Report
```

A physiological study may contain:

* waveforms;
* measurements;
* device metadata;
* structured findings;
* interpretation;
* attachments;
* report data.

Do not force waveforms into ordinary laboratory-result structures.

---

# 12. IMAGING

Imaging should be treated as a distinct diagnostic domain.

Examples:

```text
X-Ray
CT
MRI
Ultrasound
Mammography
Nuclear Medicine
```

Conceptual flow:

```text
Request
  ↓
Scheduling / Procedure
  ↓
Imaging Acquisition
  ↓
Study
  ↓
Series
  ↓
Instances
  ↓
Interpretation
  ↓
Diagnostic Report
```

The platform should integrate with imaging ecosystems rather than automatically becoming a PACS.

Use:

```text
DICOM
DICOMweb
IHE
```

where applicable.

Image storage and image management must remain clearly separated from diagnostic-information management unless explicitly expanded later.

---

# 13. GENERALIZED DIAGNOSTIC MODEL

Across domains, there is a common conceptual pattern:

```text
REQUEST
   ↓
PROCEDURE / WORK
   ↓
SUBJECT / PATIENT
   ↓
SPECIMEN / STUDY / ACQUISITION
   ↓
MEASUREMENT / OBSERVATION / FINDING
   ↓
VALIDATION
   ↓
FINALIZATION
   ↓
REPORT
   ↓
DISTRIBUTION
```

But this is a **conceptual architecture**, not a command to create one universal table.

For example:

```text
Laboratory:
Specimen → Measurement → Result

ECG:
Procedure → Waveform → Measurement → Interpretation

MRI:
Study → Series → Image Instances → Findings → Report

Microbiology:
Specimen → Culture → Organism → Susceptibility → Result

Pathology:
Specimen → Processing → Examination → Findings → Report
```

Share infrastructure.

Do not erase domain semantics.

---

# 14. DEVICE ARCHITECTURE

Devices and analyzers are first-class integration objects.

```text
DEVICE
  ↓
CONNECTION
  ↓
RAW DATA
  ↓
VALIDATION
  ↓
NORMALIZATION
  ↓
OBSERVATION
  ↓
CLINICAL VALIDATION
  ↓
FINAL RESULT
```

Potential devices include:

```text
Laboratory analyzers
ECG machines
EEG systems
EMG/NCV systems
POCT devices
Ultrasound
X-Ray
CT
MRI
Other diagnostic equipment
```

Device integration must preserve:

* manufacturer;
* model;
* device identifier;
* firmware/software version;
* protocol;
* connection state;
* source timestamp;
* measurement timestamp;
* units;
* quality;
* mapping;
* provenance;
* duplicate/replay protection;
* errors;
* quarantine;
* acknowledgement state.

Never treat raw device input as automatically finalized clinical data.

---

# 15. RESULT ARCHITECTURE

The eventual result system must support multiple data types.

```text
Result
├── Quantitative
├── Qualitative
├── Coded
├── Categorical
├── Ordinal
├── Boolean
├── Text
├── Structured
├── Component
├── Waveform-derived
├── Image-derived
├── Microbiological
├── Molecular
└── Other domain-specific results
```

Every clinically significant result should eventually preserve:

```text
Identity
Patient/Subject
Request
Procedure
Specimen/Study
Value
Unit
Status
Timestamp
Source
Operator
Device
Validation
Version
Provenance
Amendment History
```

Do not silently overwrite finalized results.

---

# 16. REPORT ARCHITECTURE

Reports are a separate concern from raw observations.

Conceptually:

```text
Observations / Findings
        ↓
Validation
        ↓
Report Composition
        ↓
Clinical Authorization
        ↓
Final Report
        ↓
Distribution
```

Reports may contain:

* structured data;
* narrative;
* findings;
* interpretations;
* measurements;
* references;
* attachments;
* diagnostic images/references;
* report metadata.

The reporting architecture must support domain-specific reports.

---

# 17. QUALITY MANAGEMENT

Quality must span diagnostic domains while respecting domain-specific requirements.

```text
Quality
├── Internal QC
├── Calibration
├── Reagents / Lots
├── Controls
├── Device Maintenance
├── Proficiency Testing
├── Quality Events
├── Corrective Actions
├── Specimen Quality
└── Result Quality
```

Quality records must remain distinguishable from patient clinical results.

---

# 18. TERMINOLOGY

The platform must support external terminology systems without embedding proprietary terminology content unnecessarily.

Potential terminology systems include:

```text
LOINC
SNOMED CT
UCUM
ICD
Other applicable terminology systems
```

Mappings should preserve:

```text
Internal Identifier
Terminology System
External Code
Version
Status
Effective Period
Mapping Metadata
```

Never invent external terminology codes.

Never assume a data model alone establishes terminology compliance.

---

# 19. INTEROPERABILITY ARCHITECTURE

The platform must be integration-first.

```text
                SWASTHYA CORE LABS
                       │
        ┌──────────────┼──────────────┐
        ▼              ▼              ▼
      HL7 v2          FHIR         DICOM
        │              │              │
        └──────────────┼──────────────┘
                       ▼
                     IHE
                       │
                       ▼
              External Healthcare
                 Ecosystem
```

External systems may include:

* HMS;
* EHR;
* EMR;
* RIS;
* PACS;
* VNA;
* laboratories;
* analyzers;
* devices;
* national systems;
* terminology services.

Interoperability must include:

* validation;
* acknowledgements;
* correlation;
* idempotency;
* replay protection;
* versioning;
* provenance;
* error handling;
* capability negotiation.

---

# 20. SECURITY MODEL

Security is cross-cutting.

```text
Authentication
       ↓
Authorization
       ↓
Tenant Scope
       ↓
Facility Scope
       ↓
Resource Scope
       ↓
Operation
       ↓
Audit
```

Security principles:

* deny by default;
* least privilege;
* authoritative server-side scope;
* no client-trusted tenancy;
* no client-trusted facility access;
* secure service identities;
* secure tokens;
* encryption;
* secret management;
* safe errors;
* security logging;
* audit;
* rate limiting;
* secure dependencies;
* secure configuration.

Never weaken security to make functionality easier to implement.

---

# 21. DATA INTEGRITY

Diagnostic data is clinically significant.

The architecture must preserve:

```text
Original Data
     +
Provenance
     +
Version
     +
Validation State
     +
Finalization State
     +
Amendment History
     +
Audit History
```

Rules:

* no silent overwrite;
* no untraceable correction;
* finalized data requires controlled amendment;
* source provenance must remain available;
* device-generated data must remain distinguishable;
* timestamps must be reliable;
* units must remain explicit;
* clinical history must remain reconstructable.

---

# 22. TENANCY AND FACILITY ISOLATION

The platform is intended to support multiple healthcare organizations.

Conceptually:

```text
Organization A
├── Facility A1
├── Facility A2
└── Facility A3

Organization B
├── Facility B1
└── Facility B2
```

Data access must respect:

```text
Organization Scope
        ↓
Facility Scope
        ↓
Department / Resource Scope
```

Forged claims must never expand access.

Missing authoritative scope must fail closed.

---

# 23. DATABASE ARCHITECTURE

The database should reflect domain boundaries.

Conceptually:

```text
Shared Platform
├── Identity
├── Organizations
├── Facilities
├── Authorization
├── Audit
└── Configuration

Diagnostic Core
├── Requests
├── Procedures
├── Specimens
├── Observations
├── Results
└── Reports

Domain Modules
├── Laboratory
├── Microbiology
├── Molecular
├── Blood Bank
├── Pathology
├── Physiological
└── Imaging
```

Avoid one enormous set of generic tables that destroys domain meaning.

Avoid unnecessary fragmentation into dozens of independent databases/services.

Use relational integrity, constraints, transactions, concurrency protection, and explicit ownership.

---

# 24. API ARCHITECTURE

The API should be:

* versioned;
* authenticated;
* authorized;
* validated;
* documented;
* correlation-aware;
* standards-friendly;
* safe by default.

Conceptually:

```text
/v1
 ├── organizations
 ├── facilities
 ├── master-data
 ├── patients/references
 ├── requests
 ├── specimens
 ├── laboratory
 ├── microbiology
 ├── molecular
 ├── blood-bank
 ├── pathology
 ├── physiological
 ├── imaging
 ├── devices
 ├── results
 ├── reports
 └── interoperability
```

Do not create endpoints for future domains before their contracts are understood.

---

# 25. AUDIT AND PROVENANCE

Administrative and clinically significant operations must be traceable.

Audit should capture appropriate:

* actor;
* service identity;
* organization;
* facility;
* operation;
* resource;
* timestamp;
* correlation ID;
* outcome;
* provenance.

Audit payloads must be explicitly allowlisted.

Never log:

* passwords;
* tokens;
* credentials;
* secrets;
* unnecessary PHI;
* complete clinical payloads without justification.

---

# 26. OBSERVABILITY

Eventually establish:

```text
Logs
Metrics
Traces
Correlation IDs
Health Checks
Dependency Health
Integration Monitoring
Security Events
Operational Alerts
```

Observability must not become a mechanism for leaking sensitive diagnostic information.

---

# 27. PHASE MAP

The project should evolve approximately as follows.

```text
FOUNDATION
Phase 0–2

        ↓

CORE LIS
Phase 3–10

        ↓

LABORATORY SPECIALTIES
Phase 11–22

        ↓

DEVICE INTEGRATION
Phase 23–26

        ↓

PHYSIOLOGICAL DIAGNOSTICS
Phase 27–31

        ↓

IMAGING
Phase 32–39

        ↓

ENTERPRISE INTEROPERABILITY
Phase 40–44

        ↓

QUALITY + ENTERPRISE OPERATIONS
Phase 45–49

        ↓

FINAL HARDENING
Phase 50–55+
```

The exact number of phases is adjustable.

The architecture is more important than the numbering.

---

# 28. PHASE DEPENDENCY MODEL

Do not implement domains in arbitrary order.

The intended dependency direction is:

```text
Foundation
   ↓
Master Data
   ↓
Requests / Orders
   ↓
Workflow
   ↓
Observations / Results
   ↓
Validation
   ↓
Reporting
   ↓
Domain Specialization
   ↓
Device Integration
   ↓
Interoperability
   ↓
Enterprise Hardening
```

Domain-specific implementations should reuse stable shared infrastructure rather than creating competing versions of:

* authorization;
* identity;
* tenancy;
* audit;
* terminology;
* identifiers;
* result status;
* provenance;
* correlation;
* error handling.

---

# 29. ARCHITECTURAL REVIEW POINTS

Perform deeper architecture reviews periodically.

Recommended review points:

```text
After Phase 5
After Phase 10
After Phase 15
After Phase 22
After Phase 31
After Phase 39
After Phase 44
After Phase 49
Before final production-readiness
```

Review:

* domain boundaries;
* database design;
* API design;
* security;
* tenancy;
* authorization;
* audit;
* clinical data integrity;
* interoperability;
* device architecture;
* terminology;
* performance;
* maintainability;
* technical debt;
* duplication;
* accidental coupling.

If an architectural defect is discovered, correct it deliberately and document the change.

Do not preserve an incorrect design simply because it was implemented earlier.

---

# 30. FINAL HARDENING

The final project stages are not merely "bug fixing."

They must validate the complete system.

Include:

```text
Architecture
Security
Authorization
Tenancy
Database
Concurrency
Performance
Scalability
Failure Recovery
Backup / Restore
Disaster Recovery
Audit
Provenance
Clinical Data Integrity
HL7
FHIR
DICOM
IHE
Terminology
Device Integration
API Contracts
Observability
Deployment
Operational Readiness
Regulatory / Accreditation Readiness
```

External certifications, regulatory compliance, accreditation, and real device interoperability must only be claimed when the appropriate evidence exists.

---

# 31. VALIDATION LANGUAGE

Every implementation must clearly distinguish:

### PROVEN LOCALLY

Actually executed and verified.

### SIMULATED / CONTRACT-TESTED

Tested against a controlled simulation, mock, fixture, or substitute.

### DOCUMENTED / ARCHITECTURALLY DEFINED

Architecture or contract exists but runtime proof is incomplete.

### REQUIRES FUTURE IMPLEMENTATION / VALIDATION

Planned but not yet implemented or proven.

Never blur these categories.

---

# 32. DEVELOPMENT RULE

The current phase has priority over future phases.

For every phase:

```text
Understand
   ↓
Implement
   ↓
Test
   ↓
Security Test
   ↓
Database Test
   ↓
Integration Test
   ↓
Review
   ↓
Document
   ↓
Regression Test
   ↓
Accept / Freeze
```

Do not prematurely implement future domains.

Do not create speculative infrastructure without a justified requirement.

Do not create fake integrations.

Do not claim runtime interoperability without runtime proof.

---

# 33. NON-NEGOTIABLE ARCHITECTURAL PRINCIPLES

Always preserve these principles:

1. **Diagnostic-domain independence**
2. **Shared infrastructure without semantic flattening**
3. **API-first architecture**
4. **Interoperability-first architecture**
5. **Security-first architecture**
6. **Clinical data integrity**
7. **Immutable/provenance-aware finalized data**
8. **Tenant and facility isolation**
9. **Vendor-neutral device integration**
10. **Standards-based interoperability**
11. **Provider-neutral architecture**
12. **Configurable master data**
13. **Explicit lifecycle/versioning**
14. **Evidence-based engineering**
15. **No invented clinical semantics**
16. **No invented terminology**
17. **No unsupported compliance claims**
18. **No unnecessary microservices**
19. **No unnecessary RLS**
20. **No production/staging experimentation**

---

# 34. CURRENT PROJECT POSITION

Current expected state:

```text
Phase 0 → ACCEPTED
Phase 1 → ACCEPTED / GREEN / FROZEN
Phase 2 → ACCEPTED / GREEN / FROZEN
Phase 3 → ACCEPTED / GREEN / FROZEN
Phase 4 → ACCEPTED / GREEN / FROZEN
Phase 5 → ACCEPTED / GREEN / FROZEN
Phase 6 → ACCEPTED / GREEN / FROZEN
Phase 7 → ACCEPTED / GREEN / FROZEN
```

Phase 7 implements **laboratory result entry and observation completion**
(Clinical stage "Observations / results"): typed `results` observations with
numeric/textual/coded/unit-bearing values, specimen, order-item and
test-version association, entry/retrieval/listing/editable-update endpoints
under `/v1/laboratory/results`, structural validation, authorization with
organization/facility scope, concurrency, audit/provenance, and the
`results` table via migration `20260918035033_AddResultEntity`
([ADR-0021](../decisions/ADR-0021.md)). Phase 7 does **not** include result
validation, verification, finalization, or controlled correction.

**Phase 8 (result validation, verification, finalization and controlled
correction) has NOT been started and must not begin without explicit
authorization.**

The immediate phase must always be completed according to its own approved specification.

This blueprint exists to ensure that the implementation remains compatible with the long-term Swasthya Core Labs architecture.

Do not automatically begin the next phase.

Wait for explicit authorization.

---

# 35. FINAL NORTH STAR

The finished Swasthya Core Labs platform should conceptually look like:

```text
                         SWASTHYA CORE LABS
                                  │
      ┌───────────────────────────┼───────────────────────────┐
      │                           │                           │
      ▼                           ▼                           ▼
 SHARED PLATFORM            DIAGNOSTIC DOMAINS          INTEROPERABILITY
      │                           │                           │
 Identity/Auth              Laboratory                    HL7 v2
 Tenancy                    Microbiology                  FHIR
 Organizations              Molecular                     DICOM
 Master Data                Blood Bank                    DICOMweb
 Audit                      Pathology                     IHE
 Provenance                 Cardiac                       Terminology
 Security                   Neurophysiology               External APIs
 Quality                    Pulmonary
 Observability              Imaging
 Notifications              POCT
      │                     Future Domains
      └───────────────────────────┼───────────────────────────┘
                                  │
                                  ▼
                         PostgreSQL / Data Layer
```

The product is not merely a laboratory application.

It is a **diagnostic information platform** whose first major foundation is the laboratory domain.

Build it incrementally.

Preserve the architecture.

Validate every important boundary.

Do not overbuild future domains before their requirements are understood.

Do not sacrifice clinical correctness, security, interoperability, or maintainability for implementation speed.
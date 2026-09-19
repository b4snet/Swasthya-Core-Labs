# Domain Boundaries

**Document owner:** Architecture | **Status:** Approved (Phase 1) | **Version:** 1.1

## Core platform vs. integration boundary

Each bounded domain is classified as:

- **CORE** — owned and implemented by Swasthya Core Labs.
- **INTEGRATION** — an external system/standard at the boundary; Swasthya
  Core Labs defines and implements the adapter but does not absorb the
  external responsibility.

| Domain | Classification | Notes |
| --- | --- | --- |
| Organization / facility | CORE | Facility structure, site configuration, sites of care. |
| Identity and access | CORE | Internal users, roles, sessions, RBAC/ABAC foundation. |
| Patient identity / reference | CORE | Internal patient record + external identifier resolution (PIX-style). Identity authority remains external. |
| Orders | CORE | Laboratory orders; may originate via integration from HMS. |
| Test catalog | CORE | Tests, methods, panels, profiles (content configured, not hard-coded). |
| Panels / profiles | CORE | Grouping model for requested tests. |
| Specimen collection | CORE | Collection events, containers, collection requirements. |
| Specimen lifecycle | CORE | From collection to disposal; location, state, aliquot tracking. |
| Accessioning | CORE | Case/accession creation, work order establishment. |
| Laboratory worklists | CORE | Task queues for bench workflows, per discipline. |
| Analyzer / device integration | CORE (adapter boundary) | Vendor-neutral device integration; devices themselves are external. |
| Observations / results | CORE | Normalized observations from device or manual entry. |
| Reference intervals | CORE | Configurable interval framework; values are content, never invented by the platform. |
| Units | CORE | UCUM-aligned unit handling. |
| Quality control | CORE | QC events, control tracking, rules configuration (Phases 45–46+). |
| Result validation | CORE | Validation/finalization workflow state. |
| Critical / urgent results | CORE | Configurable critical-value workflow, notification orchestration. |
| Reporting | CORE | Clinical report generation and delivery. |
| Amendments / corrections | CORE | Versioned, append-only correction flow. |
| Audit / provenance | CORE | Append-only audit trail. |
| Documents | CORE | Attachments, scanned documents, report artifacts. |
| Billing | INTEGRATION | External billing systems; orders/results feed billing; no financials in core. |
| Interoperability | CORE (adapter layer) | HL7 v2 / FHIR / DICOM adapters are CORE modules; the external peers are integration. |
| Terminology | CORE (adapter) | Terminology server integration boundary; LOINC/SNOMED/UCUM come from authoritative sources. |
| Notifications | CORE (orchestration) | Notification delivery may use external channels (SMS/email) but orchestration is core. |
| PACS / imaging | INTEGRATION | DICOM/DICOMweb adapters at the boundary; core does not store images. |
| External identity | INTEGRATION | National/enterprise identity providers. |
| Clinical decision support | INTEGRATION | External CDS; core provides data, not interpretation. |
| Clinical content (reference ranges, rules) | CONFIGURATION | Authoritative content, configured per facility/jurisdiction; not invented by the platform. |

## Boundary principles

1. **No artificial coupling**: no domain assumes a particular HMS/EHR vendor
   or interface version.
2. **LIS responsibilities stay inside**: anything that is fundamentally a
   laboratory workflow responsibility is built in-core, not delegated.
3. **External responsibilities stay outside**: billing, imaging storage,
   identity authority, and clinical interpretation are not absorbed.
4. **Config over code**: jurisdictional and clinical variability is data, not
   program logic.
5. **Integrity over convenience**: no boundary weakens append-only provenance,
   auditability, or PHI/PII protection.

## Decision context

See [ADR-0003](../decisions/ADR-0003.md) (core vs. integration and modular
architecture) and [ADR-0007](../decisions/ADR-0007.md) (identity/tenancy).

## Phase 1 implemented boundaries

- **Identity and access** (CORE): principals (issuer + external subject),
  roles, role permissions, principal-role assignments, organizations,
  facilities, permission catalog — persisted in the `identity-tenancy`
  model ([ADR-0009](../decisions/ADR-0009.md),
  [ADR-0010](../decisions/ADR-0010.md)).
- **Audit / provenance** (CORE): append-only `audit_records`, DB-enforced
  immutability, allowlisted non-PHI metadata ([ADR-0012](../decisions/ADR-0012.md)).

## Phase 2 implemented boundaries

- **Test catalog / panels / reference intervals / units / terminology
  (CORE, structure only):** laboratory master data is organization-scoped, with
  facility-scoped sections, configuration items, analyzers, and reference
  ranges. Definitions are separated from results; reference ranges and
  terminology carry structure and provenance only, with no invented clinical
  content or interpretation ([ADR-0016](../decisions/ADR-0016.md),
  [ADR-0017](../decisions/ADR-0017.md),
  [ADR-0018](../decisions/ADR-0018.md)).
- **Orders (CORE, Phase 3):** diagnostic order entity with OrderNumber (unique per organization), OrderItem rows pinned to exactly one test or one panel version via CHECK constraint `ck_order_items_single_target`; Request→Cancelled lifecycle only; new permissions `laboratory.order.read`/`laboratory.order.write` using facility-scoped guards; idempotent creation via `ExternalOrderId`; persisted in the `orders`/`order_items` tables from migration `AddDiagnosticOrderFoundation` ([ADR-0020](../decisions/ADR-0020.md)).
- **Still external / deferred:** patient identity, specimen collection and accessioning, results/observations, reporting, billing, CDS, and device communication are **not** implemented in Phase 3.

## Phase 7 implemented boundaries

- **Results / observations (CORE):** laboratory result entry and observation
  completion — typed observation values (numeric / textual / coded / unit
  bearing), specimen, order-item and test-version association, the `results`
  table via migration `AddResultEntity`, entry/retrieval/listing/editable
  update under `/v1/laboratory/results`, structural validation, authorization
  with organization/facility scope, concurrency, audit/provenance
  ([ADR-0021](../decisions/ADR-0021.md)).
- **Still external / deferred:** result **validation, verification,
  finalization and controlled correction** are **not** implemented in Phase 7
  and belong to Phase 8 (not authorized). Patient identity, specimen
  collection and accessioning, reporting, billing, CDS, and device
  communication likewise remain **not** implemented in Phase 7.

## Unresolved items

- Exact division of "result validation" vs. "CDS" for interpretation rules —
  requires clinical governance input in a later phase.
- Whether terminology runs as an embedded service vs. external terminology
  server — deferred to the terminology phase.
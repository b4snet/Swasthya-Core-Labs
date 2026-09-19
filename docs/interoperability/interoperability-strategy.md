# Interoperability Strategy

**Document owner:** Architecture / Interoperability | **Status:** Approved (Phase 0) | **Version:** 1.0

## Boundary

Swasthya Core Labs integrates with **arbitrary external HMS/EHR/EMR,
laboratory, diagnostic, and network systems**. The interoperability layer is
a **core adapter module**; external peers are integration boundaries. There
is **no proprietary replacement** for established interoperability standards,
and no coupling to any specific HMS product.

## Principles

1. **Standards-native**: use HL7 v2, FHIR, DICOM/DICOMweb, and terminology
   standards as the interop lingua franca. A proprietary envelope protocol is
   rejected ([ADR-0004](../decisions/ADR-0004.md)).
2. **Adapter isolation**: protocol details live inside adapters; the clinical
   core communicates through stable internal ports.
3. **Identifier-first**: external systems are correlated through an
   identifier resolution service, never by assuming a single domain-wide ID.
4. **Provenance-preserving**: every inbound/outbound artifact carries
   correlation identifiers, provenance, and versioning; nothing is silently
   overwritten.
5. **Defensive by default**: validate, dedupe, and guard every inbound
   message; malformed input is quarantined, not trusted.

## Messaging families

### HL7 v2

- Primary target standard for order/result messaging with HMS/EHR systems:
  segment-based exchange (ORM/ORU families, ADT for patient identity feeds,
  ACK acknowledgements) per HL7 v2.5.1.
- Architecture: a messaging layer responsible for parse/validate → normalize
  → internal events → outbound encoding; acknowledgement handling
  (original/commit/application modes), error acknowledgement with explicit
  codes, message control IDs, and correlation IDs.
- Uses official HL7 structure definitions; **no proprietary text** is
  reproduced in-repo.

### FHIR

- FHIR **R4** is the primary REST/API target for interoperating systems.
- Relevant resource families (data structures from Phase 2; FHIR
  interoperability framework from Phase 41):
  - **Patient / Practitioner / Organization / Location** — actors
  - **ServiceRequest / DiagnosticReport / Observation / Specimen** —
    order/result exchange
  - **Task** — workflow (accessioning/worklist boundary)
  - **Provenance / AuditEvent** — provenance of report/observation data
- API behavior follows FHIR REST semantics: `resource metadata`, versioning
  (`_history`), `$validate`, `id`/`identifier` search, `ETag`, and capability
  statements. Swasthya Core Labs publishes a **CapabilityStatement** naming
  supported resources/interactions ([ADR-0005](../decisions/ADR-0005.md)).
- Future implementation uses a conformant FHIR library; no proprietary R4
  clone.

### DICOM / DICOMweb

- Imaging is an **integration boundary (PACS)**. DICOM and DICOMweb are used
  where the product must reference or hand off imaging (pathology/cytology
  workflows, attached study references).
- Core does **not** store DICOM objects; it exchanges references and, where
  scoped, uses DICOMweb HTTP APIs for retrieval metadata. Exact workflow
  scope is deferred; classify as **partially applicable**.

### IHE profiles

- IHE Laboratory (PaLM) domain profiles such as testing workflow, specimen
  workflow, device automation, and report sharing, plus patient identity
  cross-referencing/demographics query (PIX/PDQ family), are the navigation
  map for profile-aware integrations.
- Exact profile names/status MUST be verified against the current IHE
  registry before implementation (they evolve); this document records intent,
  not committed integration scope.

## Cross-cutting interoperability concerns

| Concern | Architecture |
| --- | --- |
| Pattern vocabulary | Use official FHIR `CodeableConcept/uri` pattern vocabulary where applicable |
| Message validation | Structured validation against HL7 conformance profiles / FHIR `StructureDefinition` before acceptance; failures go to a quarantine queue with review workflow |
| Idempotency | Deterministic dedupe keys derived from external identifiers (`message.control.id + sending.system`, FHIR `id`/`identifier`); duplicate detection must not create clinical duplicates |
| Replay protection | Correlate by stable external identifier; re-sent messages resolve to the same clinical object; replay does not duplicate observations |
| Correlation identifiers | Every clinical object exposes canonical + external identifiers and trace IDs across messages/requests |
| Provenance | Provenance metadata (`who/when/what/why`, source system, version) attached at the boundary and preserved into the record ([ADR-0006](../decisions/ADR-0006.md)) |
| Versioning | Clinical objects are versioned; corrections create new versions, never silent overwrites |
| Capability negotiation | FHIR `CapabilityStatement`; HL7 v2 profile declarations; graceful rejection of unsupported structures |
| Identifier resolution | External → internal identity mapping with explicit collision handling; PIX-style resolution service |
| Acknowledgement | HL7 ACK semantics; FHIR HTTP status codes + OperationOutcome |
| Security of exchange | TLS for transport; structured interop is authenticated/authorized; audit logged |
| Terminology | See [`terminology-strategy.md`](terminology-strategy.md) |

## Interop boundary checklist for future phases

- [ ] Define inbound/outbound contract fixtures per standard (no real data).
- [ ] Design quarantine/review workflow for failed messages.
- [ ] Decide HL7 version & profile scope per jurisdiction (legal review item).
- [ ] Decide FHIR R4 vs new releases for conformance target.
- [ ] Verify IHE profile statuses against the IHE registry.
- [ ] Define consent/interaction constraints for outbound reports.

## Explicitly not planned

- A proprietary JSON envelope that replaces HL7/FHIR semantics.
- Vendor-specific device protocols inside the core (see
  [`../laboratory/device-integration-strategy.md`](../laboratory/device-integration-strategy.md)).
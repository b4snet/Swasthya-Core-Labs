# Validation Strategy

**Document owner:** Architecture / Quality & Validation | **Status:** Approved (Phase 0) | **Version:** 1.0

## Purpose and status

Software validation strategy for Swasthya Core Labs. It is a **strategy and
commitment framework**; no validation has been performed and **no product
claim** is made. Applicability of formal (regulated) validation depends on
regulatory review — see [`../compliance/`](../compliance/).

## Scope

Validation covers:

- Software/system validation of the LIS platform.
- Device/analyzer integration validation (**runtime validation against real
  devices or approved simulators** is mandatory before any interoperability
  claim).
- Interoperability conformance validation (HL7 v2 profiles, FHIR conformance,
  IHE-affiliated interactions).

## Approach

1. **Risk-based, documented**: activities proportional to the risk of each
   change (GAMP-style thinking applied pragmatically; formal classification
   pending regulatory review).
2. **Requirement traceability**: functional requirements ↔ design ↔ tests ↔
   evidence (traceability matrix).
3. **Test pyramid**: unit (domain logic), integration (API, persistence,
   adapters), contract (interop fixtures), and acceptance (end-to-end
   workflows).
4. **Deterministic CI**: the quality gate
   ([`verify.ps1`](../../scripts/verify.ps1)) is the executable baseline;
   evidence is generated/recorded per release.
5. **Config-driven rules == validation scope**: since clinical rules,
   reference intervals, and flags are configuration, validation must cover
   the **authoring and governance** of configuration content, not only code.
6. **Immutability verification**: append-only/versioning behavior of the
   clinical record is a first-class test area
   ([`../clinical/clinical-data-integrity.md`](../clinical/clinical-data-integrity.md)).

## Evidence model (future phases)

| Artifact | Purpose |
| --- | --- |
| Requirements/spec (docs + ADRs) | Design-controlled baseline |
| Test plans & automated suites | Executable evidence |
| Validation execution records | Dated, signed (accountability) evidence |
| Configuration change records | Traceability of rules changes |
| Device validation reports | Device-specific runtime validation evidence |
| Release records | Versioned, reproducible builds |

## Phase association

Validation evidence generation begins with the first clinical-domain
implementation (Phase 2+) and scales with interoperability (Phases 40–44) and
device integration (Phases 23–26).

**Current position (Phase 7):** Laboratory result entry and observation
completion is the current completed phase (Phase 7 → ACCEPTED / GREEN /
FROZEN), covering typed `results` observations, their entry/retrieval/listing
and editable update, and the `results` table via migration
`20260918035033_AddResultEntity`. Phase 7 documentation does **not** claim any
clinical validation, verification, finalization, amendment, or correction
behavior ([ADR-0021](../decisions/ADR-0021.md)).

**Phase 8 NOT started:** result validation, verification, finalization and
controlled correction are **not implemented** and must not begin without
explicit authorization.

## Gaps requiring resolution

- Applicability and formal regime (ISO 15189 software validation, ISO 17025
  data integrity, medical device software regulation, in-country rules) —
  **legal/regulatory review items** (see
  [`../compliance/regulatory-compliance-strategy.md`](../compliance/regulatory-compliance-strategy.md)).
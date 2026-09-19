# Clinical Data Integrity

**Document owner:** Architecture / Clinical Informatics | **Status:** Approved (Phase 0) | **Version:** 1.0

## Purpose

Architectural rules that guarantee the clinical record can never be silently
rewritten. These are **design constraints**; the enforcement mechanisms are
implemented in later phases. No future change may weaken these rules.

## Core principles

1. **Original-result preservation.** The original measurement/observation,
   as entered or received, is preserved forever in an append-only store. It
   may be superseded, amended, or corrected — never deleted or overwritten
   in place.
2. **Append-only finalized records.** Clinical records that enter a
   finalized/validated state are immutable: no in-place mutation.
   Corrections create new record versions referencing prior versions
   ([ADR-0006](../decisions/ADR-0006.md)).
3. **Provenance on every observation.** Every clinical value records *who*
   (operator, or external source), *what* (analyzer/instrument/method),
   *when* (obtained, entered, validated), *how* (device protocol vs. manual),
   and *why* if derived. Provenance cannot be retroactively edited.
4. **Canonical units & timestamps.** Values are stored with canonical
   (UCUM-aligned) units; all timestamps are captured in UTC with original
   local time retained where meaningful. Unit conversions are presentation
   concerns and never overwrite the stored value.
5. **Specimen traceability.** A specimen chain (source → container → aliquots
   → work items) is retained; observations reference the exact work
   item/specimen they came from.
6. **Result status lifecycle.** A governed status model
   (e.g., pending / in-progress / validated / final / corrected / superseded)
   decides mutability: only non-finalized states are editable; final states
   require amendment/correction flows.
7. **Amendments vs. corrections.** Two distinct, separately-audited flows:
   - **Amendment**: additional information appended to a final record
     (comment, clarification) without changing the original values.
   - **Correction**: supersession of a value with a new version, linking to
     the prior version; the prior version remains retrievable.
8. **Version history.** Every clinical entity keeps an immutable version
   chain; queries can always resolve "as-of" versions.
9. **Critical-result traceability.** Handling of critical/urgent results is
   traceable end-to-end (value → alert → acknowledgement → notification →
   response), with timestamps and actors recorded at each step.
10. **Auditability.** All state transitions are recorded (time, actor,
    reason) and cannot be altered. See
    [`../data-governance/data-governance.md`](../data-governance/data-governance.md).

## Consequence mapping

| Requirement | Consequence |
| --- | --- |
| No silent overwrite | Append-only persistence design; no `UPDATE` of finalized observation rows |
| Provenance | Provenance entities modeled as first-class; immutable after finalization |
| Critical traceability | Notification/acknowledgement events appended to the record |
| Operator/analyzer provenance | Actions and device identity bound to observations |

## Explicit non-goals (Phase 0)

- No implementation of these mechanics yet (no schema, no store, no API).
- No clinical rules, reference ranges, or value thresholds defined.

## Validation reference

Verification of these integrity behaviors is part of the validation strategy:
see [`../validation/validation-strategy.md`](../validation/validation-strategy.md).
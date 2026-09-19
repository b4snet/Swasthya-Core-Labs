# Quality Architecture

**Document owner:** Architecture / Laboratory Quality | **Status:** Approved (Phase 0) | **Version:** 1.0

## Purpose

Defines the **future architecture** for laboratory quality management.
Nothing here is implemented, and **no clinical quality rules** (thresholds,
Westgard rules, etc.) are defined or invented. Rule sets arrive as
**configuration** in later phases.

## Future components

| Component | Description |
| --- | --- |
| Internal quality control (QC) | Periodic control material testing, per-instrument/per-analyte control charts |
| Calibration | Calibration events, calibration curves/settings, validity windows |
| Lot tracking | Control material, calibrator, and reagent lot identification and usage history |
| Reagent tracking | Reagent lots, open-date tracking, expiry management |
| Controls | QC material registration with expected ranges (configured by the laboratory) |
| Corrective actions | Structured capture of problems → investigation → action → verification |
| Analyzer maintenance | Instrument maintenance scheduling, service events, downtime records |
| Proficiency / external testing | PT/EQA event registration, results submission boundary, participant coding |
| Result flags | Flag definitions (e.g., critical, abnormal) that are **config data**, never invented in code |
| Specimen quality | Sample integrity assessment, hemolysis/icterus/lipemia/clotted indicators (configured) |
| Rejection reasons | Standardized rejection reason catalog (config) |
| Quality events | A unified event log linking QC, corrections, maintenance, and quality incidents |

## Architecture rules

1. **Config over code**: every rule, range, flag, and rejection reason is
   configuration with provenance ([ADR-0008](../decisions/ADR-0008.md)).
2. **Traceability**: quality events link to instruments, lots, specimens, and
   results where relevant — supporting full audit trails
   ([`../data-governance/data-governance.md`](../data-governance/data-governance.md)).
3. **Non-blocking abstraction**: failed QC must never silently corrupt the
   clinical record; outcomes feed validation gating in later phases.
4. **Proficiency confidentiality**: external testing workflows must preserve
   required anonymization/identification rules per program — compliance
   review item.
5. **Auditability**: every status change in the quality subsystem is
   appended to the audit trail.

## Regulatory alignment (future)

Quality architecture must be reconcilable with ISO 15189, CLSI guidance, and
applicable accreditation/legal review items —
see [`../compliance/standards-applicability.md`](../compliance/standards-applicability.md).

## Phase association

Quality components are planned for **Phases 45–46** in
[`../../ROADMAP.md`](../../ROADMAP.md).
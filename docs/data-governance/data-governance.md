# Data Governance

**Document owner:** Architecture / Data Governance | **Status:** Approved (Phase 0) | **Version:** 1.0

## Purpose

Governance rules for data the platform creates, stores, and exchanges:
ownership, classification, retention, integrity, and provenance.

## Data ownership and stewardship

- **Clinical data** belongs to the operating laboratory/institution; the
  platform is a steward.
- **Configuration data** (catalog, mapping, rules) is content owned by
  authorized administrators, versioned and provenance-tracked.
- **Audit data** is owned by the platform operators and is immutable.

## Retention and deletion

Principles (detailed policy per jurisdiction in later phases):

- Official records (laboratory records, final reports) are retained
  for legally required minimum periods — **requirement must be sourced from
  applicable law** (legal review item).
- Deletion is **logical and audited**: finalized clinical records are
  never silently purged; requests remove linkage/minimize exposure while
  preserving audit obligations.
- Backups follow the same retention rules, encrypted and access-restricted.

## Provenance and audit

- Append-only audit trail records every access, change, export, and security
  event (see [`clinical/clinical-data-integrity.md`](../clinical/clinical-data-integrity.md)).
- Provenance metadata is preserved across integration boundaries (who/what/
  when/how + source system/version) —
  see [`interoperability/interoperability-strategy.md`](../interoperability/interoperability-strategy.md).

## Data quality

- Identifier and terminology standardization are data-quality functions:
  canonical identifiers (LOINC/SNOMED/UCUM mapping), deduplication rules with
  review workflows — no silent merges.
- Metadata (units, timestamps, specimen chain, instrumentation) is treated as
  data requiring the same integrity rules as the values.

## Sensitive data storage

- Sensitive fields handled per [`privacy/privacy-architecture.md`](../privacy/privacy-architecture.md):
  field-level protection, minimization, no PHI/PII in logs.

## Artifacts required in later phases

- Data dictionary and classification registry.
- Retention schedule configuration per jurisdiction.
- Log/access export and review tooling.
- Records-of-processing inventory (for privacy posture).
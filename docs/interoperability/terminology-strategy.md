# Terminology Strategy

**Document owner:** Architecture / Clinical Informatics | **Status:** Approved (Phase 0) | **Version:** 1.0

## Goals

- Represent laboratory concepts with **authoritative standard terminology**.
- Support **mapping** between internal codes and external systems without
  inventing a proprietary terminology standard.
- Make terminology **configuration**, not code.

## Standard terminology

| Standard | Role | Status |
| --- | --- | --- |
| **LOINC** | Primary code system for laboratory tests/observations (results, panels, specimen attributes) | Apply in catalog/observation design |
| **SNOMED CT** | Clinical concepts: findings, procedures, specimen types, body structures where applicable | Apply where concept modeling is needed |
| **UCUM** | Units of measure (canonical unit expressions) | Apply to all measurable observations |
| **ICD** | Diagnosis classification for billing/clinical use | **Integration boundary** — typically owned by external billing/EMR; not core |
| Vendor/local codes | Device and HMS-native codes | Mapped to authoritative codes via terminology mapping |

## Architecture principles

1. **Authoritative sources only**: LOINC/SNOMED CT values come from licensed,
   versioned releases via a terminology resource. No charts/ranges are
   invented by the platform ([ADR-0008](../decisions/ADR-0008.md)).
2. **Codeable by design**: clinical concepts carry a code system + code +
   human-readable display; never a bare unstructured string.
3. **Mapping registry**: an explicit, versioned mapping table
   (internal/local ↔ LOINC ↔ SNOMED ↔ vendor) managed as data with
   provenance; mapping collisions are reviewable workflow items, not silent
   overwrites.
4. **Units always UCUM**: observations must be stored with canonical UCUM
   units; display conversions are presentation concerns.
5. **Versioning**: every terminology snapshot is versioned (release identity);
   records reference the version they used, enabling later re-coding.
6. **No proprietary content**: official terminology data is licensed; this
   repository contains the strategy and references, **not** copyrighted
   terminology content (see [`../../NOTICE.md`](../../NOTICE.md)).

## Terminology boundary

- **Embedded vs. external server**: unresolved (see
  [`domain-boundaries.md`](../architecture/domain-boundaries.md)); both are
  supported by the mapping architecture.
- **SNOMED licensing**: distribution requires license terms per territory —
  legal review item.
- **LOINC** is managed by Regenstrief Institute; usage/licensing must be
  confirmed for commercial distribution.
- **ICD** is owned by WHO; use in external billing/reporting contexts has its
  own licensing/jurisdictional rules.

## Future implementation checklist

- [ ] Decide terminology service topology (embedded vs. external).
- [ ] Obtain/confirm required terminology licenses (LOINC, SNOMED CT, UCUM).
- [x] Design terminology mapping registry model with provenance
  (Phase 2: `code_systems` + `test_terminology_mappings` structure; no content)
  — [ADR-0017](../decisions/ADR-0017.md).
- [x] Define unit canonicalization approach (Phase 2: UCUM expression stored and
  structurally validated; conversion/canonicalization deferred) — [ADR-0017](../decisions/ADR-0017.md).
- [x] Define code system/version metadata model shared with interoperability
  (Phase 2 structure) — [ADR-0017](../decisions/ADR-0017.md).
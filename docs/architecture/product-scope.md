# Product Scope

**Document owner:** Architecture | **Status:** Approved (Phase 0) | **Version:** 1.0

## What Swasthya Core Labs is

A standalone, enterprise-grade **Laboratory / Diagnostic Information System
(LIS/DIS)** that:

- operates independently as deployable infrastructure for diagnostic
  laboratories and diagnostic networks;
- integrates with arbitrary external Hospital Management Systems (HMS),
  EHR/EMR platforms, clinics, laboratories, and diagnostic networks;
- supports laboratory disciplines and diagnostic workflows (Phase 1+);
- is **API-first**, **interoperability-first**, **security/privacy-first**,
  and **audit/provenance-first**;
- is **configuration-driven** where clinical or jurisdictional rules vary;
- is designed for **multi-site and enterprise deployment**.

## What Swasthya Core Labs is not

- Not a hospital management system.
- Not a billing system (billing is an integration boundary).
- Not a PACS (PACS is an integration boundary).
- Not a clinical decision support engine (CDS is an integration boundary).
- Not an identity authority for healthcare (external identity is a boundary).
- Not coupled to any specific HMS/EHR/EMR vendor.

## Responsibility separation

| Responsibility | Owner |
| --- | --- |
| Orders, accessioning, worklists, observations, results validation, reporting, QC context, laboratory workflows | **Swasthya Core Labs (LIS/DIS core)** |
| Patient registration, scheduling, clinical context, care workflows | **External HMS/EHR/EMR** |
| Financials, insurance, claims, payments | **External billing systems** |
| National/building identity assertions, patient identity authority (where applicable) | **External identity providers** |
| Image capture, storage, retrieval, viewing | **External PACS / DICOM endpoints** |
| Physical measurement generation, raw device data | **Analyzers / instruments (devices)** |
| Clinical interpretation, treatment rules, diagnostic reasoning | **External CDS / clinical staff** |

Swasthya Core Labs contains the LIS/DIS responsibilities and defines
interfaces for every boundary; it does not attempt to absorb external
responsibilities. See [`domain-boundaries.md`](domain-boundaries.md) for the
bounded-domain mapping.

## Phase 0 scope

- Product boundary and system definition.
- Domain architecture and standards applicability.
- Security, privacy, data-integrity, quality, validation, and
  interoperability architecture foundations (documentation).
- Repository engineering: .NET 10 solution skeleton with health/version
  endpoints only, CI foundation, deterministic quality gate.

**Explicitly out of Phase 0 scope:** laboratory workflows, tests, panels,
analyzers, QC execution, reporting, amendments, and production integrations.

## Success criteria for the foundation

1. Every future clinical feature lands inside a documented bounded domain
   with a core/integration classification.
2. No clinical logic exists before its domain, integrity, and validation
   architecture is defined.
3. The API-first, standards-native interoperability boundary is established
   without a proprietary envelope protocol.
4. Quality gates (build/format/test/vulnerability/secret scan) pass
   deterministically.
5. Legal/regulatory review items are tracked explicitly (see
   [`docs/compliance/`](../compliance/)).
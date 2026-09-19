# Laboratory Taxonomy

**Document owner:** Architecture / Laboratory Informatics | **Status:** Approved (Phase 0; axes implemented in Phase 2) | **Version:** 1.1

## Purpose

A **discipline taxonomy** the platform must be able to represent. The core is
**discipline-neutral**: it models orders, specimens, work items, observations,
and devices without encoding any single discipline. Discipline-specific
behavior arrives through configuration and adapters in later phases.

This document defines **capability scope**, not a test list. No tests,
methods, reference ranges, or diagnostic thresholds are defined or invented.

## Disciplines

| # | Discipline | Notes |
| --- | --- | --- |
| 1 | Hematology | CBC, morphology, hemostasis-related hematology |
| 2 | Clinical chemistry | General, automated chemistry |
| 3 | Immunology | Humoral/cellular immunology assays |
| 4 | Serology | Antibody/antigen detection |
| 5 | Microbiology | Culture, identification, susceptibility |
| 6 | Bacteriology | Sub-discipline of microbiology |
| 7 | Virology | Viral detection/serology |
| 8 | Parasitology | Parasite detection |
| 9 | Mycology | Fungal detection |
| 10 | Molecular diagnostics | PCR/NAT, amplification |
| 11 | Coagulation | Clotting studies, hemostasis |
| 12 | Blood bank / transfusion medicine | Compatibility, crossmatch, component processes |
| 13 | Urinalysis | Routine urine analysis |
| 14 | Clinical microscopy | Manual microscopy |
| 15 | Pathology interfaces | Anatomic pathology, histopathology hand-offs |
| 16 | Cytology interfaces | Cytology workflows/hand-offs |
| 17 | Genetics / molecular workflows | Cytogenetics, molecular genetics |
| 18 | Toxicology | Therapeutic drug monitoring, drugs of abuse |
| 19 | Endocrinology | Hormone assays |
| 20 | Special chemistry | Specialized chemistry panels |
| 21 | Point-of-care testing (POCT) | Decentralized testing integration |
| 22 | Other / extensible | Disciplines discovered during authoritative analysis |

## Extensibility rules

- New disciplines must not require core code changes: they are modeled as
  **configured taxonomy entries** (catalog, worklist templates, flag sets,
  report templates).
- The discipline axis is independent from the **specimen axis**, the
  **device axis**, and the **observation axis**; combinations are data-driven.
- LOINC/SNOMED CT provide the codeable representation for catalog items and
  observations ([`../interoperability/terminology-strategy.md`](../interoperability/terminology-strategy.md)).

## Boundary of this taxonomy

- **In scope now (Phase 2 implemented)**: disciplines and laboratory sections
  exist as configured, organization/facility-scoped master data (no hardcoded
  discipline enum); test/panel definitions, units, and reference-interval
  structures reference these axes ([ADR-0015](../decisions/ADR-0015.md)).
- **Out of scope now (later phases)**: catalog content, methods, workflows,
  validation rules, reference-interval values, report layouts, and device
  mappings.
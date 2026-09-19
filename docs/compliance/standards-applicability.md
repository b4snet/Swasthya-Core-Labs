# Standards Applicability

**Document owner:** Architecture / Compliance | **Status:** Approved (Phase 0) | **Version:** 1.0

## Method

Each standard/regulation/framework is classified:

| Classification | Meaning |
| --- | --- |
| **Applicable** | Directly shapes design; adopt/apply in scope |
| **Potentially Applicable** | Relevant in defined scenarios/territories; conditional |
| **Not Applicable** | No current relevance to the product scope |
| **Requires Legal/Regulatory Review** | Impact depends on law, market, or certification path |

This inventory is a planning instrument; it is **not a claim of compliance or
certification**. No proprietary standards text is reproduced. Official
terminology and source references are provided for every entry.

## Interoperability & terminology standards

| Standard | Classification | Reason | Architectural consequence | Source/version |
| --- | --- | --- | --- | --- |
| HL7 v2 | **Applicable** | Dominant messaging standard for laboratory order/result exchange with HMS/EHR/LIS systems | Messaging adapter layer; ACK semantics; message validation & quarantine | HL7 v2.5.1 (verify per jurisdiction) |
| HL7 FHIR | **Applicable** | REST API standard for diagnostics resources; primary modern API target | FHIR resource model alignment; conformance (CapabilityStatement, StructureDefinition); version policy | FHIR R4 (consider later releases) |
| DICOM & DICOMweb | **Potentially Applicable** | Imaging is an integration boundary (PACS); pathology/cytology references may flow through DICOM metadata | DICOMweb HTTP adapters for study references only; core stores no images | DICOM PS3.x |
| IHE profiles | **Potentially Applicable** | Provide profile-based interoperability contracts (laboratory + identity profiles) | Design navigation; exact profile status verified against IHE registry before implementation | IHE PaLM & ITI domains |
| LOINC | **Applicable** | Canonical code system for lab tests/observations | Catalog + observation modeling use LOINC; versioned releases; licensing | Regenstrief Institute |
| SNOMED CT | **Potentially Applicable** | Concepts (findings/procedures/specimens) useful for semantic modeling | Terminology mapping registry; license per territory | SNOMED International |
| UCUM | **Applicable** | Canonical units of measure for all measurements | Store canonical units; display conversion only | UCUM (Regenstrief) |
| ICD | **Potentially Applicable** | Diagnosis classification in billing/reporting contexts owned by external systems | Integration boundary; not core modeling | WHO ICD-10/11 |

## Quality & laboratory management standards

| Standard | Classification | Reason | Architectural consequence | Source/version |
| --- | --- | --- | --- | --- |
| ISO 15189 | **Potentially Applicable** | Accreditation framework for medical laboratories; informs quality architecture & validation evidence | Quality subsystem, audit trail, retention, validation strategy alignment; **not claimed** | ISO 15189:2023 |
| ISO/IEC 17025 | **Potentially Applicable** | Calibration/testing lab competence (if platform supports calibration/third-party testing scope) | Data-integrity features (traceability, uncertainty awareness) | ISO/IEC 17025:2017 |
| CLSI guidance | **Potentially Applicable** | Benchmark for lab practices (QC, specimen handling) feeding configuration design | Config-driven rules; QC architecture alignment | CLSI current editions |

## Privacy, security & software quality

| Standard | Classification | Reason | Architectural consequence | Source/version |
| --- | --- | --- | --- | --- |
| HIPAA | **Potentially Applicable** | US-specific; relevant only if product serves US customers/business associates | Then: PHI safeguards, BAA support, breach notification design | 45 CFR 160/164 |
| ISO/IEC 27001 | **Potentially Applicable** | ISMS certification target for the operating organization | Security architecture baseline; control evidence | ISO/IEC 27001:2022 |
| ISO/IEC 27701 | **Potentially Applicable** | Privacy information management extension | Privacy controls, records of processing | ISO/IEC 27701:2019 |
| OWASP ASVS | **Applicable** | Web application security verification baseline | Secure coding standards, verification levels, review gates | OWASP ASVS current |
| OWASP API Security Top 10 | **Applicable** | API-specific security risks | API design rules: authZ, rate limiting, validation, logging | OWASP API Security Top 10 current |

## Medical device / software regulation

| Instrument | Classification | Reason | Architectural consequence | Source/version |
| --- | --- | --- | --- | --- |
| Medical Device Regulation (EU MDR) | **Requires Legal/Regulatory Review** | Applies only if classified as a medical device for EU market; laboratory software classification is case-specific | If applicable: quality system, technical documentation, conformity assessment | EU 2017/745 |
| FDA (US) | **Requires Legal/Regulatory Review** | LIS software may or may not fall within FDA jurisdiction (device status determination required); only where the actual function is FDA-regulated | If applicable: 21 CFR 820 QMS, 510(k)/De Novo path analysis | FDA (21 CFR 820) |
| In-country medical device / digital-health rules | **Requires Legal/Regulatory Review** | Applicable national rules (e.g., Nepal) for diagnostic software must be confirmed | Compliance evidence, registration, and labeling obligations | Determined per territory |

## Nepalese requirements (in-country scope)

| Requirement | Classification | Reason | Architectural consequence | Source/version |
| --- | --- | --- | --- | --- |
| National health & laboratory policy/infrastructure | **Potentially Applicable** | MoHP policy and laboratory standardization frameworks shape facility/reporting expectations | Facility/reporting configuration aligned to national norms; verify current policy | Ministry of Health and Population (MoHP), Nepal |
| Health information management / interoperability | **Requires Legal/Regulatory Review** | National eHealth interoperability mandates must be confirmed with current policy | Interop adapter scope tuned to national standards; verify via MoHP/DOH | MoHP, Nepal |
| Privacy / personal data protection | **Requires Legal/Regulatory Review** | Comprehensive privacy law status is evolving (draft data protection legislation); obligations to be confirmed | Consent, retention, sensitive-data handling configurable per jurisdiction | Current legislation/draft — verify with legal counsel |
| Electronic transactions / records law | **Requires Legal/Regulatory Review** | Electronic Transactions Act (2063 B.S., 2008) and related rules affect electronic records/order acceptance | Electronic record authenticity and audit evidence design | Nepal Law |

> **Careful:** entries marked *Requires Legal/Regulatory Review* must be
> validated against authoritative sources by qualified counsel before any
> compliance or product-scope decision. This document records uncertainty
> explicitly rather than asserting obligations.

## Consolidated design implications

- Design once for **configurable jurisdiction**: retention schedules, consent,
  breach notification, sensitive categories, and interop profiles are
  configuration, never code (see [ADR-0008](../decisions/ADR-0008.md)).
- Mandate **audit + provenance + append-only** foundation regardless of
  jurisdiction (safe under all listed regimes).
- Keep **validation evidence** architecture ready for ISO 15189 / MDR / FDA
  outcomes ([`../validation/validation-strategy.md`](../validation/validation-strategy.md)).
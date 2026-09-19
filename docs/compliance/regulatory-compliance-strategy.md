# Regulatory & Compliance Strategy

**Document owner:** Architecture / Compliance | **Status:** Approved (Phase 0) | **Version:** 1.0

## Principle

Swasthya Core Labs **never claims compliance or certification it has not
attained**. This document records the strategy for determining and achieving
the applicable regulatory posture. Every conclusion here is provisional until
validated by qualified legal/regulatory review.

## Strategy

1. **Document first**: applicability inventory maintained in
   [`standards-applicability.md`](standards-applicability.md), updated as
   product scope and markets evolve.
2. **Legal review gates**: no compliance claims, no device classification
   decisions, and no cross-territory commitments without counsel sign-off.
3. **Design once, configure per jurisdiction**:
   - retention schedules, consent/preferences, sensitive-category handling,
     breach-notification touchpoints, and interop profiles as configuration;
   - audit/provenance/append-only foundation shipped in all markets.
4. **Validation evidence readiness**: validation strategy builds the artifact
   trail needed by ISO 15189, MDR, FDA, and in-country regimes without
   committing to a specific regime prematurely.
5. **Market entry checklist** (prospective):
   - Nepal: confirm laboratory regulation, health-information and privacy
     obligations, electronic records rules.
   - Other markets: confirm medical-device/software classification per
     territory (EU MDR, FDA, etc.) before scope commitments.

## Legal / regulatory review items (register)

| # | Item | Needed action | Status |
| --- | --- | --- | --- |
| R1 | Nepalese privacy/data protection law (incl. draft legislation) | Confirm obligations (consent, breach, retention, cross-border) | OPEN |
| R2 | Nepalese laboratory & health-information policy/interoperability | Confirm national standards and reporting norms | OPEN |
| R3 | Electronic transactions/records law application to LIS records | Confirm authenticity/evidence requirements | OPEN |
| R4 | Medical-device/software classification per market (EU MDR, FDA, in-country) | Device status determination | OPEN |
| R5 | ISO 15189 accreditation path for laboratory customers | Confirm documentation requirements for software support | OPEN |
| R6 | LOINC / SNOMED CT / UCUM distribution licensing for commercial product | Secure required licenses | OPEN |
| R7 | HIPAA applicability only if US market/BAA | Confirm market scope | OPEN |
| R8 | Software validation regime (GAMP-like vs. regulated QMS) | Confirm applicable QMS scope | OPEN |

## What is NOT claimed

- No certification under ISO 15189 / ISO 27001 / ISO 27701.
- No FDA clearance/approval, CE marking, or in-country device registration.
- No analyzer interoperability without runtime validation.
- No clinical correctness of any rule, range, or threshold (none are
  defined).

## Maintenance

Compliance documents are versioned (header: owner/status/version). Updates
require updating [`standards-applicability.md`](standards-applicability.md)
and this strategy together, and — for legal-impact changes — recording them
as review items (R# above).
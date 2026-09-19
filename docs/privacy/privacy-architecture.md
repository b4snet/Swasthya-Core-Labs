# Privacy Architecture

**Document owner:** Architecture / Privacy | **Status:** Approved (Phase 0) | **Version:** 1.0

## Purpose and status

Privacy architecture for processing personal health information (PHI) and
personally identifiable information (PII) in a laboratory/diagnostic context.
Design foundation only; **no compliance or certification is claimed**.
Jurisdiction-specific obligations (notably Nepal privacy law, which is
evolving) require legal review —
see [`../compliance/regulatory-compliance-strategy.md`](../compliance/regulatory-compliance-strategy.md).

## Data classification

| Class | Examples | Handling |
| --- | --- | --- |
| PHI (health) | Test orders, observations, diagnoses, pathology text, images references | Restricted access, audit-logged reads/writes, encryption at rest, minimized exposure |
| PII (identity) | Patient identifiers, demographics, contact details | Restricted access, minimization, retention policy |
| Sensitive fields | Marked high sensitivity (e.g., certain test categories per jurisdiction) | Masking/field-level encryption; role-gated |
| Internal | Config, audit metadata (non-clinical) | Standard protection |
| Public | Product metadata, published docs | None |

## Privacy principles

1. **Data minimization**: collect/process only what a workflow requires.
2. **Purpose limitation**: PHI/PII processed only for the documented
   diagnostic purpose (plus authorized secondary uses, e.g., proficiency/
   quality per legal framework).
3. **Consent & lawful basis per jurisdiction**: consent/legitimate-basis
   handling is configurable and jurisdiction-scoped (legal review item).
4. **Access least-privilege**: facility + role + data-class gating with full
   audit trail (see [`security-architecture.md`](../security/security-architecture.md)).
5. **Retention minimization**: documented retention schedules; deletion
   preserves audit/provenance obligations (see
   [`data-governance.md`](../data-governance/data-governance.md)).
6. **Transparency**: patients/clinicians can obtain records and correction
   flows (subject-access readiness per jurisdiction).
7. **Breach response readiness**: the platform produces the evidence artifacts
   (logs, access history, export history) required by breach-notification
   obligations.

## Boundary of privacy responsibility

- **Core**: PHI/PII capture, access control, audit, retention, export,
  correction paths.
- **Integration**: external HMS/EHR, billing, identity providers, and
  notification channels process their copies under their own obligations;
  interfaces must state what is transmitted and why.

## Privacy-in-architecture artifacts (future phases)

- Field-level sensitivity metadata on the data model.
- Consent/preference registry (jurisdiction-scoped).
- Subject access / correction / deletion request workflows.
- Data processing inventory (records of processing activities).
- Privacy impact assessments for each new processing feature.

## Related documents

- [`security-architecture.md`](../security/security-architecture.md)
- [`data-governance.md`](../data-governance/data-governance.md)
- [`regulatory-compliance-strategy.md`](../compliance/regulatory-compliance-strategy.md)
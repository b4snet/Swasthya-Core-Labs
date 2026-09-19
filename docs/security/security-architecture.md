# Security Architecture

**Document owner:** Architecture / Security | **Status:** Approved (Phase 0) | **Version:** 1.0

## Purpose and status

Foundational security architecture for Swasthya Core Labs. This is a **design
foundation**; the controls are implemented in later phases. This document
makes **no claim of compliance or certification** — see
[`../compliance/`](../compliance/).

## Threat model posture

- Diagnostics infrastructure processes **PHI/PII**; confidentiality,
  integrity, and **availability** of the clinical record are critical.
- The largest risks: data breach via excessive access, silent record
  tampering, malicious or malformed inbound interop traffic, exfiltration via
  logs/backups, and supply-chain compromise of dependencies.

## Security objectives

### Authentication
- Identity providers for humans (with MFA) and service identities
  (mTLS or OAuth 2.0 client credentials) are separated.
- External identity is an **integration boundary**; internal authentication is
  core ([ADR-0007](../decisions/ADR-0007.md)).

### Authorization
- **RBAC for roles** with **ABAC-style attribute constraints** where
  needed (facility, patient scope, data sensitivity).
- **Least privilege** by default; deny by default.
- **Organization/facility isolation**: all clinical data access is scoped to
  facility/tenant context; cross-tenant access is an explicit, audited
  exception or an integration path.

### Tenant isolation
- Multi-site deployments isolate data and configuration per facility; the
  model is defined in [ADR-0007](../decisions/ADR-0007.md).

### API security
- OWASP API Security Top 10 as the baseline for API design (see
  [`standards-applicability.md`](../compliance/standards-applicability.md)).
- Versioned, documented contracts; strict validation of all input.
- Service-to-service authentication for internal APIs.

### Cryptography
- **In transit**: TLS (HTTP/2/3) everywhere, including interop endpoints.
- **At rest**: full-disk/database-at-rest encryption; encryption keys
  managed via a **secrets/key management service**, rotated on schedule.
- **Key management**: no keys in source; key hierarchy (KEK/DEK) where
  required; backups encrypted.

### Secrets management
- All secrets (DB credentials, API keys, interop certificates) provisioned
  at runtime from a secrets manager.
- `.env`-style local config uses `.env.example` placeholders only; real
  values never committed.

### Audit logging
- Security events (auth, authz, access to PHI, admin actions, integration
  failures) recorded in **immutable, append-only** audit log.
- **Secure logging**: no PHI/PII, credentials, tokens, or secrets in logs;
  structured fields only; log data encrypted; retention per policy.

### PHI/PII protection
- Data **minimization**, **classification** (see
  [`../privacy/privacy-architecture.md`](../privacy/privacy-architecture.md)),
  **sensitive-field handling** (masking/encryption at rest for sensitive
  fields), and de-identification where appropriate.
- **No real patient data anywhere in the repository** — including tests,
  fixtures, documentation, and logs (enforced by secret scan + review).

### Abuse prevention
- **Rate limiting** and throttling on public/API endpoints.
- Anti-automation controls and DDoS-minded gateway limits.

### Session/token security
- Short-lived access tokens; refresh rotation; revocation on re-
  authentication; HttpOnly/Secure cookie flags for browser-bound tokens.

### Backup security
- Backups encrypted; access restricted; restoration tested; retention per
  data-governance policy.

### Retention/deletion
- Clinical data retention and deletion follow documented legal/jurisdictional
  policy (see [`../data-governance/data-governance.md`](../data-governance/data-governance.md)).
- Deletion of clinical records is **logical/auditable**, preserving
  provenance and audit obligations; no silent physical purge of finalized
  clinical records.

### Breach / security-event handling
- Detection, containment, notification, forensic, and documentation
  boundaries are **process obligations**; the platform must provide the
  supporting evidence artifacts (audit log, access records, integrity proofs).
- Notification obligations are jurisdiction-specific (breach notification) —
  legal review item.

## Implementation roadmap

- Phase 1: identity/authN/authZ core, secret hygiene, secure logging, audit
  store foundation, facility isolation enforcement.
- Phases 2–4: per-API security hardening as features land
  (interop validation, device ingest, notification channels).
- Continuous: OWASP ASVS-based review, dependency/vulnerability scanning,
  gitleaks, secrets hygiene.

## Phase 1 implementation status

Implemented in Phase 1 (see ADR-0009…0013):

- **Authentication:** ASP.NET Core JwtBearer validates signature, issuer,
  audience, lifetime against `Authentication:Issuer/Audience` +
  `SigningKeyBase64` or `JwksUrl`. Inbound claim mapping disabled; identity
  is `iss` + `sub` from the validated token. **`client_id` is never treated
  as identity.** Service principals are recognized via
  `Authentication:PrincipalTypeClaim` (`principal_type`) when its value
  equals `Authentication:ServicePrincipalTypeValue` (`service`).
- **Authorization:** one `identity-tenancy` policy; `AuthContext` resolved
  once per request and cached on `HttpContext`. RBAC via
  `principal_role_assignments → role → role_permissions → permission`,
  org-scoped or org+facility-scoped; deny-by-default. Approval gates:
  404/403 Problem Details via `PermissionDeniedException`.
- **Tenant isolation:** application-level enforcement (no RLS in Phase 1);
  `PermissionGuard` + scoped repositories; `<Guid.Empty>` org scope rejected.
  Cross-tenant access → 403.
- **Audit:** append-only enforced in PostgreSQL (trigger raises `55000` on
  UPDATE/DELETE of `audit_records`); non-PHI allowlisted metadata only;
  correlation ID recorded and echoed.
- **Abuse prevention:** global fixed-window rate limit (100/min/IP) → 429.
- **Secrets hygiene:** production fail-fast (issuer, audience, connection
  string, signing key or JWKS); `scripts/scan-secrets.ps1` + CI gitleaks.

Still foundational/deferred: key/material management, at-rest/Disk-level
encryption, session/refresh boundaries, per-role rate limits, RLS.

## Phase 2 implementation status

Implemented in Phase 2 (see ADR-0015…0018):

- **Master-data authorization:** organization-scoped master-data creation
  requires an **organization-wide grant** (`FacilityId is null`); facility-scoped
  changes require a grant at that facility or an organization-wide grant.
  A facility-only grant **never** authorizes another facility's data
  (`PermissionGuard`, `LaboratoryScope`).
- **Scope enforcement:** reads and writes resolve facility scope from server-side
  grants, never from client-supplied claims or route/body values alone;
  organization/facility rows are filtered accordingly.
- **Abuse/validation:** lifecycle transitions, codes, and datatypes are
  validated; conflicts and concurrency violations map to 409, validation to 400,
  scoping/authorization to 403/404, with Problem Details and no internal
  detail leakage.
- **Audit:** master-data create/modify/activate/deactivate/retire recorded via
  the Phase 1 allowlisted, append-only audit mechanism.

## Related documents

- [`privacy-architecture.md`](../privacy/privacy-architecture.md)
- [`data-governance.md`](../data-governance/data-governance.md)
- [`standards-applicability.md`](../compliance/standards-applicability.md)
- [`regulatory-compliance-strategy.md`](../compliance/regulatory-compliance-strategy.md)
- [`SECURITY.md`](../../SECURITY.md)
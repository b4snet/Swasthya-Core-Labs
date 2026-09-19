# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added (Phase 0 — Foundation)

- Product boundary and system definition for Swasthya Core Labs (LIS/DIS)
- .NET 10 (LTS) solution skeleton with ASP.NET Core Web API foundation
  (`/health`, `/version-info`, OpenAPI in Development)
- Layered project boundaries: Api, Domain, Application, Infrastructure, Tests
- Repository engineering: `global.json`, `nuget.config` (NuGet Audit),
  centralized package management, `.editorconfig`, `.gitignore`,
  `.gitattributes`, `.env.example`
- Deterministic quality gate: `scripts/verify.ps1`; secret scanner:
  `scripts/scan-secrets.ps1`
- CI foundation (GitHub Actions): build/test/format/vulnerability/secret scan
- Architecture documentation suite (product scope, system architecture,
  domain boundaries, interoperability, terminology)
- Standards applicability inventory with classification
- Security and privacy architecture foundations
- Clinical data-integrity principles
- Laboratory taxonomy and device integration strategy
- Quality architecture and validation strategy foundations
- Architecture Decision Records (ADR-0001 to ADR-0008)
- Proprietary license placeholder (requires legal review)

### Added (Phase 1 — Core platform foundations)

- Identity-tenancy persistence: organizations, facilities, principals,
  roles, role permissions, principal-role assignments, permission catalog
- JWT validation contract (issuer/audience/signing key or JWKS; inbound
  claim mapping disabled) and server-side auth-context resolution — never
  trusts `client_id`; service principals first-class
- Facility-scoped RBAC with deny-by-default (`PermissionGuard`, scoped
  queries); `/v1` endpoints: `/me`, `/organizations`,
  `/organizations/{id}/facilities`, `/audit`
- Append-only audit: `audit_records` with allowlisted non-PHI metadata,
  DB-enforced immutability (PL/pgSQL trigger, `55000`), correlation IDs
- Persistence: EF Core 10 + Npgsql on PostgreSQL 17, snake_case naming,
  `xmin` optimistic concurrency, generated migration
  `IdentityTenancyFoundation`
- API conventions: correlation IDs, Problem Details error model,
  `X-Correlation-Id`, rate limiting (429), production config fail-fast
- Tests: unit (23), real-PostgreSQL persistence (21: scoping, uniqueness,
  transactions/rollback, FK enforcement, xmin concurrency, audit
  immutability), API integration (38: facility isolation, cross-tenant/
  cross-facility denial, forged-claims non-expansion, service-principal
  scope, invalid-identifier rejection, JWT validation, 401/403 semantics,
  no-internal-detail responses, correlation, middleware error mapping);
  shared TestSupport project and scratch cluster
  (`scripts/pg-test.ps1`); CI postgres:17 service for `Category=Db` suite
- ADR-0009 to ADR-0014 (identity/auth context, RBAC, persistence/EF+xmin,
  append-only audit, API security conventions, testing foundation)

### Added (Phase 2 — Laboratory organization & master data foundation)

- Laboratory organization model: organization/facility scoping, facility-scoped
  sections, organization-scoped extensible disciplines (no hardcoded discipline
  enum) — [ADR-0015](docs/decisions/ADR-0015.md)
- Test-catalog definitions: stable identity rows plus immutable, append-only
  versions; lifecycle (`Draft/Active/Inactive/Retired`); organization/facility
  applicability; specimen requirements; terminology mappings
- Panels/profiles with ordered, referenced test membership and versioning
- Master data: result data types, units, code systems, specimen types/sources,
  container types, result statuses, analyzers, configuration items, reference
  intervals (structure only) — [ADR-0017](docs/decisions/ADR-0017.md),
  [ADR-0018](docs/decisions/ADR-0018.md)
- `/v1` laboratory master-data API (reads for all catalogs; writes for tests,
  panels, specimen requirements, terminology mappings, reference ranges,
  facility applicability, configuration items) with DTOs, validation, lifecycle
  transitions, and Problem Details mapping (400/403/404/409)
- Organization-wide vs. facility-scoped authorization for master data; a
  facility-only grant cannot authorize another facility
- Phase 2 persistence: EF Core configurations + migration
  `AddLaboratoryMasterDataFoundation` (20 tables; Phase-2-only changes)
- Tests: +33 unit, +9 real-PostgreSQL persistence, +21 API (total 145 tests;
  Phase 1 baseline 82 unchanged) — lifecycle/versioning, scoping, uniqueness,
  concurrency, authorization, and error mapping
- ADR-0015 to ADR-0018 (organization model, lifecycle/versioning, UCUM/terminology
  boundary, result-data-type/reference-interval foundation)
- Diagnostic orders and request foundation: order/item domains, CHECK constraint
  ck_order_items_single_target, permission guards (`laboratory.order.read`/`write`),
  idempotent order creation via ExternalOrderId, Request→Cancelled lifecycle,
  exactly-one-test-or-panel branch per OrderItem, phase-3 test suite (+54 tests:
  ~24 domain, ~10 persistence, ~20 API) → total 206 vs 145 baseline.
- REPORT: delivered 27-section Phase 3 Report; stop; no Phase 4 started; no
  commit/push/deploy; no production/staging access; no real patient data.

### Added (Phase 7 — Laboratory result entry and observation completion)

- Result/observation completion foundation: `results` entity with typed
  observation values (numeric / textual / coded / unit-bearing), specimen,
  order-item and test-version association — Phase 7 covers result **entry** and
  **observation completion** only
- Explicitly **out of scope for Phase 7** (Phase 8, not started): result
  validation, verification, finalization and controlled correction
- Migration `AddResultEntity` (`results` table) + EF configuration; DI wiring
  for `ResultService`/`IResultRepository`
- `/v1/laboratory/results` entry + retrieval endpoints with validation,
  authorization guardsaine, error mapping and Problem Details
- ADR-0021 (result entry & observation-completion boundary) with ADR-index row
- Tests: added Result entry/tracking tests (units + persistence + API);
  full suite 228/228 PASS (106 non-DB, 40 persistence, 82 API); no commit,
  no push, no deploy, no Phase 8

### Not yet implemented (future phases)

- Clinical workflows, patient/orders, accessioning, result entry/reporting,
  analyzer communication, QC, billing, CDS, and production integrations
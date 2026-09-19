# Swasthya Core Labs

**Standalone, enterprise-grade Laboratory / Diagnostic Information System (LIS/DIS).**

Swasthya Core Labs is an independently deployable, API-first laboratory and
diagnostic information platform designed to operate standalone and to
integrate with external Hospital Management Systems (HMS), EHR/EMR platforms,
clinics, laboratories, and diagnostic networks.

> **Status: Phase 7 — Laboratory Result Entry & Observation Completion (ACCEPTED /
  GREEN / FROZEN).** Phase 1 identity, tenancy and facility isolation, RBAC,
  append-only audit, and core persistence are implemented and tested against real
  PostgreSQL. Phase 2 adds laboratory organization (sections, disciplines), the test
  catalog with lifecycle/versioning, panels, specimens/containers, units/UCUM,
  terminology mappings, result-data-type and reference-interval structures, and
  organization/facility configuration — all reads-first via `/v1`. Phase 3 adds
  diagnostic orders and request foundation (orders, order items, CHECK constraint,
  permission guards, Request→Cancelled lifecycle, idempotent order creation,
  persistence). Phases 4–6 add specimen foundation, accessioning/tracking, and
  laboratory worklists. **Phase 7** adds laboratory result entry and observation
  completion (`results` entity, ADR-0021): typed observations tied to specimens,
  order items and test versions. **Phase 8 (result validation, verification,
  finalization and controlled correction) is NOT implemented and must not begin
  without explicit authorization.** The long-term direction is defined by the
  [master roadmap](ROADMAP.md) (`Phase 0–55`) and the [master system
  blueprint](docs/architecture/master-blueprint.md); later phases are PLANNED and
  wait explicit authorization.

## Product position

- **Standalone** diagnostic/laboratory information infrastructure
- **Independently deployable**, multi-site/enterprise capable
- **Integration-ready** for arbitrary external HMS/EHR/EMR systems
- **API-first**, **interoperability-first**, **security/privacy-first**,
  **audit/provenance-first**
- **Configuration-driven** where clinical or jurisdictional rules vary

Nothing in this project assumes or is coupled to a specific hospital
management system. See [`docs/architecture/domain-boundaries.md`](docs/architecture/domain-boundaries.md).

## Repository map

| Path | Purpose |
| --- | --- |
| `docs/architecture/` | System architecture, product scope, domain boundaries |
| `docs/laboratory/` | Laboratory taxonomy, device integration strategy |
| `docs/interoperability/` | HL7 v2 / FHIR / DICOM strategy, terminology strategy |
| `docs/clinical/` | Clinical data integrity principles |
| `docs/quality/` | Laboratory quality architecture (future) |
| `docs/security/` | Security architecture |
| `docs/privacy/` | Privacy architecture |
| `docs/compliance/` | Standards applicability, regulatory strategy |
| `docs/data-governance/` | Data governance, retention, provenance |
| `docs/validation/` | Software validation strategy |
| `docs/decisions/` | Architecture Decision Records (ADRs) |
| `src/Swasthya.CoreLabs.Api/` | ASP.NET Core Web API (`/health`, `/version-info`, `/v1` identity/tenancy/audit/laboratory) |
| `src/Swasthya.CoreLabs.Domain/` | Domain boundary (identity-tenancy, audit, laboratory master data, common) |
| `src/Swasthya.CoreLabs.Application/` | Application boundary (auth context, queries, master-data services, permission guards) |
| `src/Swasthya.CoreLabs.Infrastructure/` | Persistence (EF Core 10 + Npgsql, migrations) |
| `tests/Swasthya.CoreLabs.Api.Tests/` | xUnit integration tests (incl. facility isolation) |
| `tests/Swasthya.CoreLabs.Persistence.Tests/` | Real-PostgreSQL repository/constraint tests |
| `tests/Swasthya.CoreLabs.Tests/` | Pure unit tests |
| `tests/Swasthya.CoreLabs.TestSupport/` | Shared seeding, JWTs, scratch-PG wiring |

## Technology

- **.NET 10 (LTS)** — ASP.NET Core Web API, C#
- Solution format: `.slnx`; SDK pinned via `global.json`
- **PostgreSQL 17** via EF Core 10 + Npgsql; migrations as first-class
  artifacts; optimistic concurrency on `xmin`
- Central Package Management (`Directory.Packages.props`), NuGet Audit enabled
- Formatting/enforcement: `.editorconfig` + `dotnet format`; analyzers on,
  warnings treated as errors
- Tests: xUnit + `Microsoft.AspNetCore.Mvc.Testing`, real-PostgreSQL suite
  (`Category=Db`), scratch cluster via `scripts/pg-test.ps1`
- CI: GitHub Actions (build/test/format/vuln/secret scan on Linux + Windows;
  PostgreSQL service for DB tests)

Rationale is recorded in
[`docs/decisions/ADR-0001.md`](docs/decisions/ADR-0001.md) and
[`docs/decisions/ADR-0009.md`](docs/decisions/ADR-0009.md).

## Quick start

Prerequisites: .NET 10 SDK (see `global.json`); PostgreSQL for the full test
suite (or `scripts/pg-test.ps1`).

```powershell
# Restore, build, test
dotnet restore Swasthya.CoreLabs.slnx
dotnet build Swasthya.CoreLabs.slnx -c Release -warnaserror
dotnet test Swasthya.CoreLabs.slnx -c Release --no-build

# Run the API (persistence is enabled when ConnectionStrings:CoreLabs is set)
dotnet run --project src/Swasthya.CoreLabs.Api

# Full deterministic quality gate (auto-starts scratch PostgreSQL if available)
powershell -ExecutionPolicy Bypass -File .\scripts\verify.ps1
```

Endpoints (foundation + Phase 1 platform surfaces, not clinical features):

- `GET /health` — liveness
- `GET /version-info` — service identity
- `GET /openapi/v1.json` — OpenAPI document (Development only)
- `GET /v1/me` — authenticated caller's auth context (orgs/facilities/roles)
- `GET /v1/organizations` — organizations the caller can read
- `GET /v1/organizations/{id}/facilities` — facilities within an org
- `GET /v1/audit` — read-only audit records scoped to the caller's grants
- `/v1/organizations/{id}/laboratory/...` — laboratory master data (reads for
  all catalogs; writes for tests, panels, specimen requirements, terminology
  mappings, reference ranges, facility applicability, and configuration items)

Phase 1 scope and rationale: see
[\`docs/decisions/ADR-0009\`](docs/decisions/ADR-0009.md) through
[\`ADR-0014\`](docs/decisions/ADR-0014.md). Phase 2 scope:
[\`ADR-0015\`](docs/decisions/ADR-0015.md) through
[\`ADR-0018\`](docs/decisions/ADR-0018.md). Phase 3 scope:
[\`ADR-0019\`](docs/decisions/ADR-0019.md) and
[\`ADR-0020\`](docs/decisions/ADR-0020.md).

## Documentation index

- Product scope: [`docs/architecture/product-scope.md`](docs/architecture/product-scope.md)
- System architecture: [`docs/architecture/system-architecture.md`](docs/architecture/system-architecture.md)
- Interoperability: [`docs/interoperability/interoperability-strategy.md`](docs/interoperability/interoperability-strategy.md)
- Standards applicability: [`docs/compliance/standards-applicability.md`](docs/compliance/standards-applicability.md)
- Security: [`docs/security/security-architecture.md`](docs/security/security-architecture.md)
- Privacy: [`docs/privacy/privacy-architecture.md`](docs/privacy/privacy-architecture.md)
- Clinical data integrity: [`docs/clinical/clinical-data-integrity.md`](docs/clinical/clinical-data-integrity.md)
- Roadmap: [`ROADMAP.md`](ROADMAP.md) (master roadmap, Phases 0–55)
- Master system blueprint: [`docs/architecture/master-blueprint.md`](docs/architecture/master-blueprint.md)

## Development, security, and contribution

- Development setup and conventions: [`docs/development.md`](docs/development.md)
- Reporting security issues: [`SECURITY.md`](SECURITY.md)
- Contribution process: [`CONTRIBUTING.md`](CONTRIBUTING.md)
- Code of conduct: [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md)

## License

Proprietary commercial software. See [`LICENSE`](LICENSE) and
[`NOTICE.md`](NOTICE.md). No claims of regulatory compliance or certification
are made; see [`docs/compliance/`](docs/compliance/).

## Repository

Remote: <https://github.com/b4snet/Swasthya-Core-Labs>
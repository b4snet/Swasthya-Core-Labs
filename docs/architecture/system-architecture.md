# System Architecture

**Document owner:** Architecture | **Status:** Approved (Phase 1) | **Version:** 1.1

> This document describes the **current implemented** state. The **target
> architecture** for the entire project is defined in
> [`master-blueprint.md`](master-blueprint.md); the implementation sequence is
> governed by the [master roadmap](../../ROADMAP.md).

## Architectural style

- **API-first**: the system is consumed through versioned HTTP APIs (OpenAPI
  contract). No user-facing or machine-facing feature bypasses the API layer.
- **Standards-native**: interoperability uses established standards
  (HL7 v2, FHIR, DICOM/DICOMweb, terminology standards) — never a proprietary
  replacement envelope.
- **Layered boundaries**: presentation/API → Application → Domain →
  Infrastructure, with dependency direction enforced inward (Domain depends
  on nothing external).
- **Modular monolith for enterprise deployment**: the initial target is a
  modular monolith with clear module boundaries configured for multi-site
  isolation; extract-to-services is a documented evolution path, not an early
  tax (see [ADR-0003](../decisions/ADR-0003.md)).
- **Configuration-driven**: clinical and jurisdictional variability lives in
  a configuration subsystem, not in code (see
  [ADR-0008](../decisions/ADR-0008.md)).

## Physical layout

```text
Swasthya.CoreLabs.Api/              HTTP entry point, composition root
Swasthya.CoreLabs.Application/      use cases / workflows, integration service ports
Swasthya.CoreLabs.Domain/           domain model, invariants (identity-tenancy, audit)
Swasthya.CoreLabs.Infrastructure/   persistence (EF Core + Npgsql), messaging, adapters
tests/Swasthya.CoreLabs.Api.Tests/        xUnit integration tests (WebApplicationFactory)
tests/Swasthya.CoreLabs.Persistence.Tests/ real-PostgreSQL repository/constraint tests
tests/Swasthya.CoreLabs.Tests/            pure unit tests
tests/Swasthya.CoreLabs.TestSupport/      shared seeding, JWTs, scratch-PG wiring
```

Dependency rules (enforced by reviewer/gate, not yet by tooling):

- `Api` → `Application`
- `Application` → `Domain`
- `Infrastructure` → `Application`, `Domain` (implements ports)
- `Domain` references nothing external

Phase 1 implements the identity/tenancy + audit foundation of these
boundaries (see [ADR-0009](../decisions/ADR-0009.md)…
[ADR-0014](../decisions/ADR-0014.md)); clinical entities remain out of scope.

## Phase 1 runtime surfaces

- **Persistence:** PostgreSQL 17 via EF Core 10 + Npgsql; migrations under
  `src/Swasthya.CoreLabs.Infrastructure/Persistence/Migrations` (design-time
  model via `SCL_DB_CONNECTION`; host registers persistence only when
  `ConnectionStrings:CoreLabs` is present). Optimistic concurrency on `xmin`.
- **API surface (`/v1`):** `/me`, `/organizations`,
  `/organizations/{id}/facilities`, `/audit` — each guarded by the
  `identity-tenancy` policy and scoped to the caller's grants.
- **Middleware chain:** Correlation → ErrorHandling → RateLimiter →
  Authentication → Authorization (see [ADR-0013](../decisions/ADR-0013.md)).
- Audit writes are committed with the request (`IUnitOfWork`).

## Technology decision (.NET 10 LTS)

- **.NET 10 (LTS)** / ASP.NET Core / C#, pinned SDK via `global.json`
  ([ADR-0001](../decisions/ADR-0001.md)).
- **Why**: enterprise-grade concurrency and tooling, strong static typing,
  mature ecosystem for health interoperability (HL7/FHIR libraries), first
  party support for OpenAPI, health checks, DI, configuration, and
  observability; LTS support horizon aligned with a capacity-constrained,
  long-lived product.
- **Packages**: centralized via `Directory.Packages.props` (Central Package
  Management); pinned versions; NuGet Audit enabled
  (`nuget.config`, `Directory.Build.props`).
- **Testing**: xUnit + `Microsoft.AspNetCore.Mvc.Testing`.

## Runtime / transport concerns (architecture, not implementation)

- HTTPS everywhere in production; TLS in transit.
- API versioning and contract evolution policy (see
  [ADR-0005](../decisions/ADR-0005.md)).
- Structured logging with **no PHI/PII/credentials** in log output.
- Health and liveness endpoints; metrics/observability planned (OpenTelemetry
  later).
- Configuration from a secrets manager + typed configuration, never secrets
  in source.

## Cross-cutting concerns

| Concern | Approach (architecture) |
| --- | --- |
| Audit/provenance | Append-only audit trail; provenance preserved through reporting (see `docs/clinical/`, `docs/data-governance/`) |
| Tenancy/facility isolation | Facility-scoped data model and authorization boundary (see [ADR-0007](../decisions/ADR-0007.md)) |
| Configuration | Configuration subsystem as a first-class module ([ADR-0008](../decisions/ADR-0008.md)) |
| Interoperability | Standards adapters at the boundary ([ADR-0004](../decisions/ADR-0004.md)) |
| Observability | Structured logs, metrics, distributed tracing in later phases |

## Deployment topology (conceptual)

- Single-region, multi-facility deployment is the primary target; the
  configuration and isolation model anticipates multi-site enterprise use.
- Containerized deployment (OCI image) is planned for phases after the
  foundation; no container files exist in Phase 0.

## Open questions

- Message bus vs. in-process events — deferred.
- Analytics/reporting warehouse needs — deferred to reporting phase.
- Row-level security / `SET LOCAL` tenant context vs. application-level
  enforcement — RLS deferred (see [ADR-0010](../decisions/ADR-0010.md)).
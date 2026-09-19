# ADR Index — Swasthya Core Labs

Architecture Decision Records (MADR style: Context → Decision → Consequences).

| ADR | Title | Status |
| --- | --- | --- |
| [ADR-0001](ADR-0001.md) | Technology stack: .NET 10 (LTS), ASP.NET Core | Accepted |
| [ADR-0002](ADR-0002.md) | Proprietary commercial licensing | Accepted (legal review pending) |
| [ADR-0003](ADR-0003.md) | Modular architecture; core vs. integration boundaries | Accepted |
| [ADR-0004](ADR-0004.md) | Standards-native interoperability (no proprietary envelope) | Accepted |
| [ADR-0005](ADR-0005.md) | API-first with contract & versioning discipline | Accepted |
| [ADR-0006](ADR-0006.md) | Immutable clinical observations (append-only) | Accepted |
| [ADR-0007](ADR-0007.md) | Identity, tenancy, and facility isolation model | Accepted (design deferred; Phase 1 basis in ADR-0009/0010) |
| [ADR-0008](ADR-0008.md) | Configuration-driven clinical & jurisdictional rules | Accepted |
| [ADR-0009](ADR-0009.md) | Phase 1 identity and auth context model | Accepted |
| [ADR-0010](ADR-0010.md) | Facility-scoped RBAC with application-level enforcement | Accepted |
| [ADR-0011](ADR-0011.md) | Persistence foundation (EF Core 10, PostgreSQL, xmin) | Accepted |
| [ADR-0012](ADR-0012.md) | Append-only audit with allowlisted metadata | Accepted |
| [ADR-0013](ADR-0013.md) | API security conventions | Accepted |
| [ADR-0014](ADR-0014.md) | Testing foundation against real PostgreSQL | Accepted |
| [ADR-0015](ADR-0015.md) | Laboratory organization and scope model | Accepted |
| [ADR-0016](ADR-0016.md) | Master-data lifecycle and append-only versioning | Accepted |
| [ADR-0017](ADR-0017.md) | Units, UCUM, and terminology mapping boundary | Accepted |
| [ADR-0018](ADR-0018.md) | Result-data-type and reference-interval foundation | Accepted |
| [ADR-0019](ADR-0019.md) | Master roadmap and system blueprint adoption; 0–55 phase numbering | Accepted |
| [ADR-0020](ADR-0020.md) | Diagnostic orders and request lifecycle foundation | Accepted |
| [ADR-0021](ADR-0021.md) | Laboratory result entry and observation completion (Phase 7) | Accepted |
| [ADR-0021](ADR-0021.md) | Laboratory result entry and observation completion (Phase 7) | Accepted |
| [ADR-0022](ADR-0022.md) | Result validation, verification, finalization and controlled correction (Phase 8) | Draft — 3 decisions UNRESOLVED |
| [ADR-0023](ADR-0023.md) | Fundamental Result Entities review: inventory and requirement gaps for Phase 8 | Draft — documentation review only |
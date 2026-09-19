# Development Guide

**Status:** Approved (Phase 1) | **Version:** 1.1

## Prerequisites

- **Windows / macOS / Linux**
- **.NET 10 SDK** — version pinned in [`global.json`](../global.json)
  (`10.0.401`, `rollForward: latestPatch`). On Windows, `dotnet` may need
  `C:\Program Files\dotnet` prepended to `PATH`.
- Git. `git` available on PATH.
- Optional: PowerShell 5.1+ (for repo scripts). Works on `pwsh`/PowerShell 7 too.
- **PostgreSQL** (recommended 17.x/18 via `scripts/pg-test.ps1` for
  DB-backed tests). The DB suite (`Category=Db`) requires a reachable
  PostgreSQL; non-DB tests do not.

## Verify the toolchain

```powershell
dotnet --version   # must match global.json (10.0.4xx)
dotnet --list-sdks
```

## First-time restore

```powershell
dotnet restore Swasthya.CoreLabs.slnx
```

Restore runs **NuGet Audit** by default (`Directory.Build.props`,
`nuget.config`): a vulnerable/missing-audit-data dependency makes restore
fail. Restore also enforces package source mapping (nuget.org only).

## Commands

| Task | Command |
| --- | --- |
| Restore | `dotnet restore Swasthya.CoreLabs.slnx` |
| Build (Release, warnings-as-errors) | `dotnet build Swasthya.CoreLabs.slnx -c Release --no-restore -warnaserror` |
| Format verification | `dotnet format Swasthya.CoreLabs.slnx --verify-no-changes --no-restore` |
| Format (apply) | `dotnet format Swasthya.CoreLabs.slnx --no-restore` |
| Tests | `dotnet test Swasthya.CoreLabs.slnx -c Release --no-build` |
| Vulnerability scan | `dotnet list Swasthya.CoreLabs.slnx package --vulnerable --include-transitive` |
| Secret scan | `powershell -ExecutionPolicy Bypass -File .\scripts\scan-secrets.ps1` |
| Full quality gate | `powershell -ExecutionPolicy Bypass -File .\scripts\verify.ps1` |
| Start PostgreSQL test DB | `powershell -ExecutionPolicy Bypass -File .\scripts\pg-test.ps1 -Action start` |
| Drop PostgreSQL test DB | `powershell -ExecutionPolicy Bypass -File .\scripts\pg-test.ps1 -Action stop` |
| Migrations (design-time) | `$env:SCL_DB_CONNECTION='<connection>'; dotnet ef migrations add <Name> --project src/Swasthya.CoreLabs.Infrastructure --startup-project src/Swasthya.CoreLabs.Api` |

## PostgreSQL-backed tests

Test suites that prove database behavior (xmin concurrency, DB-level audit
protection, unique constraints, repository scoping) carry
`[Trait("Category", "Db")]` and run against a real PostgreSQL:

- Point `SCL_PG_TEST_CONNECTION` at any PostgreSQL, or let
  `scripts/pg-test.ps1` provision a throwaway cluster on
  `127.0.0.1:55432` (state under `%TEMP%\scl-pg-test`); `verify.ps1`
  auto-starts the scratch cluster when PostgreSQL is detected and no
  `SCL_PG_TEST_CONNECTION` is set.
- CI Ubuntu runs the `postgres:` service and the full suite; the Windows
  job runs `--filter "Category!=Db"`.
- Migrations are applied automatically by the test factories
  (`DbTestContextFactory.EnsureDatabase()`). Never point tests at a
  production database.

## Deterministic quality gate

`scripts/verify.ps1` runs, in order (each must pass):

1. `dotnet restore` (NuGet Audit)
2. `dotnet format --verify-no-changes`
3. `dotnet build -c Release -warnaserror`
4. `dotnet test -c Release --no-build`
5. `dotnet list package --vulnerable --include-transitive`
6. `scripts/scan-secrets.ps1`
7. `git diff --check`

Run the full gate before finishing any change.

## Secret scanning

- `scripts/scan-secrets.ps1` scans repository files with an explicit pattern
  set. It is **best-effort** — it is not a substitute for gitleaks or
  GitHub-native secret scanning.
- CI runs [gitleaks](https://github.com/gitleaks/gitleaks-action) on every
  push/PR.
- **Never** place credentials, API keys, patient information, or validated
  clinical data in source, tests, fixtures, docs, or commits.

## Dependency policy

- **Central Package Management only** — every package version is declared in
  [`Directory.Packages.props`](../Directory.Packages.props). Never add a
  `Version` attribute directly in a `.csproj`.
- Adding a dependency requires:
  1. A version entry in `Directory.Packages.props`.
  2. Justification described in the PR/ADR (why needed, alternatives).
  3. NuGet Audit clean restore and `NOTICE.md` update where material.
- Prefer framework-provided capabilities over new packages.

## Coding conventions

- C#: nullable enabled, implicit usings, `net10.0` target, analyzers enabled,
  **warnings treated as errors** (`Directory.Build.props`).
- Formatting enforced by `.editorconfig` + `dotnet format`.
- No unnecessary comments in code.
- Tests: xUnit; follow existing patterns (`HealthEndpointTests.cs`).
- Tests project scopes CA1707 (xUnit underscore naming) — do not re-enable.
- DB-backed tests are tagged `Category=Db` (see "PostgreSQL-backed tests").

## Repository layout

```text
src/Swasthya.CoreLabs.Api/            # HTTP layer (health/version, /v1 identity/tenancy/audit)
src/Swasthya.CoreLabs.Domain/         # domain boundary (identity-tenancy, audit, common)
src/Swasthya.CoreLabs.Application/    # application boundary (auth context, queries, guards)
src/Swasthya.CoreLabs.Infrastructure/ # persistence (EF Core + Npgsql, migrations)
tests/Swasthya.CoreLabs.Api.Tests/    # xUnit integration tests (incl. facility isolation)
tests/Swasthya.CoreLabs.Persistence.Tests/ # real-PostgreSQL repository/constraint tests
tests/Swasthya.CoreLabs.Tests/        # pure unit tests
tests/Swasthya.CoreLabs.TestSupport/  # shared seeding, JWTs, scratch PG wiring
docs/                                 # documentation (source of truth)
scripts/                              # deterministic repo scripts
```

## CI

GitHub Actions ([`.github/workflows/ci.yml`](../.github/workflows/ci.yml)):
on push/PR to `master`, runs restore/format/build/test/vulnerability scan on
Ubuntu + Windows, plus a gitleaks secret scan job.

## Documentation conventions

- Docs are the source of truth; keep them consistent when architecture
  changes (see [`AGENTS.md`](../AGENTS.md)).
- Authoritative documents are versioned/dated at the top (owner, status,
  version).
- ADRs live in [`docs/decisions/`](../docs/decisions/) and follow the MADR
  structure used there.
# AGENTS.md

Guidance for coding agents working in this repository.

## Repo state

Phase 1: identity/tenancy/RBAC + append-only audit + core persistence
(EF Core 10 + PostgreSQL) are implemented and tested against real PostgreSQL
(Category=Db suite). Docs and ADRs are the source of truth. Do not implement
clinical/laboratory workflows, analyzers, or production integrations.

## Commands

Windows shell. `dotnet` may require prepending `C:\Program Files\dotnet` to
PATH if it is missing. The DB-backed test suite (`Category=Db`) needs a
PostgreSQL target: set `SCL_PG_TEST_CONNECTION` or run
`powershell -ExecutionPolicy Bypass -File .\scripts\pg-test.ps1 -Action start`
(scratch cluster on 127.0.0.1:55432, connection string in
`%TEMP%\scl-pg-test\connection.txt`). `verify.ps1` auto-starts/stops the
scratch cluster when PostgreSQL is detected.

- Restore: `dotnet restore Swasthya.CoreLabs.slnx`
- Build (Release, warnings-as-errors): `dotnet build Swasthya.CoreLabs.slnx -c Release --no-restore -warnaserror`
- Format check: `dotnet format Swasthya.CoreLabs.slnx --verify-no-changes --no-restore`
- Tests (full, DB-backed): `dotnet test Swasthya.CoreLabs.slnx -c Release --no-build`
- Tests (no DB): `dotnet test Swasthya.CoreLabs.slnx -c Release --no-build --filter "Category!=Db"`
- Vulnerability scan: `dotnet list Swasthya.CoreLabs.slnx package --vulnerable --include-transitive`
- Secret scan: `powershell -ExecutionPolicy Bypass -File .\scripts\scan-secrets.ps1`
- Full gate: `powershell -ExecutionPolicy Bypass -File .\scripts\verify.ps1`
- `git diff --check` before finishing

## Hard rules

- Never commit/push/deploy unless explicitly requested.
- Never add credentials, patient data, or real clinical data.
- Never invent clinical rules, reference ranges, or compliance claims.
- Keep docs and ADRs consistent when architecture changes.
- Work only inside this repository.
- .NET 10 LTS, Central Package Management (`Directory.Packages.props`),
  warnings-as-errors, `dotnet format` enforced.
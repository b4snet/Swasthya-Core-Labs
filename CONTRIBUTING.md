# Contributing

Thanks for your interest in Swasthya Core Labs.

This is a **proprietary commercial project** operating as a closed development
repository. External contributions are accepted only under the terms of a
written contributor agreement. By default, the repository is maintainer-led.

## Ground rules

- Work only within this repository. Do not modify unrelated repositories or
  projects on the same machine.
- Do not commit, push, or deploy unless explicitly instructed.
- **Never** include credentials, API keys, patient information, or real
  clinical data in source, tests, fixtures, logs, or documentation.
- Start from the current Phase 0 foundation and documentation; follow the
  conventions in [`docs/development.md`](docs/development.md).

## Local workflow

1. Read [`README.md`](README.md) and [`docs/architecture/`](docs/architecture/).
2. For new architectural decisions, propose an ADR
   ([`docs/decisions/`](docs/decisions/)).
3. Make focused changes.
4. Run the deterministic quality gate:
   `powershell -ExecutionPolicy Bypass -File .\scripts\verify.ps1`
5. Confirm `git diff --check` passes and that only intended files changed.

## Code standards

- C#: nullable enabled, implicit usings, analyzers on, warnings as errors.
  Formatting is enforced by `dotnet format` (`.editorconfig`).
- Tests: xUnit; cover the behaviors you change.
- Dependencies: central package management only (`Directory.Packages.props`);
  new packages require justification in the PR and an updated
  `NOTICE.md`/ADR where relevant.
- No emojis in code or documentation. No code comments unless they add
  necessary context.

## Definitions of done

- Quality gate passes (build, format, tests, vulnerability scan, secret scan,
  `git diff --check`)
- No new secrets or sensitive data
- Documentation updated to stay consistent (docs are the source of truth)

## Reporting issues

Bug reports and feature requests should include a clear description,
reproduction steps, and expected behavior. Security issues must be reported
per [`SECURITY.md`](SECURITY.md) and never through public issues.
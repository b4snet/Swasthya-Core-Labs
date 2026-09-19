# Security Policy

## Supported versions

| Version | Supported |
| --- | --- |
| 0.x (pre-release) | Active development |

## Reporting a vulnerability

Swasthya Core Labs is a pre-release development project. If you discover a
security vulnerability:

- **Do not** open a public issue.
- Report via the repository security channels (GitHub Security Advisories /
  private disclosure) or as directed by the maintainers.
- Include: affected component, type of issue, reproduction steps, impact
  assessment, and any suggested mitigation.

You should receive acknowledgment within 5 business days.

## Security posture

- This project is **not yet certified** or compliant with any security
  standard. The architecture includes a security roadmap:
  [`docs/security/security-architecture.md`](docs/security/security-architecture.md)
- **Never** commit credentials, API keys, patient information, or real
  clinical data to this repository — in source, tests, fixtures, logs, or
  documentation.
- Local secret scanning: see [`docs/development.md`](docs/development.md).
  GitHub-native secret scanning plus gitleaks run in CI.
- Dependency vulnerability auditing is enabled via NuGet Audit
  (`nuget.config` / `Directory.Packages.props`).

## Deployment note

This software is intended for diagnostic/laboratory information use. Do not
deploy to production or connect to production/staging clinical systems until
the security and validation requirements documented in
[`docs/security/`](docs/security/) and [`docs/validation/`](docs/validation/)
are satisfied and approved.
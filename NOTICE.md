# NOTICE

**Swasthya Core Labs** is proprietary commercial software.
Copyright (c) 2026 Swasthya Core Labs. All rights reserved.

## Standards and third-party references

- Industry standards (HL7, FHIR, DICOM, IHE, LOINC, SNOMED CT, UCUM, ISO,
  CLSI, OWASP, etc.) are referenced by name and identifier for planning and
  classification purposes. Their specifications and rights remain with their
  respective owners. **No proprietary standards text is reproduced in this
  repository.**
- Standards versions/sources are recorded in
  [`docs/compliance/standards-applicability.md`](docs/compliance/standards-applicability.md).

## Third-party software

- NuGet package versions: see [`Directory.Packages.props`](Directory.Packages.props)
- Toolchain: .NET SDK — [dotnet/core](https://github.com/dotnet/core)
- Test tooling: xUnit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk,
  coverlet — each licensed under its own open-source license
- Runtime/data libraries (Phase 1): Entity Framework Core, Microsoft.AspNetCore
  Authentication.JwtBearer, Npgsql/Npgsql.EntityFrameworkCore.PostgreSQL,
  EFCore.NamingConventions, JWT — each licensed under its own open-source
  license; PostgreSQL is used under its own license
- CI: GitHub Actions and the gitleaks action are used; licenses per upstream

## Important disclaimers

1. **No compliance or certification is claimed.** Regulatory posture is
   classified, not asserted, in [`docs/compliance/`](docs/compliance/).
2. **No patient data.** This repository contains no patient information or
   real clinical data. All fixtures and examples are synthetic.
3. **No clinical rules or reference ranges** are defined or claimed. The
   system does not yet generate, validate, or interpret clinical results.
4. **No real analyzer interoperability is claimed** without runtime
   validation. See [`docs/laboratory/device-integration-strategy.md`](docs/laboratory/device-integration-strategy.md).

## Ownership

Repository: <https://github.com/b4snet/Swasthya-Core-Labs>
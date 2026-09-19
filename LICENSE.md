# License

Swasthya Core Labs is **proprietary commercial software** and is not
published under an open-source license.

- Full license text: [`LICENSE`](../LICENSE)
- The current `LICENSE` file is a **placeholder**. It must be finalized by
  legal counsel before any distribution, sale, or external use.
- No OSI-approved license applies to this repository.

## Third-party components

This project uses third-party packages and tooling that remain under their
own licenses. Sources of truth:

- NuGet package manifests: `Directory.Packages.props` (Central Package Management)
- Toolchain: .NET SDK (MIT-licensed) — see [dotnet/core](https://github.com/dotnet/core)
- Test frameworks: xUnit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk,
  coverlet (open-source licenses)
- Documentation tooling: none bundled; Markdown rendered by standard tooling

Final third-party attribution will be maintained in `NOTICE.md` and the
distribution notice as packages are added in later phases.

## Standards and terminology

This repository references industry standards by name and identifier
(e.g., HL7 FHIR, LOINC, SNOMED CT, ISO 15189) for interoperability planning
and classification. It does **not** incorporate proprietary standards text.
Standards bodies retain rights to their specifications.

## Regulated-health-software notice

Swasthya Core Labs is intended for diagnostic/laboratory information use.
It is not yet certified or licensed for clinical use. Regulatory status is
classified—not claimed—in `docs/compliance/`.
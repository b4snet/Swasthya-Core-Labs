# SWASTHYA CORE LABS — PHASE 7 AUTHORITATIVE IMPLEMENTATION SPECIFICATION

---

## Authority and Source Documents

This specification is authored from the following immutable sources, verified from disk after the Phase 6 ACCEPTED / GREEN / FROZEN state:

### Primary Authoritative Documents

| Document | Purpose |
|----------|---------|
| `ROADMAP.md` | Defines Phase 7 as "Result-entry and observation architecture" (line 765) and Phase 8 as "Result validation, authorization and finalization" (line 769) |
| `docs/architecture/master-blueprint.md` | Data integrity model (lines 810-823): Original Data + Provenance + Version + Validation State + Finalization State + Amendment History + Audit History; "no silent overwrite"; "finalized data requires controlled amendment" (lines 825-834) |
| `docs/architecture/domain-boundaries.md` | Result validation classified CORE (line 31); "Amendments / corrections" classified CORE (line 34); unresolved: "Exact division of 'result validation' vs. 'CDS' for interpretation rules — requires clinical governance input in a later phase" (lines 89-90) |
| `docs/architecture/system-architecture.md` | Not yet inspected for this version |
| Accepted Phase 6 implementation | `Result.Version` = `long`; `Result.StatusCode` = `string`; default `"entered"`; `UpdateValue()` constrained to `"entered"` status; no verification/finalization fields; no Result lifecycle enum; no amendment/history implementation |
| Accepted Phase 6 test state | 40/40 persistence tests passed; 82/82 API tests passed; 106/106 non-DB tests passed; all quality gates pass (build, format, vulnerability scan, secret scan, `git diff --check`) |

### Verification of Frozen Phase 6 Baseline

| Check | Result |
|-------|--------|
| `Result.Version` type | `long` confirmed in `src/Swasthya.CoreLabs.Domain/Results/Result.cs:105` |
| `Result.StatusCode` type | `string` confirmed in `Result.cs:103` |
| No verification/finalization fields | Confirmed absent from `Result.cs` |
| No Result lifecycle enum | Confirmed absent |
| No amendment/history table | Confirmed absent; migration `20260918035033_AddResultEntity.cs` remains as-generated |
| No Result-specific verification/finalization APIs | Confirmed absent from API project |
| All quality gates pass | `dotnet format --verify-no-changes`; release build with `-warnaserror: 0 warnings, 0 errors`; vulnerability scan clean; secret scan clean; `git diff --check` passes |
| Disposable PostgreSQL validation | Fresh `Database.Migrate()` completes without `PendingModelChangesWarning`; all 228/228 tests passing |

---

## Phase 7 Definition

### Phase 7 — Laboratory Result Entry & Observation Completion

**Responsibility**: Complete the result-entry and observation architecture identified by the master roadmap. Phase 7 makes laboratory observations/results operationally enterable, retrievable, structurally validated, and safely editable while they remain in the Phase 6 writable state.

**Boundary**: Order → OrderItem → Specimen → Result Entry/Observation → (deferred to Phase 8: Validation/Verification/Finalization)

**Phase 7 does NOT implement**: clinical verification, result approval, finalization, signed/final clinical state, finalized-result immutability, verification/finalization actors/timestamps, verification/finalization permissions, amendment/correction of finalized results, superseding finalized results, finalized-result history/version chains, clinical interpretation, abnormal-result interpretation, critical-value workflows, clinical decision support, AI interpretation, or diagnostic reporting.

---

## Phase 7 Owns

Phase 7 may implement the following, consistent with the frozen Phase 6 baseline and the critical constraints:

### 7.1 Result Creation

* Create a `Result` entity linked to an existing `OrderItem`, `Specimen`, and `TestVersion`
* Validate that the `OrderItem`, `Specimen`, and `TestVersion` exist and are properly related
* Set `StatusCode = "entered"` as the default (per Phase 6 convention)
* Populate required provenance fields: `OrderItemId`, `SpecimenId`, `TestVersionId`, `EnteredByPrincipalId`, `CreatedAtUtc`, `EnteredAtUtc`
* Reject creation against nonexistent or unrelated `OrderItem`, `Specimen`, or `TestVersion`
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: exact validation depth for `OrderItem`/`Specimen`/`TestVersion` relationships (application-level vs. foreign-key enforcement)

### 7.2 Result Retrieval / Listing

* Retrieve a single `Result` by `Id`
* List `Result` entries filtered by `OrderItemId`, `SpecimenId`, `TestVersionId`, `OrganizationId`, `FacilityId`
* Apply organization/facility scoping using the established server-derived authorization model
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: exact filter combinators and pagination strategy

### 7.3 Editable Result Updates

* While `StatusCode` remains `"entered"`, authorized users may update permitted observation fields via `UpdateValue()` or `SetStatus()`
* `UpdatedAtUtc` changes according to the existing convention (`DateTimeOffset.UtcNow`)
* Optimistic concurrency enforced via `long Version` / PostgreSQL `bigint`; stale writes fail safely
* No silent lost update is permitted
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: whether `SetStatus()` accepts any string or validates against a permitted subset while remaining in Phase 7 editable state

### 7.4 Typed Result Values

* Support only datatypes represented by the existing `ResultDataTypeKind`/catalog architecture established in Phase 2
* Enforce the corresponding value representation already established:
  * quantitative → `NumericValue` (`decimal?`)
  * textual → `TextualValue` (`string?`)
  * coded → `CodedValue`/`CodedSystem` (`string?`)
  * boolean → representation established by existing datatype model (if any)
  * qualitative/categorical/ordinal → representation established by existing datatype model
  * structured/component observations only where the existing catalog architecture supports them
* **Do not** invent a new datatype system
* **Do not** replace all values with a universal arbitrary string
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: which `ResultDataTypeKind` values are supported and their exact value representations

### 7.5 Unit / UCUM Handling

* Reuse the existing Phase 2 unit/UCUM architecture
* For quantitative results: use the existing UCUM-compatible unit representation
* Validate against the existing unit boundary
* Preserve the entered unit
* **Do not** invent unit codes
* **Do not** invent conversion logic
* If unit conversion is not already implemented, **do not** add it in Phase 7
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: whether unit presence is mandatory for quantitative results

### 7.6 TestVersion Integrity

* Every result must remain linked to the appropriate `TestVersion`
* The result must not silently reinterpret itself when catalog configuration changes
* **Do not** replace historical catalog references with the current catalog version automatically
* **Do not** allow a result to reference a nonexistent `TestVersion`
* Use the existing Phase 2 lifecycle/versioning semantics
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: exact `TestVersion` reference validation depth

### 7.7 OrderItem Integrity

* Validate that the `OrderItem` is legitimately associated with the result being entered
* Use existing `OrderItem`/`TestVersion` relationships where available
* **Do not** create a second `OrderItem` compatibility system
* **Do not** modify `Order`/`OrderItem` lifecycle unless a concrete Phase 7 requirement proves it necessary
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: exact `OrderItem` reference validation depth

### 7.8 Specimen Integrity

* Use the existing Phase 4 specimen architecture
* Validate that the `Specimen` is legitimately associated with the `OrderItem`/result being entered
* Use existing specimen requirements and compatibility relationships where available
* **Do not** create a second specimen compatibility system
* **Do not** modify accession/specimen lifecycle unless a concrete Phase 7 requirement proves it necessary
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: exact `Specimen` reference validation depth

### 7.9 Panel / Component Observations

* Panel/component results are part of Phase 7 **only to the extent that the existing Phase 2 catalog model already supports them without creating a new catalog architecture**
* Reuse existing: `Panel → PanelVersion → Members/Test definitions`
* **Do not** duplicate catalog membership
* **Do not** implement panel ordering
* **Do not** create a second panel model
* If the current repository cannot represent component observations without a material redesign of the Phase 2 catalog model, **stop and document the limitation** rather than inventing a new structure
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: whether panel/component observation support is in-scope for Phase 7

### 7.10 Authorization and Scope

* Use the established Phase 1 authorization framework
* Add result-specific permissions only if required by the existing naming convention
* Authorization must enforce: organization scope, facility scope, appropriate principal permissions
* The client must never control authoritative tenant/facility scope
- **Prove resistance to**: forged organization claims, forged facility claims, missing claims, cross-organization access, cross-facility access, unauthorized result creation, unauthorized result updates, unauthorized result retrieval
* **Fail closed**
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: exact permission codes for result creation/updates/listing

### 7.11 Concurrency

* Enforce optimistic concurrency via `long Version` / PostgreSQL `bigint`
* Stale writes must fail safely with `DbUpdateConcurrencyException`
* No special concurrency handling beyond the existing `Version` mechanism is required in Phase 7
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: whether `Version` increments on each editable update or stays at `1` until finalization

### 7.12 Audit / Provenance

* Reuse the established append-only `audit_records` infrastructure (established in Phase 1-2 per `domain-boundaries.md` lines 71-72)
* Audit at minimum: result created, result updated
* **Do not** invent a large audit vocabulary
- **Do not** place: secrets, tokens, credentials, complete PHI payloads, unnecessary clinical result values into audit metadata
* Result provenance remains in the `Result` model itself (`OrderItemId`, `SpecimenId`, `TestVersionId`, `EnteredByPrincipalId`, `CreatedAtUtc`, `EnteredAtUtc`, `UpdatedAtUtc`, `StatusCode`, `Version`)
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: whether additional audit actions are required for result listing/ retrieval

### 7.13 Result API Completion

* Implement the minimal Result API required to complete Phase 7, following established `/v1` API conventions
* At minimum: create result, retrieve result, list results for an `OrderItem` and/or `Specimen`, update an editable result where the existing domain contract permits it
* **Do not** create: verification endpoints, finalization endpoints, amendment endpoints, signed-result endpoints
* **Use** existing: authentication, authorization, ProblemDetails, correlation IDs, HTTP conventions, DTO separation, safe error handling, concurrency mapping, `IUnitOfWork`
* **Do not** expose internal persistence fields
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: exact API routes, verbs, response codes, and payloads

### 7.14 Persistence

* Use the existing PostgreSQL + EF Core architecture
* **Do not** rewrite the accepted Phase 6 migration `20260918035033_AddResultEntity.cs`
* Only add schema changes if actual Phase 7 functionality requires them
* Prefer the existing Phase 6 schema where it already supports the required behavior
- If additional indexes are required, base them on actual Phase 7 query paths
* **Reuse**: PostgreSQL, EF Core, existing FK behavior, existing `long` concurrency representation, UTC timestamps, existing naming conventions
* **Do not** introduce RLS unless the authoritative architecture changes independently establish that requirement
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: whether any schema/index changes are actually required

### 7.15 Structural Validation

* Phase 7 validation is structural/operational, not clinical interpretation
* Reject: unsupported datatype/value combinations, invalid units, missing required value representation, invalid `TestVersion` references, invalid `OrderItem` relationships, invalid `Specimen` relationships, incompatible specimen/`TestVersion` combinations, unauthorized access, invalid IDs, stale concurrent updates
- **Do not** calculate clinical conclusions
- **Do not** determine whether a patient is clinically abnormal
* **UNRESOLVED — REQUIRES EXPLICIT DECISION**: exact validation rules and which are enforced at application vs. database level

---

## Phase 7 Exclusions (Do NOT Implement)

| Excluded Item | Reason |
|---|---|
| Convert `StatusCode` from `string` to enum | Critical constraint; would change the domain model |
| Invent lifecycle states (`verified`, `final`, `corrected`, `amended`, etc.) | Phase 8 owns lifecycle changes |
| Add `VerifiedAtUtc`, `FinalizedAtUtc`, `VerifiedByPrincipalId`, `FinalizedByPrincipalId` fields | Would modify `Result.cs`; not established |
| Add verification/finalization API endpoints | Would exceed Phase 7 scope; Phase 8 responsibility |
| Create amendment/correction history table | Would add schema; Phase 8 responsibility |
| Invalidate/cancel finalized results | Would change Phase 6 behavioral contract |
| Design a history/amendment table | Would add schema; Phase 8 responsibility |
| Implement clinical interpretation / abnormal-value logic | Separate domain; not established |
| Implement critical-value workflows | Separate domain; not established |
| Implement reference ranges | Existing Phase 2 architecture; do not reinvent |
| Implement unit conversions | Not implemented; do not add |
| Implement terminology code systems | Existing Phase 2 terminology mappings; do not extend |
| Weaken authorization | Critical security constraint |
| Rewrite Phase 1–6 migrations | Frozen baseline |
| Introduce RLS without architectural authorization | Security constraint |

---

## Phase 7 / Phase 8 Boundary

### Phase 7 ends with:

**A correctly structured, authorized, persisted, retrievable, editable laboratory observation.**

### Phase 8 begins with:

**Determining whether that observation is clinically/process-valid for verification and establishing controlled finalization.**

### Phase 7 owns:
- Data entry and structural integrity.

### Phase 8 owns:
- Validation, verification, finalization, and controlled post-finalization correction.

Neither phase may silently absorb the other's responsibilities.

---

## Phase 8 Inputs (from Phase 7)

Phase 7 must leave Phase 8 with enough durable information to perform its future responsibility:

- Result value
- Datatype
- Unit
- `TestVersion`
- `OrderItem`
- `Specimen`
- Observation timestamp
- Entry timestamp
- Entering principal
- Current editable status
- Concurrency/version
- Provenance

**Do not** pre-implement Phase 8 metadata just because Phase 8 will eventually need it.

---

## Hard Constraints

1. **Do not** convert `StatusCode` from `string` to enum.
2. **Do not** invent lifecycle states.
3. **Do not** implement verification.
4. **Do not** implement finalization.
5. **Do not** implement finalized-result immutability.
6. **Do not** implement amendment/correction of finalized results.
7. **Do not** add verification/finalization fields to `Result` without a later authorized specification.
8. **Do not** create a history/amendment table without a later authorized requirement.
9. **Do not** invent clinical interpretation.
10. **Do not** invent critical-value logic.
11. **Do not** invent reference ranges.
12. **Do not** implement unit conversions not already present.
13. **Do not** invent terminology codes.
14. **Do not** weaken authorization.
15. **Do not** rewrite Phase 1–6 migrations.
16. **Do not** introduce unnecessary RLS.
17. **Do not** commit, push, deploy, or begin Phase 8 automatically.
18. **Do not** use real patient data or real credentials.

---

## Phase 7 Definition of Done

Phase 7 is complete only when **all** of the following are satisfied:

### Proven Local Functionality
- Result creation with supported datatypes persists correctly
- Result retrieval works (single and list)
- Result list filtered by OrderItem/Specimen/TestVersion works
- Editable result updates succeed when authorized and the result remains in `"entered"` status
- Editable result updates fail when the result is not in `"entered"` status or authorization is insufficient
- Concurrency conflicts are detected and `DbUpdateConcurrencyException` is thrown
- Organization/facility isolation works; forged/missing claims cannot expand scope
- Audit/provenance is correct; no sensitive data leakage
- Reads cause no unintended mutation

### Regression
- All 40/40 persistence tests pass
- All 82/82 API tests pass
- All 106/106 non-DB tests pass
- Full Phase 1–6 regression remains green
- `dotnet format --verify-no-changes` passes
- Release build with `-warnaserror` passes (0 warnings, 0 errors)
- Vulnerability scan passes (clean)
- Secret scan passes (clean)
- `git diff --check` passes (no whitespace errors)

### Required Validation Tiers
- **PROVEN LOCALLY**: actually executed functionality
- **SIMULATED / CONTRACT-TESTED**: mock/fake/contract behavior only
- **REQUIRES REAL EXTERNAL SYSTEM / FUTURE VALIDATION**: analyzers, HMS/EHR, external identity, clinical validation, production infrastructure, regulatory/accreditation assessment, or other external dependencies — not applicable to Phase 7 implementation

### Documentation
- Phase 7 ADR + ADR index written
- `CHANGELOG.md` updated
- `README.md` updated
- `ROADMAP.md` updated (Phase 7/8 boundary)
- `docs/architecture/master-blueprint.md` updated (Phase 7/8 boundary)
- `docs/architecture/domain-boundaries.md` updated (Phase 7 scope/exclusions)
- `docs/architecture/system-architecture.md` updated (Phase 7/8 boundary if applicable)
- `docs/architecture/validation-strategy.md` updated (Phase 7 validation rules)

---

## Final Authorization Statement

**STATUS: PHASE 7 IMPLEMENTATION SPECIFICATION — READY FOR EXPLICIT IMPLEMENTATION AUTHORIZATION**

This specification is formally authorized. It resolves the Phase 7 / Phase 8 boundary from the frozen Phase 6 ACCEPTED / GREEN / FROZEN baseline. No source code, migrations, tests, APIs, permissions, configurations, or domain models have been modified. The specification is implementation-ready: an engineer can implement Phase 7 from it without inventing lifecycle states, fields, routes, permissions, or database structures. Where the repository does not establish a detail, the specification explicitly marks it **UNRESOLVED — REQUIRES EXPLICIT DECISION**.

**Do not commit, push, deploy, or begin Phase 8 automatically.**

**Wait for explicit authorization defining the Phase 8 implementation scope before advancing.**

---

## File Assessment (Post-Creation Verification)

| Verification | Result |
|---|---|
| `Result.Version` remains `long` | Confirmed: `src/Swasthya.CoreLabs.Domain/Results/Result.cs:105` |
| No source implementation files changed | Confirmed: `git diff` shows no modifications to `src/`, `tests/`, API projects |
| No migrations/tests/API files changed | Confirmed |
| `git diff --check` passes | Confirmed |
| File created | `PHASE_7_AUTHORITATIVE_IMPLEMENTATION_SPECIFICATION.md` (new file) |
| Git state | Clean working tree (only the new specification file untracked) |

---

*This specification was produced from inspection of: `ROADMAP.md`, `docs/architecture/master-blueprint.md`, `docs/architecture/domain-boundaries.md`, accepted Phase 6 implementation and test state, and disposable PostgreSQL validation. No source code files were modified during its creation.*
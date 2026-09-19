# Device Integration Strategy

**Document owner:** Architecture / Laboratory Informatics | **Status:** Approved (Phase 0) | **Version:** 1.0

## Purpose

A **vendor-neutral** architecture for integrating analyzers and laboratory
instruments. It defines the conceptual boundaries now; implementation and
**runtime validation against real devices** happen in later phases
(Phases 23–26).

> **No real analyzer interoperability is claimed** without runtime
> validation against the actual device/protocol. Nothing in this document is
> a vendor commitment.

## Layer separation

Instrument data must flow through four distinct layers; they must never
collapse into one:

1. **Device-generated data** — raw bytes/telegrams/messages as delivered by
   the instrument (protocol-specific).
2. **Normalized observations** — device-independent, unit-normalized,
   code-mapped observations with full provenance (what/when/how).
3. **Laboratory validation** — human/QC governance over those observations
   (many instruments do not transmit validated results).
4. **Final clinical report** — the validated, released, versioned artifact
   issued to clinicians/patients.

## Concepts the architecture accounts for

| Concept | Description |
| --- | --- |
| Device identity | Stable device registry entry (manufacturer, model, serial) |
| Manufacturer / model | Canonical device registration data |
| Firmware / software version | Tracked for behavior and recall reasoning |
| Interface / protocol | Declared protocol family (HL7 v2 device profile, ASTM, LIS2, vendor API, serial, file drop) |
| Connection state | Monitored connection lifecycle, heartbeat, reconstitution |
| Test mappings | Device test/channel codes ↔ catalog/LOINC mappings (config data, versioned) |
| Specimen/work-item association | Correct association of measurements to work items (rack/tube/sample ID) |
| Measurement ingestion | Transport, framing, checksum, parsing, split handling |
| Units | Normalization to UCUM; raw unit retained alongside |
| Timestamps | Device clock + platform receipt time (both recorded, delta monitored) |
| Quality / status | Sample quality flags, result flags, RERUN/REPEAT/REFLEX semantics |
| Provenance | Device, operator, time, protocol captured per measurement |
| Duplicate detection | Dedupe of repeated transmissions without dropping legitimate repeats |
| Replay protection | Rejection/reconciliation of stale or replayed messages |
| Malformed input | Structured quarantine for unparsable/unexpected payloads |
| Quarantine | Failed/invalid ingestions held for review — never silently discarded |
| Raw vs normalized | Raw payload retained as evidence; normalized value is the working copy |
| Integration failures | Explicit failure states, alerts, manual fallback paths |
| Acknowledgements | Positive/negative acknowledgement back to the instrument |

## Boundary rules

- Device adapters live in Infrastructure as **adapters** behind an internal
  port; the clinical core is protocol-agnostic.
- Device protocols are mapped, not embedded into domain logic.
- Malformed or low-confidence ingestions go to **quarantine**, never to the
  final report.
- The platform stores the evidence (raw) plus the interpretation
  (normalized); neither can silently replace the other.
- Vendor test mappings are **configuration data with provenance and version
  history**, not code.

## Validation

- Analyzer/device claims require runtime validation against the device or an
  approved simulator with recorded evidence; see
  [`../validation/validation-strategy.md`](../validation/validation-strategy.md).

## Future phase scope (not Phase 0)

- Protocol adapters (HL7 v2 device profiles, ASTM E1381/E1394, LIS1/LIS2,
  vendor APIs, file-based extraction).
- Connection/device manager service.
- Mapping console and mapping audit trail.
- Quarantine and review UI/workflow.
- Device test/specimen association UX.
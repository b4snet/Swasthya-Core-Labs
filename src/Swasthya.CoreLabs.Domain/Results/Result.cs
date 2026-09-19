using Swasthya.CoreLabs.Domain.Laboratory;
using Swasthya.CoreLabs.Domain.Orders;
using Swasthya.CoreLabs.Domain.Specimen;

namespace Swasthya.CoreLabs.Domain.Results;

/// <summary>
/// A laboratory observation/result linked to the diagnostic chain:
/// Order → OrderItem → Specimen → Accession.
/// The result model is independent from the operational worklist.
/// A result always retains sufficient provenance to establish:
///   what was tested, which order item, which specimen, which catalog version,
///   what value was observed, unit where applicable, when, who entered it,
///   its status, and its provenance.
/// </summary>
public sealed class Result
{
    public Result()
    {
    }

    public Result(
        Guid orderItemId,
        Guid specimenId,
        Guid testVersionId,
        int valueTypeCode,
        decimal? numericValue,
        string? textualValue,
        string? codedValue,
        string? codedSystem,
        string? unitUcumCode,
        DateTimeOffset observedAtUtc,
        DateTimeOffset enteredAtUtc,
        Guid enteredByPrincipalId,
        string statusCode = "entered")
    {
        if (orderItemId == Guid.Empty)
        {
            throw new ArgumentException("An order item is required.", nameof(orderItemId));
        }

        if (specimenId == Guid.Empty)
        {
            throw new ArgumentException("A specimen is required.", nameof(specimenId));
        }

        if (testVersionId == Guid.Empty)
        {
            throw new ArgumentException("A test version is required.", nameof(testVersionId));
        }

        if (!Enum.IsDefined(typeof(ResultDataTypeKind), valueTypeCode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(valueTypeCode),
                valueTypeCode,
                "Unknown result data type kind.");
        }

        OrderItemId = orderItemId;
        SpecimenId = specimenId;
        TestVersionId = testVersionId;
        ValueTypeCode = valueTypeCode;
        NumericValue = numericValue;
        TextualValue = textualValue;
        CodedValue = codedValue;
        CodedSystem = codedSystem;
        UnitUcumCode = unitUcumCode;
        ObservedAtUtc = observedAtUtc;
        EnteredAtUtc = enteredAtUtc;
        EnteredByPrincipalId = enteredByPrincipalId;
        StatusCode = statusCode;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrderItemId { get; private set; }

    public Guid SpecimenId { get; private set; }

    public Guid TestVersionId { get; private set; }

    public int ValueTypeCode { get; private set; }

    public decimal? NumericValue { get; private set; }

    public string? TextualValue { get; private set; }

    public string? CodedValue { get; private set; }

    public string? CodedSystem { get; private set; }

    public string? UnitUcumCode { get; private set; }

    public DateTimeOffset ObservedAtUtc { get; private set; }

    public DateTimeOffset EnteredAtUtc { get; private set; }

    public Guid EnteredByPrincipalId { get; private set; }

    public string StatusCode { get; private set; } = "entered";

    public long Version { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void SetStatus(string newStatusCode)
    {
        // Only allow status transitions that are valid for the verified Phase 6 lifecycle.
        // Phase 7/8 lifecycle: "entered" is the only writable ordinary state; it is always
        // editable. Once a result is verified (verification is an explicit, persisted,
        // authorized transition), ordinary edits are prohibited. Finalization is a separate
        // explicit transition after which the result is immutable except for the controlled
        // correction path, which creates a NEW successor Result identity (the original is
        // never overwritten, deleted, or silently mutated).
        //
        // Contradiction guard: ordinary updates and any status transition that would
        // silently change a verified/finalized result are rejected. Finalization itself is
        // the single authorized transition from "verified"; correction is never exposed as
        // an ordinary SetStatus. Transition to the same code is a no-op.
        if (StatusCode == newStatusCode)
        {
            return;
        }

        // "entered" is always writable (ordinary edits are unrestricted in this state).
        // The only permitted non-"entered" transitions via this method family are the
        // explicit life cycle transitions performed by ResultService through the
        // authorized Verify/Finalize operations; a generic caller cannot use SetStatus to
        // silently finalize or mutate a verified/finalized result.
        if (StatusCode != "entered")
        {
            throw new InvalidOperationException(
                "Only results in the 'entered' state are editable through ordinary update. " +
                "Verification, finalization and correction are explicit, authorized operations " +
                "per ADR-0022 / ADR-0023 and cannot be performed by a generic status write.");
        }

        StatusCode = newStatusCode;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateValue(
        decimal? numericValue,
        string? textualValue,
        string? codedValue,
        string? codedSystem,
        string? unitUcumCode)
    {
        // Only unfinalized results may be updated.
        // Phase 6 defers finalization; this permits updates on entered/draft results.
        if (StatusCode != "entered")
        {
            throw new InvalidOperationException(
                "Only unfinalized results may be updated. Finalization is deferred to a later phase.");
        }

        NumericValue = numericValue;
        TextualValue = textualValue;
        CodedValue = codedValue;
        CodedSystem = codedSystem;
        UnitUcumCode = unitUcumCode;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}

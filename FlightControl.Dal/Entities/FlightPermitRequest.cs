using FlightControl.Dal.Enums;
using FlightControl.Dal.Exceptions;
using NetTopologySuite.Geometries;

namespace FlightControl.Dal.Entities;

public class FlightPermitRequest : BaseEntity
{
    public required Guid RequesterId { get; init; }
    public required PermitRequestType Type { get; init; }
    public required Guid AircraftId { get; init; }
    public required Guid LicenseId { get; init; }
    public required Guid PilotId { get; init; }
    public Guid? FleetId { get; init; }
    public required Polygon Area { get; init; }
    public required DateTime PlannedStart { get; init; }
    public required DateTime PlannedEnd { get; init; }
    public required double MaxAltitudeMeters { get; init; }
    public required string Purpose { get; init; }

    public PermitStatus Status { get; private set; } = PermitStatus.Pending;
    public Guid? DecidedById { get; private set; }
    public DateTime? DecidedAt { get; private set; }
    public string? DecisionNote { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public uint Version { get; private set; }

    public void Approve(Guid authorityId, DateTime at, string? note)
    {
        EnsurePending("approved");
        Status = PermitStatus.Approved;
        RecordDecision(authorityId, at, note);
    }

    public void Reject(Guid authorityId, DateTime at, string reason)
    {
        EnsurePending("rejected");
        Status = PermitStatus.Rejected;
        RecordDecision(authorityId, at, reason);
    }

    public void Cancel(DateTime at)
    {
        EnsurePending("cancelled");
        Status = PermitStatus.Cancelled;
        CancelledAt = at;
    }

    public void EnsurePending(string action)
    {
        if (Status != PermitStatus.Pending)
        {
            throw new InvalidStateTransitionException(
                $"Only pending requests can be {action}; this request is {Status}.");
        }
    }

    private void RecordDecision(Guid authorityId, DateTime at, string? note)
    {
        DecidedById = authorityId;
        DecidedAt = at;
        DecisionNote = note;
    }
}
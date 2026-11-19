using FlightControl.Dal.Entities;
using FlightControl.Dal.Enums;
using FlightControl.Dal.Exceptions;
using FluentAssertions;

namespace FlightControl.Tests.Entities;

public class FlightPermitRequestTests
{
    private static readonly DateTime DecisionTime = new(2030, 5, 20, 9, 0, 0, DateTimeKind.Utc);

    public enum Transition
    {
        Approve,
        Reject,
        Cancel,
    }

    [Fact]
    public void Approve_PendingRequest_RecordsDecision()
    {
        var permit = TestData.Permit();

        permit.Approve(TestData.AuthorityId, DecisionTime, "Cleared.");

        permit.Status.Should().Be(PermitStatus.Approved);
        permit.DecidedById.Should().Be(TestData.AuthorityId);
        permit.DecidedAt.Should().Be(DecisionTime);
        permit.DecisionNote.Should().Be("Cleared.");
        permit.CancelledAt.Should().BeNull();
    }

    [Fact]
    public void Approve_PendingRequestWithoutNote_RecordsNullNote()
    {
        var permit = TestData.Permit();

        permit.Approve(TestData.AuthorityId, DecisionTime, null);

        permit.Status.Should().Be(PermitStatus.Approved);
        permit.DecisionNote.Should().BeNull();
    }

    [Fact]
    public void Reject_PendingRequest_RecordsDecision()
    {
        var permit = TestData.Permit();

        permit.Reject(TestData.AuthorityId, DecisionTime, "Enters a restricted zone.");

        permit.Status.Should().Be(PermitStatus.Rejected);
        permit.DecidedById.Should().Be(TestData.AuthorityId);
        permit.DecidedAt.Should().Be(DecisionTime);
        permit.DecisionNote.Should().Be("Enters a restricted zone.");
        permit.CancelledAt.Should().BeNull();
    }

    [Fact]
    public void Cancel_PendingRequest_RecordsCancellation()
    {
        var permit = TestData.Permit();

        permit.Cancel(DecisionTime);

        permit.Status.Should().Be(PermitStatus.Cancelled);
        permit.CancelledAt.Should().Be(DecisionTime);
        permit.DecidedById.Should().BeNull();
        permit.DecidedAt.Should().BeNull();
        permit.DecisionNote.Should().BeNull();
    }

    [Theory]
    [InlineData(Transition.Approve, Transition.Approve)]
    [InlineData(Transition.Approve, Transition.Reject)]
    [InlineData(Transition.Approve, Transition.Cancel)]
    [InlineData(Transition.Reject, Transition.Approve)]
    [InlineData(Transition.Reject, Transition.Reject)]
    [InlineData(Transition.Reject, Transition.Cancel)]
    [InlineData(Transition.Cancel, Transition.Approve)]
    [InlineData(Transition.Cancel, Transition.Reject)]
    [InlineData(Transition.Cancel, Transition.Cancel)]
    public void Transition_RequestNotPending_ThrowsAndKeepsState(Transition first, Transition second)
    {
        var permit = TestData.Permit();
        Apply(permit, first);
        var statusBefore = permit.Status;
        var decidedAtBefore = permit.DecidedAt;
        var cancelledAtBefore = permit.CancelledAt;

        var act = () => Apply(permit, second, DecisionTime.AddDays(1));

        act.Should().Throw<InvalidStateTransitionException>()
            .WithMessage($"Only pending requests can be *; this request is {statusBefore}.");
        permit.Status.Should().Be(statusBefore);
        permit.DecidedAt.Should().Be(decidedAtBefore);
        permit.CancelledAt.Should().Be(cancelledAtBefore);
    }

    private static void Apply(FlightPermitRequest permit, Transition transition, DateTime? at = null)
    {
        var time = at ?? DecisionTime;

        switch (transition)
        {
            case Transition.Approve:
                permit.Approve(TestData.AuthorityId, time, "Note.");
                break;
            case Transition.Reject:
                permit.Reject(TestData.AuthorityId, time, "Reason.");
                break;
            case Transition.Cancel:
                permit.Cancel(time);
                break;
        }
    }
}

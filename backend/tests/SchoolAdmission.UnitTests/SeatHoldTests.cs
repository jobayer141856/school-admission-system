using SchoolAdmission.Domain.Exceptions;
using SchoolAdmission.Domain.SeatHolds;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class SeatHoldTests
{
    private readonly Guid _classId = Guid.NewGuid();
    private readonly StudentId _studentId = new("20152691234567890");
    private readonly DateTimeOffset _now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);
    private const int DefaultHoldDuration = 600; // 10 minutes

    [Fact]
    public void Create_InitializesHoldWithStatusHeldAndCorrectExpiry()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);

        Assert.NotEqual(Guid.Empty, hold.Id);
        Assert.Equal(_classId, hold.AdmissionClassId);
        Assert.Equal(_studentId, hold.StudentId);
        Assert.Equal(SeatHoldStatus.HELD, hold.Status);
        Assert.Equal(_now, hold.CreatedAtUtc);
        Assert.Equal(_now.AddSeconds(DefaultHoldDuration), hold.ExpiresAtUtc);
        Assert.Null(hold.ConfirmedAtUtc);
        Assert.Null(hold.CancelledAtUtc);
    }

    [Fact]
    public void Create_ThrowsDomainException_WhenClassIdIsEmpty()
    {
        var ex = Assert.Throws<DomainException>(() =>
            SeatHold.Create(Guid.Empty, _studentId, _now, DefaultHoldDuration));

        Assert.Contains("Admission class ID cannot be empty", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_ThrowsDomainException_WhenDurationIsNotPositive(int invalidDuration)
    {
        var ex = Assert.Throws<DomainException>(() =>
            SeatHold.Create(_classId, _studentId, _now, invalidDuration));

        Assert.Contains("Hold duration must be positive", ex.Message);
    }

    [Fact]
    public void IsExpired_ReturnsFalse_BeforeExpiry()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);
        var beforeExpiry = _now.AddSeconds(DefaultHoldDuration - 1);

        Assert.False(hold.IsExpired(beforeExpiry));
    }

    [Fact]
    public void IsExpired_ReturnsTrue_AtOrAfterExpiry()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);
        var atExpiry = _now.AddSeconds(DefaultHoldDuration);
        var afterExpiry = _now.AddSeconds(DefaultHoldDuration + 1);

        Assert.True(hold.IsExpired(atExpiry));
        Assert.True(hold.IsExpired(afterExpiry));
    }

    [Fact]
    public void Expire_TransitionsStatusToExpired_WhenPastExpiry()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);
        var afterExpiry = _now.AddSeconds(DefaultHoldDuration + 1);

        var transitioned = hold.Expire(afterExpiry);

        Assert.True(transitioned);
        Assert.Equal(SeatHoldStatus.EXPIRED, hold.Status);
    }

    [Fact]
    public void Expire_DoesNotTransition_WhenNotYetExpired()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);
        var beforeExpiry = _now.AddSeconds(100);

        var transitioned = hold.Expire(beforeExpiry);

        Assert.False(transitioned);
        Assert.Equal(SeatHoldStatus.HELD, hold.Status);
    }

    [Fact]
    public void Confirm_TransitionsStatusToConfirmed_WhenValid()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);
        var confirmTime = _now.AddSeconds(120);

        hold.Confirm(confirmTime);

        Assert.Equal(SeatHoldStatus.CONFIRMED, hold.Status);
        Assert.Equal(confirmTime, hold.ConfirmedAtUtc);
    }

    [Fact]
    public void Confirm_IsIdempotent_WhenAlreadyConfirmed()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);
        var firstConfirm = _now.AddSeconds(120);
        var secondConfirm = _now.AddSeconds(180);

        hold.Confirm(firstConfirm);
        hold.Confirm(secondConfirm);

        Assert.Equal(SeatHoldStatus.CONFIRMED, hold.Status);
        Assert.Equal(firstConfirm, hold.ConfirmedAtUtc); // original timestamp preserved
    }

    [Fact]
    public void Confirm_ThrowsDomainException_AndMarksExpired_WhenPastExpiry()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);
        var lateTime = _now.AddSeconds(DefaultHoldDuration + 10);

        var ex = Assert.Throws<DomainException>(() => hold.Confirm(lateTime));

        Assert.Contains("expired", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(SeatHoldStatus.EXPIRED, hold.Status);
        Assert.Null(hold.ConfirmedAtUtc);
    }

    [Fact]
    public void Confirm_ThrowsDomainException_WhenHoldIsCancelled()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);
        hold.Cancel(_now.AddSeconds(30));

        var ex = Assert.Throws<DomainException>(() => hold.Confirm(_now.AddSeconds(60)));
        Assert.Contains("Cancelled hold cannot be confirmed", ex.Message);
    }

    [Fact]
    public void Cancel_TransitionsStatusToCancelled_WhenValid()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);
        var cancelTime = _now.AddSeconds(60);

        hold.Cancel(cancelTime);

        Assert.Equal(SeatHoldStatus.CANCELLED, hold.Status);
        Assert.Equal(cancelTime, hold.CancelledAtUtc);
    }

    [Fact]
    public void Cancel_IsIdempotent_WhenAlreadyCancelled()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);
        var firstCancel = _now.AddSeconds(60);
        var secondCancel = _now.AddSeconds(90);

        hold.Cancel(firstCancel);
        hold.Cancel(secondCancel);

        Assert.Equal(SeatHoldStatus.CANCELLED, hold.Status);
        Assert.Equal(firstCancel, hold.CancelledAtUtc);
    }

    [Fact]
    public void Cancel_ThrowsDomainException_WhenHoldIsAlreadyConfirmed()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);
        hold.Confirm(_now.AddSeconds(60));

        var ex = Assert.Throws<DomainException>(() => hold.Cancel(_now.AddSeconds(120)));
        Assert.Contains("Confirmed admission cannot be cancelled", ex.Message);
    }

    [Fact]
    public void Cancel_ThrowsDomainException_AndMarksExpired_WhenPastExpiry()
    {
        var hold = SeatHold.Create(_classId, _studentId, _now, DefaultHoldDuration);
        var lateTime = _now.AddSeconds(DefaultHoldDuration + 50);

        var ex = Assert.Throws<DomainException>(() => hold.Cancel(lateTime));

        Assert.Contains("Expired hold cannot be cancelled", ex.Message);
        Assert.Equal(SeatHoldStatus.EXPIRED, hold.Status);
    }
}


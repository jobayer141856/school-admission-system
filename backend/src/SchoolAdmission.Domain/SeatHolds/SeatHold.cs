using SchoolAdmission.Domain.Common;
using SchoolAdmission.Domain.Exceptions;

namespace SchoolAdmission.Domain.SeatHolds;

public sealed class SeatHold : Entity
{
    public Guid AdmissionClassId { get; private set; }
    public StudentId StudentId { get; private set; }
    public SeatHoldStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? ConfirmedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }

    // Private constructor for EF Core
    private SeatHold() { }

    private SeatHold(
        Guid id,
        Guid admissionClassId,
        StudentId studentId,
        SeatHoldStatus status,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
        : base(id)
    {
        AdmissionClassId = admissionClassId;
        StudentId = studentId;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static SeatHold Create(
        Guid admissionClassId,
        StudentId studentId,
        DateTimeOffset now,
        int durationSeconds)
    {
        if (admissionClassId == Guid.Empty)
        {
            throw new DomainException("Admission class ID cannot be empty.");
        }

        if (durationSeconds <= 0)
        {
            throw new DomainException("Hold duration must be positive.");
        }

        return new SeatHold(
            Guid.NewGuid(),
            admissionClassId,
            studentId,
            SeatHoldStatus.HELD,
            now,
            now.AddSeconds(durationSeconds));
    }

    public bool IsExpired(DateTimeOffset now) => ExpiresAtUtc <= now;

    public bool Expire(DateTimeOffset now)
    {
        if (Status == SeatHoldStatus.HELD && IsExpired(now))
        {
            Status = SeatHoldStatus.EXPIRED;
            return true;
        }

        return false;
    }

    public void Confirm(DateTimeOffset now)
    {
        if (Status == SeatHoldStatus.CONFIRMED)
        {
            // Idempotent: already confirmed
            return;
        }

        if (Status == SeatHoldStatus.CANCELLED)
        {
            throw new DomainException("Cancelled hold cannot be confirmed.");
        }

        if (IsExpired(now))
        {
            Status = SeatHoldStatus.EXPIRED;
            throw new DomainException("Hold has expired and cannot be confirmed.");
        }

        if (Status != SeatHoldStatus.HELD)
        {
            throw new DomainException($"Hold in status '{Status}' cannot be confirmed.");
        }

        Status = SeatHoldStatus.CONFIRMED;
        ConfirmedAtUtc = now;
    }

    public void Cancel(DateTimeOffset now)
    {
        if (Status == SeatHoldStatus.CANCELLED)
        {
            // Idempotent: already cancelled
            return;
        }

        if (Status == SeatHoldStatus.CONFIRMED)
        {
            throw new DomainException("Confirmed admission cannot be cancelled.");
        }

        if (IsExpired(now))
        {
            Status = SeatHoldStatus.EXPIRED;
            throw new DomainException("Expired hold cannot be cancelled.");
        }

        if (Status != SeatHoldStatus.HELD)
        {
            throw new DomainException($"Hold in status '{Status}' cannot be cancelled.");
        }

        Status = SeatHoldStatus.CANCELLED;
        CancelledAtUtc = now;
    }
}


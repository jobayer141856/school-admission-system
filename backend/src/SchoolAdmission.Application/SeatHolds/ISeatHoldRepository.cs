using SchoolAdmission.Application.SeatHolds.Dtos;
using SchoolAdmission.Domain.SeatHolds;

namespace SchoolAdmission.Application.SeatHolds;

public enum CreateHoldOutcome
{
    Created,
    ExistingActiveHoldReturned,
    ClassNotFound,
    ClassFull,
    HasActiveHoldInAnotherClass
}

public sealed record CreateHoldResult(CreateHoldOutcome Outcome, SeatHold? Hold, string? Message = null);

public enum ConfirmHoldOutcome
{
    Confirmed,
    AlreadyConfirmed,
    NotFound,
    Expired,
    Conflict
}

public sealed record ConfirmHoldResult(ConfirmHoldOutcome Outcome, SeatHold? Hold, string? Message = null);

public enum CancelHoldOutcome
{
    Cancelled,
    AlreadyCancelled,
    NotFound,
    AlreadyConfirmed,
    Expired,
    Conflict
}

public sealed record CancelHoldResult(CancelHoldOutcome Outcome, string? Message = null);

public interface ISeatHoldRepository
{
    Task<CreateHoldResult> CreateHoldWithLockAsync(
        Guid classId,
        StudentId studentId,
        DateTimeOffset now,
        int durationSeconds,
        CancellationToken cancellationToken = default);

    Task<SeatHold?> GetByIdAndStudentAsync(
        Guid holdId,
        StudentId studentId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    Task<ConfirmHoldResult> ConfirmHoldAsync(
        Guid holdId,
        StudentId studentId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    Task<CancelHoldResult> CancelHoldAsync(
        Guid holdId,
        StudentId studentId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentHoldItemResponse>> GetStudentHoldsAsync(
        StudentId studentId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}


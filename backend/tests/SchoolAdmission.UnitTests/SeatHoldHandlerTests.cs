using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Application.SeatHolds;
using SchoolAdmission.Application.SeatHolds.Dtos;
using SchoolAdmission.Domain.AdmissionClasses;
using SchoolAdmission.Domain.SeatHolds;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class SeatHoldHandlerTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeSeatHoldRepository : ISeatHoldRepository
    {
        public readonly List<SeatHold> Holds = new();
        public readonly Dictionary<Guid, int> ClassCapacities = new();
        public readonly Dictionary<Guid, string> ClassNames = new();

        public Task<CreateHoldResult> CreateHoldWithLockAsync(
            Guid classId,
            StudentId studentId,
            DateTimeOffset now,
            int durationSeconds,
            CancellationToken cancellationToken = default)
        {
            if (!ClassCapacities.TryGetValue(classId, out var totalSeats))
            {
                return Task.FromResult(new CreateHoldResult(CreateHoldOutcome.ClassNotFound, null, "Class not found"));
            }

            // Check student active hold
            var activeStudentHold = Holds.FirstOrDefault(h => h.StudentId == studentId && h.Status == SeatHoldStatus.HELD && h.ExpiresAtUtc > now);
            if (activeStudentHold != null)
            {
                if (activeStudentHold.AdmissionClassId == classId)
                {
                    return Task.FromResult(new CreateHoldResult(CreateHoldOutcome.ExistingActiveHoldReturned, activeStudentHold));
                }

                return Task.FromResult(new CreateHoldResult(CreateHoldOutcome.HasActiveHoldInAnotherClass, null, "Active hold in another class"));
            }

            // Count class active
            var activeHeld = Holds.Count(h => h.AdmissionClassId == classId && h.Status == SeatHoldStatus.HELD && h.ExpiresAtUtc > now);
            var confirmed = Holds.Count(h => h.AdmissionClassId == classId && h.Status == SeatHoldStatus.CONFIRMED);

            if (activeHeld + confirmed >= totalSeats)
            {
                return Task.FromResult(new CreateHoldResult(CreateHoldOutcome.ClassFull, null, "Class full"));
            }

            var newHold = SeatHold.Create(classId, studentId, now, durationSeconds);
            Holds.Add(newHold);
            return Task.FromResult(new CreateHoldResult(CreateHoldOutcome.Created, newHold));
        }

        public Task<SeatHold?> GetByIdAndStudentAsync(
            Guid holdId,
            StudentId studentId,
            DateTimeOffset now,
            CancellationToken cancellationToken = default)
        {
            var hold = Holds.FirstOrDefault(h => h.Id == holdId && h.StudentId == studentId);
            if (hold != null && hold.Status == SeatHoldStatus.HELD && hold.IsExpired(now))
            {
                hold.Expire(now);
            }
            return Task.FromResult(hold);
        }

        public Task<ConfirmHoldResult> ConfirmHoldAsync(
            Guid holdId,
            StudentId studentId,
            DateTimeOffset now,
            CancellationToken cancellationToken = default)
        {
            var hold = Holds.FirstOrDefault(h => h.Id == holdId && h.StudentId == studentId);
            if (hold == null) return Task.FromResult(new ConfirmHoldResult(ConfirmHoldOutcome.NotFound, null));

            if (hold.Status == SeatHoldStatus.CONFIRMED)
            {
                return Task.FromResult(new ConfirmHoldResult(ConfirmHoldOutcome.AlreadyConfirmed, hold));
            }

            if (hold.IsExpired(now))
            {
                hold.Expire(now);
                return Task.FromResult(new ConfirmHoldResult(ConfirmHoldOutcome.Expired, null, "Expired"));
            }

            if (hold.Status != SeatHoldStatus.HELD)
            {
                return Task.FromResult(new ConfirmHoldResult(ConfirmHoldOutcome.Conflict, null, "Not held"));
            }

            hold.Confirm(now);
            return Task.FromResult(new ConfirmHoldResult(ConfirmHoldOutcome.Confirmed, hold));
        }

        public Task<CancelHoldResult> CancelHoldAsync(
            Guid holdId,
            StudentId studentId,
            DateTimeOffset now,
            CancellationToken cancellationToken = default)
        {
            var hold = Holds.FirstOrDefault(h => h.Id == holdId && h.StudentId == studentId);
            if (hold == null) return Task.FromResult(new CancelHoldResult(CancelHoldOutcome.NotFound));

            if (hold.Status == SeatHoldStatus.CANCELLED)
            {
                return Task.FromResult(new CancelHoldResult(CancelHoldOutcome.AlreadyCancelled));
            }

            if (hold.Status == SeatHoldStatus.CONFIRMED)
            {
                return Task.FromResult(new CancelHoldResult(CancelHoldOutcome.AlreadyConfirmed));
            }

            if (hold.IsExpired(now))
            {
                hold.Expire(now);
                return Task.FromResult(new CancelHoldResult(CancelHoldOutcome.Expired));
            }

            if (hold.Status != SeatHoldStatus.HELD)
            {
                return Task.FromResult(new CancelHoldResult(CancelHoldOutcome.Conflict));
            }

            hold.Cancel(now);
            return Task.FromResult(new CancelHoldResult(CancelHoldOutcome.Cancelled));
        }

        public Task<IReadOnlyList<StudentHoldItemResponse>> GetStudentHoldsAsync(
            StudentId studentId,
            DateTimeOffset now,
            CancellationToken cancellationToken = default)
        {
            var results = Holds
                .Where(h => h.StudentId == studentId)
                .Select(h => new StudentHoldItemResponse(
                    IdHelper.ToHoldId(h.Id),
                    IdHelper.ToClassId(h.AdmissionClassId),
                    ClassNames.GetValueOrDefault(h.AdmissionClassId, "Class"),
                    h.Status.ToString(),
                    h.ExpiresAtUtc))
                .ToList();

            return Task.FromResult<IReadOnlyList<StudentHoldItemResponse>>(results);
        }
    }

    [Fact]
    public async Task CreateSeatHoldHandler_CreatesHold_WhenCapacityAvailable()
    {
        var repo = new FakeSeatHoldRepository();
        var clock = new FakeClock();
        var classId = Guid.NewGuid();
        repo.ClassCapacities[classId] = 10;
        var handler = new CreateSeatHoldHandler(repo, clock, new HoldOptions { DurationSeconds = 600 });

        var result = await handler.HandleAsync(IdHelper.ToClassId(classId), "20152691234567890");

        Assert.Equal(CreateHoldOutcome.Created, result.Outcome);
        Assert.NotNull(result.Hold);
        Assert.StartsWith("h_", result.Hold.HoldId);
        Assert.Equal("HELD", result.Hold.Status);
    }

    [Fact]
    public async Task CreateSeatHoldHandler_ReturnsExistingHold_WhenDuplicateRequestForSameClass()
    {
        var repo = new FakeSeatHoldRepository();
        var clock = new FakeClock();
        var classId = Guid.NewGuid();
        repo.ClassCapacities[classId] = 10;
        var handler = new CreateSeatHoldHandler(repo, clock);

        var first = await handler.HandleAsync(IdHelper.ToClassId(classId), "20152691234567890");
        var second = await handler.HandleAsync(IdHelper.ToClassId(classId), "20152691234567890");

        Assert.Equal(CreateHoldOutcome.ExistingActiveHoldReturned, second.Outcome);
        Assert.Equal(first.Hold!.HoldId, second.Hold!.HoldId);
    }

    [Fact]
    public async Task CreateSeatHoldHandler_ReturnsConflict_WhenStudentHasActiveHoldInAnotherClass()
    {
        var repo = new FakeSeatHoldRepository();
        var clock = new FakeClock();
        var class1 = Guid.NewGuid();
        var class2 = Guid.NewGuid();
        repo.ClassCapacities[class1] = 10;
        repo.ClassCapacities[class2] = 10;
        var handler = new CreateSeatHoldHandler(repo, clock);

        await handler.HandleAsync(IdHelper.ToClassId(class1), "20152691234567890");
        var second = await handler.HandleAsync(IdHelper.ToClassId(class2), "20152691234567890");

        Assert.Equal(CreateHoldOutcome.HasActiveHoldInAnotherClass, second.Outcome);
    }

    [Fact]
    public async Task CreateSeatHoldHandler_ReturnsClassFull_WhenNoSeatsRemain()
    {
        var repo = new FakeSeatHoldRepository();
        var clock = new FakeClock();
        var classId = Guid.NewGuid();
        repo.ClassCapacities[classId] = 1; // capacity 1
        var handler = new CreateSeatHoldHandler(repo, clock);

        var first = await handler.HandleAsync(IdHelper.ToClassId(classId), "STUDENT_1");
        var second = await handler.HandleAsync(IdHelper.ToClassId(classId), "STUDENT_2");

        Assert.Equal(CreateHoldOutcome.Created, first.Outcome);
        Assert.Equal(CreateHoldOutcome.ClassFull, second.Outcome);
    }

    [Fact]
    public async Task ConfirmSeatHoldHandler_ConfirmsHold_AndIsIdempotent()
    {
        var repo = new FakeSeatHoldRepository();
        var clock = new FakeClock();
        var classId = Guid.NewGuid();
        repo.ClassCapacities[classId] = 10;
        var createHandler = new CreateSeatHoldHandler(repo, clock);
        var created = await createHandler.HandleAsync(IdHelper.ToClassId(classId), "STUDENT_1");

        var confirmHandler = new ConfirmSeatHoldHandler(repo, clock);
        var firstConfirm = await confirmHandler.HandleAsync(created.Hold!.HoldId, "STUDENT_1");
        var secondConfirm = await confirmHandler.HandleAsync(created.Hold.HoldId, "STUDENT_1");

        Assert.Equal(ConfirmHoldOutcome.Confirmed, firstConfirm.Outcome);
        Assert.Equal("CONFIRMED", firstConfirm.Response!.Status);
        Assert.Equal(ConfirmHoldOutcome.AlreadyConfirmed, secondConfirm.Outcome);
        Assert.Equal("CONFIRMED", secondConfirm.Response!.Status);
    }

    [Fact]
    public async Task CancelSeatHoldHandler_CancelsHold_AndIsIdempotent()
    {
        var repo = new FakeSeatHoldRepository();
        var clock = new FakeClock();
        var classId = Guid.NewGuid();
        repo.ClassCapacities[classId] = 10;
        var createHandler = new CreateSeatHoldHandler(repo, clock);
        var created = await createHandler.HandleAsync(IdHelper.ToClassId(classId), "STUDENT_1");

        var cancelHandler = new CancelSeatHoldHandler(repo, clock);
        var firstCancel = await cancelHandler.HandleAsync(created.Hold!.HoldId, "STUDENT_1");
        var secondCancel = await cancelHandler.HandleAsync(created.Hold.HoldId, "STUDENT_1");

        Assert.Equal(CancelHoldOutcome.Cancelled, firstCancel.Outcome);
        Assert.Equal(CancelHoldOutcome.AlreadyCancelled, secondCancel.Outcome);
    }
}


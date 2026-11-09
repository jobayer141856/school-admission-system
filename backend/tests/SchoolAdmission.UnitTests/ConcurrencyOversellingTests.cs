using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Application.SeatHolds;
using SchoolAdmission.Application.SeatHolds.Dtos;
using SchoolAdmission.Domain.SeatHolds;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class ConcurrencyOversellingTests
{
    private sealed class ThreadSafeSeatHoldRepository : ISeatHoldRepository
    {
        private readonly object _sync = new();
        public readonly List<SeatHold> Holds = new();
        public readonly Dictionary<Guid, int> Capacities = new();

        public Task<CreateHoldResult> CreateHoldWithLockAsync(
            Guid classId,
            StudentId studentId,
            DateTimeOffset now,
            int durationSeconds,
            CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                if (!Capacities.TryGetValue(classId, out var totalSeats))
                {
                    return Task.FromResult(new CreateHoldResult(CreateHoldOutcome.ClassNotFound, null, "Class not found"));
                }

                // Check student active hold
                var existingHold = Holds.FirstOrDefault(h => h.StudentId == studentId && h.Status == SeatHoldStatus.HELD && h.ExpiresAtUtc > now);
                if (existingHold != null)
                {
                    if (existingHold.AdmissionClassId == classId)
                    {
                        return Task.FromResult(new CreateHoldResult(CreateHoldOutcome.ExistingActiveHoldReturned, existingHold));
                    }

                    return Task.FromResult(new CreateHoldResult(CreateHoldOutcome.HasActiveHoldInAnotherClass, null, "Active hold in another class"));
                }

                // Count active seats
                var activeHeld = Holds.Count(h => h.AdmissionClassId == classId && h.Status == SeatHoldStatus.HELD && h.ExpiresAtUtc > now);
                var confirmed = Holds.Count(h => h.AdmissionClassId == classId && h.Status == SeatHoldStatus.CONFIRMED);

                if (activeHeld + confirmed >= totalSeats)
                {
                    return Task.FromResult(new CreateHoldResult(CreateHoldOutcome.ClassFull, null, "Class is full"));
                }

                var hold = SeatHold.Create(classId, studentId, now, durationSeconds);
                Holds.Add(hold);
                return Task.FromResult(new CreateHoldResult(CreateHoldOutcome.Created, hold));
            }
        }

        public Task<SeatHold?> GetByIdAndStudentAsync(Guid holdId, StudentId studentId, DateTimeOffset now, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ConfirmHoldResult> ConfirmHoldAsync(Guid holdId, StudentId studentId, DateTimeOffset now, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<CancelHoldResult> CancelHoldAsync(Guid holdId, StudentId studentId, DateTimeOffset now, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<StudentHoldItemResponse>> GetStudentHoldsAsync(StudentId studentId, DateTimeOffset now, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);
    }

    [Fact]
    public async Task SimultaneousHoldRequests_NeverExceedCapacity_AndNeverOversell()
    {
        // Arrange
        const int capacity = 5;
        const int totalAttempts = 50;
        var classId = Guid.NewGuid();

        var repo = new ThreadSafeSeatHoldRepository();
        repo.Capacities[classId] = capacity;

        var clock = new FakeClock();
        var handler = new CreateSeatHoldHandler(repo, clock, new HoldOptions { DurationSeconds = 600 });

        // Act: 50 concurrent requests competing for 5 seats
        var tasks = Enumerable.Range(1, totalAttempts)
            .Select(i => Task.Run(async () =>
            {
                var studentId = $"STUDENT_{i:D4}";
                return await handler.HandleAsync(IdHelper.ToClassId(classId), studentId);
            }))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        var successful = results.Count(r => r.Outcome == CreateHoldOutcome.Created);
        var full = results.Count(r => r.Outcome == CreateHoldOutcome.ClassFull);

        Assert.Equal(capacity, successful);
        Assert.Equal(totalAttempts - capacity, full);
        Assert.Equal(capacity, repo.Holds.Count);
        Assert.True(repo.Holds.Count <= capacity, "Holds must never exceed capacity");
    }

    [Fact]
    public async Task SimultaneousRequests_ForSameStudent_NeverCreateDuplicateActiveHolds()
    {
        // Arrange
        const int capacity = 20;
        const int totalAttempts = 25;
        var classId = Guid.NewGuid();

        var repo = new ThreadSafeSeatHoldRepository();
        repo.Capacities[classId] = capacity;

        var clock = new FakeClock();
        var handler = new CreateSeatHoldHandler(repo, clock);
        const string sameStudentId = "SAME_STUDENT_12345";

        // Act: 25 concurrent requests for the exact same student
        var tasks = Enumerable.Range(1, totalAttempts)
            .Select(_ => Task.Run(async () =>
            {
                return await handler.HandleAsync(IdHelper.ToClassId(classId), sameStudentId);
            }))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert
        var created = results.Count(r => r.Outcome == CreateHoldOutcome.Created);
        var returnedExisting = results.Count(r => r.Outcome == CreateHoldOutcome.ExistingActiveHoldReturned);

        Assert.Equal(1, created);
        Assert.Equal(totalAttempts - 1, returnedExisting);
        Assert.Single(repo.Holds);
        Assert.Equal(sameStudentId, repo.Holds[0].StudentId.Value);
    }
}


using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.API.Controllers;
using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Application.SeatHolds;
using SchoolAdmission.Application.SeatHolds.Dtos;
using SchoolAdmission.Domain.AdmissionClasses;
using SchoolAdmission.Domain.SeatHolds;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class HoldsControllerTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = DateTimeOffset.UtcNow;
    }

    private sealed class InMemorySeatHoldRepo : ISeatHoldRepository
    {
        public readonly List<SeatHold> Holds = new();
        public readonly Dictionary<Guid, int> Capacities = new();

        public Task<CreateHoldResult> CreateHoldWithLockAsync(Guid classId, StudentId studentId, DateTimeOffset now, int durationSeconds, CancellationToken cancellationToken = default)
        {
            if (!Capacities.TryGetValue(classId, out var cap))
            {
                return Task.FromResult(new CreateHoldResult(CreateHoldOutcome.ClassNotFound, null));
            }

            var hold = SeatHold.Create(classId, studentId, now, durationSeconds);
            Holds.Add(hold);
            return Task.FromResult(new CreateHoldResult(CreateHoldOutcome.Created, hold));
        }

        public Task<SeatHold?> GetByIdAndStudentAsync(Guid holdId, StudentId studentId, DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Holds.FirstOrDefault(h => h.Id == holdId && h.StudentId == studentId));
        }

        public Task<ConfirmHoldResult> ConfirmHoldAsync(Guid holdId, StudentId studentId, DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var hold = Holds.FirstOrDefault(h => h.Id == holdId && h.StudentId == studentId);
            if (hold == null) return Task.FromResult(new ConfirmHoldResult(ConfirmHoldOutcome.NotFound, null));
            hold.Confirm(now);
            return Task.FromResult(new ConfirmHoldResult(ConfirmHoldOutcome.Confirmed, hold));
        }

        public Task<CancelHoldResult> CancelHoldAsync(Guid holdId, StudentId studentId, DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var hold = Holds.FirstOrDefault(h => h.Id == holdId && h.StudentId == studentId);
            if (hold == null) return Task.FromResult(new CancelHoldResult(CancelHoldOutcome.NotFound));
            hold.Cancel(now);
            return Task.FromResult(new CancelHoldResult(CancelHoldOutcome.Cancelled));
        }

        public Task<IReadOnlyList<StudentHoldItemResponse>> GetStudentHoldsAsync(StudentId studentId, DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var list = Holds.Where(h => h.StudentId == studentId)
                .Select(h => new StudentHoldItemResponse(
                    IdHelper.ToHoldId(h.Id),
                    IdHelper.ToClassId(h.AdmissionClassId),
                    "Test Class",
                    h.Status.ToString(),
                    h.ExpiresAtUtc)).ToList();
            return Task.FromResult<IReadOnlyList<StudentHoldItemResponse>>(list);
        }
    }

    [Fact]
    public async Task CreateHold_ReturnsBadRequest_WhenXStudentIdIsMissing()
    {
        var repo = new InMemorySeatHoldRepo();
        var clock = new FakeClock();
        var controller = new HoldsController(
            new CreateSeatHoldHandler(repo, clock),
            new GetSeatHoldHandler(repo, clock),
            new ConfirmSeatHoldHandler(repo, clock),
            new CancelSeatHoldHandler(repo, clock));

        var result = await controller.CreateHold("c_123", "", CancellationToken.None);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task CreateHold_ReturnsCreatedAtAction_WhenValid()
    {
        var repo = new InMemorySeatHoldRepo();
        var clock = new FakeClock();
        var classId = Guid.NewGuid();
        repo.Capacities[classId] = 10;
        var controller = new HoldsController(
            new CreateSeatHoldHandler(repo, clock),
            new GetSeatHoldHandler(repo, clock),
            new ConfirmSeatHoldHandler(repo, clock),
            new CancelSeatHoldHandler(repo, clock));

        var result = await controller.CreateHold(IdHelper.ToClassId(classId), "20152691234567890", CancellationToken.None);
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
        var body = Assert.IsType<SeatHoldResponse>(created.Value);
        Assert.Equal("HELD", body.Status);
    }

    [Fact]
    public async Task ConfirmHold_ReturnsOk_WhenValid()
    {
        var repo = new InMemorySeatHoldRepo();
        var clock = new FakeClock();
        var classId = Guid.NewGuid();
        repo.Capacities[classId] = 10;
        var controller = new HoldsController(
            new CreateSeatHoldHandler(repo, clock),
            new GetSeatHoldHandler(repo, clock),
            new ConfirmSeatHoldHandler(repo, clock),
            new CancelSeatHoldHandler(repo, clock));

        var createResult = await controller.CreateHold(IdHelper.ToClassId(classId), "20152691234567890", CancellationToken.None);
        var createdBody = (SeatHoldResponse)((CreatedAtActionResult)createResult).Value!;

        var confirmResult = await controller.ConfirmHold(createdBody.HoldId, "20152691234567890", CancellationToken.None);
        var okResult = Assert.IsType<OkObjectResult>(confirmResult);
        var confirmBody = Assert.IsType<ConfirmHoldResponse>(okResult.Value);
        Assert.Equal("CONFIRMED", confirmBody.Status);
    }

    [Fact]
    public async Task CancelHold_ReturnsNoContent_WhenValid()
    {
        var repo = new InMemorySeatHoldRepo();
        var clock = new FakeClock();
        var classId = Guid.NewGuid();
        repo.Capacities[classId] = 10;
        var controller = new HoldsController(
            new CreateSeatHoldHandler(repo, clock),
            new GetSeatHoldHandler(repo, clock),
            new ConfirmSeatHoldHandler(repo, clock),
            new CancelSeatHoldHandler(repo, clock));

        var createResult = await controller.CreateHold(IdHelper.ToClassId(classId), "20152691234567890", CancellationToken.None);
        var createdBody = (SeatHoldResponse)((CreatedAtActionResult)createResult).Value!;

        var cancelResult = await controller.CancelHold(createdBody.HoldId, "20152691234567890", CancellationToken.None);
        Assert.IsType<NoContentResult>(cancelResult);
    }
}


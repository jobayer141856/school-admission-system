using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.API.Controllers;
using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Application.SeatHolds;
using SchoolAdmission.Application.SeatHolds.Dtos;
using SchoolAdmission.Domain.SeatHolds;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class MeControllerTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = DateTimeOffset.UtcNow;
    }

    private sealed class DummyRepo : ISeatHoldRepository
    {
        public Task<CreateHoldResult> CreateHoldWithLockAsync(Guid classId, StudentId studentId, DateTimeOffset now, int durationSeconds, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SeatHold?> GetByIdAndStudentAsync(Guid holdId, StudentId studentId, DateTimeOffset now, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ConfirmHoldResult> ConfirmHoldAsync(Guid holdId, StudentId studentId, DateTimeOffset now, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<CancelHoldResult> CancelHoldAsync(Guid holdId, StudentId studentId, DateTimeOffset now, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<IReadOnlyList<StudentHoldItemResponse>> GetStudentHoldsAsync(StudentId studentId, DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var item = new StudentHoldItemResponse("h_1", "c_1", "Class 6", "HELD", now.AddMinutes(10));
            return Task.FromResult<IReadOnlyList<StudentHoldItemResponse>>(new[] { item });
        }
    }

    [Fact]
    public async Task GetMyHolds_ReturnsBadRequest_WhenXStudentIdMissing()
    {
        var controller = new MeController(new GetStudentSeatHoldsHandler(new DummyRepo(), new FakeClock()));
        var result = await controller.GetMyHolds("", CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetMyHolds_ReturnsOkWithList_WhenValid()
    {
        var controller = new MeController(new GetStudentSeatHoldsHandler(new DummyRepo(), new FakeClock()));
        var result = await controller.GetMyHolds("20152691234567890", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IReadOnlyList<StudentHoldItemResponse>>(ok.Value);
        Assert.Single(list);
    }
}


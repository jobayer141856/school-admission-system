using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.API.Controllers;
using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.AdmissionClasses;
using SchoolAdmission.Application.AdmissionClasses.Dtos;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Domain.AdmissionClasses;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class ClassesControllerTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = DateTimeOffset.UtcNow;
    }

    private sealed class InMemoryRepo : IAdmissionClassRepository
    {
        private readonly List<AdmissionClass> _classes = new();

        public Task AddAsync(AdmissionClass admissionClass, CancellationToken cancellationToken = default)
        {
            _classes.Add(admissionClass);
            return Task.CompletedTask;
        }

        public Task<AdmissionClass?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_classes.FirstOrDefault(c => c.Id == id));
        }

        public Task<IReadOnlyList<AdmissionClassDetailResponse>> GetAllWithCountsAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var dtos = _classes.Select(c => new AdmissionClassDetailResponse(
                IdHelper.ToClassId(c.Id),
                c.Name,
                c.TotalSeats,
                0,
                0,
                c.TotalSeats)).ToList();

            return Task.FromResult<IReadOnlyList<AdmissionClassDetailResponse>>(dtos);
        }

        public Task<AdmissionClassDetailResponse?> GetByIdWithCountsAsync(Guid id, DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var c = _classes.FirstOrDefault(x => x.Id == id);
            if (c == null) return Task.FromResult<AdmissionClassDetailResponse?>(null);

            return Task.FromResult<AdmissionClassDetailResponse?>(new AdmissionClassDetailResponse(
                IdHelper.ToClassId(c.Id),
                c.Name,
                c.TotalSeats,
                0,
                0,
                c.TotalSeats));
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task CreateClass_ReturnsCreatedAtActionResult_WithLocationAndBody()
    {
        var repo = new InMemoryRepo();
        var clock = new FakeClock();
        var controller = new ClassesController(
            new CreateAdmissionClassHandler(repo, clock),
            new GetAdmissionClassesHandler(repo, clock),
            new GetAdmissionClassByIdHandler(repo, clock));

        var result = await controller.CreateClass(new CreateAdmissionClassRequest("Class 6 - Morning", 120), CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        var body = Assert.IsType<AdmissionClassResponse>(createdResult.Value);
        Assert.Equal("Class 6 - Morning", body.Name);
        Assert.Equal(120, body.TotalSeats);
        Assert.StartsWith("c_", body.Id);
    }

    [Fact]
    public async Task GetClasses_ReturnsOkWithList()
    {
        var repo = new InMemoryRepo();
        var clock = new FakeClock();
        var controller = new ClassesController(
            new CreateAdmissionClassHandler(repo, clock),
            new GetAdmissionClassesHandler(repo, clock),
            new GetAdmissionClassByIdHandler(repo, clock));

        await controller.CreateClass(new CreateAdmissionClassRequest("Class 6", 50), CancellationToken.None);
        var result = await controller.GetClasses(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IReadOnlyList<AdmissionClassDetailResponse>>(okResult.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task GetClassById_ReturnsNotFound_WhenClassDoesNotExist()
    {
        var repo = new InMemoryRepo();
        var clock = new FakeClock();
        var controller = new ClassesController(
            new CreateAdmissionClassHandler(repo, clock),
            new GetAdmissionClassesHandler(repo, clock),
            new GetAdmissionClassByIdHandler(repo, clock));

        var result = await controller.GetClassById("c_nonexistent", CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}


using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.AdmissionClasses;
using SchoolAdmission.Application.AdmissionClasses.Dtos;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Domain.AdmissionClasses;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class AdmissionClassHandlerTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeAdmissionClassRepository : IAdmissionClassRepository
    {
        public readonly List<AdmissionClass> Classes = new();
        public readonly List<AdmissionClassDetailResponse> Details = new();

        public Task AddAsync(AdmissionClass admissionClass, CancellationToken cancellationToken = default)
        {
            Classes.Add(admissionClass);
            return Task.CompletedTask;
        }

        public Task<AdmissionClass?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Classes.FirstOrDefault(c => c.Id == id));
        }

        public Task<IReadOnlyList<AdmissionClassDetailResponse>> GetAllWithCountsAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AdmissionClassDetailResponse>>(Details);
        }

        public Task<AdmissionClassDetailResponse?> GetByIdWithCountsAsync(Guid id, DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            var item = Details.FirstOrDefault(d => IdHelper.TryParseClassId(d.Id, out var parsed) && parsed == id);
            return Task.FromResult(item);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task CreateAdmissionClassHandler_CreatesClassAndReturnsPrefixedId()
    {
        var repo = new FakeAdmissionClassRepository();
        var clock = new FakeClock();
        var handler = new CreateAdmissionClassHandler(repo, clock);

        var response = await handler.HandleAsync(new CreateAdmissionClassRequest("Class 6 - Morning", 120));

        Assert.NotNull(response);
        Assert.StartsWith("c_", response.Id);
        Assert.Equal("Class 6 - Morning", response.Name);
        Assert.Equal(120, response.TotalSeats);
        Assert.Single(repo.Classes);
        Assert.Equal(response.Name, repo.Classes[0].Name);
    }

    [Fact]
    public async Task GetAdmissionClassesHandler_ReturnsAllClassesWithCounts()
    {
        var repo = new FakeAdmissionClassRepository();
        var clock = new FakeClock();
        repo.Details.Add(new AdmissionClassDetailResponse("c_1", "Class 6", 100, 10, 50, 40));
        var handler = new GetAdmissionClassesHandler(repo, clock);

        var result = await handler.HandleAsync();

        Assert.Single(result);
        Assert.Equal("Class 6", result[0].Name);
        Assert.Equal(10, result[0].HeldSeats);
        Assert.Equal(50, result[0].ConfirmedSeats);
        Assert.Equal(40, result[0].AvailableSeats);
    }

    [Fact]
    public async Task GetAdmissionClassByIdHandler_ReturnsClass_WhenExists()
    {
        var repo = new FakeAdmissionClassRepository();
        var clock = new FakeClock();
        var guid = Guid.NewGuid();
        var classId = IdHelper.ToClassId(guid);
        repo.Details.Add(new AdmissionClassDetailResponse(classId, "Class 7", 80, 5, 20, 55));
        var handler = new GetAdmissionClassByIdHandler(repo, clock);

        var result = await handler.HandleAsync(classId);

        Assert.NotNull(result);
        Assert.Equal(classId, result.Id);
        Assert.Equal("Class 7", result.Name);
        Assert.Equal(55, result.AvailableSeats);
    }

    [Fact]
    public async Task GetAdmissionClassByIdHandler_ReturnsNull_WhenNotFoundOrInvalid()
    {
        var repo = new FakeAdmissionClassRepository();
        var clock = new FakeClock();
        var handler = new GetAdmissionClassByIdHandler(repo, clock);

        var resultInvalid = await handler.HandleAsync("not-a-valid-id");
        var resultNotFound = await handler.HandleAsync($"c_{Guid.NewGuid():N}");

        Assert.Null(resultInvalid);
        Assert.Null(resultNotFound);
    }
}


using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.SeatHolds.Dtos;
using SchoolAdmission.Domain.SeatHolds;

namespace SchoolAdmission.Application.SeatHolds;

public sealed class GetStudentSeatHoldsHandler
{
    private readonly ISeatHoldRepository _repository;
    private readonly IClock _clock;

    public GetStudentSeatHoldsHandler(ISeatHoldRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<IReadOnlyList<StudentHoldItemResponse>> HandleAsync(
        string studentIdString,
        CancellationToken cancellationToken = default)
    {
        var studentId = new StudentId(studentIdString);
        return await _repository.GetStudentHoldsAsync(studentId, _clock.UtcNow, cancellationToken);
    }
}


using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Application.SeatHolds.Dtos;
using SchoolAdmission.Domain.SeatHolds;

namespace SchoolAdmission.Application.SeatHolds;

public sealed class GetSeatHoldHandler
{
    private readonly ISeatHoldRepository _repository;
    private readonly IClock _clock;

    public GetSeatHoldHandler(ISeatHoldRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<SeatHoldDetailResponse?> HandleAsync(
        string holdIdString,
        string studentIdString,
        CancellationToken cancellationToken = default)
    {
        if (!IdHelper.TryParseHoldId(holdIdString, out var holdId))
        {
            return null;
        }

        var studentId = new StudentId(studentIdString);
        var hold = await _repository.GetByIdAndStudentAsync(holdId, studentId, _clock.UtcNow, cancellationToken);
        if (hold == null)
        {
            return null;
        }

        return new SeatHoldDetailResponse(
            IdHelper.ToHoldId(hold.Id),
            IdHelper.ToClassId(hold.AdmissionClassId),
            hold.Status.ToString(),
            hold.ExpiresAtUtc);
    }
}


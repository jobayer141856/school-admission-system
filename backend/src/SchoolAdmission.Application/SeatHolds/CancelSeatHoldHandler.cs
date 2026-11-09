using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Domain.SeatHolds;

namespace SchoolAdmission.Application.SeatHolds;

public sealed class CancelSeatHoldHandler
{
    private readonly ISeatHoldRepository _repository;
    private readonly IClock _clock;

    public CancelSeatHoldHandler(ISeatHoldRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<CancelHoldResult> HandleAsync(
        string holdIdString,
        string studentIdString,
        CancellationToken cancellationToken = default)
    {
        if (!IdHelper.TryParseHoldId(holdIdString, out var holdId))
        {
            return new CancelHoldResult(CancelHoldOutcome.NotFound, "Invalid hold ID.");
        }

        var studentId = new StudentId(studentIdString);
        return await _repository.CancelHoldAsync(holdId, studentId, _clock.UtcNow, cancellationToken);
    }
}


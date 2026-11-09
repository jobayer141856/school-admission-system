using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Application.SeatHolds.Dtos;
using SchoolAdmission.Domain.SeatHolds;

namespace SchoolAdmission.Application.SeatHolds;

public sealed class ConfirmSeatHoldHandler
{
    private readonly ISeatHoldRepository _repository;
    private readonly IClock _clock;

    public ConfirmSeatHoldHandler(ISeatHoldRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<ConfirmHoldResultDto> HandleAsync(
        string holdIdString,
        string studentIdString,
        CancellationToken cancellationToken = default)
    {
        if (!IdHelper.TryParseHoldId(holdIdString, out var holdId))
        {
            return new ConfirmHoldResultDto(ConfirmHoldOutcome.NotFound, null, "Invalid hold ID.");
        }

        var studentId = new StudentId(studentIdString);
        var result = await _repository.ConfirmHoldAsync(holdId, studentId, _clock.UtcNow, cancellationToken);

        if (result.Hold == null)
        {
            return new ConfirmHoldResultDto(result.Outcome, null, result.Message);
        }

        var response = new ConfirmHoldResponse(
            IdHelper.ToHoldId(result.Hold.Id),
            result.Hold.Status.ToString());

        return new ConfirmHoldResultDto(result.Outcome, response, result.Message);
    }
}

public sealed record ConfirmHoldResultDto(
    ConfirmHoldOutcome Outcome,
    ConfirmHoldResponse? Response,
    string? Message = null);


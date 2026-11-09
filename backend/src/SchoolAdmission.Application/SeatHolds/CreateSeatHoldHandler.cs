using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Application.SeatHolds.Dtos;
using SchoolAdmission.Domain.Exceptions;
using SchoolAdmission.Domain.SeatHolds;

namespace SchoolAdmission.Application.SeatHolds;

public sealed class CreateSeatHoldHandler
{
    private readonly ISeatHoldRepository _repository;
    private readonly IClock _clock;
    private readonly HoldOptions _options;

    public CreateSeatHoldHandler(
        ISeatHoldRepository repository,
        IClock clock,
        HoldOptions? options = null)
    {
        _repository = repository;
        _clock = clock;
        _options = options ?? new HoldOptions();
    }

    public async Task<CreateHoldResultDto> HandleAsync(
        string classIdString,
        string studentIdString,
        CancellationToken cancellationToken = default)
    {
        if (!IdHelper.TryParseClassId(classIdString, out var classId))
        {
            return new CreateHoldResultDto(CreateHoldOutcome.ClassNotFound, null, $"Invalid class ID '{classIdString}'.");
        }

        var studentId = new StudentId(studentIdString);
        var result = await _repository.CreateHoldWithLockAsync(
            classId,
            studentId,
            _clock.UtcNow,
            _options.DurationSeconds,
            cancellationToken);

        if (result.Hold == null)
        {
            return new CreateHoldResultDto(result.Outcome, null, result.Message);
        }

        var response = new SeatHoldResponse(
            IdHelper.ToHoldId(result.Hold.Id),
            IdHelper.ToClassId(result.Hold.AdmissionClassId),
            result.Hold.StudentId.Value,
            result.Hold.Status.ToString(),
            result.Hold.ExpiresAtUtc);

        return new CreateHoldResultDto(result.Outcome, response, result.Message);
    }
}

public sealed record CreateHoldResultDto(
    CreateHoldOutcome Outcome,
    SeatHoldResponse? Hold,
    string? Message = null);

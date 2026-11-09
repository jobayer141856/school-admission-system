using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.AdmissionClasses.Dtos;
using SchoolAdmission.Application.Common;

namespace SchoolAdmission.Application.AdmissionClasses;

public sealed class GetAdmissionClassByIdHandler
{
    private readonly IAdmissionClassRepository _repository;
    private readonly IClock _clock;

    public GetAdmissionClassByIdHandler(IAdmissionClassRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<AdmissionClassDetailResponse?> HandleAsync(
        string classId,
        CancellationToken cancellationToken = default)
    {
        if (!IdHelper.TryParseClassId(classId, out var id))
        {
            return null;
        }

        return await _repository.GetByIdWithCountsAsync(id, _clock.UtcNow, cancellationToken);
    }
}


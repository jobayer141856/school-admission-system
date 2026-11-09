using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.AdmissionClasses.Dtos;

namespace SchoolAdmission.Application.AdmissionClasses;

public sealed class GetAdmissionClassesHandler
{
    private readonly IAdmissionClassRepository _repository;
    private readonly IClock _clock;

    public GetAdmissionClassesHandler(IAdmissionClassRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<IReadOnlyList<AdmissionClassDetailResponse>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllWithCountsAsync(_clock.UtcNow, cancellationToken);
    }
}


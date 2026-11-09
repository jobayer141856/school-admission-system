using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.AdmissionClasses.Dtos;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Domain.AdmissionClasses;

namespace SchoolAdmission.Application.AdmissionClasses;

public sealed class CreateAdmissionClassHandler
{
    private readonly IAdmissionClassRepository _repository;
    private readonly IClock _clock;

    public CreateAdmissionClassHandler(IAdmissionClassRepository repository, IClock _clock)
    {
        _repository = repository;
        this._clock = _clock;
    }

    public async Task<AdmissionClassResponse> HandleAsync(
        CreateAdmissionClassRequest request,
        CancellationToken cancellationToken = default)
    {
        var admissionClass = AdmissionClass.Create(request.Name, request.TotalSeats, _clock.UtcNow);

        await _repository.AddAsync(admissionClass, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new AdmissionClassResponse(
            IdHelper.ToClassId(admissionClass.Id),
            admissionClass.Name,
            admissionClass.TotalSeats);
    }
}


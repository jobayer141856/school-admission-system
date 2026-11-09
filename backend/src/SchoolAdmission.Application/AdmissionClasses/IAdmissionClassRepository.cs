using SchoolAdmission.Application.AdmissionClasses.Dtos;
using SchoolAdmission.Domain.AdmissionClasses;

namespace SchoolAdmission.Application.AdmissionClasses;

public interface IAdmissionClassRepository
{
    Task AddAsync(AdmissionClass admissionClass, CancellationToken cancellationToken = default);
    Task<AdmissionClass?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdmissionClassDetailResponse>> GetAllWithCountsAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
    Task<AdmissionClassDetailResponse?> GetByIdWithCountsAsync(Guid id, DateTimeOffset now, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}


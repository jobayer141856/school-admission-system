using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Application.AdmissionClasses;
using SchoolAdmission.Application.AdmissionClasses.Dtos;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Domain.AdmissionClasses;
using SchoolAdmission.Domain.SeatHolds;
using SchoolAdmission.Infrastructure.Persistence;

namespace SchoolAdmission.Infrastructure.Repositories;

public sealed class AdmissionClassRepository : IAdmissionClassRepository
{
    private readonly SchoolAdmissionDbContext _dbContext;

    public AdmissionClassRepository(SchoolAdmissionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AdmissionClass admissionClass, CancellationToken cancellationToken = default)
    {
        await _dbContext.AdmissionClasses.AddAsync(admissionClass, cancellationToken);
    }

    public async Task<AdmissionClass?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AdmissionClasses
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AdmissionClassDetailResponse>> GetAllWithCountsAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var classes = await _dbContext.AdmissionClasses
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        if (classes.Count == 0)
        {
            return Array.Empty<AdmissionClassDetailResponse>();
        }

        var classIds = classes.Select(c => c.Id).ToList();

        var activeHolds = await _dbContext.SeatHolds
            .Where(h => classIds.Contains(h.AdmissionClassId))
            .Where(h => (h.Status == SeatHoldStatus.HELD && h.ExpiresAtUtc > now) || h.Status == SeatHoldStatus.CONFIRMED)
            .Select(h => new { h.AdmissionClassId, h.Status })
            .ToListAsync(cancellationToken);

        var results = new List<AdmissionClassDetailResponse>(classes.Count);
        foreach (var c in classes)
        {
            var held = activeHolds.Count(h => h.AdmissionClassId == c.Id && h.Status == SeatHoldStatus.HELD);
            var confirmed = activeHolds.Count(h => h.AdmissionClassId == c.Id && h.Status == SeatHoldStatus.CONFIRMED);
            var available = Math.Max(0, c.TotalSeats - held - confirmed);

            results.Add(new AdmissionClassDetailResponse(
                IdHelper.ToClassId(c.Id),
                c.Name,
                c.TotalSeats,
                held,
                confirmed,
                available
            ));
        }

        return results;
    }

    public async Task<AdmissionClassDetailResponse?> GetByIdWithCountsAsync(
        Guid id,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var c = await _dbContext.AdmissionClasses
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (c == null)
        {
            return null;
        }

        var held = await _dbContext.SeatHolds
            .CountAsync(h => h.AdmissionClassId == id && h.Status == SeatHoldStatus.HELD && h.ExpiresAtUtc > now, cancellationToken);

        var confirmed = await _dbContext.SeatHolds
            .CountAsync(h => h.AdmissionClassId == id && h.Status == SeatHoldStatus.CONFIRMED, cancellationToken);

        var available = Math.Max(0, c.TotalSeats - held - confirmed);

        return new AdmissionClassDetailResponse(
            IdHelper.ToClassId(c.Id),
            c.Name,
            c.TotalSeats,
            held,
            confirmed,
            available
        );
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}


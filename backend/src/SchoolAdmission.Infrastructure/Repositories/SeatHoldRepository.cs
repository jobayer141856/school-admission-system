using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Application.SeatHolds;
using SchoolAdmission.Application.SeatHolds.Dtos;
using SchoolAdmission.Domain.AdmissionClasses;
using SchoolAdmission.Domain.SeatHolds;
using SchoolAdmission.Infrastructure.Persistence;

namespace SchoolAdmission.Infrastructure.Repositories;

public sealed class SeatHoldRepository : ISeatHoldRepository
{
    private readonly SchoolAdmissionDbContext _dbContext;
    private static readonly SemaphoreSlim InProcessLock = new(1, 1);

    public SeatHoldRepository(SchoolAdmissionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateHoldResult> CreateHoldWithLockAsync(
        Guid classId,
        StudentId studentId,
        DateTimeOffset now,
        int durationSeconds,
        CancellationToken cancellationToken = default)
    {
        var isRelational = _dbContext.Database.IsRelational();
        await InProcessLock.WaitAsync(cancellationToken);

        try
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = isRelational
                    ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                    : null;

                try
                {
                    // 1. Check if student already has an active hold across the system
                    var existingHold = await _dbContext.SeatHolds
                        .FirstOrDefaultAsync(
                            h => h.StudentId == studentId && h.Status == SeatHoldStatus.HELD && h.ExpiresAtUtc > now,
                            cancellationToken);

                    if (existingHold != null)
                    {
                        if (existingHold.AdmissionClassId == classId)
                        {
                            return new CreateHoldResult(CreateHoldOutcome.ExistingActiveHoldReturned, existingHold);
                        }

                        return new CreateHoldResult(
                            CreateHoldOutcome.HasActiveHoldInAnotherClass,
                            null,
                            "Student already has an active hold in another admission class.");
                    }

                    // 2. Lock target class row
                    AdmissionClass? admissionClass;
                    if (isRelational)
                    {
                        admissionClass = await _dbContext.AdmissionClasses
                            .FromSqlInterpolated($"SELECT * FROM admission_classes WHERE id = {classId} FOR UPDATE")
                            .FirstOrDefaultAsync(cancellationToken);
                    }
                    else
                    {
                        admissionClass = await _dbContext.AdmissionClasses
                            .FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);
                    }

                    if (admissionClass == null)
                    {
                        return new CreateHoldResult(CreateHoldOutcome.ClassNotFound, null, "Admission class not found.");
                    }

                    // 3. Expire stale holds for this class
                    var staleHolds = await _dbContext.SeatHolds
                        .Where(h => h.AdmissionClassId == classId && h.Status == SeatHoldStatus.HELD && h.ExpiresAtUtc <= now)
                        .ToListAsync(cancellationToken);

                    foreach (var stale in staleHolds)
                    {
                        stale.Expire(now);
                    }

                    // 4. Count active HELD and CONFIRMED seats
                    var activeHeldCount = await _dbContext.SeatHolds
                        .CountAsync(
                            h => h.AdmissionClassId == classId && h.Status == SeatHoldStatus.HELD && h.ExpiresAtUtc > now,
                            cancellationToken);

                    var confirmedCount = await _dbContext.SeatHolds
                        .CountAsync(
                            h => h.AdmissionClassId == classId && h.Status == SeatHoldStatus.CONFIRMED,
                            cancellationToken);

                    if (activeHeldCount + confirmedCount >= admissionClass.TotalSeats)
                    {
                        if (transaction != null) await transaction.RollbackAsync(cancellationToken);
                        return new CreateHoldResult(
                            CreateHoldOutcome.ClassFull,
                            null,
                            "No available seats remain for this admission class.");
                    }

                    // 5. Insert new hold
                    var newHold = SeatHold.Create(classId, studentId, now, durationSeconds);
                    await _dbContext.SeatHolds.AddAsync(newHold, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    if (transaction != null) await transaction.CommitAsync(cancellationToken);

                    return new CreateHoldResult(CreateHoldOutcome.Created, newHold);
                }
                catch
                {
                    if (transaction != null) await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }
        finally
        {
            InProcessLock.Release();
        }
    }

    public async Task<SeatHold?> GetByIdAndStudentAsync(
        Guid holdId,
        StudentId studentId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var hold = await _dbContext.SeatHolds
            .FirstOrDefaultAsync(h => h.Id == holdId && h.StudentId == studentId, cancellationToken);

        if (hold == null)
        {
            return null;
        }

        if (hold.Status == SeatHoldStatus.HELD && hold.IsExpired(now))
        {
            hold.Expire(now);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return hold;
    }

    public async Task<ConfirmHoldResult> ConfirmHoldAsync(
        Guid holdId,
        StudentId studentId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var hold = await _dbContext.SeatHolds
            .FirstOrDefaultAsync(h => h.Id == holdId && h.StudentId == studentId, cancellationToken);

        if (hold == null)
        {
            return new ConfirmHoldResult(ConfirmHoldOutcome.NotFound, null, "Hold not found.");
        }

        if (hold.Status == SeatHoldStatus.CONFIRMED)
        {
            // Idempotent: already confirmed
            return new ConfirmHoldResult(ConfirmHoldOutcome.AlreadyConfirmed, hold);
        }

        if (hold.Status == SeatHoldStatus.CANCELLED)
        {
            return new ConfirmHoldResult(ConfirmHoldOutcome.Conflict, null, "Cancelled hold cannot be confirmed.");
        }

        if (hold.IsExpired(now))
        {
            hold.Expire(now);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new ConfirmHoldResult(ConfirmHoldOutcome.Expired, null, "Hold has expired and cannot be confirmed.");
        }

        if (hold.Status != SeatHoldStatus.HELD)
        {
            return new ConfirmHoldResult(ConfirmHoldOutcome.Conflict, null, $"Hold in status '{hold.Status}' cannot be confirmed.");
        }

        hold.Confirm(now);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ConfirmHoldResult(ConfirmHoldOutcome.Confirmed, hold);
    }

    public async Task<CancelHoldResult> CancelHoldAsync(
        Guid holdId,
        StudentId studentId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var hold = await _dbContext.SeatHolds
            .FirstOrDefaultAsync(h => h.Id == holdId && h.StudentId == studentId, cancellationToken);

        if (hold == null)
        {
            return new CancelHoldResult(CancelHoldOutcome.NotFound, "Hold not found.");
        }

        if (hold.Status == SeatHoldStatus.CANCELLED)
        {
            // Idempotent: already cancelled
            return new CancelHoldResult(CancelHoldOutcome.AlreadyCancelled);
        }

        if (hold.Status == SeatHoldStatus.CONFIRMED)
        {
            return new CancelHoldResult(CancelHoldOutcome.AlreadyConfirmed, "Confirmed admission cannot be cancelled.");
        }

        if (hold.IsExpired(now))
        {
            hold.Expire(now);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new CancelHoldResult(CancelHoldOutcome.Expired, "Expired hold cannot be cancelled.");
        }

        if (hold.Status != SeatHoldStatus.HELD)
        {
            return new CancelHoldResult(CancelHoldOutcome.Conflict, $"Hold in status '{hold.Status}' cannot be cancelled.");
        }

        hold.Cancel(now);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CancelHoldResult(CancelHoldOutcome.Cancelled);
    }

    public async Task<IReadOnlyList<StudentHoldItemResponse>> GetStudentHoldsAsync(
        StudentId studentId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var holds = await _dbContext.SeatHolds
            .Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        if (holds.Count == 0)
        {
            return Array.Empty<StudentHoldItemResponse>();
        }

        var classIds = holds.Select(h => h.AdmissionClassId).Distinct().ToList();
        var classes = await _dbContext.AdmissionClasses
            .Where(c => classIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var hasModifications = false;
        var results = new List<StudentHoldItemResponse>(holds.Count);

        foreach (var hold in holds)
        {
            if (hold.Status == SeatHoldStatus.HELD && hold.IsExpired(now))
            {
                hold.Expire(now);
                hasModifications = true;
            }

            var className = classes.TryGetValue(hold.AdmissionClassId, out var name) ? name : "Unknown Class";

            results.Add(new StudentHoldItemResponse(
                IdHelper.ToHoldId(hold.Id),
                IdHelper.ToClassId(hold.AdmissionClassId),
                className,
                hold.Status.ToString(),
                hold.ExpiresAtUtc));
        }

        if (hasModifications)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return results;
    }
}


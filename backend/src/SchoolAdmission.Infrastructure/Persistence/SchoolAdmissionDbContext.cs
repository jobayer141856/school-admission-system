using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Domain.AdmissionClasses;
using SchoolAdmission.Domain.SeatHolds;

namespace SchoolAdmission.Infrastructure.Persistence;

public class SchoolAdmissionDbContext : DbContext
{
    public SchoolAdmissionDbContext(DbContextOptions<SchoolAdmissionDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdmissionClass> AdmissionClasses => Set<AdmissionClass>();
    public DbSet<SeatHold> SeatHolds => Set<SeatHold>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchoolAdmissionDbContext).Assembly);
    }
}

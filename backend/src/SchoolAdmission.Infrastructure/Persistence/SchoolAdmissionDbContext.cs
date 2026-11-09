using Microsoft.EntityFrameworkCore;

namespace SchoolAdmission.Infrastructure.Persistence;

public class SchoolAdmissionDbContext : DbContext
{
    public SchoolAdmissionDbContext(DbContextOptions<SchoolAdmissionDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchoolAdmissionDbContext).Assembly);
    }
}


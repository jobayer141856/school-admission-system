using Microsoft.Extensions.Diagnostics.HealthChecks;
using SchoolAdmission.Infrastructure.Persistence;

namespace SchoolAdmission.API.Health;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly SchoolAdmissionDbContext _dbContext;

    public DatabaseHealthCheck(SchoolAdmissionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database connection is healthy.")
                : HealthCheckResult.Unhealthy("Cannot connect to the database.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database health check failed.", ex);
        }
    }
}


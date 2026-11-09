using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolAdmission.Application.Abstractions;
using SchoolAdmission.Application.AdmissionClasses;
using SchoolAdmission.Infrastructure.Persistence;
using SchoolAdmission.Infrastructure.Repositories;
using SchoolAdmission.Infrastructure.Services;

namespace SchoolAdmission.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<SchoolAdmissionDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, SystemClock>();

        services.AddScoped<IAdmissionClassRepository, AdmissionClassRepository>();

        return services;
    }
}

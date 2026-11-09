using Microsoft.Extensions.DependencyInjection;
using SchoolAdmission.Application.AdmissionClasses;

namespace SchoolAdmission.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateAdmissionClassHandler>();
        services.AddScoped<GetAdmissionClassesHandler>();
        services.AddScoped<GetAdmissionClassByIdHandler>();

        return services;
    }
}

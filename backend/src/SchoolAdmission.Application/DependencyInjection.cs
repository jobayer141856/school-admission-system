using Microsoft.Extensions.DependencyInjection;

namespace SchoolAdmission.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application use case handlers will be registered here
        return services;
    }
}


using Microsoft.Extensions.DependencyInjection;
using SchoolAdmission.Application.AdmissionClasses;
using SchoolAdmission.Application.SeatHolds;

namespace SchoolAdmission.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AdmissionClasses
        services.AddScoped<CreateAdmissionClassHandler>();
        services.AddScoped<GetAdmissionClassesHandler>();
        services.AddScoped<GetAdmissionClassByIdHandler>();

        // SeatHolds
        services.AddScoped<CreateSeatHoldHandler>();
        services.AddScoped<GetSeatHoldHandler>();
        services.AddScoped<ConfirmSeatHoldHandler>();
        services.AddScoped<CancelSeatHoldHandler>();
        services.AddScoped<GetStudentSeatHoldsHandler>();

        return services;
    }
}

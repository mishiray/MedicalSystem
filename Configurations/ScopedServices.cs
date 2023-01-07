using MedicalSystem.Data;
using MedicalSystem.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MedicalSystem.Configurations
{
    public static class ScopedServices
    {
        public static IServiceCollection AddScopedServices(this IServiceCollection services) => services
            .AddScoped<IUserService, UserService>()
            .AddScoped<IRepository, Repository>()
            .AddScoped<IMedicalOfficerService, MedicalOfficerService>()
            .AddScoped<IPatientService, PatientService>()
            .AddScoped<IRecordService, RecordService>()
            .AddTransient<Seeder>();
    }
}

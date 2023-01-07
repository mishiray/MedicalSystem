using MedicalSystem.DTOs;
using MedicalSystem.DTOs.ServiceDtos;

namespace MedicalSystem.Configurations
{
    public static class IOptions
    {
        public static IServiceCollection ConfigureAppSetting(this IServiceCollection services, IConfiguration configuration)
        {
            return services.Configure<JWT>(configuration.GetSection("JWT"));
        }
    }
}

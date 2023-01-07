using MedicalSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.Configurations
{
    public static class DatabaseConfiguration
    {
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, string connectionString)
        {
            return
            services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        }
    }
}

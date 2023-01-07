using MedicalSystem.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {

        public DbSet<MedicalOfficer> MedicalOfficers { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Record> Records { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    
    }
}

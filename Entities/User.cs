using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalSystem.Entities
{
    public class User : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        [NotMapped]
        public string Password { get; set; }
        public bool IsActive { get; set; } = true;
        public UserType UserType { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public List<Record> Records { get; set; }
        public List<string> Roles { get; set; }
    }

    public enum UserType
    {
        Admin, Patient, MedicalOfficer
    }
}

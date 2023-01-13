using MedicalSystem.Entities;
using System.ComponentModel.DataAnnotations;

namespace MedicalSystem.DTOs.ControllerDtos
{
    public class CreateUserDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public List<string> Roles { get; set; }
    }

    public class GetUserDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }
        public List<string> Roles { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
    }
    public class UpdateRolesDto
    {
        [Required]
        public List<string> Roles { get; set; }
    }
}

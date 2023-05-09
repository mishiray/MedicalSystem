using MedicalSystem.Entities;
using System.ComponentModel.DataAnnotations;

namespace MedicalSystem.DTOs.ControllerDtos
{
    public class CreateRecordDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string PatientId { get; set; }
        public DateTime? NextAppointment { get; set; }
        [Required]
        public string Remark { get; set; }
    }

    public class GetRecordDto
    {
        public string Id { get; set; }
        public string MedicalOfficerId { get; set; }
        public GetUserDto MedicalOfficer { get; set; }

        public string PatientId { get; set; }
        public GetUserDto Patient { get; set; }
        public string Name { get; set; }
        
        public string Description { get; set; }

        public DateTime? NextAppointment { get; set; }

        public string Remark { get; set; }

        public string Status { get; set; }
    }

    public class UpdateRecordDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string PatientId { get; set; }
        public DateTime? NextAppointment { get; set; }
        [Required]
        public string Remark { get; set; }
        public int Status { get; set; }
    }
}

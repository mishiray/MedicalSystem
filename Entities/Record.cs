using MedicalSystem.Entities.Component;
using MedicalSystem.Services;
using System.ComponentModel.DataAnnotations;

namespace MedicalSystem.Entities
{
    public class Record : BaseEntity
    {
        [Required]
        public string MedicalOfficerId { get; set; }
        public MedicalOfficer MedicalOfficer { get; set; }

        [Required]
        public string PatientId { get; set; }
        public Patient Patient { get; set; }

        public DateTime? NextAppointment { get; set; }

        public string Remark { get; set; }

        public RecordStatus Status { get; set; }
    }

    public enum RecordStatus
    {
        Open, Closed
    }
}

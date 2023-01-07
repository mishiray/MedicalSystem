using MedicalSystem.Entities.Component;

namespace MedicalSystem.Entities
{
    public class MedicalOfficer : DbEntity
    {
        public string UserId { get; set; }
        public User User { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace MedicalSystem.Entities.Component
{
    public class DbEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateModified { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}

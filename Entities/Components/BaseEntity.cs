using System.ComponentModel.DataAnnotations;

namespace MedicalSystem.Entities.Component
{
    public class BaseEntity : DbEntity
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace MedicalSystem.Entities
{
    public class RefreshToken
    {
        [Key]
        public string Id { get; set; }

        public string Token { get; set; }
        public string JWTId { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateExpires { get; set; }

        public bool IsRevoked { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace MedicalSystem.DTOs
{
    public class GenerateRefreshTokenDto
    {
        [Required]
        public string CurrentJWT { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}

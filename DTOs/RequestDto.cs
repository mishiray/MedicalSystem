using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedicalSystem.DTOs
{
    public class LoginRequestModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class RequestBearerTokenModel
    {
        [Required]
        public string UserId { get; set; }
    }

}

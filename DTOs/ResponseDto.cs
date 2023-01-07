using System;
using System.Collections.Generic;

namespace MedicalSystem.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class ApiTokenResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}

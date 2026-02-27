using Microsoft.AspNetCore.Identity;
using System;

namespace E7gezhaa.API.Entities // غيرنا Models لـ Entities هنا
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
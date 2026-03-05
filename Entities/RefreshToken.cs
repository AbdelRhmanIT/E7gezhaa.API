using System;

namespace E7gezhaa.API.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; } = false;
        public bool IsUsed { get; set; } = false;
        public bool IsActive => !IsRevoked && !IsUsed && ExpiresAt > DateTime.UtcNow;
    }
}
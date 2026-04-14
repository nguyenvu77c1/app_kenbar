using Microsoft.AspNetCore.Mvc;

namespace Kenbar.Api.Models
{
    public class UserSession
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string RefreshToken { get; set; } = "";

        public string? DeviceId { get; set; }

        public string? DeviceName { get; set; }

        public bool IsRevoked { get; set; } = false;

        public DateTime ExpiredAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
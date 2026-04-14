using Microsoft.AspNetCore.Mvc;

namespace Kenbar.Api.Models
{
    public class OtpLog
    {
        public Guid Id { get; set; }
        public string Phone { get; set; } = "";
        public string OtpCode { get; set; } = "";
        public string Purpose { get; set; } = "login";
        public bool IsUsed { get; set; } = false;
        public DateTime ExpiredAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
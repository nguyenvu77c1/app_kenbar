using Microsoft.AspNetCore.Mvc;

namespace Kenbar.Api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Phone { get; set; } = "";
        public string FullName { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}

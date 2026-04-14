namespace Kenbar.Api.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string FullName { get; set; } = "";

        public string? Email { get; set; }

        public string? AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
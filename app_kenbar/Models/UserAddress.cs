namespace Kenbar.Api.Models
{
    public class UserAddress
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string ReceiverName { get; set; } = "";

        public string ReceiverPhone { get; set; } = "";

        public string Province { get; set; } = "";

        public string District { get; set; } = "";

        public string Ward { get; set; } = "";

        public string AddressLine { get; set; } = "";

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
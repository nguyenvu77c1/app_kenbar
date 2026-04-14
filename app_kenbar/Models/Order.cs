namespace Kenbar.Api.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;

        public string PaymentMethod { get; set; }

        // wallet / cash / bank

        public DateTime? PaidAt { get; set; }
    }
}
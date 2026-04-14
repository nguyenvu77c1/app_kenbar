namespace Kenbar.Api.Models
{
    public class CartItem
    {
        public Guid Id { get; set; }

        public Guid CartId { get; set; }

        public Guid ProductVariantId { get; set; }

        public int Quantity { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Cart Cart { get; set; } = null!;

        public ProductVariant ProductVariant { get; set; } = null!;
    }
}
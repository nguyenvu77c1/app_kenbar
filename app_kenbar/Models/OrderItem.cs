namespace Kenbar.Api.Models
{
    public class OrderItem
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Guid ProductVariantId { get; set; }

        public string ProductName { get; set; } = "";

        public string VariantName { get; set; } = "";

        public string? UnitCode { get; set; }

        public decimal? UnitValue { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal { get; set; }

        public Order Order { get; set; } = null!;
    }
}
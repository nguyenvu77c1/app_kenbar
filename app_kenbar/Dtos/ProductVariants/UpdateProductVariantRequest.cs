namespace Kenbar.Api.Dtos.ProductVariants
{
    public class UpdateProductVariantRequest
    {
        public Guid ProductId { get; set; }

        public Guid? UnitId { get; set; }

        public string VariantName { get; set; } = "";

        public decimal? UnitValue { get; set; }

        public string SKU { get; set; } = "";

        public decimal Price { get; set; }

        public decimal? SalePrice { get; set; }

        public int StockQuantity { get; set; } = 0;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }
}
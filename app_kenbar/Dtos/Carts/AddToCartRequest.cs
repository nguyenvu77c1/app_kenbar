namespace Kenbar.Api.Dtos.Carts
{
    public class AddToCartRequest
    {
        public Guid ProductVariantId { get; set; }

        public int Quantity { get; set; } = 1;
    }
}
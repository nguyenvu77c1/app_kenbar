namespace Kenbar.Api.Models
{
    public class ProductImage
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public string ImageUrl { get; set; } = "";

        public int SortOrder { get; set; } = 0;

        public bool IsThumbnail { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Product Product { get; set; } = null!;
    }
}
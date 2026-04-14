namespace Kenbar.Api.Dtos.ProductImages
{
    public class CreateProductImageRequest
    {
        public Guid ProductId { get; set; }

        public string ImageUrl { get; set; } = "";

        public int SortOrder { get; set; } = 0;

        public bool IsThumbnail { get; set; } = false;
    }
}
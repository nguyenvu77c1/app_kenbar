namespace Kenbar.Api.Dtos.Products
{
    public class CreateProductRequest
    {
        public Guid CategoryId { get; set; }

        public string Name { get; set; } = "";

        public string Slug { get; set; } = "";

        public string? Description { get; set; }

        public string? Brand { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
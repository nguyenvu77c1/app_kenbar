namespace Kenbar.Api.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public Guid CategoryId { get; set; }

        public string Name { get; set; } = "";

        public string Slug { get; set; } = "";

        public string? Description { get; set; }

        public string? Brand { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Category Category { get; set; } = null!;
    }
}
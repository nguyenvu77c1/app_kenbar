namespace Kenbar.Api.Models
{
    public class Category
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = "";

        public string Slug { get; set; } = "";

        public Guid? ParentId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
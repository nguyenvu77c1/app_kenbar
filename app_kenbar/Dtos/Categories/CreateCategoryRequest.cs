namespace Kenbar.Api.Dtos.Categories
{
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = "";

        public string Slug { get; set; } = "";

        public Guid? ParentId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
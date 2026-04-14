namespace Kenbar.Api.Models
{
    public class Unit
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = ""; // ml, g, kg, pcs

        public string Name { get; set; } = ""; // Mililit, Gram

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
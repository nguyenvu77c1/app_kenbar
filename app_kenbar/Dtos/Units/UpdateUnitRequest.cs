namespace Kenbar.Api.Dtos.Units
{
    public class UpdateUnitRequest
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
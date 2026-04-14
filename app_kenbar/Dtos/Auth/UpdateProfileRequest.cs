namespace Kenbar.Api.Dtos.Auth
{
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
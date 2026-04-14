using Microsoft.AspNetCore.Mvc;

namespace Kenbar.Api.Dtos.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = "";

        public string RefreshToken { get; set; } = "";

        public UserInfo User { get; set; } = new();
    }

    public class UserInfo
    {
        public Guid Id { get; set; }

        public string Phone { get; set; } = "";

        public string FullName { get; set; } = "";
    }
}
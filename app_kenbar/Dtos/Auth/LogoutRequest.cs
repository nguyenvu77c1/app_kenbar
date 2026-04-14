using Microsoft.AspNetCore.Mvc;

namespace Kenbar.Api.Dtos.Auth
{
    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = "";
    }
}
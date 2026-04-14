using Microsoft.AspNetCore.Mvc;

namespace Kenbar.Api.Dtos.Auth
{
    public class SendOtpRequest
    {
        public string Phone { get; set; } = "";
    }
}

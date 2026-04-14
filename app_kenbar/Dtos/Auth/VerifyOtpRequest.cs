using Microsoft.AspNetCore.Mvc;

namespace Kenbar.Api.Dtos.Auth
{
    public class VerifyOtpRequest
    {
        public string Phone { get; set; }
        public string OtpCode { get; set; }
        public string? FullName { get; set; }
    }
}
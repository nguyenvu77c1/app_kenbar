using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;
using Kenbar.Api.Common;
using Kenbar.Api.Data;
using Kenbar.Api.Dtos.Auth;
using Kenbar.Api.Models;
using Kenbar.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kenbar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(AppDbContext context, JwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Phone))
            {
                return BadRequest(BaseResponse<object>.Fail("Số điện thoại không được để trống"));
            }

            var phone = request.Phone.Trim();

            // Phase đầu: mock OTP cố định
            var otp = "123456";

            var otpLog = new OtpLog
            {
                Id = Guid.NewGuid(),
                Phone = phone,
                OtpCode = otp,
                Purpose = "login",
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddMinutes(5)
            };

            _context.OtpLogs.Add(otpLog);
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new
            {
                phone,
                otp = otp, // phase dev để test
                expiredAt = otpLog.ExpiredAt
            }, "Gửi OTP thành công"));
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.OtpCode))
            {
                return BadRequest(BaseResponse<object>.Fail("Số điện thoại và OTP là bắt buộc"));
            }

            var phone = request.Phone.Trim();
            var otpCode = request.OtpCode.Trim();

            var otpLog = await _context.OtpLogs
                .Where(x => x.Phone == phone
                         && x.OtpCode == otpCode
                         && !x.IsUsed
                         && x.ExpiredAt > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpLog == null)
            {
                return BadRequest(BaseResponse<object>.Fail("OTP không hợp lệ hoặc đã hết hạn"));
            }

            otpLog.IsUsed = true;

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Phone == phone);

            if (user == null)
            {
                var fullName = string.IsNullOrWhiteSpace(request.FullName)
                    ? "Người dùng mới"
                    : request.FullName.Trim();

                user = new User
                {
                    Id = Guid.NewGuid(),
                    Phone = phone,
                    FullName = fullName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                _context.Users.Add(user);

                var profile = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FullName = fullName,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserProfiles.Add(profile);
            }
            else
            {
                user.LastLoginAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(request.FullName))
                {
                    user.FullName = request.FullName.Trim();

                    var profile = await _context.UserProfiles
                        .FirstOrDefaultAsync(x => x.UserId == user.Id);

                    if (profile != null)
                    {
                        profile.FullName = request.FullName.Trim();
                    }
                }
            }

            await _context.SaveChangesAsync();

            var accessToken = _jwtTokenService.GenerateToken(user);
            var refreshToken = Guid.NewGuid().ToString();

            var session = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RefreshToken = refreshToken,
                DeviceId = null,
                DeviceName = null,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(7)
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserInfo
                {
                    Id = user.Id,
                    Phone = user.Phone,
                    FullName = user.FullName
                }
            };

            return Ok(BaseResponse<AuthResponse>.Ok(response, "Đăng nhập thành công"));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            var userId = Guid.Parse(userIdClaim);

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy người dùng"));
            }

            return Ok(BaseResponse<object>.Ok(new
            {
                user.Id,
                user.Phone,
                user.FullName,
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt
            }));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(BaseResponse<object>.Fail("Refresh token không được để trống"));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            var userId = Guid.Parse(userIdClaim);

            var session = await _context.UserSessions
                .FirstOrDefaultAsync(x => x.UserId == userId
                                       && x.RefreshToken == request.RefreshToken
                                       && !x.IsRevoked);

            if (session == null)
            {
                return BadRequest(BaseResponse<object>.Fail("Không tìm thấy session hợp lệ"));
            }

            session.IsRevoked = true;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { }, "Đăng xuất thành công"));
        }




        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            var userId = Guid.Parse(userIdClaim);

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy người dùng"));
            }

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserProfiles.Add(profile);
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                var fullName = request.FullName.Trim();
                user.FullName = fullName;
                profile.FullName = fullName;
            }

            profile.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            profile.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new
            {
                user.Id,
                user.Phone,
                FullName = profile.FullName,
                profile.Email,
                profile.AvatarUrl
            }, "Cập nhật hồ sơ thành công"));
        }

    }
}
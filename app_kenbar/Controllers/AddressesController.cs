using System.Security.Claims;
using Kenbar.Api.Common;
using Kenbar.Api.Data;
using Kenbar.Api.Dtos.Addresses;
using Kenbar.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kenbar.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AddressesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AddressesController(AppDbContext context)
        {
            _context = context;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return null;
            }

            return Guid.Parse(userIdClaim);
        }


        //lấy user hiện tại từ token kiểm tra dữ liệu đầu vào nếu địa chỉ mới là mặc định thì bỏ mặc định cũ tạo địa chỉ mới lưu vào DB
        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            if (string.IsNullOrWhiteSpace(request.ReceiverName) ||
                string.IsNullOrWhiteSpace(request.ReceiverPhone) ||
                string.IsNullOrWhiteSpace(request.Province) ||
                string.IsNullOrWhiteSpace(request.District) ||
                string.IsNullOrWhiteSpace(request.Ward) ||
                string.IsNullOrWhiteSpace(request.AddressLine))
            {
                return BadRequest(BaseResponse<object>.Fail("Vui lòng nhập đầy đủ thông tin địa chỉ"));
            }

            if (request.IsDefault)
            {
                var oldDefaultAddresses = await _context.UserAddresses
                    .Where(x => x.UserId == userId && x.IsDefault)
                    .ToListAsync();

                foreach (var item in oldDefaultAddresses)
                {
                    item.IsDefault = false;
                }
            }

            var address = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                ReceiverName = request.ReceiverName.Trim(),
                ReceiverPhone = request.ReceiverPhone.Trim(),
                Province = request.Province.Trim(),
                District = request.District.Trim(),
                Ward = request.Ward.Trim(),
                AddressLine = request.AddressLine.Trim(),
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserAddresses.Add(address);
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new
            {
                address.Id,
                address.ReceiverName,
                address.ReceiverPhone,
                address.Province,
                address.District,
                address.Ward,
                address.AddressLine,
                address.IsDefault,
                address.CreatedAt
            }, "Thêm địa chỉ thành công"));
        }

        //Lấy toàn bộ địa chỉ của user đang đăng nhập.
        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            var addresses = await _context.UserAddresses
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.ReceiverName,
                    x.ReceiverPhone,
                    x.Province,
                    x.District,
                    x.Ward,
                    x.AddressLine,
                    x.IsDefault,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(BaseResponse<object>.Ok(addresses));
        }

        //Sửa địa chỉ 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateAddressRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (address == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy địa chỉ"));
            }

            if (string.IsNullOrWhiteSpace(request.ReceiverName) ||
                string.IsNullOrWhiteSpace(request.ReceiverPhone) ||
                string.IsNullOrWhiteSpace(request.Province) ||
                string.IsNullOrWhiteSpace(request.District) ||
                string.IsNullOrWhiteSpace(request.Ward) ||
                string.IsNullOrWhiteSpace(request.AddressLine))
            {
                return BadRequest(BaseResponse<object>.Fail("Vui lòng nhập đầy đủ thông tin địa chỉ"));
            }

            if (request.IsDefault)
            {
                var oldDefaultAddresses = await _context.UserAddresses
                    .Where(x => x.UserId == userId && x.IsDefault && x.Id != id)
                    .ToListAsync();

                foreach (var item in oldDefaultAddresses)
                {
                    item.IsDefault = false;
                }
            }

            address.ReceiverName = request.ReceiverName.Trim();
            address.ReceiverPhone = request.ReceiverPhone.Trim();
            address.Province = request.Province.Trim();
            address.District = request.District.Trim();
            address.Ward = request.Ward.Trim();
            address.AddressLine = request.AddressLine.Trim();
            address.IsDefault = request.IsDefault;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new
            {
                address.Id,
                address.ReceiverName,
                address.ReceiverPhone,
                address.Province,
                address.District,
                address.Ward,
                address.AddressLine,
                address.IsDefault
            }, "Cập nhật địa chỉ thành công"));
        }

        //Xóa địa chỉ
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (address == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy địa chỉ"));
            }

            _context.UserAddresses.Remove(address);
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { }, "Xóa địa chỉ thành công"));
        }

        //Đây là địa chỉ mặt định
        [HttpPut("{id}/set-default")]
        public async Task<IActionResult> SetDefaultAddress(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            var address = await _context.UserAddresses
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (address == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy địa chỉ"));
            }

            var oldDefaultAddresses = await _context.UserAddresses
                .Where(x => x.UserId == userId && x.IsDefault)
                .ToListAsync();

            foreach (var item in oldDefaultAddresses)
            {
                item.IsDefault = false;
            }

            address.IsDefault = true;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { }, "Đặt địa chỉ mặc định thành công"));
        }
    }
}
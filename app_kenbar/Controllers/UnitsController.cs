using Kenbar.Api.Common;
using Kenbar.Api.Data;
using Kenbar.Api.Dtos.Units;
using Kenbar.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kenbar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UnitsController(AppDbContext context)
        {
            _context = context;
        }


        //Tao units
        [HttpPost]
        public async Task<IActionResult> CreateUnit([FromBody] CreateUnitRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(BaseResponse<object>.Fail("Code và Name là bắt buộc"));
            }

            var code = request.Code.Trim().ToLower();

            var existing = await _context.Units.FirstOrDefaultAsync(x => x.Code == code);
            if (existing != null)
            {
                return BadRequest(BaseResponse<object>.Fail("Code đã tồn tại"));
            }

            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = request.Name.Trim(),
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Units.Add(unit);
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(unit, "Tạo unit thành công"));
        }


        //Lay tat ca unit
        [HttpGet]
        public async Task<IActionResult> GetUnits()
        {
            var units = await _context.Units
                .OrderBy(x => x.Code)
                .ToListAsync();

            return Ok(BaseResponse<object>.Ok(units));
        }

        //Chinh sua unit
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUnit(Guid id, [FromBody] UpdateUnitRequest request)
        {
            var unit = await _context.Units.FirstOrDefaultAsync(x => x.Id == id);
            if (unit == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy unit"));
            }

            unit.Code = request.Code.Trim().ToLower();
            unit.Name = request.Name.Trim();
            unit.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(unit, "Cập nhật thành công"));
        }


        //Xoa unit
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnit(Guid id)
        {
            var unit = await _context.Units.FirstOrDefaultAsync(x => x.Id == id);
            if (unit == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy unit"));
            }

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { }, "Xóa thành công"));
        }

    }
}
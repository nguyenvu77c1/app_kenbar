using Kenbar.Api.Common;
using Kenbar.Api.Data;
using Kenbar.Api.Dtos.Categories;
using Kenbar.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kenbar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }


        //Tao danh muc
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(BaseResponse<object>.Fail("Tên danh mục là bắt buộc"));
            }

            // 1. Xử lý slug
            string slug;

            if (!string.IsNullOrWhiteSpace(request.Slug))
            {
                slug = request.Slug.Trim().ToLower();
            }
            else
            {
                slug = GenerateSlug(request.Name);
            }

            // 2. Xử lý slug bị trùng
            var originalSlug = slug;
            int count = 1;

            while (await _context.Categories.AnyAsync(x => x.Slug == slug))
            {
                slug = $"{originalSlug}-{count}";
                count++;
            }

            // 3. Kiểm tra danh mục cha
            if (request.ParentId.HasValue)
            {
                var parentCategory = await _context.Categories
                    .FirstOrDefaultAsync(x => x.Id == request.ParentId.Value);

                if (parentCategory == null)
                {
                    return BadRequest(BaseResponse<object>.Fail("Danh mục cha không tồn tại"));
                }
            }

            // 4. Tạo category
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Slug = slug,
                ParentId = request.ParentId,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new
            {
                category.Id,
                category.Name,
                category.Slug,
                category.ParentId,
                category.IsActive,
                category.CreatedAt
            }, "Tạo danh mục thành công"));
        }


        //Lay danh sach danh muc
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Slug,
                    x.ParentId,
                    x.IsActive,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(BaseResponse<object>.Ok(categories));
        }

        //Lay chi tiet mot danh muc
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var category = await _context.Categories
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Slug,
                    x.ParentId,
                    x.IsActive,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy danh mục"));
            }

            return Ok(BaseResponse<object>.Ok(category));
        }


        //Cap nhat danh muc
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy danh mục"));
            }

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug))
            {
                return BadRequest(BaseResponse<object>.Fail("Tên danh mục và slug là bắt buộc"));
            }

            var slug = request.Slug.Trim().ToLower();

            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(x => x.Slug == slug && x.Id != id);

            if (existingCategory != null)
            {
                return BadRequest(BaseResponse<object>.Fail("Slug đã tồn tại"));
            }

            if (request.ParentId.HasValue)
            {
                if (request.ParentId.Value == id)
                {
                    return BadRequest(BaseResponse<object>.Fail("Danh mục không thể là cha của chính nó"));
                }

                var parentCategory = await _context.Categories
                    .FirstOrDefaultAsync(x => x.Id == request.ParentId.Value);

                if (parentCategory == null)
                {
                    return BadRequest(BaseResponse<object>.Fail("Danh mục cha không tồn tại"));
                }
            }

            category.Name = request.Name.Trim();
            category.Slug = slug;
            category.ParentId = request.ParentId;
            category.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new
            {
                category.Id,
                category.Name,
                category.Slug,
                category.ParentId,
                category.IsActive
            }, "Cập nhật danh mục thành công"));
        }



        //Xoa danh muc
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy danh mục"));
            }

            var hasChildren = await _context.Categories.AnyAsync(x => x.ParentId == id);
            if (hasChildren)
            {
                return BadRequest(BaseResponse<object>.Fail("Không thể xóa danh mục đang có danh mục con"));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { }, "Xóa danh mục thành công"));
        }


        //Tu tao slug
        private string GenerateSlug(string input)
        {
            string str = input.ToLower().Trim();

            // bỏ dấu tiếng Việt
            str = System.Text.RegularExpressions.Regex.Replace(str, "[áàảạãăắằẳặẵâấầẩậẫ]", "a");
            str = System.Text.RegularExpressions.Regex.Replace(str, "[éèẻẹẽêếềểệễ]", "e");
            str = System.Text.RegularExpressions.Regex.Replace(str, "[íìỉịĩ]", "i");
            str = System.Text.RegularExpressions.Regex.Replace(str, "[óòỏọõôốồổộỗơớờởợỡ]", "o");
            str = System.Text.RegularExpressions.Regex.Replace(str, "[úùủụũưứừửựữ]", "u");
            str = System.Text.RegularExpressions.Regex.Replace(str, "[ýỳỷỵỹ]", "y");
            str = System.Text.RegularExpressions.Regex.Replace(str, "[đ]", "d");

            // bỏ ký tự đặc biệt
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // thay khoảng trắng bằng -
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "-");

            // bỏ dấu - dư
            str = System.Text.RegularExpressions.Regex.Replace(str, @"-+", "-");

            return str.Trim('-');
        }
    } 
}
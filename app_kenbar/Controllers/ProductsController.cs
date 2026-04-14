using Kenbar.Api.Common;
using Kenbar.Api.Data;
using Kenbar.Api.Dtos.Products;
using Kenbar.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Kenbar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        private string GenerateSlug(string input)
        {
            string str = input.ToLower().Trim();

            str = Regex.Replace(str, "[áàảạãăắằẳặẵâấầẩậẫ]", "a");
            str = Regex.Replace(str, "[éèẻẹẽêếềểệễ]", "e");
            str = Regex.Replace(str, "[íìỉịĩ]", "i");
            str = Regex.Replace(str, "[óòỏọõôốồổộỗơớờởợỡ]", "o");
            str = Regex.Replace(str, "[úùủụũưứừửựữ]", "u");
            str = Regex.Replace(str, "[ýỳỷỵỹ]", "y");
            str = Regex.Replace(str, "[đ]", "d");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", "-");
            str = Regex.Replace(str, @"-+", "-");

            return str.Trim('-');
        }


        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            if (request.CategoryId == Guid.Empty || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(BaseResponse<object>.Fail("CategoryId và Name là bắt buộc"));
            }

            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == request.CategoryId);
            if (category == null)
            {
                return BadRequest(BaseResponse<object>.Fail("Danh mục không tồn tại"));
            }

            string slug;

            if (!string.IsNullOrWhiteSpace(request.Slug))
            {
                slug = request.Slug.Trim().ToLower();
            }
            else
            {
                slug = GenerateSlug(request.Name);
            }

            var originalSlug = slug;
            int count = 1;

            while (await _context.Products.AnyAsync(x => x.Slug == slug))
            {
                slug = $"{originalSlug}-{count}";
                count++;
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = request.CategoryId,
                Name = request.Name.Trim(),
                Slug = slug,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim(),
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new
            {
                product.Id,
                product.CategoryId,
                product.Name,
                product.Slug,
                product.Description,
                product.Brand,
                product.IsActive,
                product.CreatedAt
            }, "Tạo sản phẩm thành công"));
        }



        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] Guid? categoryId, [FromQuery] string? keyword)
        {
            var query = _context.Products
                .Include(x => x.Category)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value != Guid.Empty)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var key = keyword.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(key));
            }

            var products = await query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.CategoryId,
                    CategoryName = x.Category.Name,
                    x.Name,
                    x.Slug,
                    x.Description,
                    x.Brand,
                    x.IsActive,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(BaseResponse<object>.Ok(products));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _context.Products
                .Include(x => x.Category)
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.CategoryId,
                    CategoryName = x.Category.Name,
                    x.Name,
                    x.Slug,
                    x.Description,
                    x.Brand,
                    x.IsActive,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy sản phẩm"));
            }

            return Ok(BaseResponse<object>.Ok(product));
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy sản phẩm"));
            }

            if (request.CategoryId == Guid.Empty || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(BaseResponse<object>.Fail("CategoryId và Name là bắt buộc"));
            }

            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == request.CategoryId);
            if (category == null)
            {
                return BadRequest(BaseResponse<object>.Fail("Danh mục không tồn tại"));
            }

            string slug;

            if (!string.IsNullOrWhiteSpace(request.Slug))
            {
                slug = request.Slug.Trim().ToLower();
            }
            else
            {
                slug = GenerateSlug(request.Name);
            }

            var originalSlug = slug;
            int count = 1;

            while (await _context.Products.AnyAsync(x => x.Slug == slug && x.Id != id))
            {
                slug = $"{originalSlug}-{count}";
                count++;
            }

            product.CategoryId = request.CategoryId;
            product.Name = request.Name.Trim();
            product.Slug = slug;
            product.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            product.Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim();
            product.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new
            {
                product.Id,
                product.CategoryId,
                product.Name,
                product.Slug,
                product.Description,
                product.Brand,
                product.IsActive
            }, "Cập nhật sản phẩm thành công"));
        }




        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy sản phẩm"));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { }, "Xóa sản phẩm thành công"));
        }
    }
}
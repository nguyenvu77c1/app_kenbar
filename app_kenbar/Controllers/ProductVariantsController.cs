using Kenbar.Api.Common;
using Kenbar.Api.Data;
using Kenbar.Api.Dtos.ProductVariants;
using Kenbar.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kenbar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductVariantsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductVariantsController(AppDbContext context)
        {
            _context = context;
        }


        //Them variant
        [HttpPost]
        public async Task<IActionResult> CreateVariant([FromBody] CreateProductVariantRequest request)
        {
            if (request.ProductId == Guid.Empty ||
                string.IsNullOrWhiteSpace(request.VariantName) ||
                string.IsNullOrWhiteSpace(request.SKU) ||
                request.Price <= 0)
            {
                return BadRequest(BaseResponse<object>.Fail("ProductId, VariantName, SKU, Price là bắt buộc"));
            }

            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);
            if (product == null)
            {
                return BadRequest(BaseResponse<object>.Fail("Sản phẩm không tồn tại"));
            }

            if (request.UnitId.HasValue)
            {
                var unit = await _context.Units.FirstOrDefaultAsync(x => x.Id == request.UnitId.Value);
                if (unit == null)
                {
                    return BadRequest(BaseResponse<object>.Fail("Đơn vị không tồn tại"));
                }
            }

            var sku = request.SKU.Trim().ToUpper();

            var existingSku = await _context.ProductVariants
                .FirstOrDefaultAsync(x => x.SKU == sku);

            if (existingSku != null)
            {
                return BadRequest(BaseResponse<object>.Fail("SKU đã tồn tại"));
            }

            if (request.IsDefault)
            {
                var oldDefaults = await _context.ProductVariants
                    .Where(x => x.ProductId == request.ProductId && x.IsDefault)
                    .ToListAsync();

                foreach (var item in oldDefaults)
                {
                    item.IsDefault = false;
                }
            }

            var variant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                UnitId = request.UnitId,
                VariantName = request.VariantName.Trim(),
                UnitValue = request.UnitValue,
                SKU = sku,
                Price = request.Price,
                SalePrice = request.SalePrice,
                StockQuantity = request.StockQuantity,
                IsDefault = request.IsDefault,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new
            {
                variant.Id,
                variant.ProductId,
                variant.UnitId,
                variant.VariantName,
                variant.UnitValue,
                variant.SKU,
                variant.Price,
                variant.SalePrice,
                variant.StockQuantity,
                variant.IsDefault,
                variant.IsActive,
                variant.CreatedAt
            }, "Tạo biến thể thành công"));
        }


        //Danh sach bien the theo product
        [HttpGet]
        public async Task<IActionResult> GetVariants([FromQuery] Guid? productId)
        {
            var query = _context.ProductVariants
                .Include(x => x.Product)
                .Include(x => x.Unit)
                .AsQueryable();

            if (productId.HasValue && productId.Value != Guid.Empty)
            {
                query = query.Where(x => x.ProductId == productId.Value);
            }

            var variants = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.VariantName)
                .Select(x => new
                {
                    x.Id,
                    x.ProductId,
                    ProductName = x.Product.Name,
                    x.UnitId,
                    UnitCode = x.Unit != null ? x.Unit.Code : null,
                    UnitName = x.Unit != null ? x.Unit.Name : null,
                    x.VariantName,
                    x.UnitValue,
                    x.SKU,
                    x.Price,
                    x.SalePrice,
                    x.StockQuantity,
                    x.IsDefault,
                    x.IsActive,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(BaseResponse<object>.Ok(variants));
        }


        //Lay chi tiet 1 bien the
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVariantById(Guid id)
        {
            var variant = await _context.ProductVariants
                .Include(x => x.Product)
                .Include(x => x.Unit)
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.ProductId,
                    ProductName = x.Product.Name,
                    x.UnitId,
                    UnitCode = x.Unit != null ? x.Unit.Code : null,
                    UnitName = x.Unit != null ? x.Unit.Name : null,
                    x.VariantName,
                    x.UnitValue,
                    x.SKU,
                    x.Price,
                    x.SalePrice,
                    x.StockQuantity,
                    x.IsDefault,
                    x.IsActive,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (variant == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy biến thể"));
            }

            return Ok(BaseResponse<object>.Ok(variant));
        }


        //Cap nhat bien the
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVariant(Guid id, [FromBody] UpdateProductVariantRequest request)
        {
            var variant = await _context.ProductVariants.FirstOrDefaultAsync(x => x.Id == id);
            if (variant == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy biến thể"));
            }

            if (request.ProductId == Guid.Empty ||
                string.IsNullOrWhiteSpace(request.VariantName) ||
                string.IsNullOrWhiteSpace(request.SKU) ||
                request.Price <= 0)
            {
                return BadRequest(BaseResponse<object>.Fail("ProductId, VariantName, SKU, Price là bắt buộc"));
            }

            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);
            if (product == null)
            {
                return BadRequest(BaseResponse<object>.Fail("Sản phẩm không tồn tại"));
            }

            if (request.UnitId.HasValue)
            {
                var unit = await _context.Units.FirstOrDefaultAsync(x => x.Id == request.UnitId.Value);
                if (unit == null)
                {
                    return BadRequest(BaseResponse<object>.Fail("Đơn vị không tồn tại"));
                }
            }

            var sku = request.SKU.Trim().ToUpper();

            var existingSku = await _context.ProductVariants
                .FirstOrDefaultAsync(x => x.SKU == sku && x.Id != id);

            if (existingSku != null)
            {
                return BadRequest(BaseResponse<object>.Fail("SKU đã tồn tại"));
            }

            if (request.IsDefault)
            {
                var oldDefaults = await _context.ProductVariants
                    .Where(x => x.ProductId == request.ProductId && x.IsDefault && x.Id != id)
                    .ToListAsync();

                foreach (var item in oldDefaults)
                {
                    item.IsDefault = false;
                }
            }

            variant.ProductId = request.ProductId;
            variant.UnitId = request.UnitId;
            variant.VariantName = request.VariantName.Trim();
            variant.UnitValue = request.UnitValue;
            variant.SKU = sku;
            variant.Price = request.Price;
            variant.SalePrice = request.SalePrice;
            variant.StockQuantity = request.StockQuantity;
            variant.IsDefault = request.IsDefault;
            variant.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new
            {
                variant.Id,
                variant.ProductId,
                variant.UnitId,
                variant.VariantName,
                variant.UnitValue,
                variant.SKU,
                variant.Price,
                variant.SalePrice,
                variant.StockQuantity,
                variant.IsDefault,
                variant.IsActive
            }, "Cập nhật biến thể thành công"));
        }



        //Xoa bien the
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVariant(Guid id)
        {
            var variant = await _context.ProductVariants.FirstOrDefaultAsync(x => x.Id == id);
            if (variant == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy biến thể"));
            }

            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { }, "Xóa biến thể thành công"));
        }
    }
}
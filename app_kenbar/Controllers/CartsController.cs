using System.Security.Claims;
using Kenbar.Api.Common;
using Kenbar.Api.Data;
using Kenbar.Api.Dtos.Carts;
using Kenbar.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kenbar.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartsController(AppDbContext context)
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

        private async Task<Cart> GetOrCreateCart(Guid userId)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }


        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            var cart = await GetOrCreateCart(userId.Value);

            var items = await _context.CartItems
                .Where(x => x.CartId == cart.Id)
                .Include(x => x.ProductVariant)
                    .ThenInclude(v => v.Product)
                .Include(x => x.ProductVariant)
                    .ThenInclude(v => v.Unit)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.CartId,
                    x.ProductVariantId,
                    ProductId = x.ProductVariant.ProductId,
                    ProductName = x.ProductVariant.Product.Name,
                    VariantName = x.ProductVariant.VariantName,
                    UnitCode = x.ProductVariant.Unit != null ? x.ProductVariant.Unit.Code : null,
                    UnitValue = x.ProductVariant.UnitValue,
                    x.ProductVariant.SKU,
                    x.ProductVariant.Price,
                    x.ProductVariant.SalePrice,
                    x.ProductVariant.StockQuantity,
                    x.Quantity,
                    LineTotal = (x.ProductVariant.SalePrice ?? x.ProductVariant.Price) * x.Quantity
                })
                .ToListAsync();

            var totalAmount = items.Sum(x => x.LineTotal);

            return Ok(BaseResponse<object>.Ok(new
            {
                cart.Id,
                cart.UserId,
                cart.CreatedAt,
                cart.UpdatedAt,
                Items = items,
                TotalAmount = totalAmount
            }));
        }

        //Them item vao cart
        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            if (request.ProductVariantId == Guid.Empty || request.Quantity <= 0)
            {
                return BadRequest(BaseResponse<object>.Fail("ProductVariantId và Quantity phải hợp lệ"));
            }

            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(x => x.Id == request.ProductVariantId && x.IsActive);

            if (variant == null)
            {
                return BadRequest(BaseResponse<object>.Fail("Biến thể sản phẩm không tồn tại"));
            }

            if (variant.StockQuantity < request.Quantity)
            {
                return BadRequest(BaseResponse<object>.Fail("Số lượng tồn kho không đủ"));
            }

            var cart = await GetOrCreateCart(userId.Value);

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(x => x.CartId == cart.Id && x.ProductVariantId == request.ProductVariantId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + request.Quantity;

                if (variant.StockQuantity < newQuantity)
                {
                    return BadRequest(BaseResponse<object>.Fail("Số lượng tồn kho không đủ"));
                }

                existingItem.Quantity = newQuantity;
            }
            else
            {
                var item = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductVariantId = request.ProductVariantId,
                    Quantity = request.Quantity,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CartItems.Add(item);
            }

            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { }, "Thêm vào giỏ hàng thành công"));
        }



        //Cap nhat so luong
        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateCartItem(Guid id, [FromBody] UpdateCartItemRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            if (request.Quantity <= 0)
            {
                return BadRequest(BaseResponse<object>.Fail("Quantity phải lớn hơn 0"));
            }

            var cart = await GetOrCreateCart(userId.Value);

            var item = await _context.CartItems
                .Include(x => x.ProductVariant)
                .FirstOrDefaultAsync(x => x.Id == id && x.CartId == cart.Id);

            if (item == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy sản phẩm trong giỏ"));
            }

            if (item.ProductVariant.StockQuantity < request.Quantity)
            {
                return BadRequest(BaseResponse<object>.Fail("Số lượng tồn kho không đủ"));
            }

            item.Quantity = request.Quantity;
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { }, "Cập nhật giỏ hàng thành công"));
        }


        //Xoa item khoi cart
        [HttpDelete("items/{id}")]
        public async Task<IActionResult> RemoveCartItem(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            var cart = await GetOrCreateCart(userId.Value);

            var item = await _context.CartItems
                .FirstOrDefaultAsync(x => x.Id == id && x.CartId == cart.Id);

            if (item == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy sản phẩm trong giỏ"));
            }

            _context.CartItems.Remove(item);
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { }, "Xóa sản phẩm khỏi giỏ thành công"));
        }


        //Xoa toan bo cart
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));
            }

            var cart = await GetOrCreateCart(userId.Value);

            var items = await _context.CartItems
                .Where(x => x.CartId == cart.Id)
                .ToListAsync();

            if (items.Any())
            {
                _context.CartItems.RemoveRange(items);
            }

            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { }, "Đã xóa toàn bộ giỏ hàng"));
        }




    }
}
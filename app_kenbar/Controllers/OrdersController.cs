using Kenbar.Api.Common;
using Kenbar.Api.Data;
using Kenbar.Api.Models;
using Kenbar.Api.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kenbar.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                return null;

            return Guid.Parse(userIdClaim);
        }


        //API QUAN TRONG CHECKOUT
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));

            if (request == null || string.IsNullOrWhiteSpace(request.PaymentMethod))
                return BadRequest(BaseResponse<object>.Fail("Vui lòng chọn phương thức thanh toán"));

            var paymentMethod = request.PaymentMethod.Trim().ToLower();

            if (paymentMethod != "cash" && paymentMethod != "bank")
                return BadRequest(BaseResponse<object>.Fail("Phương thức thanh toán không hợp lệ"));

            var cart = await _context.Carts
                .FirstOrDefaultAsync(x => x.UserId == userId.Value);

            if (cart == null)
                return BadRequest(BaseResponse<object>.Fail("Giỏ hàng trống"));

            var cartItems = await _context.CartItems
                .Where(x => x.CartId == cart.Id)
                .Include(x => x.ProductVariant)
                    .ThenInclude(v => v.Product)
                .Include(x => x.ProductVariant)
                    .ThenInclude(v => v.Unit)
                .ToListAsync();

            if (!cartItems.Any())
                return BadRequest(BaseResponse<object>.Fail("Giỏ hàng trống"));

            decimal totalAmount = 0;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                TotalAmount = 0,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                PaymentMethod = paymentMethod
            };

            _context.Orders.Add(order);

            foreach (var item in cartItems)
            {
                var variant = item.ProductVariant;

                if (variant.StockQuantity < item.Quantity)
                {
                    return BadRequest(BaseResponse<object>.Fail($"Sản phẩm {variant.VariantName} không đủ hàng"));
                }

                var price = variant.SalePrice ?? variant.Price;
                var lineTotal = price * item.Quantity;

                totalAmount += lineTotal;

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductVariantId = variant.Id,
                    ProductName = variant.Product.Name,
                    VariantName = variant.VariantName,
                    UnitCode = variant.Unit != null ? variant.Unit.Code : null,
                    UnitValue = variant.UnitValue,
                    Price = price,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal
                };

                _context.OrderItems.Add(orderItem);

                variant.StockQuantity -= item.Quantity;
            }

            order.TotalAmount = totalAmount;

            if (paymentMethod == "cash")
            {
                order.Status = "paid";
                order.PaidAt = DateTime.UtcNow;
            }

            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new
            {
                order.Id,
                order.TotalAmount,
                order.Status,
                order.PaymentMethod,
                order.CreatedAt,
                order.PaidAt
            }, "Đặt hàng thành công"));
        }



        // Danh sách đơn hàng của người dùng hiện tại
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));

            var orders = await _context.Orders
                .Where(x => x.UserId == userId.Value)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.TotalAmount,
                    x.Status,
                    x.PaymentMethod,
                    x.PaidAt,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(BaseResponse<object>.Ok(orders, "Lấy danh sách đơn hàng thành công"));
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetail(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(BaseResponse<object>.Fail("Token không hợp lệ"));

            var order = await _context.Orders
                .Where(x => x.Id == id && x.UserId == userId.Value)
                .Select(x => new
                {
                    x.Id,
                    x.TotalAmount,
                    x.Status,
                    x.PaymentMethod,
                    x.PaidAt,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy đơn hàng"));

            var items = await _context.OrderItems
                .Where(i => i.OrderId == id)
                .Select(i => new
                {
                    i.Id,
                    i.ProductVariantId,
                    i.ProductName,
                    i.VariantName,
                    i.UnitCode,
                    i.UnitValue,
                    i.Price,
                    i.Quantity,
                    i.LineTotal
                })
                .ToListAsync();

            var result = new
            {
                order.Id,
                order.TotalAmount,
                order.Status,
                order.PaymentMethod,
                order.PaidAt,
                order.CreatedAt,
                Items = items
            };

            return Ok(BaseResponse<object>.Ok(result, "Lấy chi tiết đơn hàng thành công"));
        }


    }
}
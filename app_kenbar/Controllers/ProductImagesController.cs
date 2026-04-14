using Kenbar.Api.Common;
using Kenbar.Api.Data;
using Kenbar.Api.Dtos.ProductImages;
using Kenbar.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kenbar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductImagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductImagesController(AppDbContext context)
        {
            _context = context;
        }

        //Them anh
        [HttpPost]
        public async Task<IActionResult> CreateImage([FromBody] CreateProductImageRequest request)
        {
            if (request.ProductId == Guid.Empty || string.IsNullOrWhiteSpace(request.ImageUrl))
            {
                return BadRequest(BaseResponse<object>.Fail("ProductId và ImageUrl là bắt buộc"));
            }

            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);
            if (product == null)
            {
                return BadRequest(BaseResponse<object>.Fail("Sản phẩm không tồn tại"));
            }

            var hasAnyImage = await _context.ProductImages
                .AnyAsync(x => x.ProductId == request.ProductId);

            bool isThumbnail;

            // Nếu chưa có ảnh nào thì ảnh đầu tiên tự làm ảnh chính
            if (!hasAnyImage)
            {
                isThumbnail = true;
            }
            else
            {
                isThumbnail = request.IsThumbnail;
            }

            // Nếu ảnh mới được chọn làm thumbnail thì bỏ thumbnail cũ
            if (isThumbnail)
            {
                var oldThumbs = await _context.ProductImages
                    .Where(x => x.ProductId == request.ProductId && x.IsThumbnail)
                    .ToListAsync();

                foreach (var item in oldThumbs)
                {
                    item.IsThumbnail = false;
                }
            }

            var image = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                ImageUrl = request.ImageUrl.Trim(),
                SortOrder = request.SortOrder,
                IsThumbnail = isThumbnail,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductImages.Add(image);
            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new
            {
                image.Id,
                image.ProductId,
                image.ImageUrl,
                image.SortOrder,
                image.IsThumbnail,
                image.CreatedAt
            }, "Thêm ảnh thành công"));
        }


        //Cap nhat anh
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImage(Guid id, [FromBody] CreateProductImageRequest request)
        {
            var image = await _context.ProductImages.FirstOrDefaultAsync(x => x.Id == id);
            if (image == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy ảnh"));
            }

            image.ImageUrl = request.ImageUrl.Trim();
            image.SortOrder = request.SortOrder;

            if (request.IsThumbnail)
            {
                var oldThumbs = await _context.ProductImages
                    .Where(x => x.ProductId == image.ProductId && x.IsThumbnail)
                    .ToListAsync();

                foreach (var item in oldThumbs)
                {
                    item.IsThumbnail = false;
                }
            }

            image.IsThumbnail = request.IsThumbnail;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(image, "Cập nhật ảnh thành công"));
        }



        //Lay danh sach anh theo product
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetImages(Guid productId)
        {
            var images = await _context.ProductImages
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.IsThumbnail)
                .ThenBy(x => x.SortOrder)
                .Select(x => new
                {
                    x.Id,
                    x.ProductId,
                    x.ImageUrl,
                    x.SortOrder,
                    x.IsThumbnail
                })
                .ToListAsync();

            return Ok(BaseResponse<object>.Ok(images));
        }


        //Xoa anh
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(Guid id)
        {
            var image = await _context.ProductImages.FirstOrDefaultAsync(x => x.Id == id);
            if (image == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy ảnh"));
            }

            var productId = image.ProductId;
            var isThumbnail = image.IsThumbnail;

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            // Nếu ảnh bị xoá là thumbnail → set ảnh khác làm thumbnail
            if (isThumbnail)
            {
                var nextImage = await _context.ProductImages
                    .Where(x => x.ProductId == productId)
                    .OrderBy(x => x.SortOrder)
                    .FirstOrDefaultAsync();

                if (nextImage != null)
                {
                    nextImage.IsThumbnail = true;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(BaseResponse<object>.Ok(new { }, "Xóa ảnh thành công"));
        }



        //Set anh chinh
        [HttpPut("{id}/set-thumbnail")]
        public async Task<IActionResult> SetThumbnail(Guid id)
        {
            var image = await _context.ProductImages.FirstOrDefaultAsync(x => x.Id == id);
            if (image == null)
            {
                return NotFound(BaseResponse<object>.Fail("Không tìm thấy ảnh"));
            }

            var oldThumbs = await _context.ProductImages
                .Where(x => x.ProductId == image.ProductId && x.IsThumbnail)
                .ToListAsync();

            foreach (var item in oldThumbs)
            {
                item.IsThumbnail = false;
            }

            image.IsThumbnail = true;

            await _context.SaveChangesAsync();

            return Ok(BaseResponse<object>.Ok(new { }, "Đặt thumbnail thành công"));
        }
    }
}
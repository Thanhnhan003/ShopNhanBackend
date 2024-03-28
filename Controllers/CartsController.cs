using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShopNhanBackend.Data;
using ShopNhanBackend.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ShopNhanBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public CartsController(UserManager<ApplicationUser> userManager, IConfiguration configuration, ApplicationDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }
            
        // Thêm sản phẩm vào giỏ hàng
        [HttpPost("AddToCart/{productId}")]
        [Authorize] // Yêu cầu xác thực để thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddToCart(Guid productId)
        {
            try
            {
                // Lấy ID người dùng từ Claims Principal
                var userId = User.FindFirstValue(ClaimTypes.UserData);

                // Kiểm tra xem sản phẩm có tồn tại không
                var product = await _context.products.FindAsync(productId);
                if (product == null)
                    return NotFound("Sản phẩm không tồn tại");

                // Tạo hoặc cập nhật giỏ hàng của người dùng
                var cartItem = await _context.cartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
                if (cartItem == null)
                {
                    cartItem = new CartItem
                    {
                        UserId = userId,
                        ProductId = productId,
                        Quantity = 1 // Số lượng mặc định có thể điều chỉnh tùy theo yêu cầu
                    };
                    _context.cartItems.Add(cartItem);
                }
                else
                {
                    cartItem.Quantity++; // Nếu sản phẩm đã có trong giỏ hàng, tăng số lượng lên 1
                    _context.cartItems.Update(cartItem);
                }

                await _context.SaveChangesAsync();

                return Ok("Sản phẩm đã được thêm vào giỏ hàng");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
        // Lấy sản phẩm trong giỏ hàng của người dùng
        [HttpGet("CartItems")]
        [Authorize] // Yêu cầu xác thực để lấy sản phẩm trong giỏ hàng
        public async Task<IActionResult> GetCartItems()
        {
            try
            {
                // Lấy ID người dùng từ Claims Principal
                var userId = User.FindFirstValue(ClaimTypes.UserData);

                // Lấy các sản phẩm trong giỏ hàng của người dùng
                var cartItems = await _context.cartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                // Lấy thông tin chi tiết sản phẩm cho từng mục trong giỏ hàng
                var productsInCart = cartItems.Select(cartItem => new
                {
                    cartItem.Id,
                    cartItem.Quantity,
                    Product = _context.products.FirstOrDefault(p => p.ID == cartItem.ProductId)
                });

                return Ok(productsInCart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
        // Xóa sản phẩm khỏi giỏ hàng của người dùng
        [HttpDelete("RemoveFromCart/{cartItemId}")]
        [Authorize] // Yêu cầu xác thực để xóa sản phẩm khỏi giỏ hàng
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                // Lấy ID người dùng từ Claims Principal
                var userId = User.FindFirstValue(ClaimTypes.UserData);

                // Tìm kiếm sản phẩm trong giỏ hàng của người dùng
                var cartItem = await _context.cartItems.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
                if (cartItem == null)
                {
                    return NotFound("Không tìm thấy sản phẩm trong giỏ hàng của người dùng");
                }

                // Xóa sản phẩm khỏi giỏ hàng
                _context.cartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok("Sản phẩm đã được xóa khỏi giỏ hàng");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}

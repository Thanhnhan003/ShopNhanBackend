using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using ShopNhanBackend.Models;
using System.Security.Claims;
namespace ShopNhanBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersController(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("getTokenUser")]
        [Authorize] // Yêu cầu xác thực token
        public async Task<IActionResult> GetTokenUser()
        {
            try
            {
                // Lấy userId từ token
                var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.UserData)?.Value;

                // Nếu userId không tồn tại hoặc rỗng
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Invalid token.");
                }

                // Tìm kiếm thông tin người dùng dựa trên userId
                var user = await _userManager.FindByIdAsync(userId);

                // Kiểm tra xem người dùng có tồn tại không
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Trả về thông tin người dùng
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")] // Cần quyền Admin để truy cập
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = _userManager.Users.ToList();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

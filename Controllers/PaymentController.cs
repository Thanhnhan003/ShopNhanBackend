using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopNhanBackend.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using ShopNhanBackend.Data;
using Microsoft.EntityFrameworkCore;
namespace ShopNhanBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: api/Payment
        [HttpGet]
        public ActionResult<IEnumerable<Orders>> GetOrders()
        {
            // Sử dụng Include để tải thông tin từ các bảng liên quan
            var orders = _context.orders
                                .Include(o => o.CartItem) // Tải thông tin từ bảng CartItem
                                    .ThenInclude(c => c.ProductCartId) // Tiếp tục tải thông tin từ bảng ProductCartId
                                .ToList();
            return orders;
        }
        // POST: api/Payment
        [HttpPost]
        public async Task<ActionResult<Orders>> PostPayment([FromBody] Orders order)
        {
            try
            {
                if (order == null)
                {
                    return BadRequest("Order data is null");
                }

                // Tìm kiếm CartItem dựa trên cartId
                var cartItem = await _context.cartItems
                                    .Include(x => x.ProductCartId)
                                    .FirstOrDefaultAsync(x => x.Id == order.cartId);

                if (cartItem == null)
                {
                    return BadRequest("Cart not found");
                }

                // Tạo một đơn hàng mới từ thông tin trong CartItem
                var newOrder = new Orders
                {
                    cartId = cartItem.Id,
                    TotalPrice = order.TotalPrice, // Lấy TotalPrice từ ProductCartId tương ứng
                    PaymentMethod = order.PaymentMethod,
                    ShippingAddress = order.ShippingAddress,
                    ShippingStatus = order.ShippingStatus,
                    OrderTime = DateTime.Now,
                };

                _context.orders.Add(newOrder);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOrders), new { id = newOrder.Id }, newOrder);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error creating order: {ex.Message}");
            }
        }

    }
}

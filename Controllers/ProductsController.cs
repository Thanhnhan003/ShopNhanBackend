using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ShopNhanBackend.Models;
using ShopNhanBackend.Data;

using System;
namespace ShopNhanBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        public ProductsController(ApplicationDbContext dbContext, IWebHostEnvironment hostEnvironment)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Products>>> GetProducts()
        {
            // Sử dụng Include để load thông tin về category
            var productsWithCategory = await _dbContext.products.Include(p => p.Category).ToListAsync();

            if (productsWithCategory == null || productsWithCategory.Count == 0)
            {
                return NotFound();
            }

            return productsWithCategory;
        }
        [HttpPost]
        public async Task<ActionResult<Products>> PostProduct([FromBody] Products product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                product.ID = Guid.NewGuid();
                // Thêm sản phẩm vào DbContext
                _dbContext.products.Add(product);
                await _dbContext.SaveChangesAsync();
              
                return CreatedAtAction(nameof(GetProducts), new { id = product.ID }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Products>> GetProduct(Guid id) // Change the type of id to Guid
        {
            // Tìm sản phẩm với ID được cung cấp và bao gồm thông tin về category
            var product = await _dbContext.products
                                .Include(p => p.Category)
                                .FirstOrDefaultAsync(p => p.ID == id); // Compare with Guid

            if (product == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy sản phẩm
            }

            return product; // Trả về sản phẩm với thông tin về category nếu tìm thấy
        }


        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Tìm sản phẩm với ID được cung cấp
            var product = await _dbContext.products.FindAsync(id);

            if (product == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy sản phẩm
            }

            try
            {
                _dbContext.products.Remove(product); // Xóa sản phẩm từ DbSet
                await _dbContext.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
                return Ok("Product deleted successfully"); // Trả về thông báo thành công nếu xóa thành công
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting product"); // Trả về lỗi 500 nếu có lỗi xảy ra
            }
        }
        [HttpGet("categoryproduct/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Products>>> GetProductsByCategory(int categoryId, int page = 0, int size = 10)
        {
            try
            {
                // Calculate the number of items to skip
                int skip = page * size;

                // Filter and paginate
                var query = _dbContext.products
                                      .Where(p => p.CategoryId == categoryId)
                                      .Include(p => p.Category)
                                      .Skip(skip)
                                      .Take(size);

                var productsByCategory = await query.ToListAsync();

                if (productsByCategory == null || productsByCategory.Count == 0)
                {
                    return NotFound(); // Return 404 if no products found
                }

                // Optional: You can also send the total number of items for pagination purposes
                var totalItems = await _dbContext.products.CountAsync(p => p.CategoryId == categoryId);
                Response.Headers["X-Total-Count"] = totalItems.ToString();

                return productsByCategory;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex}");
            }
        }


    }
}

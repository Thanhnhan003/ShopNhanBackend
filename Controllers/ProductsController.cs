using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopNhanBackend.Models;
using ShopNhanBackend.Data;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using System;
namespace ShopNhanBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductsController(ApplicationDbContext dbContext, IWebHostEnvironment hostEnvironment)
        {
            _dbContext = dbContext;
            _hostEnvironment = hostEnvironment;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Products>>> GetProducts()    
        {
            
            var productsWithCategory = await _dbContext.products.Include(p => p.Category).ToListAsync();

            if (productsWithCategory == null || productsWithCategory.Count == 0)
            {
                return NotFound();
            }

            return productsWithCategory;
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Products>> PostProduct([FromForm] Products productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Upload ảnh và lấy đường dẫn lưu trữ
                string imagePath = await UploadImage(productDto.ImagePath);

                // Tạo đối tượng Products từ ProductDto và đường dẫn ảnh
                var product = new Products
                {
                    ID = Guid.NewGuid(),
                    NameProduct = productDto.NameProduct,
                    Price = productDto.Price,
                    ImageFile = imagePath, // Sửa ở đây
                    Title = productDto.Title,
                    IsActive = productDto.IsActive,
                    CategoryId = productDto.CategoryId
                };

                // Thêm sản phẩm vào DbContext
                _dbContext.products.Add(product);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.ID }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        // Phương thức này thực hiện upload ảnh và trả về đường dẫn lưu trữ
        private async Task<string> UploadImage(IFormFile image)
        {
            // Tạo thư mục lưu trữ ảnh nếu chưa tồn tại
            string uploadsFolder = Path.Combine(_hostEnvironment.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file độc đáo dựa trên thời gian
            string uniqueFileName = $"{Guid.NewGuid().ToString()}_{image.FileName}";    
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Copy file ảnh vào thư mục đã tạo
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // Trả về đường dẫn ảnh
            return Path.Combine("Uploads", uniqueFileName).Replace("\\", "/");
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


        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
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
                return Ok(new { message = "Product deleted successfully", productId = product.ID }); // Trả về thông báo thành công và ID của sản phẩm đã xóa
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting product"); // Trả về lỗi 500 nếu có lỗi xảy ra
            }
        }

        [HttpGet("image/Uploads/{imageName}")]
        public IActionResult GetImage(string imageName)
        {
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Uploads", imageName);
            if (System.IO.File.Exists(imagePath))
            {
                var imageFileStream = System.IO.File.OpenRead(imagePath);
                return File(imageFileStream, "image/jpeg"); // Trả về file ảnh với content type là image/jpeg
            }
            return NotFound(); // Trả về 404 nếu không tìm thấy ảnh
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

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(Guid id, [FromForm] Products productDto)
        {
            try
            {
                // Tìm sản phẩm với ID được cung cấp
                var existingProduct = await _dbContext.products.FindAsync(id);

                if (existingProduct == null)
                {
                    return NotFound(); // Trả về 404 nếu không tìm thấy sản phẩm
                }

                // Upload ảnh mới nếu có
                string imagePath = existingProduct.ImageFile;
                if (productDto.ImagePath != null)
                {
                    imagePath = await UploadImage(productDto.ImagePath);
                }
                // Cập nhật thông tin sản phẩm với dữ liệu mới
                existingProduct.NameProduct = productDto.NameProduct;
                existingProduct.Price = productDto.Price;
                existingProduct.ImageFile = imagePath;
                existingProduct.Title = productDto.Title;
                existingProduct.IsActive = productDto.IsActive;
                existingProduct.CategoryId = productDto.CategoryId;

                // Lưu thay đổi vào cơ sở dữ liệu
                await _dbContext.SaveChangesAsync();

                return Ok(existingProduct); // Trả về sản phẩm đã được cập nhật
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex}");
            }
        }



    }
}

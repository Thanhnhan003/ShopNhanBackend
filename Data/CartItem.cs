using ShopNhanBackend.Models;

namespace ShopNhanBackend.Data
{
    public class CartItem
    {
        public int Id { get; set; }
        public string? UserId { get; set; } // Khóa ngoại trỏ đến AspNetUsers
        public ApplicationUser? User { get; set; }
        public Guid ProductId { get; set; } // Khóa ngoại trỏ đến sản phẩm
        public Products? ProductCartId { get; set; }
        public int Quantity { get; set; }
    }
}

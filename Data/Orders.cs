using ShopNhanBackend.Models;
using System;

namespace ShopNhanBackend.Data
{
    public class Orders
    {
        public int Id { get; set; } // ID đơn hàng
        public int? cartId { get; set; } // ID người dùng đặt hàng
        public string? TotalPrice { get; set; } // Tổng giá
        public string? PaymentMethod { get; set; } // Phương thức thanh toán
        public string? ShippingAddress { get; set; } // Địa chỉ nhận hàng
        public string? ShippingStatus { get; set; } // Trạng thái vận chuyển
        public DateTime OrderTime { get; set; } // Thời gian đặt hàng

        // Các khóa ngoại
        public CartItem? CartItem { get; set; } // Tham chiếu đến một cart item
 
    }
}

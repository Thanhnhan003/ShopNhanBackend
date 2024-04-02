    using System.ComponentModel.DataAnnotations.Schema;

    namespace ShopNhanBackend.Data
    {
        public class Products
        {
            public Guid ID { get; set; }
            public string? NameProduct { get; set; }
            public string? Price { get; set; }
            public string? ImageFile { get; set; }

            [NotMapped]
            public IFormFile? ImagePath { get; set; }

            public string? Title { get; set; }
            public int? IsActive { get; set; }
            // Khóa ngoại trỏ đến bảng Categorys
            public int CategoryId { get; set; }
            public Categorys? Category { get; set; }

        }

    }

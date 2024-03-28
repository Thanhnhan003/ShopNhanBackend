using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopNhanBackend.Data;
using System.Reflection.Emit;
namespace ShopNhanBackend.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Products> products { get; set; }
        public DbSet<Categorys> categorys { get; set; }
        public DbSet<CartItem> cartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //Định nghĩa mối quan hệ
            builder.Entity<Products>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId);
            builder.Entity<CartItem>()
               .HasOne(p => p.ProductCartId)
               .WithMany()
               .HasForeignKey(p => p.ProductId);
        }
    }
}

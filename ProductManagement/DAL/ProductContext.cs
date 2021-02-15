using System.Data.Entity;
using ProductManagement.Models;

namespace ProductManagement.DAL
{
    public class ProductContext : DbContext, IProductContext
    {
        public ProductContext() : base("ProductContext")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public void MarkAsModified(Product item)
        {
            Entry(item).State = EntityState.Modified;
        }

        public void MarkAsModified(Category item)
        {
            Entry(item).State = EntityState.Modified;
        }
    }
}
using ProductManagement.DAL;
using ProductManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.Tests.DAL
{
    public class TestProductContext : IProductContext
    {
        public TestProductContext()
        {
            this.Products = new TestProductDbSet();
            this.Categories = new TestCategoryDbSet();
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public int SaveChanges()
        {
            return 0;
        }

        public void MarkAsModified(Product item) { }
        public void MarkAsModified(Category item) { }
        public void Dispose() { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ProductManagement.Models;
using System.Web.Mvc;

namespace ProductManagement.DAL
{
    public class ProductInitializer : DropCreateDatabaseIfModelChanges<ProductContext> //For testing purposes
    {
        //To populate database with basic Categories and Products
        protected override void Seed(ProductContext context)
        {
            var categories = new List<Category>
            {
                new Category{Id=1, Name="Electronics" },
                new Category{Id=2, Name="Furniture" },
                new Category{Id=3, Name="Games" }
            };
            categories.ForEach(c => context.Categories.Add(c));

            var products = new List<Product>
            {
                new Product{Name="iPhone",CategoryId=1,Price=23990,Quantity=5,ShortDesc="This is a short description.",LongDesc="A long description looks like this.",ProductSmallImagePath="~/ProductImages/iphone.jpg",ProductLargeImagePath="~/ProductLargeImages/iphone.jpg"},
                new Product{Name="Samsung S20",CategoryId=1,Price=23990,Quantity=5,ShortDesc="This is a short description.",LongDesc="A long description looks like this.",ProductSmallImagePath="~/ProductImages/samsung.jpg",ProductLargeImagePath="~/ProductLargeImages/samsung.jpg"},
                new Product{Name="Pixel 4",CategoryId=1,Price=39990,Quantity=5,ShortDesc="This is a short description.",LongDesc="A long description looks like this.",ProductSmallImagePath="~/ProductImages/pixel.jpg",ProductLargeImagePath="~/ProductLargeImages/pixel.jpg"}
            };
            products.ForEach(p => context.Products.Add(p));
            context.SaveChanges();
        }

    }
}
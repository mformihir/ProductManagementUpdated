using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductManagement;
using ProductManagement.Controllers;
using ProductManagement.Models;
using ProductManagement.Tests.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using System.Web.WebSockets;
using Moq;

namespace ProductManagement.Tests.Controllers
{
    [TestClass]
    public class ProductsControllerTest
    {
        [TestMethod]
        public void IndexGet_ShouldReturnView()
        {
            // Arrange
            var dbContext = new TestProductContext();
            var categories = GetTestCategories();
            categories.ForEach(c => dbContext.Categories.Add(c));

            var products = GetTestProducts();
            products.ForEach(p => dbContext.Products.Add(p));
            ProductsController controller = new ProductsController(dbContext);

            // Act
            ViewResult result = controller.Index("", "", "", null, "") as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        //
        // Tests for Delete Action Method
        [TestMethod]
        public void DeleteGet_ShouldReturnProductToDelete()
        {
            // Arrange
            var context = new TestProductContext();
            context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
            context.Products.Add(new Product { Name = "iPhone", CategoryId = 1, Price = 23990, Quantity = 5, ShortDesc = "This is a short description.", LongDesc = "A long description looks like this.", ProductSmallImagePath = "~/ProductImages/iphone.jpg", ProductLargeImagePath = "" });
            var controller = new ProductsController(context);

            // Act
            var result = controller.Delete(0) as ViewResult;

            // Assert
            Assert.AreEqual(context.Products.Find(0), result.Model);
        }

        [TestMethod]
        public void DeleteGet_ShouldReturnBadRequest()
        {
            // Arrange
            var context = new TestProductContext();
            var controller = new ProductsController(context);

            // Act
            var result = controller.Delete(null) as HttpStatusCodeResult;
            var expected = new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Assert
            Assert.AreEqual(expected.StatusCode, result.StatusCode);
        }

        [TestMethod]
        public void DeleteGet_ShouldReturnNotFound()
        {
            // Arrange
            var context = new TestProductContext();
            var controller = new ProductsController(context);

            // Act
            var result = controller.Delete(1) as HttpNotFoundResult;

            // Assert
            Assert.IsInstanceOfType(result, typeof(HttpNotFoundResult));
        }

        [TestMethod]
        public void DeleteConfirmedPost_ShouldRedirectToIndex()
        {
            // Arrange
            var dbContext = new TestProductContext();
            dbContext.Categories.Add(new Category { Id = 1, Name = "Electronics" });
            dbContext.Products.Add(new Product { ID = 0, Name = "iPhone", CategoryId = 1, Price = 23990, Quantity = 5, ShortDesc = "This is a short description.", LongDesc = "A long description looks like this.", ProductSmallImagePath = "testPath", ProductLargeImagePath = "testPath" });
            var controller = new ProductsController(dbContext);

            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.MapPath(It.IsAny<string>())).Returns(@"TestPath");

            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            var result = controller.DeleteConfirmed(0) as RedirectToRouteResult;

            // Assert
            Assert.IsNull(dbContext.Products.Find(0)); //Check that the product is deleted
            Assert.IsNotNull(controller.TempData["NotificationSuccess"]); //Check that the notification is set
            Assert.AreEqual("Index", result.RouteValues["action"]); //Check that the request is redirected to Index View

            request.VerifyAll(); //Verify that the mock request was invoked
        }

        [TestMethod]
        public void DeleteConfirmedPost_ShouldReturnBadRequest()
        {
            // Arrange
            var context = new TestProductContext();
            var controller = new ProductsController(context);

            // Act
            var result = controller.DeleteConfirmed(2) as HttpStatusCodeResult; //Trying to delete a product that does not exist
            var expected = new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Assert
            Assert.AreEqual(expected.StatusCode, result.StatusCode);
        }

        [TestMethod]
        public void DeleteMultipleGet_ShouldReturnProductsToDelete()
        {
            // Arrange
            var context = new TestProductContext();

            var categories = GetTestCategories();
            categories.ForEach(c => context.Categories.Add(c));

            var products = GetTestProducts();
            products.ForEach(p => context.Products.Add(p));

            var controller = new ProductsController(context);

            // Act
            int?[] deleteProductIds = { 1, 2 };
            var result = controller.DeleteMultiple(deleteProductIds) as ViewResult;
            var resultDeletedProducts = (IEnumerable<Product>)result.Model;
            var expected = new List<Product>
            {
                context.Products.Find(1),
                context.Products.Find(2)
            };

            // Assert
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(expected[i], resultDeletedProducts.ElementAt(i));
            }
        }

        [TestMethod]
        public void DeleteMultipleGet_ShouldRedirectToIndex()
        {
            // Arrange
            var context = new TestProductContext();

            var categories = GetTestCategories();
            categories.ForEach(c => context.Categories.Add(c));

            var products = GetTestProducts();
            products.ForEach(p => context.Products.Add(p));

            var controller = new ProductsController(context);

            // Act
            var result = controller.DeleteMultiple(null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(controller.TempData["NotificationInfo"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void DeleteMultipleConfirmedPost_ShouldRedirectToIndex()
        {
            // Arrange
            var dbContext = new TestProductContext();

            var categories = GetTestCategories();
            categories.ForEach(c => dbContext.Categories.Add(c));
            var products = GetTestProducts();
            products.ForEach(p => dbContext.Products.Add(p));

            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.MapPath(It.IsAny<string>())).Returns(@"TestPath");

            var controller = new ProductsController(dbContext);
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            // Act
            int[] deleteProductIds = { 1, 2 };
            var result = controller.DeleteMultipleConfirmed(deleteProductIds) as RedirectToRouteResult;

            // Assert
            foreach (int i in deleteProductIds)
            {
                Assert.IsNull(dbContext.Products.Find(i));
            }
            Assert.IsNotNull(controller.TempData["NotificationSuccess"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);

            request.VerifyAll(); //Verify that the mock request was invoked
        }

        [TestMethod]
        public void CreateGet_ShouldReturnView()
        {
            // Arrange
            var dbContext = new TestProductContext();

            var categories = GetTestCategories();
            categories.ForEach(c => dbContext.Categories.Add(c));
            ProductsController controller = new ProductsController(dbContext);

            // Act
            ViewResult result = controller.Create() as ViewResult;

            // Assert
            Assert.IsNotNull(result.ViewData["DBCategories"]); //Check that DBCategories DropDown is populated from the database
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreatePost_ShouldReturnModelError()
        {
            // Arrange
            var dbContext = new TestProductContext();

            var categories = GetTestCategories();
            categories.ForEach(c => dbContext.Categories.Add(c));
            var product = new Product
            {
                ID = 0,
                Name = "iPhone",
                CategoryId = 1,
                Price = 23990,
                Quantity = 5,
                ShortDesc = "This is a short description.",
                LongDesc = "A long description looks like this.",
                ProductSmallImagePath = null,
                ProductLargeImagePath = null
            };
            ProductsController controller = new ProductsController(dbContext);

            // Act
            ViewResult result = controller.Create(product) as ViewResult;

            // Assert
            Assert.IsNotNull(controller.ModelState["ProductSmallImage"]);
            Assert.IsNotNull(result.ViewData["DBCategories"]); //Check that DBCategories DropDown is populated from the database
            Assert.AreEqual(product, result.Model);
        }

        [TestMethod]
        public void CreatePost_ShouldRedirectToIndex()
        {
            // Arrange
            var dbContext = new TestProductContext();

            var categories = GetTestCategories();
            categories.ForEach(c => dbContext.Categories.Add(c));
            var product = new Product
            {
                ID = 0,
                Name = "iPhone",
                CategoryId = 1,
                Price = 23990,
                Quantity = 5,
                ShortDesc = "This is a short description.",
                LongDesc = "A long description looks like this.",
                ProductSmallImagePath = null,
                ProductLargeImagePath = null
            };
            ProductsController controller = new ProductsController(dbContext);

            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.MapPath(It.IsAny<string>())).Returns(@"TestPath");

            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            string mockImagePath = Path.GetFullPath("../../TestSaveDir/iphone.jpg");

            Mock<HttpPostedFileBase> mockImage = new Mock<HttpPostedFileBase>();

            mockImage
                .Setup(f => f.FileName)
                .Returns("iphone.jpg");
            product.ProductSmallImage = mockImage.Object;
            product.ProductLargeImage = mockImage.Object;

            // Act
            var result = controller.Create(product) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(controller.TempData["NotificationSuccess"]); //Check that Success Notification is set
            Assert.IsNotNull(dbContext.Products.Find(0));
            Assert.AreEqual("Index", result.RouteValues["action"]);

            mockImage.VerifyAll(); //Verify that the mock image was invoked
            request.VerifyAll(); //Verify that the mock request was invoked
        }

        [TestMethod]
        public void CreatePost_ShouldHaveValidationErrors()
        {
            // Arrange
            var dbContext = new TestProductContext();

            var categories = GetTestCategories();
            categories.ForEach(c => dbContext.Categories.Add(c));
            var product = new Product();
            ProductsController controller = new ProductsController(dbContext);

            Mock<HttpPostedFileBase> mockImage = new Mock<HttpPostedFileBase>();

            product.ProductSmallImage = mockImage.Object;

            controller.ModelState.AddModelError("Key", "ErrorMessage"); //For testing invalid model

            // Act
            var result = controller.Create(product) as ViewResult;

            // Assert
            Assert.IsNotNull(result.ViewData["DBCategories"]); //Check that DBCategories DropDown is populated from the database
            Assert.AreEqual(product, result.Model);

            mockImage.VerifyAll(); //Verify that the mock image was invoked
        }

        [TestMethod]
        public void EditGet_ShouldReturnBadRequest()
        {
            // Arrange
            var dbContext = new TestProductContext();
            var controller = new ProductsController(dbContext);
            var products = GetTestProducts();
            products.ForEach(p => dbContext.Products.Add(p));

            // Act
            var result = controller.Edit(id: null) as HttpStatusCodeResult;
            var expected = new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // Assert
            Assert.AreEqual(expected.StatusCode, result.StatusCode);
        }

        [TestMethod]
        public void EditGet_ShouldReturnNotFound()
        {
            // Arrange
            var context = new TestProductContext();
            var controller = new ProductsController(context);

            // Act
            var result = controller.Edit(1) as HttpNotFoundResult;

            // Assert
            Assert.IsInstanceOfType(result, typeof(HttpNotFoundResult));
        }

        [TestMethod]
        public void EditGet_ShouldReturnProductToEdit()
        {
            // Arrange
            var dbContext = new TestProductContext();
            var controller = new ProductsController(dbContext);
            var products = GetTestProducts();
            products.ForEach(p => dbContext.Products.Add(p));

            // Act
            var result = controller.Edit(id: 1) as ViewResult;

            // Assert
            Assert.IsNotNull(result.ViewData["DBCategories"]);
            Assert.AreEqual(dbContext.Products.Find(1), result.Model);
        }

        [TestMethod]
        public void EditPost_ShouldRedirectToIndex()
        {
            // Arrange
            var dbContext = new TestProductContext();

            var categories = GetTestCategories();
            categories.ForEach(c => dbContext.Categories.Add(c));
            var product = new Product
            {
                ID = 0,
                Name = "iPhone",
                CategoryId = 1,
                Price = 23990,
                Quantity = 5,
                ShortDesc = "This is a short description.",
                LongDesc = "A long description looks like this.",
                ProductSmallImagePath = null,
                ProductLargeImagePath = null
            };
            dbContext.Products.Add(product);
            ProductsController controller = new ProductsController(dbContext);

            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.MapPath(It.IsAny<string>())).Returns(@"TestPath");

            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            Mock<HttpPostedFileBase> mockImage = new Mock<HttpPostedFileBase>();

            mockImage
                .Setup(f => f.FileName)
                .Returns("iphone.jpg");
            product.ProductSmallImage = mockImage.Object;
            product.ProductLargeImage = mockImage.Object;

            // Act
            var result = controller.Edit(product) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(controller.TempData["NotificationSuccess"]); //Check that Success Notification is set
            //Assert.IsNotNull(dbContext.Products.Find(0));
            Assert.AreEqual("Index", result.RouteValues["action"]);

            mockImage.VerifyAll(); //Verify that the mock image was invoked
            request.VerifyAll(); //Verify that the mock request was invoked
        }

        [TestMethod]
        public void EditPost_ShouldReturnModelError()
        {
            // Arrange
            var dbContext = new TestProductContext();

            var categories = GetTestCategories();
            categories.ForEach(c => dbContext.Categories.Add(c));
            var product = new Product
            {
                ID = 0,
                Name = "iPhone",
                CategoryId = 1,
                Price = 23990,
                Quantity = 5,
                ShortDesc = "This is a short description.",
                LongDesc = "A long description looks like this.",
                ProductSmallImagePath = null,
                ProductLargeImagePath = null
            };
            dbContext.Products.Add(product);
            ProductsController controller = new ProductsController(dbContext);

            controller.ModelState.AddModelError("Key", "ErrorMessage"); //For testing invalid model

            // Act
            ViewResult result = controller.Edit(product) as ViewResult;

            // Assert
            Assert.IsNotNull(result.ViewData["DBCategories"]); //Check that DBCategories DropDown is populated from the database
            Assert.AreEqual(product, result.Model);
        }

        private List<Product> GetTestProducts()
        {
            var testProducts = new List<Product>
            {
                new Product{ID = 0, Name="iPhone",CategoryId=1,Price=23990,Quantity=5,ShortDesc="This is a short description.",LongDesc="A long description looks like this.",ProductSmallImagePath="TestPath",ProductLargeImagePath="TestPath"},
                new Product{ID = 1, Name="Samsung S20",CategoryId=1,Price=23990,Quantity=5,ShortDesc="This is a short description.",LongDesc="A long description looks like this.",ProductSmallImagePath="TestPath",ProductLargeImagePath="TestPath"},
                new Product{ID = 2, Name="Pixel 4",CategoryId=1,Price=39990,Quantity=5,ShortDesc="This is a short description.",LongDesc="A long description looks like this.",ProductSmallImagePath="TestPath",ProductLargeImagePath="TestPath"}
            };
            return testProducts;
        }

        private List<Category> GetTestCategories()
        {
            var testCategories = new List<Category>
            {
                new Category{Id=1, Name="Electronics" },
                new Category{Id=2, Name="Furniture" },
                new Category{Id=3, Name="Games" }
            };
            return testCategories;
        }

    }
}

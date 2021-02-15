using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using log4net;
using Microsoft.AspNet.Identity;
using PagedList;
using ProductManagement.DAL;
using ProductManagement.Models;

namespace ProductManagement.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly IProductContext _db = new ProductContext();
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProductsController));

        public ProductsController() { }

        public ProductsController(IProductContext context)
        {
            _db = context;
        }

        // GET: Products
        public ActionResult Index(string sortOrder, string searchString, string searchBy, int? page, string currentFilter)
        {
            try
            {
                //Switching Sorting Parameters from Ascending to Descending and vice versa
                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewBag.CategorySortParm = sortOrder == "category" ? "ctgr_desc" : "category";
                ViewBag.PriceSortParm = sortOrder == "price" ? "price_desc" : "price";

                var products = from p in _db.Products
                               select p;

                //To return products based on searchString
                if (!String.IsNullOrEmpty(searchString))
                {
                    switch (searchBy)
                    {
                        case "name":
                            products = products.Where(p => p.Name.Contains(searchString));
                            break;
                        case "category":
                            products = products.Where(p => p.Category.Name.Contains(searchString));
                            break;
                        case "description":
                            products = products.Where(p => p.ShortDesc.ToLower().Contains(searchString.ToLower()));
                            break;
                        default:
                            products = products.Where(p => p.Name.Contains(searchString));
                            break;
                    }
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }
                ViewBag.CurrentFilter = searchString;

                //To return results in the specified sortOrder from View
                switch (sortOrder)
                {
                    case "name_desc":
                        products = products.OrderByDescending(p => p.Name);
                        break;
                    case "category":
                        products = products.OrderBy(p => p.Category.Name);
                        break;
                    case "ctgr_desc":
                        products = products.OrderByDescending(p => p.Category.Name);
                        break;
                    case "price":
                        products = products.OrderBy(p => p.Price);
                        break;
                    case "price_desc":
                        products = products.OrderByDescending(p => p.Price);
                        break;
                    default: //If no sortOrder is applied, return Products in Ascending order of Name
                        products = products.OrderBy(p => p.Name);
                        break;
                }
                int pageSize = 3;
                int pageNumber = (page ?? 1);

                return View(products.Include(c => c.Category).ToPagedList(pageNumber, pageSize));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error("User:" + User.Identity.GetUserName(), e);
                throw;
            }
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            try
            {
                //Populating the Categories Dropdown from database
                ViewData["DBCategories"] = new SelectList(_db.Categories.ToList(), "Id", "Name");

                return View();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error("User:" + User.Identity.GetUserName(), e);
                throw;
            }
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,CategoryId,Price,Quantity,ShortDesc,LongDesc,ProductSmallImage,ProductLargeImage")] Product product)
        {
            try
            {
                //Server side validation of ProductSmallImage
                if (product.ProductSmallImage == null)
                {
                    ModelState.AddModelError("ProductSmallImage", "Product Small Image is required.");

                    //Repopulating the Categories DropDown from the database
                    ViewData["DBCategories"] = new SelectList(_db.Categories.ToList(), "Id", "Name");

                    return View(product);
                }

                if (ModelState.IsValid)
                {
                    //Saving the ProductSmallImage to the server and the Image Path to database
                    product.ProductSmallImagePath = SaveImage("~/ProductImages/", product.ID, product.ProductSmallImage);

                    if (product.ProductLargeImage != null)
                    {
                        //Saving the ProductLargeImage to the server and saving the Large Image Path to database
                        product.ProductLargeImagePath = SaveImage("~/ProductLargeImages/", product.ID, product.ProductLargeImage);
                    }

                    _db.Products.Add(product);
                    _db.SaveChanges();

                    //Success notification
                    TempData["NotificationSuccess"] = "Successfully added Product: " + product.Name;
                    return RedirectToAction("Index");
                }

                //Repopulating the Categories SelectList
                ViewData["DBCategories"] = new SelectList(_db.Categories.ToList(), "Id", "Name");

                return View(product);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error("User:" + User.Identity.GetUserName(), e);
                throw;
            }
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var product = _db.Products.Find(id);

                if (product == null)
                {
                    return HttpNotFound();
                }

                //Populating the Categories SelectList from the Database
                ViewData["DBCategories"] = new SelectList(_db.Categories.ToList(), "Id", "Name");

                return View(product);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error("User:" + User.Identity.GetUserName(), e);
                throw;
            }
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,CategoryId,Price,Quantity,ShortDesc,LongDesc,ProductSmallImage,ProductLargeImage")] Product product)
        {
            try
            {
                //Finding the same product from the database to update data
                //Used AsNoTracking() because it is not to be updated (read-only)
                var productInDb = _db.Products.AsNoTracking().Where(p => p.ID == product.ID).Single();

                //If the user didn't change the ProductSmallImage
                if (product.ProductSmallImage == null)
                {
                    //populate from the database
                    product.ProductSmallImagePath = productInDb.ProductSmallImagePath;
                }
                else //If the user updated the ProductSmallImage
                {
                    //Delete old image from the server
                    var oldFilePath = Request.MapPath(productInDb.ProductSmallImagePath);
                    System.IO.File.Delete(oldFilePath);

                    //Save new image to the server and Add path to database
                    product.ProductSmallImagePath = SaveImage("~/ProductImages/", product.ID, product.ProductSmallImage);
                }

                if (ModelState.IsValid)
                {
                    //Storing large image to the server if it was updated
                    if (product.ProductLargeImage != null)
                    {
                        //Delete the old image if exists
                        if (productInDb.ProductLargeImagePath != null)
                        {
                            var oldFilePath = Request.MapPath(productInDb.ProductLargeImagePath);
                            System.IO.File.Delete(oldFilePath);
                        }
                        //Save new image to the server and Add path to database
                        product.ProductLargeImagePath = SaveImage("~/ProductLargeImages/", product.ID, product.ProductLargeImage);
                    }
                    else //If the image was not updated
                    {
                        //Updating the current model with old path in database
                        product.ProductLargeImagePath = productInDb.ProductLargeImagePath;
                    }
                    //Saving model to database
                    _db.MarkAsModified(product);
                    _db.SaveChanges();

                    //Notifying the user of successful operation
                    TempData["NotificationSuccess"] = "Successfully saved changes.";
                    return RedirectToAction("Index");
                }

                //Populating the Categories DropDown from the database
                ViewData["DBCategories"] = new SelectList(_db.Categories.ToList(), "Id", "Name");

                return View(product);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error("User:" + User.Identity.GetUserName(), e);
                throw;
            }
        }


        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                //Getting the product to be deleted from the database
                var product = _db.Products.Where(p => p.ID == id).Include(c => c.Category).Single();
                return View(product);
            }
            catch (InvalidOperationException e) //If no product found
            {
                Log.Error("Product not found", e);
                return HttpNotFound();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error("User:" + User.Identity.GetUserName(), e);
                throw;
            }

        }

        [HttpPost]
        public JsonResult DeleteAjax(int? id)
        {
            try
            {
                //Getting the product to be deleted from the database
                var product = _db.Products.Where(p => p.ID == id).Include(c => c.Category).Single();

                //Delete product image (small, large) stored on server
                if (product.ProductSmallImagePath != null)
                {
                    System.IO.File.Delete(Request.MapPath(product.ProductSmallImagePath));
                }

                if (product.ProductLargeImagePath != null)
                {
                    System.IO.File.Delete(Request.MapPath(product.ProductLargeImagePath));
                }

                _db.Products.Remove(product);
                _db.SaveChanges();

                //Setting the notification
                TempData["NotificationSuccess"] = "Successfully deleted " + product.Name;
                return Json(new { Message = "Success", JsonRequestBehavior.AllowGet });
                //return RedirectToAction("Index");
            }
            catch (InvalidOperationException e) //If no product found
            {
                Log.Error("Product not found", e);
                return Json(new { Message = "Error", JsonRequestBehavior.AllowGet });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error("User:" + User.Identity.GetUserName(), e);
                return Json(new { Message = "Error", JsonRequestBehavior.AllowGet });
                throw;
            }
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                //Getting the product to be deleted from the database
                var product = _db.Products.Where(p => p.ID == id).Include(c => c.Category).Single();

                //Delete product image (small, large) stored on server
                if (product.ProductSmallImagePath != null)
                {
                    System.IO.File.Delete(Request.MapPath(product.ProductSmallImagePath));
                }

                if (product.ProductLargeImagePath != null)
                {
                    System.IO.File.Delete(Request.MapPath(product.ProductLargeImagePath));
                }

                _db.Products.Remove(product);
                _db.SaveChanges();

                //Setting the notification
                TempData["NotificationSuccess"] = "Successfully deleted " + product.Name;
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException e)
            {
                Log.Error("Tried to delete a product which does not exist", e);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error("User:" + User.Identity.GetUserName(), e);
                throw;
            }
        }

        // GET: Products/DeleteMultiple?deleteItemIDs=1&deleteItemIDs=3 
        public ActionResult DeleteMultiple(int?[] deleteItemIDs)
        {
            try
            {
                //Getting products to be deleted and adding them to the list
                var deleteProducts = new List<Product>();
                foreach (int deleteItemID in deleteItemIDs)
                {
                    var pDelete = _db.Products.Where(p => p.ID == deleteItemID).Include(c => c.Category).First();
                    deleteProducts.Add(pDelete);
                }

                //Returning products to be deleted to the view
                return View(deleteProducts);
            }
            catch (NullReferenceException e)
            {
                TempData["NotificationInfo"] = "Please select at least one product to delete.";
                Log.Error("Called DeleteMultiple without selecting any products.", e);
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error("User:" + User.Identity.GetUserName(), e);
                throw;
            }

        }

        // POST: Products/DeleteMultiple?deleteItemIDs=1&deleteItemIDs=3
        [HttpPost, ActionName("DeleteMultiple")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMultipleConfirmed(int[] deleteItemIDs)
        {
            try
            {
                //Deleted Product Names
                string deletedItemNames = "";

                //Deleting specified items
                foreach (int deleteItemID in deleteItemIDs)
                {
                    var pDelete = _db.Products.Where(p => p.ID == deleteItemID).Single();

                    //Append deleted product name to the list (used for the notification)
                    deletedItemNames += pDelete.Name + ", ";

                    //delete product image (small, large) stored on server
                    if (pDelete.ProductSmallImagePath != null)
                    {
                        System.IO.File.Delete(Request.MapPath(pDelete.ProductSmallImagePath));
                    }
                    if (pDelete.ProductLargeImagePath != null)
                    {
                        System.IO.File.Delete(Request.MapPath(pDelete.ProductLargeImagePath));
                    }

                    //Delete the product from Database
                    _db.Products.Remove(pDelete);
                }
                _db.SaveChanges();

                //Removing the trailing commas and whitespace
                deletedItemNames = deletedItemNames.TrimEnd(' ', ',');

                //Notifying the user
                TempData["NotificationSuccess"] = "Successfully deleted " + deletedItemNames;

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error("User:" + User.Identity.GetUserName(), e);
                throw;
            }
        }

        /// <summary>
        /// Saves the specified image to the server.
        /// </summary>
        /// <returns>
        /// Returns the relative path of the saved image.
        /// </returns>
        /// <param name="saveDir"> The directory in which the image is to be saved. </param>
        /// <param name="productId"> Product ID which will be appended at the end of image filename (for uniqueness) </param>
        /// <param name="image"> The image which is to be saved. </param>
        [NonAction]
        public string SaveImage(string saveDir, int productId, HttpPostedFileBase image)
        {
            //Saving image to the server, appending productId to keep the image name unique
            string fileName = Path.GetFileNameWithoutExtension(image.FileName) + "_" + productId + Path.GetExtension(image.FileName);
            string filePath = Path.Combine(Request.MapPath(saveDir), fileName);

            //To copy and save file into server.  
            image.SaveAs(filePath);

            //returns image path to be saved in the database
            return saveDir + fileName;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

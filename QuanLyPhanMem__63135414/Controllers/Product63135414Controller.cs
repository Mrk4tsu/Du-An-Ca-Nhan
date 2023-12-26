using QuanLyPhanMem__63135414.Models;
using QuanLyPhanMem__63135414.Models.Catalog.ProductSystem;
using QuanLyPhanMem__63135414.Models.Extension;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QuanLyPhanMem__63135414.Controllers
{
    public class Product63135414Controller : Controller
    {
        QLPM_63135414_Entities db = new QLPM_63135414_Entities();

        static string productIDToCreate = Utilities.instance.getIdAsync(5);
        #region[Danh sách sản phẩm]
        public async Task<ActionResult> ListProduct(string search = "", int page = 1, string sort = "productName", string sortDir = "asc", int pageSize = 10)
        {
            //Logic phân trang khi truy vấn danh sách
            int totalRecord = 0;
            if (page < 1) page = 1;
            int skip = (page * pageSize) - pageSize;

            //Gọi phương thức getUserAsync và nhận kết quả về
            var dataResult = await getProductAsync(search, sort, sortDir, skip, pageSize);

            // Trích xuất dữ liệu và số lượng bản ghi từ kết quả
            var data = dataResult.Item1;
            totalRecord = dataResult.Item2;

            ViewBag.TotalRows = totalRecord;
            ViewBag.PageSize = pageSize; // Đưa giá trị pageSize vào ViewBag để sử dụng trong view
            return View(data);
        }
        public async Task<(List<Product>, int)> getProductAsync(string search, string sort, string sortDir, int skip, int pageSize)
        {
            var v = from a in db.Products
                    where
                    (a.id.Contains(search) ||
                    a.productName.Contains(search) ||
                    a.User.firstname.Contains(search)) ||
                    a.User.lastname.Contains(search)
                    select a;
            //Đếm tổng số lượng bản ghi
            int totalRecord = await v.CountAsync();
            v = v.OrderBy(sort + " " + sortDir);
            if (pageSize > 0)
            {
                v = v.Skip(skip).Take(pageSize);
            }
            return (v.ToList(), totalRecord);
        }
        #endregion
        #region[Tạo sản phẩm]
        public ActionResult CreateProduct()
        {
            //ViewBag.ProductID = Utilities.instance.getIdAsync(5);
            ViewBag.ProductID = productIDToCreate;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateProduct(ProductViewModel model)
        {
            if (Session["User"] != null)
            {
                var user = (UserViewModel)Session["User"];
                if (ModelState.IsValid)
                {
                    try
                    {
                        var product = new Product
                        {
                            id = productIDToCreate,
                            userId = user.userId,
                            dateUpload = DateTime.Now,
                            dateUpdate = DateTime.Now,
                            productName = model.productName,
                            description = model.description,
                            productUrl = model.productUrl,
                            priceOriginal = model.priceOriginal,
                            price = model.price,
                            sellCount = 0,
                            viewCount = 0,
                        };
                        db.Products.Add(product);
                        await db.SaveChangesAsync();

                        // Lấy id của sản phẩm vừa tạo
                        string productId = product.id;
                        // Chuyển sang Action tạo ảnh sản phẩm và truyền id của sản phẩm
                        return RedirectToAction("CreateImage", new { productId = productId });
                    }
                    catch
                    {
                        return RedirectToAction("Error", "Admin63135414");
                    }
                }
                return View(model);
            }
            else
                return RedirectToAction("Login", "Customer63135414");
        }
        #endregion
        #region[Tạo ảnh sản phẩm]
        public ActionResult CreateImage(string productId)
        {
            ViewBag.ProductID = productId;
            return PartialView("CreateImage");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateImage(ProductImage productImage)
        {
            if (ModelState.IsValid)
            {
                productImage.imgDefault = true;
                productImage.dateCreate = DateTime.Now;
                productImage.caption = "Ảnh minh họa";
                db.ProductImages.Add(productImage);
                await db.SaveChangesAsync();
            }

            // Chuyển về View hoặc thực hiện các xử lý khác sau khi tạo ảnh

            return PartialView(productImage);
        }
        #endregion

        #region[Tạo sản phẩm và thêm ảnh]
        [HttpGet]
        public ActionResult CreateProductAndImage()
        {
            var viewModel = new ProductCreateViewModel
            {
                Product = new ProductViewModel(),
                ProductImage = new ProductImage()
            };
            ViewBag.ProductID = productIDToCreate;
            return View(viewModel);
        }
        public async Task<ActionResult> CreateProductAndImage(ProductCreateViewModel viewModel)
        {
            if (Session["User"] != null)
            {
                var user = (UserViewModel)Session["User"];
                if (ModelState.IsValid)
                {
                    //try
                    //{
                    // Tạo sản phẩm
                    var product = new Product
                    {
                        id = productIDToCreate,
                        userId = user.userId,
                        dateUpload = DateTime.Now,
                        dateUpdate = DateTime.Now,
                        productName = viewModel.Product.productName,
                        description = viewModel.Product.description,
                        productUrl = viewModel.Product.productUrl,
                        priceOriginal = viewModel.Product.priceOriginal,
                        price = viewModel.Product.price,
                        sellCount = 0,
                        viewCount = 0,
                    };
                    db.Products.Add(product);
                    await db.SaveChangesAsync();
                    //var product = new Product
                    //    {
                    //        id = Utilities.instance.getIdAsync(5),
                    //        dateUpload = DateTime.Now,
                    //        dateUpdate = DateTime.Now,
                    //        productName = viewModel.Product.productName,
                    //        description = viewModel.Product.description,
                    //        productUrl = viewModel.Product.productUrl,
                    //        priceOriginal = viewModel.Product.priceOriginal,
                    //        price = viewModel.Product.price,
                    //        sellCount = 0,
                    //        viewCount = 0,
                    //    };

                    //    db.Products.Add(product);
                    //    await db.SaveChangesAsync();

                        // Lấy id của sản phẩm vừa tạo
                        string productId = product.id;

                        // Tạo ảnh sản phẩm
                        var productImage = new ProductImage
                        {
                            productId = productId,
                            imgDefault = true,
                            dateCreate = DateTime.Now,
                            caption = "Ảnh minh họa"
                            // Thêm các thuộc tính ảnh sản phẩm khác nếu cần
                        };

                        db.ProductImages.Add(productImage);
                        await db.SaveChangesAsync();

                    // Chuyển sang Action tạo ảnh thêm cho sản phẩm và truyền id của sản phẩm
                    //return RedirectToAction("CreateImage", new { productId = productId });
                    return RedirectToAction("ListProduct");
                    //}
                    //catch
                    //{
                    //    return RedirectToAction("Error", "Admin63135414");
                    //}
                }
                return View(viewModel);
            }
            else
                return RedirectToAction("Login", "Customer63135414");
        }
        #endregion
    }
}
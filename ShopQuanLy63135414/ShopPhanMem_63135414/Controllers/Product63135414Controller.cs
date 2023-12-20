using ShopPhanMem_63135414.Models.Catalog.UserSystem;
using ShopPhanMem_63135414.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ShopPhanMem_63135414.Models.Catalog.ProductSystem;
using ImageResizer;
using System.Drawing;
using System.IO;
using System.Net;

namespace ShopPhanMem_63135414.Controllers
{
    public class Product63135414Controller : Controller
    {
        QLPM63135414Entities db = new QLPM63135414Entities();

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
        public async Task<ActionResult> ListImage(string search = "", int page = 1, string sort = "productId", string sortDir = "asc", int pageSize = 10)
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
        public async Task<(List<ProductImage>, int)> getImageAsync(string search, string sort, string sortDir, int skip, int pageSize)
        {
            var v = from a in db.ProductImages
                    where
                    (a.id.Contains(search) ||
                    a.imagePath.Contains(search))
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
            ViewBag.ProductID = Utilities.instance.getIdAsync(5);
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
                            id = Utilities.instance.getIdAsync(5),
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
        public async Task<ActionResult> CreateProductAndImage()
        {
            if (Session["User"] != null)
            {
                // Lấy danh sách danh mục
                var categories = await db.Categories.ToListAsync();

                // Tạo một đối tượng ProductAndImageViewModel và gán danh sách danh mục vào nó
                var viewModel = new ProductAndImageViewModel
                {
                    Categories = categories
                        .Select(c => new SelectListItem
                        {
                            Value = c.id,
                            Text = c.categoryName
                        })
                };
                ViewBag.ProductID = Utilities.instance.getIdAsync(5);
                // Hiển thị view với form trống để người dùng nhập thông tin
                return View(viewModel);
            }
            else
            {
                return RedirectToAction("Login", "Customer63135414");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateProductAndImage(ProductAndImageViewModel viewModel, HttpPostedFileBase imageUpload)
        {
            if (Session["User"] != null)
            {
                var user = (UserViewModel)Session["User"];
                if (ModelState.IsValid)
                {
                    try
                    {

                        // Lấy danh sách danh mục
                        var categories = await db.Categories.ToListAsync();

                        // Gán danh sách danh mục vào viewModel
                        viewModel.Categories = categories
                            .Select(c => new SelectListItem
                            {
                                Value = c.id,
                                Text = c.categoryName
                            });
                        #region[Tạo sản phẩm]
                        // Tạo sản phẩm
                        var product = new Product
                        {
                            id = Utilities.instance.getIdAsync(5),
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
                        #endregion

                        // Lấy id của sản phẩm vừa tạo
                        string productId = product.id;

                        #region[Thêm ảnh default cho sản phẩm]
                        var productImg = SaveUploadedFile(imageUpload, product.productName, Utilities.PRODUCT_IMAGE_DEFAULT);
                        // Tạo ảnh sản phẩm
                        var productImage = new ProductImage
                        {
                            id = Utilities.instance.getIDImage(5),
                            productId = productId,
                            imagePath = productImg,
                            imgDefault = true,
                            dateCreate = DateTime.Now,
                            caption = "Ảnh minh họa"
                            // Thêm các thuộc tính ảnh sản phẩm khác nếu cần
                        };

                        db.ProductImages.Add(productImage);
                        await db.SaveChangesAsync();
                        #endregion
                        #region[Thêm sản phẩm vào danh mục]
                        // Lấy id của danh mục từ form
                        string categoryId = viewModel.SelectedCategoryId;
                        // Tạo ProductInCategory và lưu vào database
                        var productInCategory = new ProductInCategory
                        {
                            categoryId = categoryId,
                            productId = productId,
                            description = "Mô tả danh mục"  // Có thể thay đổi theo nhu cầu
                        };

                        db.ProductInCategories.Add(productInCategory);
                        await db.SaveChangesAsync();
                        #endregion
                        // Chuyển sang Action tạo ảnh thêm cho sản phẩm và truyền id của sản phẩm
                        //return RedirectToAction("CreateImage", new { productId = productId });
                        return RedirectToAction("ListProduct");
                    }
                    catch
                    {
                        return RedirectToAction("Error", "Admin63135414");
                    }
                }

                return View(viewModel);
            }
            else
                return RedirectToAction("Login", "Customer63135414");
        }
        #endregion
        #region[Xóa sản phẩm]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tìm người dùng theo id
            var productToDelete = db.Products.Find(id);

            if (productToDelete == null)
            {
                return HttpNotFound();
            }

            try
            {
                // Xóa sản phẩm
                var productImagesToDelete = db.ProductImages.Where(pi => pi.productId == id);
                foreach (var productImage in productImagesToDelete)
                {
                    //// Xóa từ thư mục lưu trữ hình ảnh (nếu cần)
                    //deteleFileProduct(productToDelete.id);
                    // Xóa khỏi cơ sở dữ liệu
                    db.ProductImages.Remove(productImage);
                }

                db.Products.Remove(productToDelete);
                db.SaveChanges();


                return RedirectToAction("ListProduct");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                ViewBag.Error = "Không thể xóa sản phẩm này. " + ex.Message;
                return RedirectToAction("Error", "Admin63135414");
            }
        }
        #endregion
        #region[Xem chi tiết sản phẩm]
        public async Task<ActionResult> DetailProduct(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            // Lấy danh sách hình ảnh của sản phẩm
            var productImages = await db.ProductImages.Where(pi => pi.productId == id).ToListAsync();

            // Map thông tin sang ViewModel
            var viewModel = new ProductAndImageViewModel
            {
                Product = new ProductViewModel
                {
                    productName = product.productName,
                    userId = product.userId,
                    dateUpload = product.dateUpload,
                    dateUpdate = product.dateUpdate,
                    viewCount = product.viewCount,
                    sellCount = product.sellCount,
                    description = product.description,
                    price = product.price,
                    priceOriginal = product.priceOriginal,
                },
                ProductImage = productImages.FirstOrDefault() // Lấy ảnh đầu tiên, bạn có thể thay đổi tùy theo yêu cầu
            };
            return View(viewModel);
        }
        #endregion
        #region[Phương thức hỗ trỡ]
        public void deteleFileProduct(string fileDelete)
        {
            string folderPath = Server.MapPath($"~/assets/product/{fileDelete}");
            // Gọi phương thức xóa thư mục
            // Kiểm tra xem tệp tồn tại trước khi xóa
            if (System.IO.Directory.Exists(folderPath))
            {
                // Xóa tệp hình ảnh từ thư mục lưu trữ
                System.IO.Directory.Delete(folderPath);
            }
        }
        public string SaveUploadedFile(HttpPostedFileBase file, string subFolder, string fail)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var directoryPath = Server.MapPath($"~/assets/product/{subFolder}");

                // Tạo thư mục nếu không tồn tại
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var filePath = Path.Combine(directoryPath, fileName);

                // Lưu tệp lên máy chủ
                file.SaveAs(filePath);
                // Resize và crop ảnh về kích thước 540
                var settings = new ResizeSettings
                {
                    Width = 540,
                    Height = 460,
                    Mode = FitMode.Crop,
                    Scale = ScaleMode.Both,
                    Anchor = ContentAlignment.MiddleCenter,
                };

                ImageBuilder.Current.Build(filePath, filePath, settings);
                return fileName;
            }

            return fail;
        }
        #endregion
    }
}
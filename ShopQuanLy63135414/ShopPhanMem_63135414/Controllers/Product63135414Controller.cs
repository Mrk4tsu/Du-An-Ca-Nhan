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
        public async Task<ActionResult> CreateProductAndImage(ProductAndImageViewModel viewModel, HttpPostedFileBase imageUpload, IEnumerable<HttpPostedFileBase> files)
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

                        // Xử lý các tệp đã tải lên sử dụng phương thức SaveUploadedFiles hoặc logic khác
                        SaveUploadedFiles(files, product.productName, "fail_message");

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
        public async Task<ActionResult> DetailsProduct(string id)
        {
            var product = await db.Products.FindAsync(id);
            // Kiểm tra xem sản phẩm có tồn tại không
            if (product == null)
            {
                return HttpNotFound();
            }
            if (Session["User"] != null)
            {
                var user = (UserViewModel)Session["User"];
                if (!product.ViewedUserIds.Contains(user.userId))
                {
                    // Người dùng chưa xem sản phẩm, cập nhật viewCount và thêm id người dùng vào danh sách đã xem
                    product.viewCount++;
                    product.ViewedUserIds.Add(user.userId);
                    await db.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
                }
            }
            // Tạo một đối tượng ViewModel từ sản phẩm
            ProductViewListVM viewModel = new ProductViewListVM(product, null);
            // Lấy danh sách hình ảnh từ thư mục trên máy chủ
            string imagePath = Server.MapPath("~/assets/product/" + product.productName + "/");
            string[] imageFiles = Directory.GetFiles(imagePath);
            // Thêm đường dẫn của các hình ảnh vào ViewModel
            viewModel.ImagePaths = imageFiles.Select(Path.GetFileName).ToList();
            // Chuyển đến View để hiển thị thông tin chi tiết sản phẩm


            var topProducts = db.Products.OrderByDescending(p => p.viewCount ?? 0).Take(4).ToList();
            ViewBag.TopProducts = topProducts;
            return View(viewModel);
        }
        #endregion
        // Phương thức để thêm sản phẩm vào khuyến mãi
        #region[Khuyến mãi]
        public ActionResult CreatePromotion()
        {
            return View();
        }
       [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePromotion([Bind(Include = "id,dateFrom,dateTo,applyAll,promotionPercent,description")] Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                promotion.applyAll = false;
                db.Promotions.Add(promotion);
                await db.SaveChangesAsync();
                return RedirectToAction("ListProduct");
            }

            return View(promotion);
        }
        public ActionResult AddProductToPromotion()
        {
            ViewBag.Promotions = new SelectList(db.Promotions, "id", "description");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddProductToPromotion(string productId, int promotionID)
        {
            // Retrieve the product and category from the database
            Product product = db.Products.Find(productId);
            Promotion promotion = db.Promotions.Find(promotionID);

            if (product != null && promotion != null)
            {
                // Check if the product is already in the category
                if (db.ProductPromotions.Any(p => p.productId == productId && p.promotionId == promotionID))
                {
                    // Product is already in the category, handle accordingly (e.g., show a message)
                    ViewBag.Message = "Product này đã được giảm giá!";
                }
                else
                {
                    // Add the product to the category
                    ProductPromotion productPromotion = new ProductPromotion
                    {
                        promotionId = promotionID,
                        productId = productId,
                        description = promotion.description // You can set the description accordingly
                    };

                    db.ProductPromotions.Add(productPromotion);
                    await db.SaveChangesAsync();

                    ViewBag.Message = "Product added to the category successfully";
                }
            }
            else
            {
                // Product or category not found, handle accordingly (e.g., show an error message)
                ViewBag.Message = "Product or category not found";
            }

            // Redirect or return a view
            return RedirectToAction("DetailsProduct", "Product63135414", new { id = product.id }); // Redirect to the product list or any other action

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
        public List<string> SaveUploadedFiles(IEnumerable<HttpPostedFileBase> files, string subFolder, string fail)
        {
            List<string> savedFileNames = new List<string>();

            foreach (var file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    // Generate a unique file name using timestamp and Guid
                    var fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";

                    var directoryPath = Server.MapPath($"~/assets/product/{subFolder}");

                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var filePath = Path.Combine(directoryPath, fileName);

                    // Save the file to the server
                    file.SaveAs(filePath);

                    // Resize and crop the image to 540x460
                    var settings = new ResizeSettings
                    {
                        Width = 540,
                        Height = 460,
                        Mode = FitMode.Crop,
                        Scale = ScaleMode.Both,
                        Anchor = ContentAlignment.MiddleCenter,
                    };

                    ImageBuilder.Current.Build(filePath, filePath, settings);

                    // Add the saved file name to the list
                    savedFileNames.Add(fileName);
                }
            }

            if (savedFileNames.Count > 0)
            {
                return savedFileNames;
            }

            return new List<string> { fail };
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
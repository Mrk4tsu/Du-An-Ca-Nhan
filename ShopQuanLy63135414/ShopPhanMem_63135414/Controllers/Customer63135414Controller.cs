using ImageResizer;
using ShopPhanMem_63135414;
using ShopPhanMem_63135414.Models;
using ShopPhanMem_63135414.Models.Catalog.ProductSystem;
using ShopPhanMem_63135414.Models.Catalog.UserSystem;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using static System.Net.WebRequestMethods;

namespace QuanLyPhanMem__63135414.Controllers
{
    public class Customer63135414Controller : Controller
    {
        #region[Register]
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        //Register Post Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Exclude = "isActive,codeActive")] User user, HttpPostedFileBase userAvatar, string password, string confirmPassword)
        {
            bool status = false;
            string message = "";
            // Tạo đường dẫn lưu trữ cho ảnh đại diện và ảnh nền
            var userAvatarPath = SaveUploadedFile(userAvatar, "avatar");

            //Model Validation
            if (ModelState.IsValid)
            {
                #region Email already exist
                var isExist = isEmailExist(user.email);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email đã tồn tại");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code
                user.codeActive = Guid.NewGuid().ToString();
                #endregion

                #region Password Hashing
                if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(confirmPassword))
                {
                    // Only update password if both new password and confirm password are provided
                    if (password.Length < 6)
                    {
                        ModelState.AddModelError("newPassword", "Mật khẩu ít nhất phải có 6 ký tự!");
                    }
                    else if (!password.Equals(confirmPassword))
                    {
                        ModelState.AddModelError("confirmPassword", "Mật khẩu không khớp, vui lòng kiểm tra lại!");
                    }
                    else
                    {
                        // Update password if validation passes
                        user.password = Utilities.Hash(password);
                        user.confirmPassword = Utilities.Hash(confirmPassword);
                    }
                }
                #endregion

                #region[Thiết lập thông tin mặc định sau khi đăng kí]
                user.userId = Utilities.instance.getNewGuid();
                user.isActive = false;
                user.roleId = "R03";//Set mặc định là khách hàng
                user.userWallpaper = "defaultwallpaper.png";
                user.userAvatar = userAvatarPath;
                user.address = "Chưa thiết lập";
                user.birthday = DateTime.Now;
                user.bio = string.Empty;
                #endregion

                #region Save to Database
                using (QLPM63135414Entities db = new QLPM63135414Entities())
                {
                    db.Users.Add(user);
                    db.SaveChanges();

                    //Send Email to User
                    sendVerificationLinkEmail(user.email, user.codeActive.ToString());
                    message = "Đăng ký được thực hiện thành công. Liên kết kích hoạt tài khoản đã được gửi đến id email của bạn: " + user.email;
                    status = true;
                }
                #endregion
            }
            else
                message = "Yêu cầu không hợp lệ!";

            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(user);
        }
        #endregion
        #region [Verify Email]
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool status = false;
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                //Dòng này thêm vào đây để tránh xác nhận mật khẩu không khớp với vấn đề khi lưu thay đổi
                db.Configuration.ValidateOnSaveEnabled = false;

                var v = db.Users.Where(a => a.codeActive == new Guid(id).ToString()).FirstOrDefault();
                if (v != null)
                {
                    v.isActive = true;
                    db.SaveChanges();
                    status = true;
                }
                else
                {
                    ViewBag.Message = "Yêu cầu không hợp lệ!";
                }
            }
            ViewBag.Status = status;
            return View();
        }
        #endregion
        #region[Login]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        //Login Post
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin user, string returnUrl = "")
        {
            string message = "";
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                var v = db.Users.Where(em => em.email == user.email).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(Utilities.Hash(user.password), v.password) == 0)
                    {
                        //Dặt thời gian hết hạn của vé xác thực dựa trên việc người dùng có tick remember me
                        int timeOut = user.rememberMe ? 5256000 : 1; //5256000 phút = 1 năm
                        //Tạo một vé xác thực Forms mới với địa chỉ email của người dùng
                        var ticket = new FormsAuthenticationTicket(user.email, user.rememberMe, timeOut);

                        //Mã hóa vé xác thực
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);

                        //Đặt thời gian hết hạn của cookie thành thời gian hết hạn của vé xác thực
                        cookie.Expires = DateTime.Now.AddMinutes(timeOut);
                        //Đặt HttpOnly = true ngăn cookie được truy cập bởi mã JavaScript
                        cookie.HttpOnly = true;

                        //Gửi cookie đến trình duyệt của người dùng. Lưu cookie vào máy tính của người dùng khi web nhận cookies
                        Response.Cookies.Add(cookie);
                        var userViewModel = getUserViewModel(v);

                        // Lưu thông tin người dùng vào Session để sử dụng trong các request tiếp theo nếu cần
                        Session["User"] = userViewModel;

                        var roleId = v.roleId;// Lấy roleId từ thông tin người dùng

                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            if (roleId.Contains("R03"))
                            {
                                return RedirectToAction("Home", "Customer63135414");
                            }
                            else
                                return RedirectToAction("AdminHome", "Admin63135414");
                        }
                    }
                    else message = "Tài khoản hoặc mật khẩu không chính xác!";
                }
                else
                    message = "Tài khoản hoặc mật khẩu không chính xác!";
            }
            ViewBag.Message = message;
            return View(user);
        }
        #endregion
        #region[Change Password]
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword([Bind(Include = "password")] User user, string oldPassword, string newPassword)
        {
            bool status = false;
            string message = "";
            string newP = Utilities.Hash(newPassword);
            string oldP = Utilities.Hash(oldPassword);
            if (ModelState.IsValid)
            {
                using (QLPM63135414Entities db = new QLPM63135414Entities())
                {
                    user = (User)Session["User"];
                    if (!oldP.Contains(user.password))
                    {
                        message = "Mật khẩu cũ nhập không đúng";
                        return View(user);
                    }
                    if (oldP.Contains(newP))
                    {
                        message = "Mật khẩu này đã được sử dụng, vui lòng nhập mật khẩu khác!";
                        return View(user);
                    }
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                    message = "Mật khẩu của bạn đã được thay đổi thành công!";
                    status = true;
                }
            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(user);
        }
        #endregion
        [Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login", "Customer63135414");
        }
        [HttpGet]
        public ActionResult UserProfile(string email)
        {
            UserViewModel userViewModel = (UserViewModel)Session["User"];

            // Pass thông tin người dùng đến View
            return View(userViewModel);
        }
        [HttpGet]
        [Authorize]
        public ActionResult Home()
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                // Lấy danh sách sản phẩm từ database
                List<ProductAndImageViewModel> productList = db.Products
                    .Select(p => new ProductAndImageViewModel
                    {
                        Product = new ProductViewModel
                        {
                            productId = p.id,
                            productName = p.productName,
                            price = p.price
                        },
                        // Không tạo đối tượng ProductImage ở đây
                    })
                    .ToList();

                // Tạo đối tượng ProductImage sau khi đã lấy dữ liệu từ database
                foreach (var product in productList)
                {
                    // Bạn có thể thực hiện logic để lấy dữ liệu từ bảng ProductImage tương ứng
                    // Ví dụ:
                    var productImage = db.ProductImages.FirstOrDefault(pi => pi.productId == product.Product.id);
                    if (productImage != null)
                    {
                        product.ProductImage = productImage;
                    }
                }

                // Lưu danh sách vào ViewBag
                ViewBag.ProductList = productList;







                // Lấy danh sách sản phẩm từ database
                ViewBag.Products = db.Products.ToList();

                ViewBag.Categories = db.Categories.ToList().OrderBy(c => c.categoryName);
            }
            return View();
        }
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Products()
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                ViewBag.Categories = db.Categories.ToList().OrderBy(c => c.categoryName);

                var products = await db.Products
                    .Include(i => i.ProductImages)
                    .Include(pic => pic.ProductInCategories.Select(c => c.Category))
                    .ToListAsync();
                // Chuyển danh sách sản phẩm sang danh sách ViewModel
                var productViewModels = products.Select(product => new ProductViewListVM(product)).ToList();
                return View(productViewModels);
            }
        }
        [HttpGet]
        public ActionResult GetAllProducts()
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                var productsWithImages = db.Products
                    .Include(i => i.ProductImages)
                    .Include(pic => pic.ProductInCategories.Select(c => c.Category))
                    .ToList();

                var productViewModels = productsWithImages.Select(product => new ProductViewListVM(product)).ToList();

                return Json(productViewModels, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetProductsByFilters(decimal minPrice, decimal maxPrice, string categoryId)
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                IQueryable<Product> products = db.Products;

                // Áp dụng bộ lọc khoảng giá
                if (maxPrice == 1501)
                {
                    products = products.Where(p => p.price >= minPrice);
                }
                else
                {
                    products = products.Where(p => p.price >= minPrice && p.price <= maxPrice);
                }

                // Áp dụng bộ lọc danh mục
                if (!string.IsNullOrEmpty(categoryId))
                {
                    products = products.Where(p => p.ProductInCategories.Any(pc => pc.Category.id == categoryId));
                }

                var productsWithImages = products
                    .Include(i => i.ProductImages)
                    .Include(pic => pic.ProductInCategories.Select(c => c.Category))
                    .ToList();

                var productViewModels = productsWithImages.Select(product => new ProductViewListVM(product)).ToList();

                return Json(productViewModels, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetProductsByCategory(string categoryId)
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                
                var products = db.Products
                    .Where(p => p.ProductInCategories.Any(pc => pc.Category.id == categoryId))
                    .Include(i => i.ProductImages)
                    .Include(pic => pic.ProductInCategories.Select(c => c.Category))
                    .ToList();

                var productViewModels = products.Select(product => new ProductViewListVM(product)).ToList();
                
                return Json(productViewModels, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                var products = db.Products
                    .Where(p => p.price >= minPrice && p.price <= maxPrice)
                    .Include(i => i.ProductImages)
                    .Include(pic => pic.ProductInCategories.Select(c => c.Category))
                    .ToList();

                var productViewModels = products.Select(product => new ProductViewListVM(product)).ToList();

                return Json(productViewModels, JsonRequestBehavior.AllowGet);
            }
        }


        #region[Xem chi tiết sản phẩm]
        public async Task<ActionResult> DetailsProduct(string id)
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
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
                // Đường dẫn đến thư mục chứa ảnh
                string folderPath = Server.MapPath($"~/assets/product/{product.productName}");
                // Kiểm tra xem thư mục có tồn tại không
                if (Directory.Exists(folderPath))
                {
                    // Lấy danh sách các đường dẫn của tất cả các tệp trong thư mục
                    string[] imagePaths = Directory.GetFiles(folderPath);
                    foreach (string filePath in imagePaths)
                    {

                        ViewBag.Image1 = Path.GetFileName(filePath);
                        ViewBag.Image2 = Path.GetFileName(filePath);

                    }
                }

                return View(viewModel);
            }
        }
        #endregion
        public ActionResult Error()
        {
            return View();
        }
        #region[Phương thức hỗ trợ]
        [NonAction]
        private string SaveUploadedFile(HttpPostedFileBase file, string subFolder)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var directoryPath = Server.MapPath($"~/assets/{subFolder}");

                // Tạo thư mục nếu không tồn tại
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var filePath = Path.Combine(directoryPath, fileName);

                // Lưu tệp lên máy chủ
                file.SaveAs(filePath);

                // Resize và crop ảnh về kích thước 300x300
                var settings = new ResizeSettings
                {
                    Width = 300,
                    Height = 300,
                    Mode = FitMode.Crop,
                    Scale = ScaleMode.Both,
                    Anchor = ContentAlignment.MiddleCenter,
                };

                ImageBuilder.Current.Build(filePath, filePath, settings);

                return fileName;
            }

            return "avatardefault.png";
        }
        [NonAction]
        private bool isEmailExist(string email)
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                var v = db.Users.Where(e => e.email == email).FirstOrDefault();
                return v != null;
            }
        }
        [NonAction]
        private UserViewModel getUserViewModel(User user)
        {
            return new UserViewModel
            {
                userId = user.userId,
                roleId = user.roleId,
                email = user.email,
                password = user.password,
                firstname = user.firstname,
                lastname = user.lastname,
                userAvatar = user.userAvatar,
                userWallpaper = user.userWallpaper,
                birthday = user.birthday,
                address = user.address,
                phoneNumber = user.phoneNumber,
                bio = user.bio,
                codeActive = user.codeActive,
                isActive = user.isActive,
                UserRole = user.UserRole
            };
        }
        #endregion
        [NonAction]
        //Gửi email xác nhận link
        public void sendVerificationLinkEmail(string emailID, string activationCode)
        {
            var verifyUrl = "/Customer63135414/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("thang.ndu.63cntt@ntu.edu.vn", "MrKatsu Shop");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "thangnguyen2212"; // Replace with actual password
            string subject = "Tài khoản của bạn đã được tạo thành công!";

            string body = "<br/><br/>Chúng tôi vui mừng thông báo với bạn rằng tài khoản MrKatsu Shop của bạn được tạo thành công. Vui lòng nhấp vào liên kết bên dưới để xác minh tài khoản của bạn" +
                " <br/><br/><a href='" + link + "'>" + link + "</a> ";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }
    }
}
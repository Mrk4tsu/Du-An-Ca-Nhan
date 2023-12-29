using ImageResizer;
using ShopPhanMem_63135414;
using ShopPhanMem_63135414.Models;
using ShopPhanMem_63135414.Models.Catalog.OrderSystem;
using ShopPhanMem_63135414.Models.Catalog.ProductSystem;
using ShopPhanMem_63135414.Models.Catalog.UserSystem;
using System;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

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
                var products = db.Products
                     .Include(i => i.ProductImages)
                     .Include(pic => pic.ProductInCategories.Select(c => c.Category))
                     .OrderByDescending(p => p.viewCount ?? 0)
                     .Take(4)
                     .ToList();

                // Chuyển danh sách sản phẩm sang danh sách ViewModel
                var productViewModels = products.Select(product => new ProductViewListVM(product, null)).ToList();

                ViewBag.Categories = db.Categories.ToList().OrderBy(c => c.categoryName);

                // Lấy 4 sản phẩm mới nhất
                var productsList = db.Products
                    .OrderByDescending(p => p.dateUpload)
                    .Take(4)
                    .ToList();


                
                // Chuyển đổi danh sách sản phẩm thành danh sách ProductViewListVM
                var productUploadViewModels = productsList.Select(p => new ProductViewListVM(p, null)).ToList();


                var productsMostSellList = db.Products
                    .OrderByDescending(p => p.sellCount ?? 0)
                    .Take(4)
                    .ToList();
                var productBestSellViewModels = productsMostSellList.Select(p => new ProductViewListVM(p, null)).ToList();
                // Lưu danh sách sản phẩm mới nhất vào ViewBag
                ViewBag.ProductUploadViewModels = productUploadViewModels;
                ViewBag.ProductBestSellViewModels = productBestSellViewModels;

                return View(productViewModels);
            }
        }
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Products()
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                var topProducts = db.Products.OrderByDescending(p => p.viewCount ?? 0).Take(3).ToList();
                var topProductsMore = db.Products.OrderByDescending(p => p.viewCount ?? 0).Skip(3).Take(3).ToList();

                var comparer = new ProductEqualityComparer();

                var distinctTopProducts = topProducts.Except(topProductsMore, comparer).ToList();
                var distinctTopProductsMore = topProductsMore.Except(topProducts, comparer).ToList();

                ViewBag.TopProducts = distinctTopProducts;
                ViewBag.TopProductsMore = distinctTopProductsMore;

                ViewBag.Categories = db.Categories.ToList().OrderBy(c => c.categoryName);

                var products = await db.Products
                    .Include(i => i.ProductImages)
                    .Include(pic => pic.ProductInCategories.Select(c => c.Category))
                    .ToListAsync();
                // Chuyển danh sách sản phẩm sang danh sách ViewModel
                var productViewModels = products.Select(product => new ProductViewListVM(product, -1)).ToList();
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

                var productViewModels = productsWithImages.Select(product => new ProductViewListVM(product, -1)).ToList();

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

                var productViewModels = productsWithImages.Select(product => new ProductViewListVM(product, -1)).ToList();

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

                var productViewModels = products.Select(product => new ProductViewListVM(product, -1)).ToList();

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

                var productViewModels = products.Select(product => new ProductViewListVM(product, -1)).ToList();

                return Json(productViewModels, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddToCart(string productId)
        {
            if (Session["User"] != null)
            {
                using (QLPM63135414Entities db = new QLPM63135414Entities())
                {
                    var user = (UserViewModel)Session["User"];
                    var userCart = db.Carts.Include(c => c.CartItems)
                                      .FirstOrDefault(c => c.UserId == user.userId);

                    if (userCart == null)
                    {
                        // Create a new cart if the user doesn't have one
                        userCart = new Cart
                        {
                            Id = Utilities.instance.getIdCart(),
                            UserId = user.userId,
                            DateCreate = DateTime.Now
                        };
                        db.Carts.Add(userCart);
                    }

                    // Check if the product is already in the cart
                    var cartItem = userCart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

                    if (cartItem != null)
                    {
                        // Product already in cart, return a JSON result indicating the failure
                        return Json(new { success = false, message = "Sản phẩm đã tồn tại." });
                    }
                    else
                    {
                        // Add a new item to the cart if the product is not present
                        var product = db.Products.Find(productId);

                        if (product != null)
                        {
                            // Create a new CartItem
                            var newCartItem = new CartItem
                            {
                                Id = Utilities.instance.getIdCartItem(),
                                ProductId = productId,
                                Quantity = 1,
                                Price = product.price,
                                CartId = userCart.Id // Set the CartId here
                            };

                            userCart.CartItems.Add(newCartItem);

                            db.SaveChanges();

                            // Return a JSON result indicating the success
                            return Json(new { success = true, message = "Thêm vào giỏ hàng thành công." });

                        }
                    }
                }
            }

            // User not logged in, return a JSON result indicating the failure
            return Json(new { success = false, message = "User not logged in." });
        }

        [HttpGet]
        public ActionResult CartView()
        {
            if (Session["User"] != null)
            {
                using (QLPM63135414Entities db = new QLPM63135414Entities())
                {
                    var user = (UserViewModel)Session["User"];
                    // Phương thức để lấy ID của người dùng (thực hiện phương thức này theo authentication của bạn)
                    var userCart = db.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefault(c => c.UserId == user.userId);

                    if (userCart == null)
                    {
                        // Người dùng chưa có giỏ hàng, tạo mới một giỏ hàng
                        userCart = new Cart
                        {
                            Id = Utilities.instance.getIdCart(),
                            UserId = user.userId,
                            DateCreate = DateTime.Now
                        };
                        db.Carts.Add(userCart);
                        db.SaveChanges(); // Lưu thay đổi để có ID của giỏ hàng mới tạo

                        // Lấy lại giỏ hàng với thông tin đầy đủ bao gồm CartItems
                        userCart = db.Carts
                            .Include(c => c.CartItems)
                            .FirstOrDefault(c => c.UserId == user.userId);
                    }
                    var cartViewModel = userCart.CartItems.Select(cartItem => new ProductViewListVM(cartItem.Product, cartItem.Quantity)).ToList();
                    return View(cartViewModel);
                }
            }
            return RedirectToAction("Login");
        }
        [HttpPost]
        public ActionResult RemoveFromCart(string productId)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (Session["User"] != null)
            {
                using (QLPM63135414Entities db = new QLPM63135414Entities())
                {
                    var user = (UserViewModel)Session["User"];
                    var userCart = db.Carts.Include(c => c.CartItems)
                        .FirstOrDefault(c => c.UserId == user.userId);

                    if (userCart != null)
                    {
                        // Tìm kiếm và xóa sản phẩm khỏi giỏ hàng nếu tồn tại
                        var cartItemToRemove = userCart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                        if (cartItemToRemove != null)
                        {
                            db.CartItems.Remove(cartItemToRemove);
                            db.SaveChanges();
                        }
                    }

                    // Chuyển hướng về trang giỏ hàng
                    return RedirectToAction("CartView");
                }
            }
            else
            {
                // Người dùng chưa đăng nhập, bạn có thể chuyển hướng đến trang đăng nhập hoặc xử lý theo ý của bạn
                return RedirectToAction("Login");
            }
        }
        [HttpPost]
        public ActionResult ProcessCheckout()
        {
            if (Session["User"] != null)
            {
                using (QLPM63135414Entities db = new QLPM63135414Entities())
                {
                    var user = (UserViewModel)Session["User"];
                    var userCart = db.Carts.Include(c => c.CartItems)
                                           .FirstOrDefault(c => c.UserId == user.userId);

                    if (userCart != null && userCart.CartItems.Any())
                    {
                        // Tạo đối tượng Order
                        var order = new Order
                        {
                            OrderId = Utilities.instance.getIdOrder(),
                            UserId = user.userId,
                            OrderDate = DateTime.Now,
                            TotalAmount = userCart.CartItems.Sum(ci => ci.Quantity * ci.Price),
                            Status = "Đã thanh toán" ,// Có thể đặt trạng thái khác tùy thuộc vào logic của bạn
                        };

                        // Tạo đối tượng OrderItem cho mỗi sản phẩm trong giỏ hàng
                        foreach (var cartItem in userCart.CartItems)
                        {
                            // Tăng sellCount của sản phẩm
                            var product = db.Products.Find(cartItem.ProductId);
                            if (product != null)
                            {
                                product.sellCount += 1;
                            }
                            var orderItem = new OrderItem
                            {
                                OrderItemId = cartItem.Id, // Đảm bảo giá trị mới mỗi lần được gọi
                                OrderId = order.OrderId,
                                ProductId = cartItem.ProductId,
                                Quantity = cartItem.Quantity,
                                Price = cartItem.Price
                            };

                            order.OrderItems.Add(orderItem);
                        }

                        // Lưu thay đổi vào cơ sở dữ liệu
                        db.Orders.Add(order);
                        db.SaveChanges();

                        // Xóa các mục trong giỏ hàng, nhưng giữ nguyên giỏ hàng
                        foreach (var cartItem in userCart.CartItems.ToList())
                        {
                            db.CartItems.Remove(cartItem);
                        }

                        db.SaveChanges();
                        // Thêm thông tin đơn hàng vào ViewBag
                        ViewBag.OrderId = order.OrderId;
                        ViewBag.TotalAmount = order.TotalAmount;
                        // Redirect đến trang xác nhận đơn hàng
                        return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
                    }
                }
            }

            // Người dùng chưa đăng nhập hoặc giỏ hàng trống, xử lý theo ý của bạn
            return RedirectToAction("CartView");
        }
        public ActionResult OrderConfirmation(string orderId)
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                // Lấy thông tin đơn hàng từ cơ sở dữ liệu
                var order = db.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.OrderId == orderId);

                if (order != null)
                {
                    // Truyền thông tin đơn hàng đến view để hiển thị
                    return View(order);
                }
                else
                {
                    // Đơn hàng không tồn tại, xử lý theo ý của bạn, ví dụ: chuyển hướng về trang chính
                    return RedirectToAction("Home");
                }
            }
        }
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
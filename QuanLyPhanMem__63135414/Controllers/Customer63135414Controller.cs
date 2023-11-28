using QuanLyPhanMem__63135414.Models;
using QuanLyPhanMem__63135414.Models.Extension;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
        public ActionResult Register([Bind(Exclude = "isActive,codeActive")] User user)
        {
            bool status = false;
            string message = "";
            //System.Web.HttpPostedFileBase Avatar;
            var imgNV = Request.Files["Avatar"];
            if (imgNV != null && imgNV.ContentLength > 0)
            {
                // Lưu hình đại diện về Server
                var fileName = System.IO.Path.GetFileName(imgNV.FileName);
                var path = Server.MapPath("/assets/images/users/" + fileName);
                imgNV.SaveAs(path);

                user.userAvatar = fileName;
            }
            else
                user.userAvatar = "avatardefault.png";
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
                user.password = Utils.Hash(user.password);
                user.confirmPassword = Utils.Hash(user.confirmPassword);
                #endregion

                #region[Thiết lập 1 số thông tin mặc định]
                user.userId = Utils.getUserId();
                user.isActive = false;
                user.roleId = "R03";//Set mặc định là khách hàng
                user.userWallpaper = "defaultwallpaper.png";
                #endregion

                #region Save to Database
                using (QLPM_63135414Entities db = new QLPM_63135414Entities())
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
            using (QLPM_63135414Entities db = new QLPM_63135414Entities())
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
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin user, string returnUrl = "")
        {
            string message = "";
            using (QLPM_63135414Entities db = new QLPM_63135414Entities())
            {
                var v = db.Users.Where(em => em.email == user.email).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(Utils.Hash(user.password), v.password) == 0)
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

                        //Nếu ở trang chủ (địa chỉ set mặc định)
                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else return RedirectToAction("Index", "Home");
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
        [Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Customer63135414");
        }
        [HttpGet]
        public ActionResult UserProfile(string email)
        {
            UserViewModel userViewModel = (UserViewModel)Session["User"];

            // Pass thông tin người dùng đến View
            return View(userViewModel);
        }
        #region[Phương thức hỗ trợ]
        [NonAction]
        private bool isEmailExist(string email)
        {
            using (QLPM_63135414Entities db = new QLPM_63135414Entities())
            {
                var v = db.Users.Where(e => e.email == email).FirstOrDefault();
                return v != null;
            }
        }
        [NonAction]
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
                quantityProject = user.quantityProject,
                bio = user.bio,
                codeActive = user.codeActive,
                isActive = user.isActive,
                UserRole = user.UserRole
            };
        }
        #endregion
        [Authorize]
        public ActionResult Home()
        {
            return View();
        }
    }
}
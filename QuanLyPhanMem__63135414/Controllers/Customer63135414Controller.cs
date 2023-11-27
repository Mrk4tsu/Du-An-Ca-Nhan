using QuanLyPhanMem__63135414.Models.Extension;
using QuanLyPhanMem__63135414.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace QuanLyPhanMem__63135414.Controllers
{
    public class Customer63135414Controller : Controller
    {
        //Register Action
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
                user.userId = Utils.getUserId();
                user.isActive = false;
                user.roleId = "R03";//Set mặc định là khách hàng
                #region Save to Database
                using (QLPM63135414Entities db = new QLPM63135414Entities())
                {
                    db.Users.Add(user);
                    db.SaveChanges();

                    //Send Email to User
                    SendVerificationLinkEmail(user.email, user.codeActive.ToString());
                    message = "Đăng ký được thực hiện thành công. Liên kết kích hoạt tài khoản đã được gửi đến id email của bạn: " + user.email;
                    status = true;
                }
                #endregion
            }
            else
            {
                message = "Yêu cầu không hợp lệ!";
            }

            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(user);
        }
        //Verify Email
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
        
        //Login
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
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                var v = db.Users.Where(em => em.email == user.email).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(Utils.Hash(user.password), v.password) == 0)
                    {
                        int timeOut = user.rememberMe ? 5256000 : 1; //5256000 phút = 1 năm
                        var ticket = new FormsAuthenticationTicket(user.email, user.rememberMe, timeOut);

                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);

                        cookie.Expires = DateTime.Now.AddMinutes(timeOut);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Yêu cầu chứng chỉ không hợp lệ!";
                    }
                }
                else
                {
                    message = "Yêu cầu chứng chỉ không hợp lệ!";
                }
            }
                return View(user);
        }
        //Logout
        [Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Customer63135414");
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
        public void SendVerificationLinkEmail(string emailID, string activationCode)
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
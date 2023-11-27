using QuanLyPhanMem_63135414.Models;
using QuanLyPhanMem_63135414.Models.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace QuanLyPhanMem_63135414.Controllers
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
                user.codeActive = Guid.NewGuid();
                #endregion

                #region Password Hashing
                user.password = Utils.Hash(user.password);
                user.confirmPassword = Utils.Hash(user.confirmPassword);
                #endregion
                user.userId = Utils.getUserId();
                user.isActive = false;
                user.roleId = "R03";//Set mặc định là khách hàng
                #region Save to Database
                using (QLPM_63135414Entities db = new QLPM_63135414Entities())
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
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(user);
        }
        //Verify Email

        //Verify Email Link

        //Login

        //Login Post

        //Logout
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
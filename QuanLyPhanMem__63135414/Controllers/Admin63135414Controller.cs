using ImageResizer;
using QuanLyPhanMem__63135414.Models;
using QuanLyPhanMem__63135414.Models.Extension;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QuanLyPhanMem__63135414.Controllers
{
    public class Admin63135414Controller : Controller
    {
        private QLPM63135414_Entities db = new QLPM63135414_Entities();
        // GET: Admin63135414
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> AdminHome()
        {
            using (QLPM63135414_Entities db = new QLPM63135414_Entities())
            {
                ViewBag.TotalUsers = await db.Users.CountAsync();
                ViewBag.TotalCategories = await db.Categories.CountAsync();
                ViewBag.TotalProjects = await db.Products.CountAsync();
                ViewBag.TotalProjectImgs = await db.ProductImages.CountAsync();
                return View();
            }
        }
        #region[Danh sách người dùng]
        public async Task<ActionResult> ListUser(string search = "", string adress = "", string name = "", string roleName = "", int page = 1, string sort = "lastname", string sortDir = "asc", int pageSize = 5)
        {
            //Logic phân trang khi truy vấn danh sách
            int totalRecord = 0;
            if (page < 1) page = 1;
            int skip = (page * pageSize) - pageSize;

            //Lấy danh sách Roles để hiển thị trong DropDownList
            ViewBag.Roles = new SelectList(db.UserRoles, "roleName", "roleName");

            // Tạo danh sách cho DropDownList chọn pageSize
            // Lưu ý rằng pageSize được chuyển vào ViewBag.PageSizeList
            ViewBag.PageSizeList = new SelectList(new List<int> { 5, 10, 15, 20, 25, 50 }, pageSize);
            //Gọi phương thức getUserAsync và nhận kết quả về
            var dataResult = await getUserAsync(search, adress, name, roleName, sort, sortDir, skip, pageSize);

            // Trích xuất dữ liệu và số lượng bản ghi từ kết quả
            var data = dataResult.Item1;
            totalRecord = dataResult.Item2;

            ViewBag.TotalRows = totalRecord;
            ViewBag.PageSize = pageSize; // Đưa giá trị pageSize vào ViewBag để sử dụng trong view
            return View(data);
        }
        public async Task<(List<User>, int)> getUserAsync(string search, string adress, string name, string roleName, string sort, string sortDir, int skip, int pageSize)
        {
            var v = (from a in db.Users
                     where
                     (a.email.Contains(search) ||
                      a.address.Contains(search) ||
                      a.UserRole.roleName.Contains(search) ||
                      a.bio.Contains(search) ||
                      a.userWallpaper.Contains(search) ||
                      a.userAvatar.Contains(search) ||
                      a.lastname.Contains(search) ||
                      a.firstname.Contains(search) ||
                      a.phoneNumber.Contains(search)) &&
                      (string.IsNullOrEmpty(name) || a.lastname.Contains(name) || a.firstname.Contains(name)) &&
                      (string.IsNullOrEmpty(adress) || a.address.Contains(adress)) &&
                      (string.IsNullOrEmpty(roleName) || a.UserRole.roleName.Contains(roleName))
                     select a);
            //Đếm tổng số lượng bản ghi
            int totalRecord = await v.CountAsync();
            v = v.OrderBy(sort + " " + sortDir);//Download System.Linq.Dynamic để sử dụng OrderBy
            if (pageSize > 0)
            {
                v = v.Skip(skip).Take(pageSize);
            }
            return (v.ToList(), totalRecord);
        }
        #endregion
        #region[Tạo người dùng]
        public ActionResult CreateUser()
        {
            ViewBag.USERID = Utils.instance.getNewGuid();
            //Lấy danh sách Roles để hiển thị trong DropDownList
            ViewBag.Roles = new SelectList(db.UserRoles, "roleId", "roleName");
            ViewBag.CodeActive = Utils.instance.getNewGuid();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(User user, HttpPostedFileBase userAvatar, HttpPostedFileBase userWallpaper)
        {
            var userAvt = SaveUploadedFile(userAvatar, "avatar", Utils.AVATAR_DEFAULT);
            var userWpp = SaveUploadedFile(userWallpaper, "wallpaper", Utils.WALLPAPER_DEFAULT);
            if (ModelState.IsValid)
            {
                try
                {
                    if (Utils.instance.isEmailExist(user.email))
                    {
                        ModelState.AddModelError("EmailExist", "Email đã tồn tại");
                        return View(user);
                    }
                    // Gọi private phương thức để xử lý logic tạo mới người dùng
                    // Thêm logic xử lý mật khẩu, mã hóa mật khẩu trước khi lưu vào database, v.v.
                    user.userId = Utils.instance.getNewGuid();
                    user.password = Utils.Hash(user.password);
                    user.confirmPassword = Utils.Hash(user.confirmPassword);
                    user.codeActive = Utils.instance.getNewGuid();
                    user.userAvatar = userAvt;
                    user.userWallpaper = userWpp;
                    // Thêm logic tạo mới người dùng
                    user.isActive = false; // Hoặc giá trị mặc định tùy thuộc vào yêu cầu của bạn
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Console.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        }
                    }
                }

                return RedirectToAction("ListUser"); // Chuyển hướng đến trang chính sau khi tạo mới
            }
            //Lấy danh sách Roles để hiển thị trong DropDownList
            ViewBag.Roles = new SelectList(db.UserRoles, "roleId", "roleName");
            //ViewBag.UserRoles = db.UserRoles.ToList();
            return View(user);
        }
        #endregion
        #region[Xem chi tiết người dùng]
        [HttpGet]
        public async Task<ActionResult> DetailsUser(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        #endregion
        #region[Xóa người dùng]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tìm người dùng theo id
            var userToDelete = db.Users.Find(id);

            if (userToDelete == null)
            {
                return HttpNotFound();
            }

            try
            {
                // Xóa người dùng
                db.Users.Remove(userToDelete);
                db.SaveChanges();

                return RedirectToAction("ListUser");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                ViewBag.Error = "Không thể xóa người dùng. " + ex.Message;
                return View("Error");
            }
        }
        #endregion
        #region[Sửa thông tin người dùng]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.AVARTAR = user.userAvatar;

            ViewBag.USERID = user.userId;
            ViewBag.CurrentPass = user.password;
            ViewBag.CodeActive = user.codeActive;
            ViewBag.roleId = new SelectList(db.UserRoles, "roleId", "roleName", user.roleId);
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string newPassword, [Bind(Include = "userId, roleId, email, password, firstname, lastname, userAvatar, userWallpaper, address, birthday, codeActive, isActive, phoneNumber, bio")] User user)
        {
            var imgNV = Request.Files["Avatar"];
            var fileName = System.IO.Path.GetFileName(imgNV.FileName);
            try
            {
                if (imgNV != null && imgNV.ContentLength > 0)
                {
                    // Lưu hình đại diện về Server

                    var path = Server.MapPath("/assets/images/users/" + fileName);
                    imgNV.SaveAs(path);

                    // Resize và crop ảnh về kích thước 300x300
                    var settings = new ResizeSettings
                    {
                        Width = 300,
                        Height = 300,
                        Mode = FitMode.Crop,
                        Scale = ScaleMode.Both,
                        Anchor = ContentAlignment.MiddleCenter,
                    };

                    ImageBuilder.Current.Build(path, path, settings);
                }
            }
            catch { }
            if (ModelState.IsValid)
            {
                var currentPass = user.password;
                // Kiểm tra nếu newPassword được nhập và nếu nó khác với mật khẩu cũ
                if (newPassword != currentPass)
                {
                    // Thực hiện các thao tác cần thiết với mật khẩu mới
                    // Ví dụ: Hash mật khẩu mới trước khi lưu vào database
                    user.password = Utils.Hash(newPassword);
                }
                else
                {
                    user.password = currentPass;
                }
                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("ListUser");
            }
            ViewBag.roleId = new SelectList(db.UserRoles, "roleId", "roleName", user.roleId);
            return View(user);
        }
        #endregion
        public ActionResult Error()
        {
            return View();
        }
        [NonAction]
        public string SaveUploadedFile(HttpPostedFileBase file, string subFolder, string fail)
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

            return fail;
        }
    }
}
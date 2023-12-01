using ImageResizer;
using QuanLyPhanMem__63135414.Models;
using QuanLyPhanMem__63135414.Models.Extension;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        private QLPM_63135414Entities db = new QLPM_63135414Entities();
        // GET: Admin63135414
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> AdminHome()
        {
            using (QLPM_63135414Entities db = new QLPM_63135414Entities())
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateUser(UserViewModel model)
        {
            // Kiểm tra tính hợp lệ của model
            if (ModelState.IsValid)
            {
                try
                {
                    var newUser = new User
                    {
                        userId = Utils.getUserId(),
                        roleId = model.roleId,
                        email = model.email,
                        password = Utils.Hash(model.password),
                        firstname = model.firstname,
                        lastname = model.lastname,
                        phoneNumber = model.phoneNumber,
                        address = model.phoneNumber,
                        isActive = false,

                    };
                    db.Users.Add(newUser);
                    await db.SaveChangesAsync();
                    // Redirect đến trang danh sách người dùng hoặc trang chi tiết người dùng mới được tạo
                    return RedirectToAction("ListUser");
                }
                catch
                {
                    return View("Error");
                }
            }
            // Nếu có lỗi, trả về view với model để hiển thị thông báo lỗi
            return View(model);
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
        public ActionResult Error()
        {
            return View();
        }
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
                // Lưu tệp lên máy chủ
                file.SaveAs(filePath);

                return filePath;
            }

            return "avatardefault.png";
        }
    }
}
using ShopPhanMem_63135414.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ImageResizer;
using System.Linq.Dynamic;

namespace ShopPhanMem_63135414.Controllers
{
    public class Category63135414Controller : Controller
    {
        QLPM63135414Entities db = new QLPM63135414Entities();
        #region[Danh sách]
        public async Task<ActionResult> ListCategory(string search = "", int page = 1, string sort = "categoryName", string sortDir = "asc", int pageSize = 10)
        {
            //Logic phân trang khi truy vấn danh sách
            int totalRecord = 0;
            if (page < 1) page = 1;
            int skip = (page * pageSize) - pageSize;

            //Gọi phương thức getUserAsync và nhận kết quả về
            var dataResult = await getCategoryAsync(search, sort, sortDir, skip, pageSize);

            // Trích xuất dữ liệu và số lượng bản ghi từ kết quả
            var data = dataResult.Item1;
            totalRecord = dataResult.Item2;

            ViewBag.TotalRows = totalRecord;
            ViewBag.PageSize = pageSize; // Đưa giá trị pageSize vào ViewBag để sử dụng trong view
            return View(data);
        }
        public async Task<(List<Category>, int)> getCategoryAsync(string search, string sort, string sortDir, int skip, int pageSize)
        {
            var v = from a in db.Categories
                    where
                    (a.id.Contains(search) ||
                    a.categoryName.Contains(search) ||
                    a.categoryImage.Contains(search))
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
        #region[Tạo mới]
        public ActionResult CreateCategory()
        {
            ViewBag.CateId = Utilities.instance.getIdCategory();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCategory([Bind(Include = "id,categoryName,categoryImage")] Category category, HttpPostedFileBase categoryImage)
        {
            var imgCategory = SaveUploadedFile(categoryImage, "category", Utilities.CATEGORY_DEFAULT);
            if (ModelState.IsValid)
            {
                category.id = Utilities.instance.getIdCategory();
                category.categoryImage = imgCategory;
                db.Categories.Add(category);
                await db.SaveChangesAsync();

                return RedirectToAction("ListCategory");
            }
            return View(category);
        }
        #endregion
        #region[Xóa danh mục]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tìm người dùng theo id
            var categoriesToDelete = db.Categories.Find(id);

            if (categoriesToDelete == null)
            {
                return HttpNotFound();
            }

            try
            {
                // Xóa người dùng
                db.Categories.Remove(categoriesToDelete);
                db.SaveChanges();

                return RedirectToAction("ListCategory");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
                ViewBag.Error = "Không thể xóa danh mục. " + ex.Message;
                return View("Error");
            }
        }
        #endregion
        #region[Edit]
        public async Task<ActionResult> EditCategory(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryID = category.id;
            ViewBag.CategoryImg = category.categoryImage;
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCategory([Bind(Include = "id, categoryName, categoryImage")] Category category, HttpPostedFileBase categoryImage)
        {
            var newCategoryImg = SaveUploadedFile(categoryImage, "category", null);
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;

                var currentImg = category.categoryImage;
                if (!string.IsNullOrEmpty(newCategoryImg) && newCategoryImg != currentImg)
                {
                    category.categoryImage = newCategoryImg;
                }
                await db.SaveChangesAsync();
                return RedirectToAction("ListCategory");
            }
            return View(category);
        }
        #endregion
        [NonAction]
        private string SaveUploadedFile(HttpPostedFileBase file, string subFolder, string fail)
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
                    Width = 270,
                    Height = 270,
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
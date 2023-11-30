using QuanLyPhanMem__63135414.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
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
        public async Task<ActionResult> ListUser(int page = 1, string sort = "lastname", string sortDir = "asc", string search = "")
        {
            //Logic phân trang khi truy vấn danh sách
            int pageSize = 5;
            int totalRecord = 0;
            if (page < 1) page = 1;
            int skip = (page * pageSize) - pageSize;

            //Gọi phương thức getUserAsync và nhận kết quả về
            var dataResult = await getUserAsync(search, sort, sortDir, skip, pageSize);

            // Trích xuất dữ liệu và số lượng bản ghi từ kết quả
            var data = dataResult.Item1;
            totalRecord = dataResult.Item2;

            // Thêm điều kiện kiểm tra và thiết lập đường dẫn ảnh
            var uSERS = db.Users.Include(u => u.UserRole);
            ViewBag.TotalRows = totalRecord;
            return View(data);
        }
        public async Task<(List<User>, int)> getUserAsync(string search, string sort, string sortDir, int skip, int pageSize)
        {
            var v = (from a in db.Users
                     where
                     a.firstname.Contains(search) ||
                     a.lastname.Contains(search) ||
                     a.email.Contains(search) ||
                     a.address.Contains(search) ||
                     a.UserRole.roleName.Contains(search)||
                     a.bio.Contains(search)||
                     a.userWallpaper.Contains(search)||
                     a.userAvatar.Contains(search)||
                     a.phoneNumber.Contains(search)
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
    }
}
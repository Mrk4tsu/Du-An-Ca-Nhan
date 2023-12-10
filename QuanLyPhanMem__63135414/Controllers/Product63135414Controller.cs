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
    public class Product63135414Controller : Controller
    {
        QLPM63135414_Entities db = new QLPM63135414_Entities();

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
                    a.User.firstname.Contains(search))||
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
    }
}
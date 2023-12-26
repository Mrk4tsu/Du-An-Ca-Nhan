using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopPhanMem_63135414.Models.Catalog.ProductSystem
{
    public class ProductAndImageViewModel : ProductViewListVM
    {
        public ProductViewModel Product { get; set; }
        public ProductImage ProductImage { get; set; }
        public string SelectedCategoryId { get; set; } // Thêm thuộc tính này cho danh mục được chọn
        public IEnumerable<SelectListItem> Categories { get; set; } // Thêm thuộc tính này để lưu trữ danh sách các danh mục
    }
}
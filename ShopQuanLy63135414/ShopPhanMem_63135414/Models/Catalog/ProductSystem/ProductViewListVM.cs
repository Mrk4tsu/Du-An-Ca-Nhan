using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopPhanMem_63135414.Models.Catalog.ProductSystem
{
    public class ProductViewListVM
    {
        // Constructor để dễ dàng tạo đối tượng từ đối tượng Product
        public string Id { get; set; }
        public string ProductName { get; set; }
        public string ImagePath { get; set; }
        public string CategoryName { get; set; }
        public string CategoryNameFull { get; set; }
        public decimal? Price { get; set; }
        public decimal? OgPrice { get; set; }
        public ProductViewListVM()
        {

        }
        public ProductViewListVM(Product product)
        {
            Id = product.id;
            ProductName = product.productName;
            Price = product.price;
            OgPrice = product.priceOriginal;

            if (product.ProductImages.Any())
            {
                ImagePath = product.ProductImages.First().imagePath;
            }
            else
            {
                ImagePath = null; // hoặc có thể đặt một đường dẫn mặc định
            }

            if (product.ProductInCategories.Any())
            {
                //CategoryName = product.ProductInCategories.First().Category.categoryName;

                //var categoryNames = product.ProductInCategories.Select(pc => pc.Category.categoryName).Take(3).ToList();

                //CategoryName = string.Join(", ", categoryNames);
                var categoryNames = product.ProductInCategories.Select(pc => pc.Category.categoryName).ToList();

                if (categoryNames.Count > 3)
                {
                    categoryNames = categoryNames.Take(3).ToList();
                    categoryNames.Add("..."); // Thêm giá trị "..." vào cuối danh sách
                }

                CategoryName = string.Join(", ", categoryNames);
            }
            else
            {
                CategoryName = null; // hoặc có thể đặt một giá trị mặc định khác
            }
            if (product.ProductInCategories.Any())
            {
                var categoryNames = product.ProductInCategories.Select(pc => pc.Category.categoryName).ToList();
                CategoryNameFull = string.Join(", ", categoryNames);
            }
            else
            {
                CategoryNameFull = null; // hoặc có thể đặt một giá trị mặc định khác
            }
            // Gán các thuộc tính khác cần thiết
        }
    }
}
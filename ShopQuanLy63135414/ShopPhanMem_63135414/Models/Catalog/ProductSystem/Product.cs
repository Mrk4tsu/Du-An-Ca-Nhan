using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShopPhanMem_63135414.Models
{
    [MetadataType(typeof(ProductMetaData))]
    public partial class Product
    {    
    }
    public class ProductMetaData
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Vui lòng không để trống!")]
        public string productName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Vui lòng không để trống!")]
        public string productUrl { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? dateUpload { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? dateUpdate { get; set; }
    }
    public class ProductEqualityComparer : IEqualityComparer<Product>
    {
        public bool Equals(Product x, Product y)
        {
            // So sánh theo id của sản phẩm
            return x.id == y.id;
        }

        public int GetHashCode(Product obj)
        {
            // Lấy hash code của id để tối ưu hiệu suất
            return obj.id.GetHashCode();
        }
    }
}
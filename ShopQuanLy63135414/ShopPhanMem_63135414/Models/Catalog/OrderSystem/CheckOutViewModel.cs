using ShopPhanMem_63135414.Models.Catalog.ProductSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopPhanMem_63135414.Models.Catalog.OrderSystem
{
    public class CheckOutViewModel
    {
        public string OrderId { get; set; }
        public decimal? TotalAmount { get; set; }
    }

}
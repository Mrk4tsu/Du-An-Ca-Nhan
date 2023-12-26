using System;

namespace QuanLyPhanMem__63135414.Models.Catalog.ProductSystem
{
    public class ProductViewModel
    {
        public string id { get; set; }
        public string userId { get; set; }
        public string productName { get; set; }
        public string productUrl { get; set; }
        public decimal? priceOriginal { get; set; }
        public decimal? price { get; set; }
        public DateTime dateUpload { get; set; }
        public DateTime? dateUpdate { get; set; }
        public int? viewCount { get; set; }
        public int? sellCount { get; set; }
        public string description { get; set; }
    }
}
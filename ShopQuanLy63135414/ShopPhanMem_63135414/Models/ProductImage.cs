//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ShopPhanMem_63135414.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductImage
    {
        public string id { get; set; }
        public string productId { get; set; }
        public string imagePath { get; set; }
        public string caption { get; set; }
        public Nullable<bool> imgDefault { get; set; }
        public Nullable<System.DateTime> dateCreate { get; set; }
    
        public virtual Product Product { get; set; }
    }
}

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
    
    public partial class ProductInCategory
    {
        public string categoryId { get; set; }
        public string productId { get; set; }
        public string description { get; set; }
    
        public virtual Category Category { get; set; }
        public virtual Product Product { get; set; }
    }
}

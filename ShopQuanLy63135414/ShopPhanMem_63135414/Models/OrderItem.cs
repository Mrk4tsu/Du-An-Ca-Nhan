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
    
    public partial class OrderItem
    {
        public string OrderItemId { get; set; }
        public string OrderId { get; set; }
        public string ProductId { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> Price { get; set; }
    
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
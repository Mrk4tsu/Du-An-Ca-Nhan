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
    
    public partial class HistoryPayment
    {
        public int Id { get; set; }
        public Nullable<int> PaymentId { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public System.DateTime CreatedAt { get; set; }
    
        public virtual Payment Payment { get; set; }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QuanLyPhanMem_63135414.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Payment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Payment()
        {
            this.HistoryPayments = new HashSet<HistoryPayment>();
        }
    
        public int Id { get; set; }
        public string orderId { get; set; }
        public string paymentMethodId { get; set; }
        public decimal aMount { get; set; }
        public string statusPayment { get; set; }
        public System.DateTime createAt { get; set; }
        public Nullable<System.DateTime> createUpdate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HistoryPayment> HistoryPayments { get; set; }
        public virtual Order Order { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }
    }
}
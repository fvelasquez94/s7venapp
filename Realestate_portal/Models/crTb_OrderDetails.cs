//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Realestate_portal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class crTb_OrderDetails
    {
        public int ID_detail { get; set; }
        public string Resource { get; set; }
        public string Style { get; set; }
        public string Size { get; set; }
        public string Company_name { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Cellphone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Facebook_url { get; set; }
        public int Quantity { get; set; }
        public int GroupQty { get; set; }
        public decimal Price { get; set; }
        public int ID_order { get; set; }
        public string Status { get; set; }
        public Nullable<int> ID_Company { get; set; }
    
        public virtual Sys_Company Sys_Company { get; set; }
    }
}

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
    
    public partial class Tb_ToVerifyTaxes
    {
        public int Id_Document { get; set; }
        public string Doc_Name { get; set; }
        public string Url { get; set; }
        public string Size { get; set; }
        public Nullable<System.DateTime> Upload_Date { get; set; }
        public Nullable<int> Id_User { get; set; }
        public string Extension { get; set; }
        public Nullable<int> ID_Company { get; set; }
    }
}

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
    
    public partial class Tb_Process
    {
        public int ID_Process { get; set; }
        public string Description { get; set; }
        public int ID_User { get; set; }
        public int ID_Customer { get; set; }
        public string ID_Property { get; set; }
        public string Property { get; set; }
        public string Address { get; set; }
        public decimal Purchase_price { get; set; }
        public decimal Commission_amount { get; set; }
        public decimal Commissionperc { get; set; }
        public System.DateTime Closing_date { get; set; }
        public System.DateTime Under_contract_date { get; set; }
        public System.DateTime Offer_accepted_date { get; set; }
        public System.DateTime Inspection_date { get; set; }
        public string Stage { get; set; }
        public string Source { get; set; }
        public string TypeofAgency { get; set; }
        public string Loan_Officer_name { get; set; }
        public string Attorneys_name { get; set; }
        public string Notes { get; set; }
        public System.DateTime Creation_date { get; set; }
        public System.DateTime Last_update { get; set; }
        public string Loan_Officer_tel { get; set; }
        public string Attorneys_tel { get; set; }
    
        public virtual Tb_Customers Tb_Customers { get; set; }
    }
}

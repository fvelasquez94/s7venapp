//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Realestate_portal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Sys_Users
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Sys_Users()
        {
            this.Sys_Notifications = new HashSet<Sys_Notifications>();
            this.Tb_Data = new HashSet<Tb_Data>();
            this.Tb_Reminders = new HashSet<Tb_Reminders>();
            this.Tb_Notes = new HashSet<Tb_Notes>();
            this.Tb_DocuAgent = new HashSet<Tb_DocuAgent>();
        }
    
        public int ID_User { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public System.DateTime Birth { get; set; }
        public System.DateTime Creation_date { get; set; }
        public System.DateTime Last_update { get; set; }
        public System.DateTime Last_login { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public string Main_telephone { get; set; }
        public string Secundary_telephone { get; set; }
        public string Fb_url { get; set; }
        public string Ins_url { get; set; }
        public string Tw_url { get; set; }
        public string Other_url { get; set; }
        public string Image { get; set; }
        public int ID_Company { get; set; }
        public int Status { get; set; }
        public bool Active { get; set; }
        public bool Email_active { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string Roles { get; set; }
        public string Brokerage_name { get; set; }
        public string Brokerage_address { get; set; }
        public string Broker_Contact { get; set; }
        public string Broker_License { get; set; }
        public string My_License { get; set; }
        public System.DateTime Member_since { get; set; }
        public string Bank { get; set; }
        public string Bank_number { get; set; }
        public string Bank_typeaccount { get; set; }
        public string Credit_number { get; set; }
        public string Credit_name { get; set; }
        public string Credit_classification { get; set; }
        public string Credit_month { get; set; }
        public string Credit_year { get; set; }
    
        public virtual Sys_Company Sys_Company { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Sys_Notifications> Sys_Notifications { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tb_Data> Tb_Data { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tb_Reminders> Tb_Reminders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tb_Notes> Tb_Notes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tb_DocuAgent> Tb_DocuAgent { get; set; }
    }
}

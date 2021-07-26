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
    
    public partial class Tb_Customers
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tb_Customers()
        {
            this.Tb_Process = new HashSet<Tb_Process>();
            this.Tb_LeadDocs = new HashSet<Tb_LeadDocs>();
            this.Tb_Customers_Users = new HashSet<Tb_Customers_Users>();
        }
    
        public int ID_Customer { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public System.DateTime Birthday { get; set; }
        public string Marital_status { get; set; }
        public string Type { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public string Zipcode { get; set; }
        public bool Lead { get; set; }
        public string User_assigned { get; set; }
        public bool Active { get; set; }
        public int ID_Company { get; set; }
        public System.DateTime Creation_date { get; set; }
        public string Source { get; set; }
    
        public virtual Sys_Company Sys_Company { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tb_Process> Tb_Process { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tb_LeadDocs> Tb_LeadDocs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tb_Customers_Users> Tb_Customers_Users { get; set; }
    }
}

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
    
    public partial class Tb_Conversation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tb_Conversation()
        {
            this.Tb_Message = new HashSet<Tb_Message>();
        }
    
        public int ID_Conversation { get; set; }
        public int ID_Sender { get; set; }
        public int ID_Receptor { get; set; }
        public System.DateTime Created_at { get; set; }
        public int ID_Company { get; set; }
    
        public virtual Sys_Company Sys_Company { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tb_Message> Tb_Message { get; set; }
    }
}

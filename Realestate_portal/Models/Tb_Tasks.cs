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
    
    public partial class Tb_Tasks
    {
        public int ID_task { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Finished { get; set; }
        public System.DateTime Createdat { get; set; }
        public System.DateTime Lastupdate { get; set; }
        public int ID_User { get; set; }
        public string Username { get; set; }
        public int ID_Company { get; set; }
    }
}

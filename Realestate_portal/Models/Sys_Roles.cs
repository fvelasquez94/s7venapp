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
    
    public partial class Sys_Roles
    {
        public int ID_Rol { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ID_Company { get; set; }
    
        public virtual Sys_Company Sys_Company { get; set; }
    }
}

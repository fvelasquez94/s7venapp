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
    
    public partial class Template_categories
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Template_categories()
        {
            this.Saved_Templates = new HashSet<Saved_Templates>();
            this.template_subcategories = new HashSet<template_subcategories>();
            this.Template_Details = new HashSet<Template_Details>();
        }
    
        public int id { get; set; }
        public string category { get; set; }
        public string path { get; set; }
        public Nullable<bool> childs { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Saved_Templates> Saved_Templates { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<template_subcategories> template_subcategories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Template_Details> Template_Details { get; set; }
    }
}

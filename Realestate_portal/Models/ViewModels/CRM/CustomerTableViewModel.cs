using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels.CRM
{
    public class CustomerTableViewModel
    {
        public virtual Sys_Company Sys_Company { get; set; }
        public int Id { get; set; }
        public int ID_Company { get; set; }
        public int ID_User { get; set; }
        public string Name { get; set; }
        public string Marital_status { get; set; }
        public string Type { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public bool Lead { get; set; }
        public string User_assigned { get; set; }
        public System.DateTime Creation_date { get; set; }
        public string DateString { get; set; }

    }
}
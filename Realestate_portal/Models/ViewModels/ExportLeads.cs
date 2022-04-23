using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels
{
    public class ExportLeads
    {
        public int ID_Customer { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string State { get; set; }

        public string Zipcode { get; set; }
        public string Stage { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
   
        public string Active { get; set; }
        public int Broker { get; set; }
        public System.DateTime Creation_date { get; set; }
 
    }
    public class ExportLeadsTemplate
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }

    }
}
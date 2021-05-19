using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels.CRM
{
    public  class CustomerViewModel
    {

        public  List<CustomerTableViewModel> customers { get; set; }

        public  List<string> initials { get; set; }
    }
}
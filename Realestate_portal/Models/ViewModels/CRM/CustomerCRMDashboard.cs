using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels.CRM
{
    public class CustomerCRMDashboard
    {
        public List<Tb_Notes> notes { get; set; }
        public List<Tb_Process> properties { get; set; }
        public Tb_Notes note { get; set; }
        public Tb_Process property { get; set; }
        public Tb_Customers customer { get; set; }
        public Tb_Docpackages package { get; set; }
        public List<Tb_Docpackages_details> pack_Det { get; set; }


    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels
{
    public class ReviewView
    {
        public int Id_Review { get; set; }
        public string Comment { get; set; }
        public int Id_Network { get; set; }
        public DateTime Date_Review { get; set; }
        public string Date { get; set; }
        public int Review_Rate { get; set; }

        public string User { get; set; }
    }
}
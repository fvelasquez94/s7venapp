using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels
{
    public class TasksView
    {
        public int ID_task { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Finished { get; set; }
        public System.DateTime Lastupdate { get; set; }
        public int ID_User { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Url_image { get; set; }
        public int ID_Company { get; set; }
        public string Customer { get; set; }
    }
}
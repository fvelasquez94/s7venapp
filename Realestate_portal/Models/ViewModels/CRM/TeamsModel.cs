using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels.CRM
{
    public class TeamsModel
    {
        public int ID_team { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ID_Company { get; set; }
        public bool Active { get; set; }
        public System.DateTime Creation_date { get; set; }
        public System.DateTime Last_update { get; set; }
        public List<TeamsModel_Users> Users { get; set; }
    }
    public class TeamsModel_Users
    {
        public int Id_User { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Url_image { get; set; }
  
    }
}
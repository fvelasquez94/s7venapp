using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels
{
    public class AgentsView
    {
        public int ID_User { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public System.DateTime Last_login { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public string Main_telephone { get; set; }
        public string Secundary_telephone { get; set; }
        public string Image { get; set; }
        public int ID_Company { get; set; }
        public bool Active { get; set; }
        public bool Team_Leader { get; set; }
        public string Position { get; set; }
        public string Brokerage_name { get; set; }
        public string My_License { get; set; }
        public System.DateTime Member_since { get; set; }

        public List<LeadsAgents> Leads { get; set; }
        public List<TeamsAgents> Teams { get; set; }

    }
    public class LeadsAgents
    {
        public int ID_lead { get; set; }
        public string Name { get; set; }
    }
    public class TeamsAgents
    {
        public int id_team { get; set; }
        public string Name { get; set; }
    }

    public class US_State
    {

        public US_State()
        {
            Name = null;
            Abbreviations = null;
        }

        public US_State(string ab, string name)
        {
            Name = name;
            Abbreviations = ab;
        }

        public string Name { get; set; }

        public string Abbreviations { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Abbreviations, Name);
        }

    }
}
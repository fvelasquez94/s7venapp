using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels.Market
{
    public class Template_Type
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public string Company { get; set; }

        public string Path { get; set; }

        public int TypeId { get; set; }

        public int CompanyId { get; set; }


    }
}
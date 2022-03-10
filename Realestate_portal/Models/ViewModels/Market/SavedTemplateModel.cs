using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels.Market
{
    public class SavedTemplateModel
    {
        public int Id { get; set; }
        public string Templateid { get; set; }

        public DateTime CreationDate { get; set; }

        public string Material { get; set; }

        public string size { get; set; }

        public string Holes { get; set; }

        public string Category { get; set; }

        public int Amount { get; set; }

        public decimal? Price { get; set; }

        public string Location { get; set; }

        public bool Laminate  { get; set; }

        public bool Clearcoat { get; set; }

        public int? Template_Type { get; set; }

        public int? dimension { get; set; }              

    }
}
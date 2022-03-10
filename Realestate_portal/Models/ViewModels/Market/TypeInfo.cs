using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels.Market
{
    public class TypeInfo
    {

        public List<decimal> Prices { get; set; }

        public List<int> Quantity { get; set; }

        public string Description { get; set; }

        public int? dimension { get; set; }
    }
}
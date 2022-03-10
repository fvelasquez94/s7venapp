using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels.Market
{
    public class AmountsByType
    {     
        public int[] amounts { get; set; }

        public int? type { get; set; }
    }
}
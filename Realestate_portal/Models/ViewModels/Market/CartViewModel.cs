using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels.Market
{
    public class CartViewModel
    {

        public List<SavedTemplateModel>  Templates { get; set; }

        public List<saved_labels> Labels { get; set; }

        public List<AmountsByType> Amounts { get; set; }
    }
}
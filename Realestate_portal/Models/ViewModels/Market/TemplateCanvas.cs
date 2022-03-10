using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels.Market
{
    public class TemplateCanvas
    {
        public Template_Details Details { get; set; }

        public labels Header { get; set; }

        public labels Company { get; set; }

        public labels Agent { get; set; }

        public labels Phone { get; set; }

        public labels Photo { get; set; }

        public labels Logo { get; set; }

        public labels Website { get; set; }

        public labels Title { get; set; }

        public labels Email { get; set; }

        public labels Other { get; set; }

        public labels Adress_1 { get; set; }

        public labels Adress_2 { get; set; }

        public labels Message { get; set; }

        public labels Facebook { get; set; }

        public labels Instagram { get; set; }
    }
}
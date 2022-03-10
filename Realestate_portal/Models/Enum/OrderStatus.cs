using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.Enum
{

    public class OrderStatus
{
        public static  string  pending { get { return "pending"; } }
        public static string succeded { get { return "succeded"; } }

        public static string confirmed { get { return "confirmed"; } }

        public static string completed { get { return "completed"; } }

          public static string cancel { get { return "cancel"; } }

       
}
}
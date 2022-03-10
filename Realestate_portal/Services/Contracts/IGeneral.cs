using Realestate_portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Services.Contracts
{
    public interface IGeneral
    {
        bool checkSession();

        Sys_Users GetUser();
    }
}
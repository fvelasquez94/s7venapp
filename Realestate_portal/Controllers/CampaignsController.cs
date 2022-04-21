using Realestate_portal.Models;
using Realestate_portal.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Realestate_portal.Controllers
{
    public class CampaignsController : Controller
    {
        // GET: Campaigns
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();

        public ActionResult CampaignsSMS(int broker = 0, string token = "")
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                ViewBag.notifications = lstAlerts; 
                //HEADER DATA
                ViewBag.activeuser = activeuser;
                ViewBag.userCompany = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();
                ViewBag.token = token;
                //ROLES
                //FIN HEADER

                ViewBag.rol = "";


                List<Sys_Users> agents = new List<Sys_Users>();
                agents = (from a in db.Sys_Users where (a.Active && a.Main_telephone != "" && a.Roles.Contains("Agent")) select a).ToList();

                if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";
                }
                else if (activeuser.Roles.Contains("Super User"))
                {
                    ViewBag.rol = "Super User";

                }
                else
                {
                    ViewBag.rol = "SA";
                }


                

                ViewBag.agents = agents;
                ViewBag.selbroker = broker;

                return View();
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }
    }
}
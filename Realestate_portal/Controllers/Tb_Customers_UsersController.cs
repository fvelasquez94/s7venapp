using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Postal;
using Realestate_portal.Models;
using Realestate_portal.Models.ViewModels.CRM;


namespace Realestate_portal.Controllers
{
    public class Tb_Customers_UsersController : Controller
    {

        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();
        private Cls_GoogleCalendar cls_GoogleCalendar = new Cls_GoogleCalendar();
        // GET: Tb_Customers_Users
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AssignList(int id, int broker) {

            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Dashboard";
                ViewBag.menunameid = "";
                ViewBag.submenunameid = "";
                List<string> s = new List<string>(activeuser.Department.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                ViewBag.notifications = lstAlerts;
                ViewBag.userID = activeuser.ID_User;
                ViewBag.userName = activeuser.Name + " " + activeuser.LastName;
                ViewBag.customer = id;
                ViewBag.rol = "";

                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.userslist = (from u in db.Sys_Users where (u.Sys_Company.ID_Company == activeuser.ID_Company && u.ID_User == activeuser.ID_User && u.Active == true) orderby u.LastName ascending select u).ToList();
                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        RedirectToAction("Dashboard", "Portal", new { broker = brokersel.ID_Company });
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {
                        }
                        else
                        {
                            ViewBag.rol = "SA";

                        }
                    }

                    ViewBag.userslist = (from u in db.Sys_Users where (u.Sys_Company.ID_Company == activeuser.ID_Company && (u.Roles == "Agent" || u.Roles == "Admin") && u.Active == true) orderby u.LastName ascending select u).ToList();

                }
                ViewBag.selbroker = broker;
                var propertiesprojectedgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "UNDER CONTRACT") select f).ToList();
                var propertiesgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "CLOSED") select f).ToList();
                var totalproperties = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User) select f).Count();

                decimal totalprojectedgains = 0;
                decimal totalgains = 0;
                if (propertiesprojectedgains.Count > 0) { totalprojectedgains = propertiesprojectedgains.Select(c => c.Commission_amount).Sum(); }
                if (propertiesgains.Count > 0) { totalgains = propertiesgains.Select(c => c.Commission_amount).Sum(); }

                ViewBag.totalcustomers = totalproperties;
                ViewBag.totalgainsprojected = totalprojectedgains;
                ViewBag.totalgains = totalgains;


                ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name");
            

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }


            return View();
        
        }

        [HttpPost]
        public ActionResult Create(int idCustomer, int[] agentsId)
        {
            var result = "";
            try
            {
                Tb_Customers_Users customerUsers = new Tb_Customers_Users();

                foreach (var item in agentsId)
                {
                    customerUsers.Id_Customer = idCustomer;
                    customerUsers.Id_User = item;
                    db.Tb_Customers_Users.Add(customerUsers);
                    db.SaveChanges();
                }
                result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                result = "ERROR";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            
            //return RedirectToAction("CustomerDashboard", "CRM", new { id = idCustomer, broker = 0 });
        }
    }
}
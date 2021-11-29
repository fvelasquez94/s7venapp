using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Realestate_portal.Models;

namespace Realestate_portal.Controllers
{
    public class Tb_OptionsController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();
        // GET: Tb_Options
        public ActionResult Index(int broker=0, string token="")
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
                ViewBag.company = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();
                ViewBag.token = token;
                //FIN HEADER
                List<Tb_Options> lstCategoria = new List<Tb_Options>();
                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    if (activeuser.Roles.Contains("SA"))
                    {
                        ViewBag.rol = "SA";

                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                    }
                }
                ViewBag.selbroker = broker;

                lstCategoria = (from a in db.Tb_Options where (a.ID_Company == activeuser.ID_Company || a.ID_Company==1) select a).ToList();

                return View(lstCategoria);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
         
        }

        // GET: Tb_Options/Details/5
        public ActionResult Details(int? id)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Options";
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
                //FIN HEADER
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    ViewBag.rol = "Admin";


                }

                Tb_Options tb_Options = db.Tb_Options.Find(id);
                return View(tb_Options);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }


        }

        // GET: Tb_Options/Create
        public ActionResult Create()
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
                ViewBag.company = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();
         
                //FIN HEADER
                //FIN HEADER
                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    ViewBag.rol = "Admin";


                }

  
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

        }

        // POST: Tb_Options/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_option,Description,Type,Active,ID_Company")] Tb_Options tb_Options)
        {
            tb_Options.Active = true;
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            tb_Options.ID_Company = activeuser.ID_Company;
            if (ModelState.IsValid)
            {
                db.Tb_Options.Add(tb_Options);
                db.SaveChanges();
                return RedirectToAction("Index", "Tb_Options", new { token = "success" });
            }
            else {
                return RedirectToAction("Index", "Tb_Options", new { token = "error" });
            }
        }

        // GET: Tb_Options/Edit/5
        public ActionResult Edit(int? id)
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
                ViewBag.company = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();
              
                //FIN HEADER
                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    ViewBag.rol = "Admin";


                }

                Tb_Options tb_Options = db.Tb_Options.Find(id);
                return View(tb_Options);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

        // POST: Tb_Options/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_option,Description,Type,Active,ID_Company")] Tb_Options tb_Options)
        {
            tb_Options.Active = true;

            if (ModelState.IsValid)
            {
                db.Entry(tb_Options).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tb_Options);
        }

        // GET: Tb_Options/Delete/5
        public ActionResult Delete(int? id)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Options";
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
                //FIN HEADER
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    ViewBag.rol = "Admin";


                }

                Tb_Options tb_Options = db.Tb_Options.Find(id);

                return View(tb_Options);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

        }

        // POST: Tb_Options/Delete/5
    
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Tb_Options tb_Options = db.Tb_Options.Find(id);
                db.Tb_Options.Remove(tb_Options);
                db.SaveChanges();

                var result = "Success";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

         
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

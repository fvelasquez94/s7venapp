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
    public class BookingController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();

        // GET: Booking
        public ActionResult Index()
        {
            var tb_Webinars = db.Tb_Webinars.Include(t => t.Sys_Company);
            return View(tb_Webinars.ToList());
        }

        // GET: Booking/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Webinars tb_Webinars = db.Tb_Webinars.Find(id);
            if (tb_Webinars == null)
            {
                return HttpNotFound();
            }
            return View(tb_Webinars);
        }

        // GET: Booking/Create
        public ActionResult Create()
        {
            ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name");
            return View();
        }

        // POST: Booking/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_Webinar,Date,Description,Title,Max_users,Url,Active,Status,Notes,Image,ID_Company,Category,Url_video")] Tb_Webinars tb_Webinars)
        {
            if (ModelState.IsValid)
            {
                db.Tb_Webinars.Add(tb_Webinars);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name", tb_Webinars.ID_Company);
            return View(tb_Webinars);
        }

        // GET: Booking/Edit/5
        public ActionResult Edit(int? id, int broker=1)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Booking";
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

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;


         
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                   
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


                }
                var sunday = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var saturday = sunday.AddMonths(1).AddDays(-1);
                //WEBINAR TYPE:

                ViewBag.selbroker = broker;
                Tb_Webinars tb_Webinars = db.Tb_Webinars.Find(id);

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

                return View(tb_Webinars);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

           
        }

        // POST: Booking/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_Webinar,Date,Description,Title,Max_users,Url,Active,Status,Notes,Image,ID_Company,Category,Url_video,Date_end")] Tb_Webinars tb_Webinars)
        {
            if (tb_Webinars.Description == null) { tb_Webinars.Description = ""; }
            if (tb_Webinars.Url == null) { tb_Webinars.Url = ""; }


            tb_Webinars.Url_video = "";
            tb_Webinars.Notes = "";
            tb_Webinars.Max_users = 0;
            tb_Webinars.Status = "0";
            tb_Webinars.Image = "";
            tb_Webinars.Active = 1;
            tb_Webinars.Category = "Booking";
            try
            {
                db.Entry(tb_Webinars).State = EntityState.Modified;
                db.SaveChanges();

                TempData["exito"] = "Booking edited successfully.";
                return RedirectToAction("Reservations_management", "Portal", new { broker = 1 });
            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("Reservations_management", "Portal", new { broker = 1 });
            }

        }

        // GET: Booking/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Webinars tb_Webinars = db.Tb_Webinars.Find(id);
            if (tb_Webinars == null)
            {
                return HttpNotFound();
            }
            return View(tb_Webinars);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tb_Webinars tb_Webinars = db.Tb_Webinars.Find(id);
            db.Tb_Webinars.Remove(tb_Webinars);
            db.SaveChanges();
            return RedirectToAction("Index");
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

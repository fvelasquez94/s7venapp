﻿using System;
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
    public class Tb_SourceController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();
        // GET: Tb_Source
        public ActionResult Index(int broker = 0)
        {
            try
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
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_User == activeuser.ID_User) select usd).FirstOrDefault();

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
                                ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_User == activeuser.ID_User) select usd).FirstOrDefault();

                            }
                            else
                            {

                                ViewBag.rol = "SA";

                                ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == broker && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                                var brokersel = (from b in db.Sys_Users where (b.ID_Company == broker && b.Roles.Contains("Admin")) select b).FirstOrDefault();

                            }
                        }



                    }
                    ViewBag.selbroker = broker;
                    return View(db.Tb_Source.ToList());
                }
                else
                {

                    return RedirectToAction("Login", "Portal", new { access = false });

                }
            }
            catch (Exception)
            {

                return RedirectToAction("Login", "Portal", new { access = false });
            }
            
        }

        // GET: Tb_Source/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Source tb_Source = db.Tb_Source.Find(id);
            if (tb_Source == null)
            {
                return HttpNotFound();
            }
            return View(tb_Source);
        }

        // GET: Tb_Source/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tb_Source/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id_Source,Source_name,Id_Company")] Tb_Source tb_Source)
        {
            if (ModelState.IsValid)
            {
                db.Tb_Source.Add(tb_Source);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tb_Source);
        }
        [HttpPost]
        public ActionResult CreateSource(string source) {
            try
            {
                Sys_Users activeUser = Session["activeUser"] as Sys_Users;
                Tb_Source newSource = new Tb_Source();
                if (activeUser.Roles == "Admin")
                {
                    newSource.Source_name = source;
                    newSource.Id_Company = activeUser.ID_Company;
                }
                else
                {
                    newSource.Source_name = source;
                 
                }
                db.Tb_Source.Add(newSource);
                db.SaveChanges();
                var result = "SUCCESS";
                return Json(result);
            }
            catch (Exception ex)
            {

                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            
        }

        // GET: Tb_Source/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Source tb_Source = db.Tb_Source.Find(id);
            if (tb_Source == null)
            {
                return HttpNotFound();
            }
            return View(tb_Source);
        }

        // POST: Tb_Source/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_Source,Source_name,Id_Company")] Tb_Source tb_Source)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_Source).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tb_Source);
        }

        // GET: Tb_Source/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Source tb_Source = db.Tb_Source.Find(id);
            if (tb_Source == null)
            {
                return HttpNotFound();
            }
            return View(tb_Source);
        }

        // POST: Tb_Source/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tb_Source tb_Source = db.Tb_Source.Find(id);
            db.Tb_Source.Remove(tb_Source);
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Postal;
using Realestate_portal.Models;

namespace Realestate_portal.Controllers
{
    public class Sys_CompanyController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();

        // GET: Sys_Company
        public ActionResult Index(int broker=0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Sys_Company";
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

                ViewBag.rol = "SA";
                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;
                //
                List<Sys_Company> lstBrokers = new List<Sys_Company>();
                lstBrokers = db.Sys_Company.ToList();


              
                return View(lstBrokers);

            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        // GET: Sys_Company/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sys_Company sys_Company = db.Sys_Company.Find(id);
            if (sys_Company == null)
            {
                return HttpNotFound();
            }
            return View(sys_Company);
        }

        // GET: Sys_Company/Create
        public ActionResult Create()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Sys_Company";
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

                ViewBag.rol = "";


                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    ViewBag.rol = "Admin";


                }
                //FIN HEADER

                ViewBag.ID_User = new SelectList(db.Sys_Users, "ID_User", "Name");
                return View();

            }else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        // POST: Sys_Company/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create([Bind(Include = "ID_Company,Name,Web,ShortName")] Sys_Company sys_Company, HttpPostedFileBase logo, string email, string information)
        {
            sys_Company.Logo = "";
            if (sys_Company.Web == null) { sys_Company.Web = ""; }
            if (sys_Company.ShortName == null) { sys_Company.ShortName = ""; }
            if (ModelState.IsValid)
            {
                

                try
                {

                    if (sys_Company.Logo == null)
                    {
                        sys_Company.Logo = "";
                    }
                    else
                    {
                        string fName = Path.GetFileName(logo.FileName);
                        // string fName = Path.GetFileName(imagen.FileName);
                        string fPath = Path.Combine(Server.MapPath("~/Content/Uploads/Images/logos/"), fName);


                        logo.SaveAs(fPath);


                        sys_Company.Logo = "~/Content/Uploads/Images/logos/" + logo.FileName;
                    }
                   
                }
                catch
                {

                }

                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
               
                db.Sys_Company.Add(sys_Company);
                db.SaveChanges();


                //Crear usuari broker para nueva Sys_company
                Sys_Users nuevoUsuario = new Sys_Users();

                nuevoUsuario.Name = sys_Company.Name;
                nuevoUsuario.LastName = "";
                nuevoUsuario.Gender = "";
                nuevoUsuario.Email = email;
                nuevoUsuario.Password = "Admin@2020";
                nuevoUsuario.Birth = DateTime.Today;
                nuevoUsuario.Creation_date = DateTime.Today;
                nuevoUsuario.Last_update = DateTime.Today;
                nuevoUsuario.Last_login = DateTime.Today;
                nuevoUsuario.State = "";
                nuevoUsuario.Address = "";
                nuevoUsuario.Main_telephone = "";
                nuevoUsuario.Secundary_telephone = "";
                nuevoUsuario.Fb_url = "";
                nuevoUsuario.Ins_url = "";
                nuevoUsuario.Tw_url = "";
                nuevoUsuario.Other_url = "";
                nuevoUsuario.Image = sys_Company.Logo;
                nuevoUsuario.ID_Company = sys_Company.ID_Company;
                nuevoUsuario.Status = 1;
                nuevoUsuario.Active = true;
                nuevoUsuario.Email_active = true;
                nuevoUsuario.Position = "Administrador";
                nuevoUsuario.Department = "";
                nuevoUsuario.Roles = "Admin";
                nuevoUsuario.Brokerage_name = "";
                nuevoUsuario.Brokerage_address = "";
                nuevoUsuario.Broker_Contact = "";
                nuevoUsuario.Broker_License = "";
                nuevoUsuario.My_License = "";
                nuevoUsuario.Member_since = DateTime.Today;
                nuevoUsuario.Bank = "";
                nuevoUsuario.Bank_number = "";
                nuevoUsuario.Bank_typeaccount = "";
                nuevoUsuario.Credit_number = "";
                nuevoUsuario.Credit_name = "";
                nuevoUsuario.Credit_classification = "";
                nuevoUsuario.Credit_month = "";
                nuevoUsuario.Credit_year = "";
                nuevoUsuario.Team_Leader = false;
                nuevoUsuario.Id_Leader = 0;
                db.Sys_Users.Add(nuevoUsuario); 
                db.SaveChanges();


                try
                {
                    var brokeremail = activeuser.Sys_Company.Web;

                    if (brokeremail != null && brokeremail != "")
                    {
                        //Enviamos correo para notificar
                        dynamic emailtosend = new Email("newBroker");
                        emailtosend.To = email;
                        emailtosend.From = "customercare@premiumgrealty.com";
                        emailtosend.correo = nuevoUsuario.Email;
                        emailtosend.contrasena = nuevoUsuario.Password;
                        emailtosend.Send();
                 
                    }
                    else
                    {
                 
                    }

                }
                catch (Exception ex)
                {
                    
                }

                //try {
                //    if (information == "Yes") {
                //        var videos = (from a in db.Tb_Videos where (a.ID_Company == 1) select a).ToList();
                //        videos.Select(c => { c.ID_Company = sys_Company.ID_Company; return c; }).ToList();
                //        db.Tb_Videos.AddRange(videos);
                //        db.SaveChanges();


                //        var marketing = (from a in db.Tb_Marketing where (a.ID_Company == 1) select a).ToList();
                //        marketing.Select(c => { c.ID_Company = sys_Company.ID_Company; return c; }).ToList();
                //        db.Tb_Marketing.AddRange(marketing);
                //        db.SaveChanges();

                //        var network = (from a in db.Tb_Network where (a.ID_Company == 1) select a).ToList();
                //        network.Select(c => { c.ID_Company = sys_Company.ID_Company; return c; }).ToList();
                //        db.Tb_Network.AddRange(network);
                //        db.SaveChanges();
                //    }
                //}
                //catch (Exception ex)
                //{


                //}


                return RedirectToAction("Index", "Sys_Company");
            }

            return View(sys_Company);
        }

        // GET: Sys_Company/Edit/5
        public ActionResult Edit(int? id)
        {

            if (generalClass.checkSession())
            {

                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Posts";
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

                ViewBag.rol = "";


                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    ViewBag.rol = "Admin";


                }
                //FIN HEADER

                List<Sys_Users> lstuseradmin = new List<Sys_Users>();
                lstuseradmin = (from c in db.Sys_Users where (c.ID_Company == id && c.Roles.Contains("Admin")) select c).ToList();
                ViewBag.users = lstuseradmin;
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Sys_Company sys_Company = db.Sys_Company.Find(id);

                if (sys_Company == null)
                {
                    return HttpNotFound();
                }
                return View(sys_Company);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }

                
        }

        // POST: Sys_Company/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_Company,Name,Web,ShortName")] Sys_Company sys_Company, HttpPostedFileBase logo)
        {

            sys_Company.Logo = "";
            if (ModelState.IsValid)
            {
               

                try
                {
                    if (sys_Company.Logo == null)
                    {
                        sys_Company.Logo = "";
                    }
                    else
                    {
                        string fName = Path.GetFileName(logo.FileName);
                        // string fName = Path.GetFileName(imagen.FileName);
                        string fPath = Path.Combine(Server.MapPath("~/Content/Uploads/Images/logos/"), fName);
                        logo.SaveAs(fPath);

                        sys_Company.Logo = "~/Content/Uploads/Images/logos/" + logo.FileName;
                    }
                    
                }
                catch
                {

                }

                db.Entry(sys_Company).State = EntityState.Modified;
               
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sys_Company);
        }

        // GET: Sys_Company/Delete/5
        public ActionResult Delete(int? id)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Posts";
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

                ViewBag.rol = "";


                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    ViewBag.rol = "Admin";


                }
                //FIN HEADER


                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Sys_Company sys_Company = db.Sys_Company.Find(id);
                if (sys_Company == null)
                {
                    return HttpNotFound();
                }
                return View(sys_Company);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        // POST: Sys_Company/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Sys_Company sys_Company = db.Sys_Company.Find(id);
            db.Sys_Company.Remove(sys_Company);
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

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
using Realestate_portal.Services.Contracts;

namespace Realestate_portal.Controllers
{
    public class Sys_CompanyController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();

        private Imarket repo;

        public Sys_CompanyController(Imarket _repo)
        {
            repo = _repo;
        }

        public Sys_CompanyController()
        {
        }


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
                ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
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
                ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
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


        [HttpPost]
        public ActionResult changeCompanyImg(int id)
        {
            var path = "";
            var fileName = "";


            try
            {

                if (Request.Files.Count > 0)
                {

                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];

                        fileName = Path.GetFileName(file.FileName);

                        path = Path.Combine(Server.MapPath("~/Content/Uploads/Images/profiles/"), fileName);
                        file.SaveAs(path);
                    }

                    var company = (from a in db.Sys_Company where (a.ID_Company == id) select a).FirstOrDefault();
                    company.Logo = "~/Content/Uploads/Images/profiles/" + fileName;
                
                    db.Entry(company).State = EntityState.Modified;
                    db.SaveChanges();

              


                    var result = "SUCCESS";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {


                    var result = "No image";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        // POST: Sys_Company/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create([Bind(Include = "ID_Company,Name,Web,ShortName")] Sys_Company sys_Company, HttpPostedFileBase logo, string email, string information)
        {
            
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
                nuevoUsuario.Leader_Name = "";
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
                ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
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
                ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
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
   
        public ActionResult DeleteConfirmed(int id)
        {
     


            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                
                Sys_Company sys_Company = db.Sys_Company.Find(id);

                if ( id!=1)
                {
                    int id_company = sys_Company.ID_Company;
              


                    var customer = db.Tb_Customers.Where(c => c.ID_Company == id_company).ToList();
                    var customerarr = db.Tb_Customers.Where(c => c.ID_Company == id_company).Select(c => c.ID_Customer).ToArray();
                    //Eliminar asignacion de agentes a clientes
                    var agentsteams = db.Tb_Customers_Users.Where(c => customerarr.Contains(c.Id_Customer)).ToList();
                    if (agentsteams.Count > 0)
                    {
                        db.Tb_Customers_Users.RemoveRange(agentsteams);
                        db.SaveChanges();
                    }
                    //eliminar equipos
                    var teams = db.Tb_WorkTeams.Where(c => c.ID_Company == id_company).ToList();
                    if (teams.Count > 0)
                    {
                        db.Tb_WorkTeams.RemoveRange(teams);
                        db.SaveChanges();
                    }
                    //Eliminar tareas
                    var task = db.Tb_Tasks.Where(c => c.ID_Company == id_company).ToList();
                    if (task.Count > 0)
                    {
                        db.Tb_Tasks.RemoveRange(task);
                        db.SaveChanges();
                    }
                    //eliminar notas
                    var notas = db.Tb_Notes.Where(c => customerarr.Contains(c.ID_Customer)).ToList();
                    if (notas.Count > 0)
                    {
                        db.Tb_Notes.RemoveRange(notas);
                        db.SaveChanges();
                    }
                    //eliminar documentos
                    var docs = db.Tb_Docpackages.Where(c => c.ID_Company == id_company).ToList();
                    var docsarr = db.Tb_Docpackages.Where(c => c.ID_Company == id_company).Select(c => c.ID_docpackage).ToArray();
                    if (docs.Count > 0)
                    {
                        var details = db.Tb_Docpackages_details.Where(c => docsarr.Contains(c.ID_docpackage)).ToList();
                        if (details.Count > 0)
                        {
                            db.Tb_Docpackages_details.RemoveRange(details);
                            db.SaveChanges();
                        }
                        db.Tb_Docpackages.RemoveRange(docs);
                        db.SaveChanges();
                    }

                    //eliminar propiedades
                    var properties = db.Tb_Process.Where(c => customerarr.Contains(c.ID_Customer)).ToList();
                    if (properties.Count > 0)
                    {
                        db.Tb_Process.RemoveRange(properties);
                        db.SaveChanges();
                    }
                    //eliminar agentes
                    var agents = db.Sys_Users.Where(c => c.ID_Company == id_company).ToList();
                    var agentsarr = db.Sys_Users.Where(c => c.ID_Company == id_company).Select(c => c.ID_User).ToList();

                    var shipping = db.Billing_Shipping_details.Where(s => s.userid == id).ToList();
                    db.Billing_Shipping_details.RemoveRange(shipping);
                    db.SaveChanges();


                    if (agents.Count > 0)
                    {
                        //eliminamos documentos de clientes
                        var docsagent = db.Tb_DocuAgent.Where(c => agentsarr.Contains((int)c.Id_User)).ToList();
                        if (docsagent.Count > 0)
                        {
                            db.Tb_DocuAgent.RemoveRange(docsagent);
                            db.SaveChanges();
                        }

                        agents.ForEach(us => {
                            var shipp = db.Billing_Shipping_details.Where(s => s.userid ==us.ID_User).ToList();
                            db.Billing_Shipping_details.RemoveRange(shipp);
                            db.SaveChanges();

                            var lab = db.saved_labels.Where(s => s.userid == us.ID_User).ToList();
                            db.saved_labels.RemoveRange(lab);
                            db.SaveChanges();


                            var ord = db.marketing_orders.Where(s => s.user_id == us.ID_User).ToList();
                            db.marketing_orders.RemoveRange(ord);
                            db.SaveChanges();

                            var paym = db.Payment_Intent.Where(s => s.user_id == us.ID_User).ToList();
                            db.Payment_Intent.RemoveRange(paym);
                            db.SaveChanges();


                            var rec = db.Receipts.Where(s => s.user_id == us.ID_User).ToList();
                            db.Receipts.RemoveRange(rec);
                            db.SaveChanges();

                            var templat = db.Saved_Templates.Where(s => s.user_id == us.ID_User).ToList();
                            db.Saved_Templates.RemoveRange(templat);
                            db.SaveChanges();

                            var not = db.Sys_Notifications.Where(s => s.ID_user == us.ID_User).ToList();
                            db.Sys_Notifications.RemoveRange(not);
                            db.SaveChanges();

                            var appoin = db.Tb_Appointments.Where(s => s.ID_User == us.ID_User).ToList();
                            db.Tb_Appointments.RemoveRange(appoin);
                            db.SaveChanges();

                            var cust_us = db.Tb_Customers_Users.Where(s => s.Id_User == us.ID_User).ToList();
                            db.Tb_Customers_Users.RemoveRange(cust_us);
                            db.SaveChanges();                           
                        });

                        
                        db.Sys_Users.RemoveRange(agents);
                        db.SaveChanges();
                    }
                   

                    //Eliminar clientes     
                    if (customer.Count > 0)
                    {
                        //eliminamos documentos de clientes
                        var docscust = db.Tb_LeadDocs.Where(c => customerarr.Contains((int)c.Id_Customer)).ToList();
                        if (docscust.Count > 0) {
                            db.Tb_LeadDocs.RemoveRange(docscust);
                            db.SaveChanges();
                        }
                        db.Tb_Customers.RemoveRange(customer);
                        db.SaveChanges();
                    }

                    var labe = db.saved_labels.Where(s => s.userid == id).ToList();
                    db.saved_labels.RemoveRange(labe);
                    db.SaveChanges();

                    var order = db.marketing_orders.Where(s => s.user_id == id).ToList();
                    db.marketing_orders.RemoveRange(order);
                    db.SaveChanges();

                    var payment = db.Payment_Intent.Where(s => s.user_id == id).ToList();
                    db.Payment_Intent.RemoveRange(payment);
                    db.SaveChanges();


                    var receipt = db.Receipts.Where(s => s.user_id == id).ToList();
                    db.Receipts.RemoveRange(receipt);
                    db.SaveChanges();

                    var templates = db.Saved_Templates.Where(s => s.user_id == id).ToList();
                    db.Saved_Templates.RemoveRange(templates);
                    db.SaveChanges();

                    var noty = db.Sys_Notifications.Where(s => s.ID_user == id).ToList();
                    db.Sys_Notifications.RemoveRange(noty);
                    db.SaveChanges();

                    var appoint = db.Tb_Appointments.Where(s => s.ID_User == id).ToList();
                    db.Tb_Appointments.RemoveRange(appoint);
                    db.SaveChanges();

                    var cust_user = db.Tb_Customers_Users.Where(s => s.Id_User == id).ToList();
                    db.Tb_Customers_Users.RemoveRange(cust_user);
                    db.SaveChanges();

                    var net = db.Tb_Network.Where(s => s.ID_Company == id).ToList();

                    net.ForEach(n => {
                        var rev = db.Tb_Reviews.Where(s => s.Id_Network == n.ID_Network).ToList();
                        db.Tb_Reviews.RemoveRange(rev);
                        db.SaveChanges();
                    });
                    
                    
                    db.Tb_Network.RemoveRange(net);
                    db.SaveChanges();


                    db.Sys_Company.Remove(sys_Company);
                    db.SaveChanges();
                    var result = "Success";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else {
                    var result = "Error, you can not delete the selected Broker because is in use.";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }


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

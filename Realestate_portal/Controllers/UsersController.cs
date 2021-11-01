using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Postal;
using Realestate_portal.Models;
using Realestate_portal.Models.ViewModels;

namespace Realestate_portal.Controllers
{
    public class UsersController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();

        [HttpPost]
        public ActionResult changeProfilePicture(int id)
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

                    var user = (from a in db.Sys_Users where (a.ID_User == id) select a).FirstOrDefault();
                    user.Image = "~/Content/Uploads/Images/profiles/" + fileName;
                    user.Last_update = DateTime.UtcNow;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    Session["activeUser"] = user;


                    var result = "SUCCESS";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else {


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

        // GET: Users
        public ActionResult Index(int broker=0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Users";
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

                List<Sys_Users> lstAgentes = new List<Sys_Users>();


                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.teamleader = activeuser.Team_Leader;

                    if (activeuser.Team_Leader == true)
                    {
                        lstAgentes = db.Sys_Users.Where(t => t.ID_User != 4 && t.Roles.Contains("Agent") && t.ID_Company == activeuser.ID_Company && t.Id_Leader == activeuser.ID_User).OrderBy(t => t.LastName).Include(t => t.Sys_Company).ToList();
                    }
                }
                else
                {
                    ViewBag.rol = "Admin";
                    if (broker == 0)
                    {
                        // se utiliza id = 4 para registros no asignados
                        lstAgentes = db.Sys_Users.Where(t => t.ID_User != 4 && !t.Roles.Contains("Admin") && t.ID_Company == activeuser.ID_Company && !t.Roles.Contains("SA")).OrderBy(t => t.LastName).Include(t => t.Sys_Company).ToList();
                    }
                    else
                    {
                        // se utiliza id = 4 para registros no asignados
                        ViewBag.rol = "SA";
                        lstAgentes = db.Sys_Users.Where(t => t.ID_User != 4 && !t.Roles.Contains("Admin")).OrderBy(t => t.LastName).Include(t => t.Sys_Company).ToList();

                    }
                  
                    
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
                
                return View(lstAgentes);
            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

        }

        public ActionResult TeamList(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Users";
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

                List<Sys_Users> lstAgentes = new List<Sys_Users>();


                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                   
                }
                else
                {
                    ViewBag.rol = "Admin";
                    if (broker == 0)
                    {
                        // se utiliza id = 4 para registros no asignados
                        lstAgentes = db.Sys_Users.Where(t => t.ID_User != 4 && !t.Roles.Contains("Admin") && t.ID_Company == activeuser.ID_Company && !t.Roles.Contains("SA") && t.Team_Leader == true).OrderBy(t => t.LastName).Include(t => t.Sys_Company).ToList();
                    }
                    else
                    {
                        // se utiliza id = 4 para registros no asignados
                        ViewBag.rol = "SA";
                        lstAgentes = db.Sys_Users.Where(t => t.ID_User != 4 && !t.Roles.Contains("Admin") && t.Team_Leader == true).OrderBy(t => t.LastName).Include(t => t.Sys_Company).ToList();

                    }


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

                return View(lstAgentes);
            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

        }



        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sys_Users sys_Users = db.Sys_Users.Find(id);
            if (sys_Users == null)
            {
                return HttpNotFound();
            }
            return View(sys_Users);
        }

        public ActionResult CreateBroker(int broker = 0, bool leader = false)
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
                else if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";


                }
                else
                {
                    ViewBag.rol = "SA";
                }

                ViewBag.activeuser = activeuser;
                ViewBag.selbroker = broker;
         
              
                return View();
            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }


        }
        // GET: Users/Create
        public ActionResult Create(int broker=0, bool leader = false)
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
                var leaders = (from l in db.Sys_Users where (l.Team_Leader == true && l.ID_Company == activeuser.ID_Company) orderby l.LastName ascending select l).ToList();
                ViewBag.leaders = leaders;
                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";


                }
                else
                {
                    ViewBag.rol = "SA";
                }

                ViewBag.activeuser = activeuser;
                ViewBag.selbroker = broker;
                ViewBag.leader = leader;
                ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name");
                return View();
            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

   
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBroker(string BrokerName, string Name, string LastName, string Email, string Brokerage_name, string Brokerage_address, string Broker_Contact, string Broker_License)
        {
            try
            {
                Sys_Company company = new Sys_Company();
                company.Name = BrokerName;
                company.Web = "";
                company.Logo = "";
                company.ShortName = "";
                db.Sys_Company.Add(company);
                db.SaveChanges();

                Sys_Users sys_Users = new Sys_Users();
                sys_Users.Name = Name;
                sys_Users.LastName = LastName;
                sys_Users.Email = Email;
                sys_Users.Brokerage_name = Brokerage_name;
                sys_Users.Brokerage_address = Brokerage_address;
                sys_Users.Broker_Contact = Broker_Contact;
                sys_Users.Broker_License = Broker_License;
                /////
                sys_Users.Roles = "Admin";
                sys_Users.Birth = DateTime.UtcNow;
                sys_Users.Creation_date = DateTime.UtcNow;
                sys_Users.Last_login = DateTime.UtcNow;
                sys_Users.Last_update = DateTime.UtcNow;
         
                sys_Users.Department = "";
                sys_Users.Email_active = true;
                sys_Users.Active = true;
                sys_Users.Status = 1;
                sys_Users.ID_Company = company.ID_Company;
                sys_Users.Member_since = DateTime.UtcNow;
                sys_Users.Gender = "Male";
                sys_Users.Password = CreatePassword(8);
                if (sys_Users.LastName == null) { sys_Users.LastName = ""; }
                if (sys_Users.Address == null) { sys_Users.Address = ""; }
                if (sys_Users.State == null) { sys_Users.State = ""; }
                if (sys_Users.Bank == null) { sys_Users.Bank = ""; }
                if (sys_Users.Bank_number == null) { sys_Users.Bank_number = ""; }
                if (sys_Users.Bank_typeaccount == null) { sys_Users.Bank_typeaccount = ""; }
                if (sys_Users.Credit_classification == null) { sys_Users.Credit_classification = ""; }
                if (sys_Users.Credit_month == null) { sys_Users.Credit_month = ""; }
                if (sys_Users.Credit_year == null) { sys_Users.Credit_year = ""; }
                if (sys_Users.Credit_number == null) { sys_Users.Credit_number = ""; }
                if (sys_Users.Credit_month == null) { sys_Users.Credit_month = ""; }
                if (sys_Users.Credit_name == null) { sys_Users.Credit_name = ""; }
                if (sys_Users.Brokerage_address == null) { sys_Users.Brokerage_address = ""; }
                if (sys_Users.Brokerage_name == null) { sys_Users.Brokerage_name = ""; }
                if (sys_Users.Broker_Contact == null) { sys_Users.Broker_Contact = ""; }
                if (sys_Users.Broker_License == null) { sys_Users.Broker_License = ""; }
                if (sys_Users.My_License == null) { sys_Users.My_License = ""; }
                if (sys_Users.Fb_url == null) { sys_Users.Fb_url = ""; }
                if (sys_Users.Ins_url == null) { sys_Users.Ins_url = ""; }
                if (sys_Users.Tw_url == null) { sys_Users.Tw_url = ""; }
                if (sys_Users.Other_url == null) { sys_Users.Other_url = ""; }
                if (sys_Users.Image == null) { sys_Users.Image = ""; }
                if (sys_Users.Department == null) { sys_Users.Department = ""; }
                if (sys_Users.Secundary_telephone == null) { sys_Users.Secundary_telephone = ""; }
                if (sys_Users.Main_telephone == null) { sys_Users.Main_telephone = ""; }
                if (sys_Users.Position == null) { sys_Users.Position = "Admin Broker"; }
               
                    sys_Users.Id_Leader = 0;
              
                if (sys_Users.Id_Leader == null) { sys_Users.Id_Leader = 0; }
                if (sys_Users.Leader_Name == null) { sys_Users.Leader_Name = ""; }

                db.Sys_Users.Add(sys_Users);
                db.SaveChanges();


                if (sys_Users.Email != "")
                {
                    //Enviamos correo para notificar
                    dynamic emailtosend = new Email("newBroker");
                    emailtosend.To = sys_Users.Email.ToString();
                    emailtosend.From = "support@s7ven.co";
                    emailtosend.correo = sys_Users.Email;
                    emailtosend.contrasena = sys_Users.Password;
                    emailtosend.Send();


                    return RedirectToAction("Brokers", "CRM", new { token = "success" });
                }
                else
                {
                    return RedirectToAction("Brokers", "CRM", new { token = "success" });
                }

            }
            catch (Exception ex)
            {
               
                return RedirectToAction("Brokers", "CRM", new { token = "error" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBroker(string BrokerName, string Web, string Name, string LastName, string Email, string Password, int ID_Company)
        {
            try
            {
                Sys_Company company = db.Sys_Company.Where(c => c.ID_Company == ID_Company).FirstOrDefault();
                company.Name = BrokerName;
                company.Web = Web;
                db.Entry(company).State = EntityState.Modified;
                db.SaveChanges();

                Sys_Users sys_Users = db.Sys_Users.Where(c=>c.ID_Company==ID_Company && c.Roles.Contains("Admin")).FirstOrDefault();
                if (sys_Users.Address == null) { sys_Users.Address = ""; }
                if (sys_Users.LastName == null) { sys_Users.LastName = ""; }
                if (sys_Users.State == null) { sys_Users.State = ""; }
                if (sys_Users.Bank == null) { sys_Users.Bank = ""; }
                if (sys_Users.Bank_number == null) { sys_Users.Bank_number = ""; }
                if (sys_Users.Bank_typeaccount == null) { sys_Users.Bank_typeaccount = ""; }
                if (sys_Users.Credit_classification == null) { sys_Users.Credit_classification = ""; }
                if (sys_Users.Credit_month == null) { sys_Users.Credit_month = ""; }
                if (sys_Users.Credit_year == null) { sys_Users.Credit_year = ""; }
                if (sys_Users.Credit_number == null) { sys_Users.Credit_number = ""; }
                if (sys_Users.Credit_month == null) { sys_Users.Credit_month = ""; }
                if (sys_Users.Credit_name == null) { sys_Users.Credit_name = ""; }
                if (sys_Users.Brokerage_address == null) { sys_Users.Brokerage_address = ""; }
                if (sys_Users.Brokerage_name == null) { sys_Users.Brokerage_name = ""; }
                if (sys_Users.Broker_Contact == null) { sys_Users.Broker_Contact = ""; }
                if (sys_Users.Broker_License == null) { sys_Users.Broker_License = ""; }
                if (sys_Users.My_License == null) { sys_Users.My_License = ""; }
                if (sys_Users.Fb_url == null) { sys_Users.Fb_url = ""; }
                if (sys_Users.Ins_url == null) { sys_Users.Ins_url = ""; }
                if (sys_Users.Tw_url == null) { sys_Users.Tw_url = ""; }
                if (sys_Users.Other_url == null) { sys_Users.Other_url = ""; }
                if (sys_Users.Image == null) { sys_Users.Image = ""; }
                if (sys_Users.Department == null) { sys_Users.Department = ""; }
                if (sys_Users.Secundary_telephone == null) { sys_Users.Secundary_telephone = ""; }
                if (sys_Users.Main_telephone == null) { sys_Users.Main_telephone = ""; }
                if (sys_Users.Position == null) { sys_Users.Position = ""; }
                if (sys_Users.Gender == null) { sys_Users.Gender = ""; }
                sys_Users.Team_Leader = false;
                sys_Users.Id_Leader = 0;
                sys_Users.Leader_Name = ""; 

                db.Entry(sys_Users).State = EntityState.Modified;
                db.SaveChanges();

     
                TempData["exito"] = "Data saved successfully.";
                return RedirectToAction("BrokerProfile", "Users", new { id= ID_Company });

            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("UserProfile", "Users", new { id = ID_Company });
            }
        }
        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_User,Name,LastName,Gender,Email,Password,Birth,Creation_date,Last_update,Last_login,State,Address,Main_telephone,Secundary_telephone,Fb_url,Ins_url,Tw_url,Other_url,Image,ID_Company,Status,Active,Email_active,Position,Department,Roles,Brokerage_name,Brokerage_address,Broker_Contact,Broker_License,My_License,Member_since,Bank,Bank_number,Bank_typeaccount,Credit_number,Credit_name,Credit_classification,Credit_month,Credit_year,Team_Leader,Id_Leader,Leader_Name")] Sys_Users sys_Users)
        {
      


                try
                {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                sys_Users.Birth = DateTime.UtcNow;
                sys_Users.Creation_date = DateTime.UtcNow;
                sys_Users.Last_login = DateTime.UtcNow;
                sys_Users.Last_update = DateTime.UtcNow;
                sys_Users.Roles = "Agent";
                sys_Users.Department = "";
                sys_Users.Email_active = true;
                sys_Users.Active = true;
                sys_Users.Status = 1;
                sys_Users.ID_Company = activeuser.ID_Company;
                sys_Users.Member_since = DateTime.UtcNow;
                sys_Users.Gender = "Male";
                sys_Users.Password = CreatePassword(8);
                if (sys_Users.Address == null) { sys_Users.Address = ""; }
                if (sys_Users.State == null) { sys_Users.State = ""; }
                if (sys_Users.Bank == null) { sys_Users.Bank = ""; }
                if (sys_Users.Bank_number == null) { sys_Users.Bank_number = ""; }
                if (sys_Users.Bank_typeaccount == null) { sys_Users.Bank_typeaccount = ""; }
                if (sys_Users.Credit_classification == null) { sys_Users.Credit_classification = ""; }
                if (sys_Users.Credit_month == null) { sys_Users.Credit_month = ""; }
                if (sys_Users.Credit_year == null) { sys_Users.Credit_year = ""; }
                if (sys_Users.Credit_number == null) { sys_Users.Credit_number = ""; }
                if (sys_Users.Credit_month == null) { sys_Users.Credit_month = ""; }
                if (sys_Users.Credit_name == null) { sys_Users.Credit_name = ""; }
                if (sys_Users.Brokerage_address == null) { sys_Users.Brokerage_address = ""; }
                if (sys_Users.Brokerage_name == null) { sys_Users.Brokerage_name = ""; }
                if (sys_Users.Broker_Contact == null) { sys_Users.Broker_Contact = ""; }
                if (sys_Users.Broker_License == null) { sys_Users.Broker_License = ""; }
                if (sys_Users.My_License == null) { sys_Users.My_License = ""; }
                if (sys_Users.Fb_url == null) { sys_Users.Fb_url = ""; }
                if (sys_Users.Ins_url == null) { sys_Users.Ins_url = ""; }
                if (sys_Users.Tw_url == null) { sys_Users.Tw_url = ""; }
                if (sys_Users.Other_url == null) { sys_Users.Other_url = ""; }
                if (sys_Users.Image == null) { sys_Users.Image = ""; }
                if (sys_Users.Department == null) { sys_Users.Department = ""; }
                if (sys_Users.Secundary_telephone == null) { sys_Users.Secundary_telephone = ""; }
                if (sys_Users.Main_telephone == null) { sys_Users.Main_telephone = ""; }
                if (sys_Users.Position == null) { sys_Users.Position = "Real Estate Salesperson"; }
                if (sys_Users.Team_Leader == true)
                {
                    sys_Users.Id_Leader = 0;
                    //if (sys_Users.Leader_Name.Equals(""))
                    //{
                    //    sys_Users.Leader_Name = "Team " + sys_Users.Name + " " + sys_Users.LastName;
                    //}
                }
                if (sys_Users.Id_Leader == null) { sys_Users.Id_Leader = 0; }
                if (sys_Users.Leader_Name == null) { sys_Users.Leader_Name = ""; }

                db.Sys_Users.Add(sys_Users);
                db.SaveChanges();


                if (sys_Users.Email != "")
                    {
                        //Enviamos correo para notificar
                        dynamic emailtosend = new Email("newBroker");
                        emailtosend.To = sys_Users.Email.ToString();
                        emailtosend.From = "support@s7ven.co";
                        emailtosend.correo = sys_Users.Email;
                        emailtosend.contrasena = sys_Users.Password;
                        emailtosend.Send();


                    return RedirectToAction("Agents", "CRM", new { token="success"});
                    }
                    else
                    {
                    return RedirectToAction("Agents", "CRM", new { token = "success" });
                }

                    //Sys_Notifications newnotification = new Sys_Notifications();
                    //if (sys_Users.Team_Leader == true)
                    //{
                        
                    //    newnotification.Active = true;
                    //    newnotification.Date = DateTime.UtcNow;
                    //    newnotification.Title = "You've been assigned as a team leader.";
                    //    newnotification.Description = "Team leader: "+ sys_Users.LastName + " " + sys_Users.Name;
                    //    newnotification.ID_user = sys_Users.ID_User;
                     
                    //}
                    //if (sys_Users.Id_Leader != 0)
                    //{
                    //var leader = (from l in db.Sys_Users where l.ID_User == sys_Users.Id_Leader select l).FirstOrDefault();
                    //    newnotification.Active = true;
                    //    newnotification.Date = DateTime.UtcNow;
                    //    newnotification.Title = "You're now part of the team team of "+ leader.LastName + " " +leader.Name;
                    //    newnotification.Description = "Team assignment" + sys_Users.LastName + " " + sys_Users.Name;
                    //    newnotification.ID_user = sys_Users.ID_User;
                    //}
                   

                    //db.Sys_Notifications.Add(newnotification);
                    //db.SaveChanges();
                }
                catch (Exception ex)
                {
                return RedirectToAction("Agents", "CRM", new { token = "error" });
            }


        }


        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890@#!";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        // GET: Users/Edit/5
        public ActionResult EditAgent(int? id, string modulo, int broker=0)
        {

            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Users";
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
                var lstDocsAgent = (from d in db.Tb_DocuAgent where (d.Id_User == id) select d).ToList();
                ViewBag.docsAgent = lstDocsAgent;
                var notes = (from n in db.Tb_Notes where (n.ID_Customer == 0 && n.ID_Property == 0 && n.ID_User == id) select n).ToList();
                ViewBag.lstNotes = notes;
                ViewBag.rol = "";
                ViewBag.modulo = modulo;
                var leaders = (from l in db.Sys_Users where (l.Team_Leader == true && l.ID_Company == activeuser.ID_Company) orderby l.LastName ascending select l).ToList();
                ViewBag.leaders = leaders;

                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else if(r.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";


                }
                else
                {
                    ViewBag.rol = "SA";
                }

                Sys_Users sys_Users = db.Sys_Users.Find(id);
                ViewBag.activeuser = activeuser;
                ViewBag.selbroker = broker;
                ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name", sys_Users.ID_Company);
                return View(sys_Users);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }



        }

        public ActionResult AgentProfile(int id)
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

                List<US_State> states;

                states = new List<US_State>(50);
                states.Add(new US_State("AL", "Alabama"));
                states.Add(new US_State("AK", "Alaska"));
                states.Add(new US_State("AZ", "Arizona"));
                states.Add(new US_State("AR", "Arkansas"));
                states.Add(new US_State("CA", "California"));
                states.Add(new US_State("CO", "Colorado"));
                states.Add(new US_State("CT", "Connecticut"));
                states.Add(new US_State("DE", "Delaware"));
                states.Add(new US_State("DC", "District Of Columbia"));
                states.Add(new US_State("FL", "Florida"));
                states.Add(new US_State("GA", "Georgia"));
                states.Add(new US_State("HI", "Hawaii"));
                states.Add(new US_State("ID", "Idaho"));
                states.Add(new US_State("IL", "Illinois"));
                states.Add(new US_State("IN", "Indiana"));
                states.Add(new US_State("IA", "Iowa"));
                states.Add(new US_State("KS", "Kansas"));
                states.Add(new US_State("KY", "Kentucky"));
                states.Add(new US_State("LA", "Louisiana"));
                states.Add(new US_State("ME", "Maine"));
                states.Add(new US_State("MD", "Maryland"));
                states.Add(new US_State("MA", "Massachusetts"));
                states.Add(new US_State("MI", "Michigan"));
                states.Add(new US_State("MN", "Minnesota"));
                states.Add(new US_State("MS", "Mississippi"));
                states.Add(new US_State("MO", "Missouri"));
                states.Add(new US_State("MT", "Montana"));
                states.Add(new US_State("NE", "Nebraska"));
                states.Add(new US_State("NV", "Nevada"));
                states.Add(new US_State("NH", "New Hampshire"));
                states.Add(new US_State("NJ", "New Jersey"));
                states.Add(new US_State("NM", "New Mexico"));
                states.Add(new US_State("NY", "New York"));
                states.Add(new US_State("NC", "North Carolina"));
                states.Add(new US_State("ND", "North Dakota"));
                states.Add(new US_State("OH", "Ohio"));
                states.Add(new US_State("OK", "Oklahoma"));
                states.Add(new US_State("OR", "Oregon"));
                states.Add(new US_State("PA", "Pennsylvania"));
                states.Add(new US_State("RI", "Rhode Island"));
                states.Add(new US_State("SC", "South Carolina"));
                states.Add(new US_State("SD", "South Dakota"));
                states.Add(new US_State("TN", "Tennessee"));
                states.Add(new US_State("TX", "Texas"));
                states.Add(new US_State("UT", "Utah"));
                states.Add(new US_State("VT", "Vermont"));
                states.Add(new US_State("VA", "Virginia"));
                states.Add(new US_State("WA", "Washington"));
                states.Add(new US_State("WV", "West Virginia"));
                states.Add(new US_State("WI", "Wisconsin"));
                states.Add(new US_State("WY", "Wyoming"));


                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";


                }
                else
                {
                    ViewBag.rol = "SA";
                }

                Sys_Users sys_Users = db.Sys_Users.Find(id);
                ViewBag.activeuser = activeuser;

                ViewBag.states = states;

                ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name", sys_Users.ID_Company);

                List<Tb_DocuAgent> lstDocsAgent = new List<Tb_DocuAgent>();
                List<Tb_Customers> lstleads = new List<Tb_Customers>();
                List<Tb_Notes> notes = new List<Tb_Notes>();
                List<TasksView> lst_tasks = new List<TasksView>();

                lstDocsAgent = (from d in db.Tb_DocuAgent where (d.Id_User == id) select d).ToList();
                ViewBag.docsAgent = lstDocsAgent;

                notes = (from n in db.Tb_Notes where (n.ID_Customer == 0 && n.ID_Property == 0 && n.ID_User == id) select n).ToList();
                ViewBag.lstNotes = notes;

                var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == id) select c.Id_Customer).Distinct().ToArray();
                lstleads = (from f in db.Tb_Customers where (leadsassigned.Contains(f.ID_Customer) && f.Lead) select f).ToList();

                ViewBag.leads = lstleads;


                lst_tasks = (from a in db.Tb_Tasks
                             where (a.ID_User == id)
                             select new TasksView
                             {
                                 ID_Company = a.ID_Company,
                                 Description = a.Description,
                                 Finished = a.Finished,
                                 ID_task = a.ID_task,
                                 ID_User = a.ID_User,
                                 Lastupdate = a.Createdat,
                                 Title = a.Title,
                                 Url_image = (from b in db.Sys_Users where (b.ID_User == a.ID_User) select b.Image).FirstOrDefault(),
                                 Name = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.Name).FirstOrDefault(),
                                 Lastname = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.LastName).FirstOrDefault()
                             }).ToList();

                ViewBag.lsttask = lst_tasks;

                IQueryable<Tb_Process> Tb_Process;
                Tb_Process = db.Tb_Process.Where(a => a.ID_User == id).Include(t => t.Tb_Customers);
                ViewBag.lstproperties = Tb_Process;
                return View(sys_Users);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }



        }

        public ActionResult BrokerProfile(int id)
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

                Sys_Users lstAdmins = new Sys_Users();
                lstAdmins =(from a in db.Sys_Users where (a.Roles.Contains("Admin") && a.ID_Company == id) select a).FirstOrDefault();
                ViewBag.users = lstAdmins;

                Sys_Company selectedBroker = new Sys_Company();
                selectedBroker = db.Sys_Company.Where(c => c.ID_Company == id).FirstOrDefault();

             

                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";


                }
                else
                {
                    ViewBag.rol = "SA";
                }


                return View(selectedBroker);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }



        }

        public ActionResult UserProfile()
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

                List<US_State> states;

                states = new List<US_State>(50);
                states.Add(new US_State("AL", "Alabama"));
                states.Add(new US_State("AK", "Alaska"));
                states.Add(new US_State("AZ", "Arizona"));
                states.Add(new US_State("AR", "Arkansas"));
                states.Add(new US_State("CA", "California"));
                states.Add(new US_State("CO", "Colorado"));
                states.Add(new US_State("CT", "Connecticut"));
                states.Add(new US_State("DE", "Delaware"));
                states.Add(new US_State("DC", "District Of Columbia"));
                states.Add(new US_State("FL", "Florida"));
                states.Add(new US_State("GA", "Georgia"));
                states.Add(new US_State("HI", "Hawaii"));
                states.Add(new US_State("ID", "Idaho"));
                states.Add(new US_State("IL", "Illinois"));
                states.Add(new US_State("IN", "Indiana"));
                states.Add(new US_State("IA", "Iowa"));
                states.Add(new US_State("KS", "Kansas"));
                states.Add(new US_State("KY", "Kentucky"));
                states.Add(new US_State("LA", "Louisiana"));
                states.Add(new US_State("ME", "Maine"));
                states.Add(new US_State("MD", "Maryland"));
                states.Add(new US_State("MA", "Massachusetts"));
                states.Add(new US_State("MI", "Michigan"));
                states.Add(new US_State("MN", "Minnesota"));
                states.Add(new US_State("MS", "Mississippi"));
                states.Add(new US_State("MO", "Missouri"));
                states.Add(new US_State("MT", "Montana"));
                states.Add(new US_State("NE", "Nebraska"));
                states.Add(new US_State("NV", "Nevada"));
                states.Add(new US_State("NH", "New Hampshire"));
                states.Add(new US_State("NJ", "New Jersey"));
                states.Add(new US_State("NM", "New Mexico"));
                states.Add(new US_State("NY", "New York"));
                states.Add(new US_State("NC", "North Carolina"));
                states.Add(new US_State("ND", "North Dakota"));
                states.Add(new US_State("OH", "Ohio"));
                states.Add(new US_State("OK", "Oklahoma"));
                states.Add(new US_State("OR", "Oregon"));
                states.Add(new US_State("PA", "Pennsylvania"));
                states.Add(new US_State("RI", "Rhode Island"));
                states.Add(new US_State("SC", "South Carolina"));
                states.Add(new US_State("SD", "South Dakota"));
                states.Add(new US_State("TN", "Tennessee"));
                states.Add(new US_State("TX", "Texas"));
                states.Add(new US_State("UT", "Utah"));
                states.Add(new US_State("VT", "Vermont"));
                states.Add(new US_State("VA", "Virginia"));
                states.Add(new US_State("WA", "Washington"));
                states.Add(new US_State("WV", "West Virginia"));
                states.Add(new US_State("WI", "Wisconsin"));
                states.Add(new US_State("WY", "Wyoming"));
            

            if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";


                }
                else
                {
                    ViewBag.rol = "SA";
                }

                Sys_Users sys_Users = db.Sys_Users.Find(activeuser.ID_User);
                ViewBag.activeuser = activeuser;

                ViewBag.states = states;

                ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name", sys_Users.ID_Company);

                //List<Tb_DocuAgent> lstDocsAgent = new List<Tb_DocuAgent>();
                //List<Tb_Customers> lstleads = new List<Tb_Customers>();
                //List<Tb_Notes> notes = new List<Tb_Notes>();

                //lstDocsAgent = (from d in db.Tb_DocuAgent where (d.Id_User == activeuser.ID_User) select d).ToList();
                //ViewBag.docsAgent = lstDocsAgent;

                //notes = (from n in db.Tb_Notes where (n.ID_Customer == 0 && n.ID_Property == 0 && n.ID_User == activeuser.ID_User) select n).ToList();
                //ViewBag.lstNotes = notes;

                //var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == activeuser.ID_User) select c.Id_Customer).Distinct().ToArray();
                //lstleads = (from f in db.Tb_Customers where (leadsassigned.Contains(f.ID_Customer) && f.Lead) select f).ToList();

                //ViewBag.leads = lstleads;

                return View(sys_Users);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }



        }
        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAgent([Bind(Include = "ID_User,Name,LastName,Gender,Email,Password,Birth,Creation_date,Last_update,Last_login,State,Address,Main_telephone,Secundary_telephone,Fb_url,Ins_url,Tw_url,Other_url,Image,ID_Company,Status,Active,Email_active,Position,Department,Roles,Brokerage_name,Brokerage_address,Broker_Contact,Broker_License,My_License,Member_since,Bank,Bank_number,Bank_typeaccount,Credit_number,Credit_name,Credit_classification,Credit_month,Credit_year,Team_Leader,Id_Leader,Leader_Name")] Sys_Users sys_Users)
        {
            try
            {
                var team = (from t in db.Sys_Users where (t.Id_Leader == sys_Users.ID_User) select t).ToList();
                if (sys_Users.Address == null) { sys_Users.Address = ""; }
                if (sys_Users.LastName == null) { sys_Users.LastName = ""; }
                if (sys_Users.State == null) { sys_Users.State = ""; }
                if (sys_Users.Bank == null) { sys_Users.Bank = ""; }
                if (sys_Users.Bank_number == null) { sys_Users.Bank_number = ""; }
                if (sys_Users.Bank_typeaccount == null) { sys_Users.Bank_typeaccount = ""; }
                if (sys_Users.Credit_classification == null) { sys_Users.Credit_classification = ""; }
                if (sys_Users.Credit_month == null) { sys_Users.Credit_month = ""; }
                if (sys_Users.Credit_year == null) { sys_Users.Credit_year = ""; }
                if (sys_Users.Credit_number == null) { sys_Users.Credit_number = ""; }
                if (sys_Users.Credit_month == null) { sys_Users.Credit_month = ""; }
                if (sys_Users.Credit_name == null) { sys_Users.Credit_name = ""; }
                if (sys_Users.Brokerage_address == null) { sys_Users.Brokerage_address = ""; }
                if (sys_Users.Brokerage_name == null) { sys_Users.Brokerage_name = ""; }
                if (sys_Users.Broker_Contact == null) { sys_Users.Broker_Contact = ""; }
                if (sys_Users.Broker_License == null) { sys_Users.Broker_License = ""; }
                if (sys_Users.My_License == null) { sys_Users.My_License = ""; }
                if (sys_Users.Fb_url == null) { sys_Users.Fb_url = ""; }
                if (sys_Users.Ins_url == null) { sys_Users.Ins_url = ""; }
                if (sys_Users.Tw_url == null) { sys_Users.Tw_url = ""; }
                if (sys_Users.Other_url == null) { sys_Users.Other_url = ""; }
                if (sys_Users.Image == null) { sys_Users.Image = ""; }
                if (sys_Users.Department == null) { sys_Users.Department = ""; }
                if (sys_Users.Secundary_telephone == null) { sys_Users.Secundary_telephone = ""; }
                if (sys_Users.Main_telephone == null) { sys_Users.Main_telephone = ""; }
                if (sys_Users.Position == null) { sys_Users.Position = "Real Estate Salesperson"; }
                if (sys_Users.Team_Leader == true) 
                { 
                    //sys_Users.Id_Leader = 0;
                    //if (sys_Users.Leader_Name == null)
                    //{
                    //    sys_Users.Leader_Name = "Team " + sys_Users.Name + " " + sys_Users.LastName;
                    //}
                    //foreach (var item in team)
                    //{
                    //    item.Leader_Name = sys_Users.Leader_Name;
                    //    db.Entry(item).State = EntityState.Modified;
                    //    db.SaveChanges();
                    //}
                }
                if (sys_Users.Id_Leader == null) { sys_Users.Id_Leader = 0; }
                if (sys_Users.Leader_Name == null){ sys_Users.Leader_Name = ""; }

                db.Entry(sys_Users).State = EntityState.Modified;
                db.SaveChanges();
                
                
                //if (sys_Users.Active == false)
                //{
                    
                //    SetToBroker(sys_Users.ID_User);
                    
                //}
                //if (sys_Users.Team_Leader == false)
                //{
                //    foreach (var item in team)
                //    {
                //        item.Id_Leader = 0;
                //        item.Leader_Name = "";
                //        db.Entry(item).State = EntityState.Modified;
                //        db.SaveChanges();
                //    }
                //}
                
                //try
                //{
                //    Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //    Session["activeUser"] = (from a in db.Sys_Users where (a.Email == activeuser.Email && a.Password == activeuser.Password) select a).FirstOrDefault();
                //}
                //catch
                //{

                //}

                TempData["exito"] = "Data saved successfully.";
                return RedirectToAction("AgentProfile", "Users", new { id = sys_Users.ID_User });

            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("AgentProfile", "Users", new { id = sys_Users.ID_User });
            }


        }
        // GET: Users/Edit/5
        public ActionResult Edit(int? id, string modulo, int broker=0)
        {

            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Users";
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

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;

                ViewBag.rol = "";
                ViewBag.modulo = modulo;

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
                Sys_Users sys_Users = db.Sys_Users.Find(id);

                ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name", sys_Users.ID_Company);
                return View(sys_Users);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }



        }


        // GET: Users/Edit/5
        public ActionResult EditAdminB(int? id, int brokerupd, string modulo, int broker = 0)
        {

            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Users";
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

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;

                ViewBag.rol = "";
                ViewBag.modulo = modulo;

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
                Sys_Users sys_Users = db.Sys_Users.Find(id);

                ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name", sys_Users.ID_Company);
                return View(sys_Users);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }



        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAdminB(int ID_User, string Name,string Email,string Password,Boolean Active)
        {
            var sys_Users = db.Sys_Users.Find(ID_User);
            try
            {
          
                sys_Users.Name = Name;
                sys_Users.Email = Email;
                sys_Users.Password = Password;
                sys_Users.Active = Active;
                db.Entry(sys_Users).State = EntityState.Modified;
                db.SaveChanges();
                TempData["exito"] = "Data saved successfully.";
                return RedirectToAction("Edit", "Sys_Company", new { id = sys_Users.ID_Company });

            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("Edit", "Sys_Company", new { id = sys_Users.ID_Company });
            }


        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_User,Name,LastName,Gender,Email,Password,Birth,Creation_date,Last_update,Last_login,State,Address,Main_telephone,Secundary_telephone,Fb_url,Ins_url,Tw_url,Other_url,Image,ID_Company,Status,Active,Email_active,Position,Department,Roles,Brokerage_name,Brokerage_address,Broker_Contact,Broker_License,My_License,Member_since,Bank,Bank_number,Bank_typeaccount,Credit_number,Credit_name,Credit_classification,Credit_month,Credit_year,Team_Leader,Id_Leader,Leader_Name")] Sys_Users sys_Users)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                if (sys_Users.Address == null) { sys_Users.Address = ""; }
                if (sys_Users.LastName == null) { sys_Users.LastName = ""; }
                if (sys_Users.State == null) { sys_Users.State = ""; }
                if (sys_Users.Bank == null) { sys_Users.Bank = ""; }
                if (sys_Users.Bank_number == null) { sys_Users.Bank_number = ""; }
                if (sys_Users.Bank_typeaccount == null) { sys_Users.Bank_typeaccount = ""; }
                if (sys_Users.Credit_classification == null) { sys_Users.Credit_classification = ""; }
                if (sys_Users.Credit_month == null) { sys_Users.Credit_month = ""; }
                if (sys_Users.Credit_year == null) { sys_Users.Credit_year = ""; }
                if (sys_Users.Credit_number == null) { sys_Users.Credit_number = ""; }
                if (sys_Users.Credit_month == null) { sys_Users.Credit_month = ""; }
                if (sys_Users.Credit_name == null) { sys_Users.Credit_name = ""; }
                if (sys_Users.Brokerage_address == null) { sys_Users.Brokerage_address = ""; }
                if (sys_Users.Brokerage_name == null) { sys_Users.Brokerage_name = ""; }
                if (sys_Users.Broker_Contact == null) { sys_Users.Broker_Contact = ""; }
                if (sys_Users.Broker_License == null) { sys_Users.Broker_License = ""; }
                if (sys_Users.My_License == null) { sys_Users.My_License = ""; }
                if (sys_Users.Fb_url == null) { sys_Users.Fb_url = ""; }
                if (sys_Users.Ins_url == null) { sys_Users.Ins_url = ""; }
                if (sys_Users.Tw_url == null) { sys_Users.Tw_url = ""; }
                if (sys_Users.Other_url == null) { sys_Users.Other_url = ""; }
                if (sys_Users.Image == null) { sys_Users.Image = ""; }
                if (sys_Users.Department == null) { sys_Users.Department = ""; }
                if (sys_Users.Secundary_telephone == null) { sys_Users.Secundary_telephone = ""; }
                if (sys_Users.Main_telephone == null) { sys_Users.Main_telephone = ""; }
                if (sys_Users.Position == null) { sys_Users.Position = activeuser.Position; }
                if (sys_Users.Gender == null) { sys_Users.Gender = ""; }
                if (sys_Users.Team_Leader == false){ sys_Users.Team_Leader = activeuser.Team_Leader; }
                if (sys_Users.Id_Leader == null){ sys_Users.Id_Leader= activeuser.Id_Leader; }
                if (sys_Users.Leader_Name == null){ sys_Users.Leader_Name = activeuser.Leader_Name; }

                //var team = (from t in db.Sys_Users where (t.Id_Leader == sys_Users.ID_User) select t).ToList();
                //if (sys_Users.Team_Leader == true)
                //{
                //    foreach (var item in team)
                //    {
                //        item.Leader_Name = sys_Users.Leader_Name;
                //        db.Entry(item).State = EntityState.Modified;
                //        db.SaveChanges();
                //    }
                //}
             
                if (sys_Users.Roles.Contains("Admin")) {
                    var agents = (from a in db.Sys_Users where (a.ID_Company == sys_Users.ID_Company && a.Roles.Contains("Agent")) select a).ToList();
                    if (agents.Count() > 0)
                    {
                        foreach (var item in agents) {
                            item.Brokerage_address = sys_Users.Brokerage_address;
                            item.Brokerage_name = sys_Users.Brokerage_name;
                            item.Broker_Contact = sys_Users.Broker_Contact;
                            item.Broker_License = sys_Users.Broker_License;

                            db.Entry(item).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                }


                db.Entry(sys_Users).State = EntityState.Modified;
                db.SaveChanges();

                Session["activeUser"] = (from a in db.Sys_Users where (a.Email == sys_Users.Email && a.Password == sys_Users.Password) select a).FirstOrDefault();


                TempData["exito"] = "Data saved successfully.";
                return RedirectToAction("UserProfile", "Users");
              
            }
            catch (Exception ex) {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("UserProfile", "Users");
            }

     
        }


        // POST: Users/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {

            
                Sys_Users activeUser = Session["activeUser"] as Sys_Users;

                SetToBroker(id);
                DeleteData(id);
                Sys_Users sys_Users = db.Sys_Users.Find(id);
                db.Sys_Users.Remove(sys_Users);
                db.SaveChanges();

                var result = "Success";
                 return Json(result);
            }
            catch (Exception ex)
            {

                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void SetToBroker(int id) {
            try
            {
                var user = (from u in db.Sys_Users where u.ID_User == id select u).FirstOrDefault();
                var companybroker = (from b in db.Sys_Users where (b.ID_Company == user.ID_Company && b.Roles == "Admin") select b).FirstOrDefault();

                var posts = (from p in db.Tb_Posts where (p.ID_User == id) select p).ToList();
                    foreach (var item in posts)
                    {
                        item.ID_User = companybroker.ID_User;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                var appointment = (from a in db.Tb_Appointments where (a.ID_User == id) select a).ToList();
                    foreach (var item in appointment)
                    {
                        item.ID_User = companybroker.ID_User;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                var message = (from m in db.Tb_Message where (m.User_ID == id) select m).ToList();
                    foreach (var item in message)
                    {
                        item.User_ID = companybroker.ID_User;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                var process = (from s in db.Tb_Process where (s.ID_User == id) select s).ToList();
                    foreach (var item in process)
                    {
                        item.ID_User = companybroker.ID_User;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                var docPack = (from g in db.Tb_Docpackages where (g.ID_User == id) select g).ToList();
                    foreach (var item in docPack)
                    {
                        item.ID_User = companybroker.ID_User;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                var resources = (from r in db.Tb_Resources where (r.Id_Leader == id) select r).ToList();
                    foreach (var item in resources)
                    {
                        item.Id_Leader = 0;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    var team = (from t in db.Sys_Users where (t.Id_Leader == id) select t).ToList();
                    foreach (var item in team)
                    {
                        item.Id_Leader = 0;
                        item.Leader_Name = "";
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }



            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public void DeleteData(int id) {
            try
            {
                var docagent = (from d in db.Tb_DocuAgent where (d.Id_User == id) select d).ToList();
                    foreach (var item in docagent)
                    {
                        Tb_DocuAgent docuAgent = db.Tb_DocuAgent.Find(item.Id_Document);
                        db.Tb_DocuAgent.Remove(docuAgent);
                        db.SaveChanges();
                    }

                var notes = (from n in db.Tb_Notes where (n.ID_User == id) select n).ToList();
                    foreach (var item in notes)
                    {
                        Tb_Notes note = db.Tb_Notes.Find(item.ID_note);
                        db.Tb_Notes.Remove(note);
                        db.SaveChanges();
                    }

                var notifications = (from f in db.Sys_Notifications where (f.ID_user == id) select f).ToList();
                    foreach (var item in notifications)
                    {
                        Sys_Notifications noti = db.Sys_Notifications.Find(item.ID_notification);
                        db.Sys_Notifications.Remove(noti);
                        db.SaveChanges();
                    }

                var reminders = (from r in db.Tb_Reminders where (r.ID_User == id) select r).ToList();
                    foreach (var item in reminders)
                    {
                        Tb_Reminders reminder = db.Tb_Reminders.Find(item.ID_Reminder);
                        db.Tb_Reminders.Remove(reminder);
                        db.SaveChanges();
                    }
                var team = (from t in db.Sys_Users where (t.Id_Leader == id) select t).ToList();
                    foreach (var item in team)
                    {
                        item.Id_Leader = 0;
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                var customer = (from c in db.Tb_Customers_Users where (c.Id_User == id) select c).ToList();
                    foreach (var item in customer)
                    {
                        Tb_Customers_Users customers_Users = db.Tb_Customers_Users.Find(item.Id_Customer_User);
                        db.Tb_Customers_Users.Remove(customers_Users);
                        db.SaveChanges();
                    }

            }
            catch (Exception ex)
            {

                throw;
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

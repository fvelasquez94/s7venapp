using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Postal;
using Realestate_portal.Models;
using Realestate_portal.Models.ViewModels;
using Realestate_portal.Models.ViewModels.CRM;
using Realestate_portal.Services.Contracts;
using Realestate_portal.Controllers.TwilioAPI;

namespace Realestate_portal.Controllers
{
    public class CustomersController : Controller
    {
        private Imarket repo;
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();
        private Cls_GoogleCalendar cls_GoogleCalendar = new Cls_GoogleCalendar();

        public CustomersController()
        {          
        }

        public CustomersController(Imarket _repo)
        {
            repo = _repo;
        }


        // GET: Tb_Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Customers tb_Customers = db.Tb_Customers.Find(id);
            if (tb_Customers == null)
            {
                return HttpNotFound();
            }
            return View(tb_Customers);
        }

        // GET: Tb_Customers/Create
        public ActionResult Create(int broker=0)
        {

            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
                //HEADER DATA
                ViewBag.activeuser = activeuser;
                ViewBag.userCompany = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();
              
                //ROLES
                //FIN HEADER

                ViewBag.userslist = (from u in db.Sys_Users where (u.Sys_Company.ID_Company == activeuser.ID_Company && (u.Roles== "Agent" || u.Roles =="Admin") && u.Active == true) orderby u.LastName ascending select u).ToList();
             
                ViewBag.rol = "";

                //Filtros SA
                var lstsource = (from o in db.Tb_Source where (o.Id_Company == activeuser.ID_Company) select o).ToList();
                ViewBag.lstSource = lstsource;
                var lststatus = (from t in db.Tb_Status where (t.Id_Company == activeuser.ID_Company) select t).ToList();
                ViewBag.lstStatus = lststatus;
                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;


                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.userslist = (from u in db.Sys_Users where (u.Sys_Company.ID_Company == activeuser.ID_Company && u.ID_User == activeuser.ID_User && u.Active == true) orderby u.LastName ascending select u).ToList();
                }
                else
                {
                    if (activeuser.Roles.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("SA")) select b).FirstOrDefault();
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


                ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name");
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

        }

        // POST: Tb_Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_Customer,Name,LastName,Gender,Birthday,Marital_status,Type,Email,Phone,Mobile,State,Address,Zipcode,Lead,Active,ID_Company,Creation_date,Source")] Tb_Customers tb_Customers)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (tb_Customers.Birthday == null) { tb_Customers.Birthday = DateTime.UtcNow; }
            if (tb_Customers.Address == null) { tb_Customers.Address = ""; }
            var birthdemo = Convert.ToDateTime(tb_Customers.Birthday);
            var theDate = new DateTime(1900, 01, 01, 00, 00, 00);
            if (birthdemo < theDate) {
                tb_Customers.Birthday = DateTime.UtcNow;
            }
            
        
           
            tb_Customers.ID_Company = activeuser.ID_Company;
            tb_Customers.Lead = true;
            if (tb_Customers.Zipcode == null) { tb_Customers.Zipcode = ""; }
            if (tb_Customers.Mobile == null) { tb_Customers.Mobile = ""; }
            if (tb_Customers.Address == null) { tb_Customers.Address = ""; }

            if (tb_Customers.Email == null) { tb_Customers.Email = ""; }
            tb_Customers.Active = true;
            tb_Customers.Creation_date = DateTime.UtcNow;
            tb_Customers.ID_team = 0;
            tb_Customers.Gender = "";
            

        
                db.Tb_Customers.Add(tb_Customers);
                db.SaveChanges();

            //asignamos agente
            try
            {

                var agentsassigned = (from c in db.Tb_Customers_Users where (c.Id_Customer == tb_Customers.ID_Customer && c.ID_team == 0) select c).ToList();
                if (agentsassigned.Count() > 0)
                {
                    db.Tb_Customers_Users.RemoveRange(agentsassigned);
                }
                db.SaveChanges();
                if (activeuser != null)
                {

                    Tb_Customers_Users customerUsers = new Tb_Customers_Users();

                    customerUsers.Id_Customer = tb_Customers.ID_Customer;
                    customerUsers.Id_User = activeuser.ID_User;
                    customerUsers.ID_team = 0;
                    customerUsers.Teamleader = false;
                    db.Tb_Customers_Users.Add(customerUsers);
                    db.SaveChanges();

                    //Sys_Notifications newnotification = new Sys_Notifications();
                    //newnotification.Active = true;
                    //newnotification.Date = DateTime.UtcNow;
                    //newnotification.Title = "Lead assigned.";
                    //newnotification.Description = "Customer: " + tb_Customers.Name + " " + tb_Customers.LastName + ".";
                    //newnotification.ID_user = activeuser.ID_User;
                    //db.Sys_Notifications.Add(newnotification);



                    ////Send the email
                    //dynamic semail = new Email("NewNotification_createLead");
                    //semail.To = activeuser.Email.ToString();
                    //semail.From = "support@s7ven.co";
                    //semail.title = tb_Customers.Name + " " + tb_Customers.LastName;

                    //semail.Send();

              
                    //db.SaveChanges();

                }



            }
            catch (Exception ex)
            {
            }


            return RedirectToAction("CustomerDashboard", "CRM", new {id= tb_Customers.ID_Customer });
            
                
            
        }


        [HttpPost]
        public ActionResult NewLeadfromWeb(string idproperty, string property, string listingtype, string price, string firstname, string lastname, string email, string telephone, DateTime date, DateTime time, string category, string service, string comments)
        {
            try
            {
                //Creamos cliente y luego propiedad

                Tb_Customers newlead = new Tb_Customers();
                newlead.Name = firstname;
                newlead.LastName = lastname;
                newlead.Gender = "";
                newlead.Email = email;
                newlead.Phone = telephone;
                newlead.Birthday = DateTime.UtcNow;
                newlead.Marital_status = "";
                newlead.Type = "";
                newlead.State = "";
                newlead.Lead = true;
                newlead.ID_Company = 1;
                newlead.Zipcode = ""; 
                newlead.Mobile = ""; 
                newlead.Address = ""; 
                newlead.Active = true;
                newlead.Creation_date = DateTime.UtcNow;

                db.Tb_Customers.Add(newlead);
                db.SaveChanges();

                //Agregamos la propiedad
                Tb_Process newproperty = new Tb_Process();
    
                newproperty.ID_Property = idproperty;
                newproperty.Description=listingtype;

                string strContent = price;
                strContent = strContent.Replace("$", string.Empty);
                strContent = strContent.Replace(",", string.Empty);

                newproperty.Purchase_price = Convert.ToDecimal(strContent);
                newproperty.Address = property;
                newproperty.ID_User = 4;
                newproperty.ID_Customer = newlead.ID_Customer;
                newproperty.Commission_amount = 0;
                newproperty.Commissionperc = 0;
                newproperty.Closing_date = DateTime.UtcNow;
                newproperty.Under_contract_date = DateTime.UtcNow;
                newproperty.Offer_accepted_date = DateTime.UtcNow;
                newproperty.Inspection_date = DateTime.UtcNow;
                newproperty.Stage = "Hot Lead";
                newproperty.Source = "PGR Web";
                newproperty.TypeofAgency = "";
                newproperty.Loan_Officer_name = "";
                newproperty.Attorneys_name = "";
                newproperty.Notes = "";
                newproperty.Creation_date = DateTime.UtcNow;
                newproperty.Last_update = DateTime.UtcNow;
                newproperty.Property = "------";


                db.Tb_Process.Add(newproperty);
                db.SaveChanges();

                var details = "Lead: " + firstname + " " + lastname + "<br>";
                details += "Email: " + email + "<br>";
                details += "Tel: " + telephone + "<br>";
                details += "Category: " + category + "<br>";
                details += "Service: " + service + "<br>";
                details += "Listing address: " + property + "<br>";
                details += "Message from lead: " + comments + "<br>";

                //Enviamos notificacion a GOOGLE CALENDAR
                try
                {
                    cls_GoogleCalendar.POST_googleEvents(date, time, "New Appointment for Real Sate", details);
                }
                catch { }
              
                //


                var admins = (from a in db.Sys_Users where (a.Roles.Contains("Admin") && a.ID_Company==1) select a).ToList();

                if (admins.Count > 0) {
                    foreach (var item in admins)
                    {
                        Sys_Notifications newnotification = new Sys_Notifications();
                        newnotification.Active = true;
                        newnotification.Date = DateTime.UtcNow;
                        newnotification.Title = "New lead Created.";
                        newnotification.Description = "Request information for property ID: " + newproperty.ID_Property + ".";
                        newnotification.ID_user = item.ID_User;
                        db.Sys_Notifications.Add(newnotification);
                        try
                        {
                            //Enviamos correo para notificar
                            dynamic emailtosend = new Email("newNotification_lead");
                            emailtosend.To = item.Email;
                            emailtosend.From = "customercare@premiumgrealty.com";
                            emailtosend.IDproperty = newproperty.ID_Property;
                            emailtosend.property = newproperty.Address;
                            emailtosend.listingtype = newproperty.Description;
                            emailtosend.price = newproperty.Purchase_price;
                            emailtosend.leadname = newlead.Name + " " + newlead.LastName;
                            emailtosend.leademail = newlead.Email;

                            emailtosend.subject = "Request information for property - S7VEN WEB";
                            emailtosend.Send();
                        }
                        catch
                        {

                        }

                    }
                    db.SaveChanges();




                }

                try
                {
                    //Enviamos correo para notificar
                    dynamic emailtosend = new Email("newNotification_client");
                    emailtosend.To = newlead.Email;
                    emailtosend.From = "customercare@premiumgrealty.com";
                    emailtosend.IDproperty = newproperty.ID_Property;
                    emailtosend.property = newproperty.Address;
                    emailtosend.listingtype = newproperty.Description;
                    emailtosend.price = newproperty.Purchase_price;


                    emailtosend.subject = "Request information for property - Premium Group Realty NY";
                    emailtosend.Send();
                }
                catch
                {

                }



                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { 
                var resulterror = ex.Message;
                return Json(resulterror, JsonRequestBehavior.AllowGet);
            }

           
           
        }

        [HttpPost]
        public ActionResult NewCareerfromWeb(string firstname, string lastname, string email, string telephone, DateTime date, DateTime time, string licensedagent, string way, string modulo)
        {
            try
            {

                var details = "Name: " + firstname + " " + lastname + "<br>";
                details += "Email: " + email + "<br>";
                details += "Tel: " + telephone + "<br>";
              
              
                var titlemodu = "";

                if (modulo == "")
                {

                }
                else if (modulo == "Careers") {
                    titlemodu = "New Appointment for Careers";
                    details += "Licensed Agent: " + licensedagent + "<br>";
                }
                else if (modulo == "Sponsorship")
                {
                    titlemodu = "New Appointment for Sponsorship";
                }
                else if (modulo == "Virtual")
                {
                    titlemodu = "New Appointment for Virtual Brokerage";
                    details += "Real State Broker: " + licensedagent + "<br>";
                }
                details += "Form of contact: " + way + "<br>";
                //Enviamos notificacion a GOOGLE CALENDAR
                try
                {
                    cls_GoogleCalendar.POST_googleEvents(date, time, titlemodu, details);
                }
                catch { }

                //

                var admins = (from a in db.Sys_Users where (a.Roles.Contains("Admin") && a.ID_Company == 1) select a).ToList();


                if (admins.Count > 0)
                {
                    foreach (var item in admins)
                    {
                        try
                        {
                            //Enviamos correo para notificar
                            dynamic emailtosend = new Email("newNotification_lead");
                            emailtosend.To = item.Email;
                            emailtosend.From = "customercare@premiumgrealty.com";
                            emailtosend.leadname = firstname + " " + lastname;
                            emailtosend.leademail = email;

                            emailtosend.subject = "New Appointment - S7VEN WEB";
                            emailtosend.Send();
                        }
                        catch
                        {

                        }

                    }
            




                }

                try
                {
                    //Enviamos correo para notificar
                    dynamic emailtosend = new Email("newNotification_client");
                    emailtosend.To = email;
                    emailtosend.From = "customercare@premiumgrealty.com";

                    emailtosend.subject = "Request information for Careers - Premium Group Realty NY";
                    emailtosend.Send();
                }
                catch
                {

                }



                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var resulterror = ex.Message;
                return Json(resulterror, JsonRequestBehavior.AllowGet);
            }



        }

        [HttpGet]
        public ActionResult GetCustomerData(int id)
        {
            var customer = (from a in db.Tb_Customers where (a.ID_Customer == id) select a).Include(f=> f.Sys_Company).FirstOrDefault();

            var result = new { id = customer.ID_Customer, Name = customer.Name,Phone=customer.Phone, LastName = customer.LastName, Email = customer.Email, DateCreated = customer.Creation_date };
            
            return Json( result , JsonRequestBehavior.AllowGet);
        }

        public ActionResult Getnotes(int idcustomer)
        {
            List<Tb_Notes> lstnotes = new List<Tb_Notes>();

            lstnotes = db.Tb_Notes.Where(c => c.ID_Customer == idcustomer).ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(lstnotes);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        // GET: Tb_Customers/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id, int broker=0, string token="")
        {

            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
                //HEADER DATA
                ViewBag.activeuser = activeuser;
                ViewBag.userCompany = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();
                ViewBag.token = token;
                //ROLES
                //FIN HEADER

 

                Tb_Customers tb_Customers = db.Tb_Customers.Find(id);

                ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name", tb_Customers.ID_Company);


                var lstsource = (from o in db.Tb_Source where (o.Id_Company == activeuser.ID_Company) select o).ToList();
                ViewBag.lstSource = lstsource;
                var lststatus = (from t in db.Tb_Status where (t.Id_Company == activeuser.ID_Company) select t).ToList();
                ViewBag.lstStatus = lststatus;

                ViewBag.rol = "";

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;
                List<Sys_Users> lstagents = new List<Sys_Users>();

                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                    lstagents = (from t in db.Sys_Users where (t.ID_Company == activeuser.ID_Company) select t).ToList();
                                                  
                }
                else
                {
                    if (activeuser.Roles.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        lstagents = (from t in db.Sys_Users where (t.ID_Company == activeuser.ID_Company) select t).ToList();
                        RedirectToAction("Dashboard", "Portal", new { broker = brokersel.ID_Company });
                    
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {
                            lstagents = (from t in db.Sys_Users where  (t.ID_Company == activeuser.ID_Company || t.ID_User == 4)  select t).ToList();
               
                        }
                        else
                        {
                            ViewBag.rol = "SA";

                        }
                    }

                   


                }
                ViewBag.users = lstagents;
                ViewBag.selbroker = broker;


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

                ViewBag.states = states;
                //Verificamos si hay un agente directo asignado
                TeamsModel_Users assigneduser = new TeamsModel_Users();
                assigneduser = (from cu in db.Tb_Customers_Users
                              join u in db.Sys_Users on cu.Id_User equals u.ID_User
                              where ((cu.Id_Customer == id) && cu.ID_team == 0)
                              select new TeamsModel_Users
                              {
                                  Id_User = cu.Id_User,
                                  Name = u.Name,
                                  Lastname = u.LastName,
                                  Email = u.Email,
                                  Url_image = u.Image
                              }).FirstOrDefault();

                ViewBag.assigneduser = assigneduser;
                return View(tb_Customers);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }


        }


       


        // POST: Tb_Customers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_Customer,Name,LastName,Gender,Birthday,Marital_status,Type,Email,Phone,Mobile,State,Address,Zipcode,Lead,Active,ID_Company,Creation_date,Source")] Tb_Customers tb_Customers, int id_agent)
        {
            try
            {               
                if (tb_Customers.Zipcode == null) { tb_Customers.Zipcode = ""; }
                if (tb_Customers.Mobile == null) { tb_Customers.Mobile = ""; }
                if (tb_Customers.Address == null) { tb_Customers.Address = ""; }
                if (tb_Customers.State == null) { tb_Customers.State = ""; }
                if (tb_Customers.Type == null) { tb_Customers.Type = ""; }
                if (tb_Customers.Gender == null) { tb_Customers.Gender = ""; }
                tb_Customers.Creation_date = DateTime.UtcNow;
                //Tb_Customers customer = (from a in db.Tb_Customers.Where(a=> a.ID_Customer==tb_Customers.ID_Customer) select a ).AsNoTracking().FirstOrDefault();
                //tb_Customers.Sys_Company = db.Sys_Company.Find(tb_Customers.ID_Company);
                //tb_Customers.Tb_Process = (from a in db.Tb_Process.Where(a => a.ID_Customer == tb_Customers.ID_Customer) select a).ToList();
               

                db.Entry(tb_Customers).State=EntityState.Modified;
                db.SaveChanges();

                //asignamos agente
                //try
                //{

                //    var agentsassigned = (from c in db.Tb_Customers_Users where (c.Id_Customer == tb_Customers.ID_Customer && c.ID_team == 0) select c).ToList();
                //    if (agentsassigned.Count() > 0)
                //    {
                //        db.Tb_Customers_Users.RemoveRange(agentsassigned);
                //    }
                //    db.SaveChanges();
                //    if (id_agent != 0)
                //    {

                //        Tb_Customers_Users customerUsers = new Tb_Customers_Users();

                //        customerUsers.Id_Customer = tb_Customers.ID_Customer;
                //        customerUsers.Id_User = id_agent;
                //        customerUsers.ID_team = 0;
                //        customerUsers.Teamleader = false;
                //        db.Tb_Customers_Users.Add(customerUsers);
                //        db.SaveChanges();

                //        Sys_Notifications newnotification = new Sys_Notifications();
                //            newnotification.Active = true;
                //            newnotification.Date = DateTime.UtcNow;
                //            newnotification.Title = "Customer assigned.";
                //            newnotification.Description = "Customer: " + tb_Customers.Name + " " + tb_Customers.LastName + ".";
                //            newnotification.ID_user = id_agent;
                //            db.Sys_Notifications.Add(newnotification);
                //            db.SaveChanges();
                        
                //    }
                

                   
                //}
                //catch (Exception ex)
                //{
                //}

                return RedirectToAction("Edit", "Customers", new {id=tb_Customers.ID_Customer ,token="success" });
            }
            catch (Exception ex) {
                return RedirectToAction("Edit", "Customers", new { id = tb_Customers.ID_Customer, token = "error" });
            }        
        }



        // GET: Tb_Customers/Delete/5
        public ActionResult Delete(int? id, int broker=0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
                //HEADER DATA
                ViewBag.activeuser = activeuser;
                ViewBag.userCompany = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();
              
                //ROLES
                //FIN HEADER
                ViewBag.rol = "";

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;




                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    if (activeuser.Roles.Contains("SA") && broker == 0)
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
                Tb_Customers tb_Customers = db.Tb_Customers.Find(id);

                ViewBag.selbroker = broker;
                return View(tb_Customers);
               


            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

        }


        public ActionResult Success()
        {
            return View();
        }

        // POST: Tb_Customers/Delete/5
    
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Tb_Customers tb_Customers = db.Tb_Customers.Find(id);

                var docscust = db.Tb_LeadDocs.Where(c => (int)c.Id_Customer ==id).ToList();
                if (docscust.Count > 0)
                {
                    db.Tb_LeadDocs.RemoveRange(docscust);
                    db.SaveChanges();
                }

                Tb_Customers_UsersController tb_Customers_Users = new Tb_Customers_UsersController();
                var delete_Custo = tb_Customers_Users.Delete(id);

                db.Tb_Customers.Remove(tb_Customers);
                db.SaveChanges();

             

                var result = "Success";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception EX)
            {
                var result2 = "error";
                return Json(result2, JsonRequestBehavior.AllowGet);
            }
        
           
        }

      [HttpPost]
        public ActionResult DeleteByAjax(int id)
        {
            var result="";
            try
            {
            
               
                Tb_Customers_UsersController tb_Customers_Users = new Tb_Customers_UsersController();
                var delete_Custo = tb_Customers_Users.Delete(id);
                if (delete_Custo)
                {
                    Tb_Customers tb_Customers = db.Tb_Customers.Find(id);
                    db.Tb_Customers.Remove(tb_Customers);
                    db.SaveChanges();

                    result = "SUCCESS";
                }
                else {

                    result = "ERROR";
                }
                
                
            }
            catch(Exception EX)
            {
                 result = "An error occurred "+EX.Message;
            }
            

            
            return Json(result);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



        public ActionResult ExportLeads()
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //UTILIZANDO LIBRERIA 
                DataSet ds = getDataSetExportToExcel(activeuser.ID_Company);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(ds);
                    wb.Worksheet(1).Name = "Leads";
                    wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wb.Style.Font.Bold = true;
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=Leads.xlsx");
                    using (MemoryStream MyMemoryStream = new MemoryStream())
                    {
                        wb.SaveAs(MyMemoryStream);
                        MyMemoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                }
                return View();
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2);
                return RedirectToAction("Leads", "CRM");
            }

        }

        public DataSet getDataSetExportToExcel(int id)
        {
            
  
            List<ExportLeads> leads = db.Tb_Customers.Where(c=>c.ID_Company==id).Select(c=> new ExportLeads { ID_Customer=c.ID_Customer, Active=c.Active ? "YES" : "NO", Address=c.Address, Broker=c.ID_Company, Creation_date=c.Creation_date,
            Email=c.Email, LastName=c.LastName, Name=c.Name, Phone=c.Phone, Source=c.Source, Stage=c.Marital_status, State=c.State, Type=c.Type, Zipcode=c.Zipcode}).ToList();

            DataSet ds = new DataSet();

            DataTable dtEmpResume = new DataTable("Leads");
            dtEmpResume = ToDataTable(leads);

            ds.Tables.Add(dtEmpResume);

            return ds;
        }


        public DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public DataSet getDataSetExportToExcelTemplate(int id)
        {


            List<ExportLeadsTemplate> leads = db.Tb_Customers.Where(c => c.ID_Company == id).Select(c => new ExportLeadsTemplate
            {
                Address = c.Address,
                Email = c.Email,
                LastName = c.LastName,
                Name = c.Name,
                Phone = c.Phone,
                State = c.State,
                Zipcode = c.Zipcode
            }).ToList();

            DataSet ds = new DataSet();

            DataTable dtEmpResume = new DataTable("Leads");
            dtEmpResume = ToDataTable(leads);

            ds.Tables.Add(dtEmpResume);

            return ds;
        }
        public ActionResult ExportLeadsTemplate()
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //UTILIZANDO LIBRERIA 
                DataSet ds = getDataSetExportToExcelTemplate(0);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(ds);
                    wb.Worksheet(1).Name = "Leads";
                    wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wb.Style.Font.Bold = true;
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=Leads.xlsx");
                    using (MemoryStream MyMemoryStream = new MemoryStream())
                    {
                        wb.SaveAs(MyMemoryStream);
                        MyMemoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                }
                return View();
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2);
                return RedirectToAction("Leads", "CRM");
            }

        }

        public ActionResult ImportLeads(int broker = 0)
        {

            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
                //HEADER DATA
                ViewBag.activeuser = activeuser;
                ViewBag.userCompany = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();

                //ROLES
                //FIN HEADER

                ViewBag.userslist = (from u in db.Sys_Users where (u.Sys_Company.ID_Company == activeuser.ID_Company && (u.Roles == "Agent" || u.Roles == "Admin") && u.Active == true) orderby u.LastName ascending select u).ToList();

                ViewBag.rol = "";



                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.userslist = (from u in db.Sys_Users where (u.Sys_Company.ID_Company == activeuser.ID_Company && u.ID_User == activeuser.ID_User && u.Active == true) orderby u.LastName ascending select u).ToList();
                }
                else
                {
                    if (activeuser.Roles.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("SA")) select b).FirstOrDefault();
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


                ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name");
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

        }

        [HttpPost]
        public ActionResult ImportLeads(HttpPostedFileBase file)
        {
            DataTable dt = new DataTable();
            //Checking file content length and Extension must be .xlsx  
            if (file != null && file.ContentLength > 0 && System.IO.Path.GetExtension(file.FileName).ToLower() == ".xlsx")
            {
                string path = Path.Combine(Server.MapPath("~/Content/Uploads"), Path.GetFileName(file.FileName));
                //Saving the file  
                file.SaveAs(path);
                //Started reading the Excel file.  
                using (XLWorkbook workbook = new XLWorkbook(path))
                {
                    IXLWorksheet worksheet = workbook.Worksheet(1);
                    bool FirstRow = true;
                    //Range for reading the cells based on the last cell used.  
                    string readRange = "1:1";
                    foreach (IXLRow row in worksheet.RowsUsed())
                    {
                        //If Reading the First Row (used) then add them as column name  
                        if (FirstRow)
                        {
                            //Checking the Last cellused for column generation in datatable  
                            readRange = string.Format("{0}:{1}", 1, row.LastCellUsed().Address.ColumnNumber);
                            foreach (IXLCell cell in row.Cells(readRange))
                            {
                                dt.Columns.Add(cell.Value.ToString());
                            }
                            FirstRow = false;
                        }
                        else
                        {
                            //Adding a Row in datatable  
                            dt.Rows.Add();
                            int cellIndex = 0;
                            //Updating the values of datatable  
                            foreach (IXLCell cell in row.Cells(readRange))
                            {
                                dt.Rows[dt.Rows.Count - 1][cellIndex] = cell.Value.ToString();
                                cellIndex++;
                            }
                        }
                    }
                    //If no data in Excel file  
                    if (FirstRow)
                    {
                        ViewBag.Message = "Empty Excel File!";
                    }
                }
            }
            else
            {
                //If file extension of the uploaded file is different then .xlsx  
                ViewBag.Message = "Please select file with .xlsx extension!";
            }
            //Creamos nuevos clientes

            ///
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            //NOTIFICATIONS
            DateTime now = DateTime.Today;
            List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
            ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
            //HEADER DATA
            ViewBag.activeuser = activeuser;
            ViewBag.userCompany = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();

            //ROLES
            //FIN HEADER

            ViewBag.userslist = (from u in db.Sys_Users where (u.Sys_Company.ID_Company == activeuser.ID_Company && (u.Roles == "Agent" || u.Roles == "Admin") && u.Active == true) orderby u.LastName ascending select u).ToList();

            ViewBag.rol = "";



            if (activeuser.Roles.Contains("Agent"))
            {
                ViewBag.rol = "Agent";
                ViewBag.userslist = (from u in db.Sys_Users where (u.Sys_Company.ID_Company == activeuser.ID_Company && u.ID_User == activeuser.ID_User && u.Active == true) orderby u.LastName ascending select u).ToList();
            }
            else
            {
                if (activeuser.Roles.Contains("SA"))
                {
                    ViewBag.rol = "SA";
                    ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company) select usd).FirstOrDefault();
                    var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("SA")) select b).FirstOrDefault();
                    RedirectToAction("Dashboard", "Portal", new { broker = brokersel.ID_Company });
                }
                else
                {
                    ViewBag.rol = "Admin";
                }

                ViewBag.userslist = (from u in db.Sys_Users where (u.Sys_Company.ID_Company == activeuser.ID_Company && (u.Roles == "Agent" || u.Roles == "Admin") && u.Active == true) orderby u.LastName ascending select u).ToList();

            }



            ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name");

            return View(dt);
        }
    }
}

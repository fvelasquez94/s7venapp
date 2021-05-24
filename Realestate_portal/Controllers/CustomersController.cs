using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Postal;
using Realestate_portal.Models;
using Realestate_portal.Models.ViewModels.CRM;

namespace Realestate_portal.Controllers
{
    public class CustomersController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();
        private Cls_GoogleCalendar cls_GoogleCalendar = new Cls_GoogleCalendar();
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
                ViewBag.userslist = (from u in db.Sys_Users where u.Sys_Company.ID_Company == activeuser.ID_Company && (u.Roles== "Agent" || u.Roles =="Admin")select u).ToList();

                ViewBag.rol = "";

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
        public ActionResult Create([Bind(Include = "ID_Customer,Name,LastName,Gender,Birthday,Marital_status,Type,Email,Phone,Mobile,Country,State,City,Address,Zipcode,Lead,ID_User,Active,ID_Company,Creation_date,Source")] Tb_Customers tb_Customers)
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            if (tb_Customers.Birthday == null) { tb_Customers.Birthday = DateTime.UtcNow; }
            var birthdemo = Convert.ToDateTime(tb_Customers.Birthday);
            var theDate = new DateTime(1900, 01, 01, 00, 00, 00);
            if (birthdemo < theDate) {
                tb_Customers.Birthday = DateTime.UtcNow;
            }
            var user_assigned = (from u in db.Sys_Users where u.ID_User == tb_Customers.ID_User select u).FirstOrDefault();
        
            tb_Customers.User_assigned = user_assigned.Name + " " + user_assigned.LastName;
            tb_Customers.ID_Company = activeuser.ID_Company;
            tb_Customers.Lead = false;
            if (tb_Customers.Zipcode == null) { tb_Customers.Zipcode = ""; }
            if (tb_Customers.Mobile == null) { tb_Customers.Mobile = ""; }
            if (tb_Customers.Address == null) { tb_Customers.Address = ""; }

            if (tb_Customers.Email == null) { tb_Customers.Email = ""; }
            tb_Customers.Active = true;
            tb_Customers.Creation_date = DateTime.UtcNow;

        
                db.Tb_Customers.Add(tb_Customers);
                db.SaveChanges();

            Sys_Notifications newnotification = new Sys_Notifications();
            newnotification.Active = true;
            newnotification.Date = DateTime.UtcNow;
            newnotification.Title = "New Customer assigned.";
            newnotification.Description = "Customer: " + tb_Customers.Name + " " + tb_Customers.LastName + ".";
            newnotification.ID_user = user_assigned.ID_User;
            db.Sys_Notifications.Add(newnotification);
            db.SaveChanges();


            return RedirectToAction("Customers", "CRM");
            
                
            
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
                newlead.ID_User = 4;
                newlead.User_assigned = "Not Assigned";
                newlead.Country = "";
                newlead.State = "";
                newlead.City = "";
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

                            emailtosend.subject = "Request information for property - PGR WEB";
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

                            emailtosend.subject = "New Appointment - PGR WEB";
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

            var result = new { id = customer.ID_Customer, Name = customer.Name,Phone=customer.Phone, LastName = customer.LastName, Email = customer.Email, User = customer.User_assigned, DateCreated = customer.Creation_date };
            
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
        public ActionResult Edit(int? id, int broker=0)
        {

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


                Tb_Customers tb_Customers = db.Tb_Customers.Find(id);

                ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name", tb_Customers.ID_Company);
        



                ViewBag.rol = "";

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;


                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.ID_User = new SelectList((from t in db.Sys_Users
                                                          // where (t.Roles.Contains("Agent"))
                                                      select new
                                                      {
                                                          ID = t.ID_User,
                                                          FullName = t.Name + " " + t.LastName
                                                      }), "ID", "FullName", tb_Customers.ID_User);
                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        RedirectToAction("Dashboard", "Portal", new { broker = brokersel.ID_Company });
                        ViewBag.ID_User = new SelectList((from t in db.Sys_Users
                                                              // where (t.Roles.Contains("Agent"))
                                                          select new
                                                          {
                                                              ID = t.ID_User,
                                                              FullName = t.Name + " " + t.LastName
                                                          }), "ID", "FullName", tb_Customers.ID_User);
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {
                            ViewBag.ID_User = new SelectList((from t in db.Sys_Users
                                                                  where (t.ID_Company == activeuser.ID_Company || t.ID_User==4)
                                                              select new
                                                              {
                                                                  ID = t.ID_User,
                                                                  FullName = t.Name + " " + t.LastName
                                                              }), "ID", "FullName", tb_Customers.ID_User);
                        }
                        else
                        {
                            ViewBag.rol = "SA";

                        }
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
        public ActionResult Edit([Bind(Include = "ID_Customer,Name,LastName,Gender,Birthday,Marital_status,Type,Email,Phone,Mobile,Country,State,City,Address,Zipcode,Lead,ID_User,User_assigned,Active,ID_Company,Creation_date,Source")] Tb_Customers tb_Customers)
        {
            try
            {               
                if (tb_Customers.Zipcode == null) { tb_Customers.Zipcode = ""; }
                if (tb_Customers.Mobile == null) { tb_Customers.Mobile = ""; }
                if (tb_Customers.Address == null) { tb_Customers.Address = ""; }
                if (tb_Customers.City == null) { tb_Customers.City = ""; }
                if (tb_Customers.Country == null) { tb_Customers.Country = ""; }
                if (tb_Customers.State == null) { tb_Customers.State = ""; }
                if (tb_Customers.Type == null) { tb_Customers.Type = ""; }
                tb_Customers.Creation_date = DateTime.UtcNow;
                Tb_Customers customer = (from a in db.Tb_Customers.Where(a=> a.ID_Customer==tb_Customers.ID_Customer) select a ).AsNoTracking().FirstOrDefault();
                tb_Customers.Sys_Company = db.Sys_Company.Find(tb_Customers.ID_Company);
                tb_Customers.Tb_Process = (from a in db.Tb_Process.Where(a => a.ID_Customer == tb_Customers.ID_Customer) select a).ToList();
                var usuarioantes = db.Tb_Customers.Where(c => c.ID_Customer == tb_Customers.ID_Customer).Select(c => c.ID_User).FirstOrDefault();

                db.Entry(tb_Customers).State=EntityState.Modified;
                db.SaveChanges();
                TempData["exito"] = "Customer info updated successfully.";

                if (usuarioantes != tb_Customers.ID_User)
                {
                    var customerislead = (from h in db.Tb_Customers where (h.ID_Customer == tb_Customers.ID_Customer) select h).FirstOrDefault();

                    if (tb_Customers.ID_User == 4)
                    {
                        customerislead.Lead = true;
                        db.Entry(customerislead).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else {
                        if (customerislead.Lead == true)
                        {
                            customerislead.Lead = false;
                            db.Entry(customerislead).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
           
                    Sys_Notifications newnotification = new Sys_Notifications();
                    newnotification.Active = true;
                    newnotification.Date = DateTime.UtcNow;
                    newnotification.Title = "New Customer assigned.";
                    newnotification.Description = "Customer: " + tb_Customers.Name + " " + tb_Customers.LastName + ".";
                    newnotification.ID_user = tb_Customers.ID_User;
                    db.Sys_Notifications.Add(newnotification);
                    db.SaveChanges();                                       
                }

                return RedirectToAction("CustomerDashboard", "CRM", new {id=tb_Customers.ID_Customer, broker= 0 });
            }
            catch (Exception ex) {
                TempData["advertencia"] = "Something went wrong, the customer info could not be updated, please try again";
                return RedirectToAction("CustomerDashboard", "CRM", new { id = tb_Customers.ID_Customer, broker = 0 });
            }        
        }



        // GET: Tb_Customers/Delete/5
        public ActionResult Delete(int? id, int broker=0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "CRM";
                ViewData["Page"] = "Properties";
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tb_Customers tb_Customers = db.Tb_Customers.Find(id);
            db.Tb_Customers.Remove(tb_Customers);
            db.SaveChanges();
            return RedirectToAction("Customers", "CRM");
        }

      [HttpPost]
        public ActionResult DeleteByAjax(int id)
        {
            var result="";
            try
            {
                Tb_Customers tb_Customers = db.Tb_Customers.Find(id);
                db.Tb_Customers.Remove(tb_Customers);
                db.SaveChanges();

                 result = "SUCCESS";
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
    }
}

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

namespace Realestate_portal.Controllers
{
    public class PropertiesController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();
        // GET: Properties
        public ActionResult Index()
        {
            var tb_Process = db.Tb_Process.Include(t => t.Tb_Customers);
            return View(tb_Process.ToList());
        }

        // GET: Properties/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Process tb_Process = db.Tb_Process.Find(id);
            if (tb_Process == null)
            {
                return HttpNotFound();
            }
            return View(tb_Process);
        }

        // GET: Properties/Create
        public ActionResult Create(int broker=0)
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
                ViewBag.rol = "";

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;


                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.ID_User = new SelectList((from t in db.Sys_Users
                                                          where (t.ID_User==activeuser.ID_User)
                                                      select new
                                                      {
                                                          ID = t.ID_User,
                                                          FullName = t.Name + " " + t.LastName
                                                      }), "ID", "FullName");

                    ViewBag.ID_Customer = new SelectList((from t in db.Tb_Customers
                                                          where (t.Lead == false)
                                                          select new
                                                          {
                                                              ID = t.ID_Customer,
                                                              FullName = t.Name + " " + t.LastName
                                                          }), "ID", "FullName");
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
                            ViewBag.ID_User = new SelectList((from t in db.Sys_Users
                                                                  where (t.Roles.Contains("Agent") && t.ID_Company==activeuser.ID_Company)
                                                              select new
                                                              {
                                                                  ID = t.ID_User,
                                                                  FullName = t.Name + " " + t.LastName
                                                              }), "ID", "FullName");

                            ViewBag.ID_Customer = new SelectList((from t in db.Tb_Customers
                                                                  where (t.Lead == false && t.ID_Company == activeuser.ID_Company)
                                                                  select new
                                                                  {
                                                                      ID = t.ID_Customer,
                                                                      FullName = t.Name + " " + t.LastName
                                                                  }), "ID", "FullName");
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

                return View("Create");

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }


         
        }
        public ActionResult Getnotes(int idprocess)
        {
            List<Tb_Notes> lstnotes = new List<Tb_Notes>();

            lstnotes = db.Tb_Notes.Where(c => c.ID_Property == idprocess).ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(lstnotes);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        // POST: Properties/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProperty([Bind(Include = "ID_Process,Description,ID_User,ID_Customer,ID_Property,Property,Address,Purchase_price,Commission_amount,Commissionperc,Closing_date,Under_contract_date,Offer_accepted_date,Inspection_date,Stage,Source,TypeofAgency,Loan_Officer_name,Attorneys_name,Notes,Creation_date,Last_update,Loan_Officer_tel,Attorneys_tel")] Tb_Process tb_Process)
        {
            if (tb_Process.Address == null) { tb_Process.Address = ""; }
            if (tb_Process.TypeofAgency == null) { tb_Process.TypeofAgency = ""; }
            if (tb_Process.Loan_Officer_name == null) { tb_Process.Loan_Officer_name = ""; }
            if (tb_Process.Loan_Officer_tel == null) { tb_Process.Loan_Officer_tel = ""; }
            if (tb_Process.Attorneys_tel == null) { tb_Process.Attorneys_tel = ""; }
            if (tb_Process.Attorneys_name == null) { tb_Process.Attorneys_name = ""; }
            if (tb_Process.Notes == null) { tb_Process.Notes = ""; }
            tb_Process.Last_update = DateTime.UtcNow;
            tb_Process.Closing_date = DateTime.UtcNow;
            tb_Process.Creation_date = DateTime.UtcNow;
            tb_Process.Inspection_date = DateTime.UtcNow;
            tb_Process.Offer_accepted_date = DateTime.UtcNow;
            tb_Process.Under_contract_date = DateTime.UtcNow;
                      
            var customer = (from a in db.Tb_Customers.Where(a => a.ID_Customer == tb_Process.ID_Customer) select a).FirstOrDefault();
            tb_Process.ID_Customer = customer.ID_Customer;
            tb_Process.Description = tb_Process.Address + " " + customer.Name + " " + customer.LastName + " " + tb_Process.Creation_date.ToShortDateString();
            tb_Process.ID_Property= tb_Process.Address + customer.Name+ customer.LastName + tb_Process.Creation_date.ToShortDateString();
            tb_Process.Source = customer.Source;
            if (tb_Process.Stage == null)
            {
                tb_Process.Stage = customer.Marital_status;
            }
            if (customer.Source == null)
            {
                tb_Process.Source = "";
            }
            customer.Address = tb_Process.Address;
            db.Entry(customer).State = EntityState.Modified;
            db.Tb_Process.Add(tb_Process);
            db.SaveChanges();
  
            return RedirectToAction("CustomerDashboard", "CRM", new { id = customer.ID_Customer, token="success" });
        }

        // GET: Properties/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id, int broker=0)
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

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;

                Tb_Process tb_Process = db.Tb_Process.Find(id);
                ViewBag.rol = "";

                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.ID_User = new SelectList((from t in db.Sys_Users
                                                      where (t.ID_User == activeuser.ID_User)
                                                      select new
                                                      {
                                                          ID = t.ID_User,
                                                          FullName = t.Name + " " + t.LastName
                                                      }), "ID", "FullName");

                    ViewBag.ID_Customer = new SelectList((from t in db.Tb_Customers
                                                          
                                                          select new
                                                          {
                                                              ID = t.ID_Customer,
                                                              FullName = t.Name + " " + t.LastName
                                                          }), "ID", "FullName");
      



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


                        ViewBag.ID_Customer = new SelectList((from t in db.Tb_Customers
                                                              where (t.ID_Company == activeuser.ID_Company)
                                                              select new
                                                              {
                                                                  ID = t.ID_Customer,
                                                                  FullName = t.Name + " " + t.LastName
                                                              }), "ID", "FullName", tb_Process.ID_Customer);


                        ViewBag.ID_User = new SelectList((from t in db.Sys_Users
                                                              //where (t.Roles.Contains("Agent"))
                                                          select new
                                                          {
                                                              ID = t.ID_User,
                                                              FullName = t.Name + " " + t.LastName
                                                          }), "ID", "FullName", tb_Process.ID_User);
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
                return View(tb_Process);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }



        }

        // POST: Properties/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_Process,Description,ID_User,ID_Customer,ID_Property,Property,Address,Purchase_price,Commission_amount,Commissionperc,Closing_date,Under_contract_date,Offer_accepted_date,Inspection_date,Stage,Source,TypeofAgency,Loan_Officer_name,Attorneys_name,Notes,Creation_date,Last_update,Loan_Officer_tel,Attorneys_tel")] Tb_Process tb_Process)
        {
            try
            {
                if (tb_Process.Address == null) { tb_Process.Address = ""; }
                if (tb_Process.TypeofAgency == null) { tb_Process.TypeofAgency = ""; }
                if (tb_Process.Loan_Officer_name == null) { tb_Process.Loan_Officer_name = ""; }
                if (tb_Process.Attorneys_name == null) { tb_Process.Attorneys_name = ""; }
                if (tb_Process.Notes == null) { tb_Process.Notes = ""; }
                if (tb_Process.Loan_Officer_tel == null) { tb_Process.Loan_Officer_tel = ""; }
                if (tb_Process.Attorneys_tel == null) { tb_Process.Attorneys_tel = ""; }
                if (tb_Process.Notes == null) { tb_Process.Notes = ""; }
                if (tb_Process.Source == null) { tb_Process.Source = ""; }
                //var usuarioantes = (from a in db.Tb_Process.Where(c => c.ID_Process == tb_Process.ID_Process) select a).AsNoTracking().FirstOrDefault();                                          
                db.Entry(tb_Process).State = EntityState.Modified;
                db.SaveChanges();
                TempData["exito"] = "Property updated successfully.";

                //if (usuarioantes.ID_User != tb_Process.ID_User)
                //{
                    //var customerislead = (from h in db.Tb_Customers where (h.ID_Customer == tb_Process.ID_Customer) select h).AsNoTracking().FirstOrDefault();
                    //if (customerislead != null) {
                    //    if (customerislead.Lead == true) {
                    //        customerislead.Lead = false;
                    //        db.Entry(customerislead).State = EntityState.Modified;
                    //        db.SaveChanges();
                    //    }
                   
                    //}


                        //    Sys_Notifications newnotification = new Sys_Notifications();
                        //    newnotification.Active = true;
                        //    newnotification.Date = DateTime.UtcNow;
                        //    newnotification.Title = "New property assigned.";
                        //    newnotification.Description = "Property ID: " + tb_Process.ID_Property + ".";
                        //    newnotification.ID_user = tb_Process.ID_User;
                        //    db.Sys_Notifications.Add(newnotification);
                        //db.SaveChanges();

                    //if (tb_Process.ID_User !=4) {
                    //    try
                    //    {
                    //        var user = (db.Sys_Users.Where(d => d.ID_User == tb_Process.ID_User).Select(d => d.Email)).FirstOrDefault();

                    //        //Enviamos correo para notificar
                    //        dynamic emailtosend = new Email("newNotification_propertyAgent");
                    //        emailtosend.To = user;
                    //        emailtosend.From = "customercare@premiumgrealty.com";
                    //        emailtosend.IDproperty = tb_Process.ID_Property;
                    //        emailtosend.property = tb_Process.Property;
                    //        emailtosend.listingtype = tb_Process.Description;
                    //        emailtosend.price = tb_Process.Purchase_price;
                    //        emailtosend.customerID = tb_Process.ID_Customer;

                    //        emailtosend.subject = "New property assigned - S7VEN WEB";
                    //        emailtosend.Send();
                    //    }
                    //    catch(Exception ex)
                    //    {

                    //    }
                    //}


                //}

                return RedirectToAction("Edit", "Properties", new { id = tb_Process.ID_Process });
            }
            catch (Exception ex) {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("Edit", "Properties", new { id = tb_Process.ID_Process});
            }
          
        }

        // GET: Properties/Delete/5
        [HttpGet]
        public ActionResult Delete(int? id, int broker=0)
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
                ViewBag.selbroker = broker;



                Tb_Process tb_Process = db.Tb_Process.Find(id);

                

                return View(tb_Process);

                
            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

        }

        // POST: Properties/Delete/5

        public ActionResult DeleteConfirmed(int id)
        {
            Tb_Process tb_Process = db.Tb_Process.Find(id);
            try
            {               
                db.Tb_Process.Remove(tb_Process);
                db.SaveChanges();
             

                var packages = (from a in db.Tb_Docpackages where (a.ID_Process == id) select a).ToList();
                if (packages.Count > 0)
                {
                    db.Tb_Docpackages.RemoveRange(packages);
                    db.SaveChanges();
                }

                var result = "Success";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {

                var result = "error";
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

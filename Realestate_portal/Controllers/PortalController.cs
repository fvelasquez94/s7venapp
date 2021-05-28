using Ionic.Zip;
using Newtonsoft.Json;
using Postal;
using Realestate_portal.Models;
using Realestate_portal.Models.ViewModels.CRM;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Realestate_portal.Controllers
{
    public class PortalController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();
        private Cls_GoogleCalendar cls_GoogleCalendar = new Cls_GoogleCalendar();
        //Credentials

        public ActionResult Log_out()
        {
            Session.RemoveAll();
            //Global_variables.active_user.Name = null;
            //Global_variables.active_Departments = null;
            //Global_variables.active_Roles = null;
            if (Request.Cookies["correo"] != null)
            {
                var c = new HttpCookie("correo");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }
            if (Request.Cookies["pass"] != null)
            {
                var c = new HttpCookie("pass");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }
            if (Request.Cookies["rememberme"] != null)
            {
                var c = new HttpCookie("rememberme");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }



            return RedirectToAction("Login", "Portal", new { access = true, logpage = 2 });
        }

        [HttpPost]
        public ActionResult SendPackage(int idpackage, string email)
        {
            var result = "";
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                string company_email = (from c in db.Sys_Company where c.ID_Company == activeuser.ID_Company select c.Web).FirstOrDefault<string>();
                if (activeuser != null)
                {
                    var zippackage = "";
                    var docpackage = (from h in db.Tb_Docpackages where (h.ID_docpackage == idpackage) select h).FirstOrDefault();
                    var customer = (from a in db.Tb_Customers.Where(a => a.ID_Customer == docpackage.ID_Customer) select a).FirstOrDefault();
                    if (docpackage != null)
                    {

                        docpackage.Finished = true;
                        docpackage.Sent = true;
                        db.Entry(docpackage).State = EntityState.Modified;
                        db.SaveChanges();

                        var listdetais = (from a in db.Tb_Docpackages_details where (a.ID_docpackage == idpackage && a.URL != "") select a).ToList();

                        zippackage = Server.MapPath("~/Content/Uploads/DocumentsPackages/" + docpackage.ID_docpackage + "_documentsPack.zip");
                        if (!System.IO.File.Exists(zippackage))
                        {
                            using (ZipFile zip = new ZipFile())
                            {
                                foreach (var item in listdetais)
                                {
                                    // add this map file into the "images" directory in the zip archive
                                    zip.AddFile(Server.MapPath(item.URL), "documents");
                                }

                                zip.Save(zippackage);
                            }
                        }
                        else
                        {

                        }
                    

                            var brokeremail = company_email;
                        try
                        {


                            if (brokeremail != null && brokeremail != "")
                            {
                                //Enviamos correo para notificar
                                dynamic emailtosend = new Email("newpackage_notification");
                                emailtosend.To = brokeremail;
                                emailtosend.From = "support@s7ven.co";
                                emailtosend.subject = "New documents Package from " + activeuser.Name + " " + activeuser.LastName + " for Lead "+customer.Name +" "+customer.LastName+" - PGR Agents Portal";                               
                                emailtosend.Attach(new Attachment(zippackage));
                                emailtosend.Send();
                                
                                if (email!=null && email!="")
                                {
                                    //Enviamos correo para notificar
                                    dynamic emailtosendagent = new Email("newpackage_notification");
                                    emailtosendagent.To = email;
                                    emailtosendagent.From = "support@s7ven.co";
                                    emailtosendagent.subject = "New documents Package from " + activeuser.Name + " " + activeuser.LastName + "  - PGR Agents Portal";                                   
                                    emailtosendagent.Attach(new Attachment(zippackage));
                                    emailtosendagent.Send();
                                }

                                result = "SUCCESS";
                            }
                            else {
                                result = "Data saved but email was not configured";
                            }

                        }
                        catch (Exception ex)
                        {
                            result = "Data saved but email was not configured.";
                        }


                    }
                }
                else {
                    result = "No user data was found.";
                }






                return Json(result);
            }
            catch (Exception ex)
            {
                return Json("Something when wrong...");
            }

        }


        [HttpGet]
        public ActionResult markasread()
        {

            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                db.Database.ExecuteSqlCommand("update Sys_Notifications set Active=0 where ID_user={0}", activeuser.ID_User);

                return Redirect(Request.UrlReferrer.ToString());
            }
            catch {
                return Redirect(Request.UrlReferrer.ToString());
            }



        }

        public ActionResult Login(bool access = true, int? logpage = 0)
        {
            if (access == false)
            {
                if (logpage == 0)
                {
                    TempData["advertencia"] = "Expired Session.";
                }
                else if (logpage == 1)
                {
                    TempData["advertencia"] = "Wrong email or password.";
                }
                else {

                }

            }

            HttpCookie aCookieCorreo = Request.Cookies["correo"];
            HttpCookie aCookiePassword = Request.Cookies["pass"];
            HttpCookie aCookieRememberme = Request.Cookies["rememberme"];
            ViewBag.correo = "";
            ViewBag.pass = "";
            ViewBag.remember = false;
            //try
            //{
            //    var correo = Server.HtmlEncode(aCookieCorreo.Value).ToString();
            //    var pass = Server.HtmlEncode(aCookiePassword.Value).ToString();
            //    int remember = Convert.ToInt32(Server.HtmlEncode(aCookieRememberme.Value));

            //    if (remember == 1) { ViewBag.remember = true; } else { ViewBag.remember = false; }
            //    ViewBag.correo = correo;
            //    ViewBag.pass = pass;

            //}
            //catch
            //{
            //    ViewBag.remember = false;

            //}



            return View();
        }

        public ActionResult Log_in(string email, string password, bool? rememberme)
        {
            if (rememberme == null) { rememberme = true; }
            rememberme = true;
            Session["activeUser"] = (from a in db.Sys_Users where (a.Email == email && a.Password == password && a.Active == true) select a).FirstOrDefault();
            if (Session["activeUser"] != null)
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                ///PARA RECORDAR DATOS
                if (rememberme == true)
                {
                    if (Request.Cookies["correo"] != null)
                    {
                        if (Request.Cookies["correo"] != null)
                        {
                            var c = new HttpCookie("correo");
                            c.Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies.Add(c);
                        }
                        if (Request.Cookies["pass"] != null)
                        {
                            var c = new HttpCookie("pass");
                            c.Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies.Add(c);
                        }
                        if (Request.Cookies["rememberme"] != null)
                        {
                            var c = new HttpCookie("rememberme");
                            c.Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies.Add(c);
                        }

                        HttpCookie aCookie = new HttpCookie("correo");
                        aCookie.Value = activeuser.Email.ToString();
                        aCookie.Expires = DateTime.Now.AddMonths(3);

                        HttpCookie aCookie2 = new HttpCookie("pass");
                        aCookie2.Value = activeuser.Password.ToString();
                        aCookie2.Expires = DateTime.Now.AddMonths(3);

                        HttpCookie aCookie3 = new HttpCookie("rememberme");
                        aCookie3.Value = "1";
                        aCookie3.Expires = DateTime.Now.AddMonths(3);


                        Response.Cookies.Add(aCookie);
                        Response.Cookies.Add(aCookie2);
                        Response.Cookies.Add(aCookie3);
                    }
                    else
                    {
                        HttpCookie aCookie = new HttpCookie("correo");
                        aCookie.Value = activeuser.Email.ToString();
                        aCookie.Expires = DateTime.Now.AddMonths(3);

                        HttpCookie aCookie2 = new HttpCookie("pass");
                        aCookie2.Value = activeuser.Password.ToString();
                        aCookie2.Expires = DateTime.Now.AddMonths(3);

                        HttpCookie aCookie3 = new HttpCookie("rememberme");
                        aCookie3.Value = "1";
                        aCookie3.Expires = DateTime.Now.AddMonths(3);


                        Response.Cookies.Add(aCookie);
                        Response.Cookies.Add(aCookie2);
                        Response.Cookies.Add(aCookie3);
                    }
                }
                else
                {
                    if (Request.Cookies["correo"] != null)
                    {
                        var c = new HttpCookie("correo");
                        c.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(c);
                    }
                    if (Request.Cookies["pass"] != null)
                    {
                        var c = new HttpCookie("pass");
                        c.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(c);
                    }
                    if (Request.Cookies["rememberme"] != null)
                    {
                        var c = new HttpCookie("rememberme");
                        c.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(c);
                    }

                }


                if (activeuser.Roles.Contains("SA"))
                {
                    ViewBag.rol = "SA";
                    return RedirectToAction("Dashboard", "Portal", new { broker = 1 });
                }
                else if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";

                    return RedirectToAction("Dashboard", "Portal", null);
                }
                else if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                    return RedirectToAction("Dashboard", "Portal", null);
                }





            }

            return RedirectToAction("Login", "Portal", new { access = false, logpage = 1 });
        }


        public ActionResult Appointment(int? id, string idproperty, string property, string price, string listingtype, string urlimg)
        {

            try
            {

                if (idproperty != "" && idproperty != null) { ViewBag.hasproperty = 1; } else { ViewBag.hasproperty = 0; }

            }
            catch
            {
                ViewBag.hasproperty = 0;

            }

            ViewBag.idproperty = idproperty;
            ViewBag.property = property;
            ViewBag.price = price;
            ViewBag.listingtype = listingtype;
            ViewBag.urlimg = urlimg;

            return View();
        }

        public ActionResult AppointmentCareers(string sub)
        {
            ViewBag.modulo = sub;
            return View();
        }
        public ActionResult Process(int? id, string idproperty, string property, string price, string listingtype, string urlimg)
        {

            try
            {

                if (idproperty != "" && idproperty != null) { ViewBag.hasproperty = 1; } else { ViewBag.hasproperty = 0; }

            }
            catch
            {
                ViewBag.hasproperty = 0;

            }

            ViewBag.idproperty = idproperty;
            ViewBag.property = property;
            ViewBag.price = price;
            ViewBag.listingtype = listingtype;
            ViewBag.urlimg = urlimg;

            return View();
        }
        public ActionResult Forgot_password(bool token = false, bool email = false, string errorconsole = "")
        {
            if (token == true)
            {
                ViewData["msgnotexist"] = "";
                ViewBag.msg = "Your password has been changed successfully! Your new password has been sent to your email address.";
            }
            else
            {
                if (email == true)
                {
                    ViewData["msgnotexist"] = "This email does not exist.";
                }
                else
                {
                    ViewData["msgnotexist"] = "";
                }

                ViewBag.msg = "";
            }

            ViewBag.errorconsole = errorconsole;

            return View();
        }

        public ActionResult Reset_pass(string email)
        {


            Sys_Users User = (from a in db.Sys_Users where (a.Email == email && a.Active == true) select a).FirstOrDefault();
            if (User != null)
            {
                User.Password = "pgr2020";
                db.Entry(User).State = EntityState.Modified;
                db.SaveChanges();

                try
                {
                    if (User.Email != "")
                    {
                        //Send the email
                        dynamic semail = new Email("reset_password");
                        semail.To = User.Email.ToString();
                        semail.From = "support@s7ven.co";
                        semail.user = User.Name + " " + User.LastName;
                        semail.email = User.Email;
                        semail.password = User.Password;

                        semail.Send();
                        return RedirectToAction("Forgot_password", "Portal", new { token = true, email = false, errorconsole = "" });
                    }
                    else {
                        return RedirectToAction("Forgot_password", "Portal", new { token = false, email = true, errorconsole = "No email configured" });
                    }

                    //FIN email
                }
                catch (Exception ex) {
                    return RedirectToAction("Forgot_password", "Portal", new { token = false, email = true, errorconsole = ex.Message });

                }


            }

            return RedirectToAction("Forgot_password", "Portal", new { token = false, email = true, errorconsole = "No user email found" });
        }


        public ActionResult test_API()
        {
            Cls_GoogleCalendar sevent = new Cls_GoogleCalendar();
            sevent.Test_googleEvents();
            return View();
        }

        public ActionResult PGR_Market(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "PGR Market";
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
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
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
                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;



                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        // END - Credentials
        public ActionResult Dashboard(int broker = 0)
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
                //FIN HEADER

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;
                //
                List<Tb_Posts> lstpost = new List<Tb_Posts>();
                Tb_Posts Broker_post = new Tb_Posts();
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                    Broker_post = (from a in db.Tb_Posts where ((a.Post_type == 1 && a.Active == true) && (a.ID_User == brokersel.ID_User)) select a).FirstOrDefault();
                    ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_User == activeuser.ID_User) select usd).FirstOrDefault();
                    ViewBag.userdataBroker = (from usd in db.Sys_Users where (usd.Roles.Contains("Admin") && usd.ID_Company == activeuser.ID_Company) select usd).FirstOrDefault();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_User == activeuser.ID_User && usd.Roles.Contains("SA")) select usd).FirstOrDefault();
                        ViewBag.userdataBroker = (from usd in db.Sys_Users where (usd.ID_User == activeuser.ID_User && usd.Roles.Contains("SA")) select usd).FirstOrDefault();

                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        RedirectToAction("Dashboard", "Portal", new { broker = brokersel.ID_Company });
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {
                            ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_User == activeuser.ID_User) select usd).FirstOrDefault();
                            ViewBag.userdataBroker = (from usd in db.Sys_Users where (usd.ID_User == activeuser.ID_User) select usd).FirstOrDefault();
                            Broker_post = (from a in db.Tb_Posts where ((a.Post_type == 1 && a.Active == true) && (a.ID_User == activeuser.ID_User)) select a).FirstOrDefault();
                        }
                        else
                        {

                            ViewBag.rol = "SA";

                            ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_User == activeuser.ID_User && usd.Roles.Contains("SA")) select usd).FirstOrDefault();
                            ViewBag.userdataBroker = (from usd in db.Sys_Users where (usd.ID_User == activeuser.ID_User && usd.Roles.Contains("SA")) select usd).FirstOrDefault();
                            var brokersel = (from b in db.Sys_Users where (b.ID_Company == broker && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                            Broker_post = (from a in db.Tb_Posts where ((a.Post_type == 1 && a.Active == true) && (a.ID_User == brokersel.ID_User)) select a).FirstOrDefault();
                        }
                    }



                }

                ViewBag.selbroker = broker;
                //POST TYPE: 1-BROKER | 2-AGENT | 3-ADMIN
                //BROKER MESSAGE

                int likepost = 0;
                int idparentpost = 0;


                if (Broker_post != null && Broker_post.Title != null)
                {
                    idparentpost = Broker_post.ID_Post;
                    lstpost.Add(Broker_post);
                    var agent_posts = (from a in db.Tb_Posts where (a.Post_type == 2 && a.Post_parent == Broker_post.ID_Post) select a).ToList();
                    if (agent_posts.Count > 0) {
                        lstpost.AddRange(agent_posts);
                    }
                    //Verificamos si le dio like al post
                    if (Broker_post.User_likes == "") { }
                    else
                    {
                        List<int> TagIds = Broker_post.User_likes.Split(',').Select(int.Parse).ToList();
                        if (TagIds.Contains(activeuser.ID_User))
                        {
                            likepost = 1;
                        }

                    }




                }
                var quote = "";
                var author = "";


                var today = DateTime.Today.ToString("yyyy-MM-dd");
                var quotedb = (from a in db.Tb_Quotes where (a.Active == true && a.Date == today) select a).FirstOrDefault();

                if (quotedb == null)
                {
                    var quotedb2 = (from a in db.Tb_Quotes where (a.Active == false && a.Date == "") select a).FirstOrDefault();
                    if (quotedb2 == null)
                    {

                    }
                    else
                    {
                        quotedb2.Active = true;
                        quotedb2.Date = today;
                        db.Entry(quotedb2).State = EntityState.Modified;
                        db.SaveChanges();
                        quote = quotedb2.Text;
                        author = quotedb2.Author;
                    }
                }
                else {
                    quote = quotedb.Text;
                    author = quotedb.Author;
                }

                ViewBag.quote = quote;
                ViewBag.author = author;

                ViewBag.idparentpost = idparentpost;
                ViewBag.likepost = likepost;

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

                return View(lstpost);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult Documents_upload(DateTime? date, int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Documents Upload";
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

                List<Tb_Docpackages> lstpackages = new List<Tb_Docpackages>();
                DateTime dateClose = new DateTime();
                if (date != null)
                {
                    dateClose = Convert.ToDateTime(date).AddHours(24);
                    //dateClose = dateOpen.AddHours(24);
                }
               

                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.ID_Property = new SelectList((from t in db.Tb_Process
                                                          where (t.ID_User == activeuser.ID_User)
                                                          select new
                                                          {
                                                              ID = t.ID_Process,
                                                              FullName = t.Address + " | CUSTOMER: " + t.Tb_Customers.Name + " " + t.Tb_Customers.LastName
                                                          }), "ID", "FullName");
                    if (date == null)
                    {
                        lstpackages = (from a in db.Tb_Docpackages where (a.ID_User == activeuser.ID_User && a.original == false) select a).ToList();
                    }
                    else {
                        lstpackages = (from a in db.Tb_Docpackages where (a.ID_User == activeuser.ID_User && a.original == false && (a.Last_update < dateClose && a.Last_update >= date)) select a).ToList();
                    }
                   

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        if (date == null)
                        {
                            lstpackages = (from a in db.Tb_Docpackages where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                        }
                        else {
                            lstpackages = (from a in db.Tb_Docpackages where (a.ID_Company == activeuser.ID_Company && (a.Last_update < dateClose && a.Last_update >= date)) select a).ToList();
                        }

                        var agentes = db.Sys_Users.Where(c => c.ID_Company == activeuser.ID_Company).Select(c => c.ID_User).ToArray();
                        ViewBag.ID_Property = new SelectList((from t in db.Tb_Process
                                                              where (agentes.Contains(t.ID_User))
                                                              select new
                                                              {
                                                                  ID = t.ID_Process,
                                                                  FullName = t.Address + " | CUSTOMER: " + t.Tb_Customers.Name + " " + t.Tb_Customers.LastName
                                                              }), "ID", "FullName");
                    }
                    else
                    {
                        ViewBag.rol = "Admin";

                        if (broker == 0)
                        {
                            if (date == null)
                            {
                                lstpackages = (from a in db.Tb_Docpackages where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                            }
                            else
                            {
                                lstpackages = (from a in db.Tb_Docpackages where (a.ID_Company == activeuser.ID_Company && (a.Last_update < dateClose && a.Last_update >= date)) select a).ToList();
                            }
                            var agentes = db.Sys_Users.Where(c => c.ID_Company == activeuser.ID_Company && c.Roles.Contains("Agent")).Select(c => c.ID_User).ToArray();
                            ViewBag.ID_Property = new SelectList((from t in db.Tb_Process
                                                                  where (agentes.Contains(t.ID_User))
                                                                  select new
                                                                  {
                                                                      ID = t.ID_Process,
                                                                      FullName = t.Address + " | CUSTOMER: " + t.Tb_Customers.Name + " " + t.Tb_Customers.LastName
                                                                  }), "ID", "FullName");
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            if (date == null)
                            {
                                lstpackages = (from a in db.Tb_Docpackages where (a.ID_Company == broker) select a).ToList();
                            }
                            else
                            {
                                lstpackages = (from a in db.Tb_Docpackages where (a.ID_Company == broker && (a.Last_update < dateClose && a.Last_update >= date)) select a).ToList();
                            }
                            var agentes = db.Sys_Users.Where(c => c.ID_Company == broker).Select(c => c.ID_User).ToArray();
                            ViewBag.ID_Property = new SelectList((from t in db.Tb_Process
                                                                  where (agentes.Contains(t.ID_User))
                                                                  select new
                                                                  {
                                                                      ID = t.ID_Process,
                                                                      FullName = t.Address + " | CUSTOMER: " + t.Tb_Customers.Name + " " + t.Tb_Customers.LastName
                                                                  }), "ID", "FullName");
                        }

                    }

                }


                ViewBag.selbroker = broker;

                var users = (from a in db.Sys_Users select a).ToList();
                foreach (var item in lstpackages) {
                    var user = (from a in users where (item.ID_User == a.ID_User) select a).FirstOrDefault();
                    if (user != null) {
                        item.Description = item.Description + " | AGENT: " + user.Name + " " + user.LastName;
                    }
                }

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

                return View(lstpackages);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult Documents_upload_management(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Documents Uploads";
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

                List<Tb_Docpackages> lstdocpacakges = new List<Tb_Docpackages>();
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstdocpacakges = (from a in db.Tb_Docpackages where (a.ID_User == activeuser.ID_User && a.original == false) select a).ToList();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        lstdocpacakges = (from a in db.Tb_Docpackages where (a.ID_Company == activeuser.ID_Company && a.original == false) select a).ToList();
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {

                            lstdocpacakges = (from a in db.Tb_Docpackages where (a.ID_Company == activeuser.ID_Company && a.original == false) select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstdocpacakges = (from a in db.Tb_Docpackages where (a.ID_Company == broker && a.original == false) select a).ToList();
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

                return View(lstdocpacakges);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }



        public ActionResult Package_details(int idpackage)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Documents Upload";
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

                var package = (from a in db.Tb_Docpackages where (a.ID_docpackage == idpackage && a.original == false) select a).FirstOrDefault();
                var property = (from a in db.Tb_Process where (a.ID_Process == package.ID_Process) select a).FirstOrDefault();
                var customer = (from a in db.Tb_Customers where (a.ID_Customer == property.ID_Customer) select a).FirstOrDefault();

                ViewBag.package = package;
                ViewBag.property = property;
                ViewBag.customer = customer;

                List<Tb_Docpackages_details> lstpackages = new List<Tb_Docpackages_details>();
                lstpackages = (from a in db.Tb_Docpackages_details where (a.ID_docpackage == idpackage && a.original == false) select a).ToList();

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

                return View(lstpackages);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult Webinar(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Webinar";
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

                List<Tb_Webinars> lstwebinars = new List<Tb_Webinars>();

                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstwebinars = (from a in db.Tb_Webinars where (a.ID_Company == activeuser.ID_Company) select a).ToList();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        RedirectToAction("Dashboard", "Portal", new { broker = brokersel.ID_Company });
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {

                            lstwebinars = (from a in db.Tb_Webinars where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstwebinars = (from a in db.Tb_Webinars where (a.ID_Company == broker) select a).ToList();
                        }
                    }


                }
                var sunday = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var saturday = sunday.AddMonths(1).AddDays(-1);
                //WEBINAR TYPE:

                ViewBag.selbroker = broker;

                lstwebinars = (from a in db.Tb_Webinars where ((a.Date >= sunday && a.Date <= saturday) && a.Active == 1) select a).ToList();
                List<Routes_calendar> rutaslst = new List<Routes_calendar>();
                foreach (var item in lstwebinars)
                {
                    Routes_calendar rt = new Routes_calendar();

                    rt.title = item.Title.ToUpper();
                    rt.url = item.Url;
                    rt.start = item.Date.ToString("yyyy-MM-dd");
                    rt.category = item.Category;
                    rt.time = item.Date.ToShortTimeString();
                    rt.end = item.Date.AddDays(1).ToString("yyyy-MM-dd");
                    if (item.Category == "Meeting")
                    {
                        rt.className = "block b-t b-t-2x b-secondary";//"#2081d6";
                    }

                    else
                    {
                        rt.className = "block b-t b-t-2x b-info";//"#2081d6";
                    }

                    rutaslst.Add(rt);
                }
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result = javaScriptSerializer.Serialize(rutaslst.ToArray());
                ViewBag.calroutes = result;


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

                return View(lstwebinars);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult Reservations(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Webinar";
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
                var sunday = new DateTime(DateTime.Today.Year, 1, 1);
                var saturday = sunday.AddMonths(13);

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;

                List<Tb_Webinars> lstwebinars = new List<Tb_Webinars>();

                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstwebinars = (from a in db.Tb_Webinars where (a.ID_Company == activeuser.ID_Company && (a.Date >= sunday && a.Date <= saturday) && a.Active == 1) select a).ToList();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        RedirectToAction("Dashboard", "Portal", new { broker = brokersel.ID_Company });
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {

                            lstwebinars = (from a in db.Tb_Webinars where (a.ID_Company == activeuser.ID_Company && (a.Date >= sunday && a.Date <= saturday) && a.Active == 1) select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstwebinars = (from a in db.Tb_Webinars where (a.Date >= sunday && a.Date <= saturday) && a.Active == 1 select a).ToList();
                        }
                    }


                }

                //WEBINAR TYPE:

                ViewBag.selbroker = broker;


                List<Routes_calendar> rutaslst = new List<Routes_calendar>();
                foreach (var item in lstwebinars)
                {
                    Routes_calendar rt = new Routes_calendar();

                    rt.title = item.Title.ToUpper();
                    rt.url = item.Url;
                    rt.start = item.Date.ToString("yyyy-MM-dd");
                    rt.category = item.Category;
                    rt.time = item.Date.ToShortTimeString();
                    rt.end = item.Date.AddDays(1).ToString("yyyy-MM-dd");
                    if (item.Category == "Meeting")
                    {
                        rt.className = "block b-t b-t-2x b-secondary";//"#2081d6";
                    }

                    else
                    {
                        rt.className = "block b-t b-t-2x b-info";//"#2081d6";
                    }

                    rutaslst.Add(rt);
                }
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result = javaScriptSerializer.Serialize(rutaslst.ToArray());
                ViewBag.calroutes = result;


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

                return View(lstwebinars);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

        public ActionResult Obtener_actividadesConflicto(DateTime horainicio)
        {
            var events = cls_GoogleCalendar.get_events(horainicio);

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(events);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Reservations_management(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Webinar";
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


                List<Tb_Webinars> lstwebinars = new List<Tb_Webinars>();
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstwebinars = (from a in db.Tb_Webinars where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        RedirectToAction("Dashboard", "Portal", new { broker = brokersel.ID_Company });
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {

                            lstwebinars = (from a in db.Tb_Webinars select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstwebinars = (from a in db.Tb_Webinars select a).ToList();
                        }

                    }


                }
                var sunday = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var saturday = sunday.AddMonths(1).AddDays(-1);
                //WEBINAR TYPE:

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

                return View(lstwebinars);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }


        public ActionResult Obtener_actividadesConflictoBooking(string resource, DateTime horainicio, DateTime horafin)
        {
            List<Tb_Webinars> lst_planificacion = new List<Tb_Webinars>();
            lst_planificacion = (from a in db.Tb_Webinars
                                 where (a.Title == resource)
                                 where (horafin >= a.Date)
                                 where (horainicio <= a.Date_end)
                                 select a).ToList();
            string result = "0";
            if (lst_planificacion.Count > 0) {
                result = "1";
            }

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UploadWebinar(string title, string url, string description, string category, DateTime date, DateTime datefin)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                Tb_Webinars newebinar = new Tb_Webinars();
                newebinar.Title = title;
                if (description == null) { newebinar.Description = ""; } else { newebinar.Description = description; }

                newebinar.Url = "";
                newebinar.Url_video = "";
                newebinar.Notes = "";
                newebinar.Category = "Booking";
                newebinar.Max_users = 0;
                newebinar.Status = "0";
                newebinar.Image = "";
                newebinar.Date = date;
                newebinar.Date_end = datefin;
                newebinar.Active = 1;

                newebinar.ID_Company = activeuser.ID_Company;


                db.Tb_Webinars.Add(newebinar);
                db.SaveChanges();

                try
                {//Enviamos notificaciones  a todos los Admin y SA
                    var agents = (from a in db.Sys_Users where (a.Active == true && a.ID_Company == activeuser.ID_Company && a.Roles.Contains("Admin") || a.Roles.Contains("SA")) select a).ToList();

                    if (agents.Count > 0)
                    {
                        foreach (var item in agents)
                        {
                            Sys_Notifications newnotification = new Sys_Notifications();
                            newnotification.Active = true;
                            newnotification.Date = DateTime.UtcNow;
                            newnotification.Title = "New Booking.";
                            newnotification.Description = "A new booking was added on " + newebinar.Date.ToShortDateString();
                            newnotification.ID_user = item.ID_User;
                            db.Sys_Notifications.Add(newnotification);

                            try
                            {
                                var brokeremail = item.Email;

                                if (brokeremail != null && brokeremail != "")
                                {
                                    //Enviamos correo para notificar
                                    dynamic emailtosend = new Email("newNotification_booking");
                                    emailtosend.To = brokeremail;
                                    emailtosend.From = "support@s7ven.co";
                                    emailtosend.subject = "New Booking added - PGR Agents Portal";
                                    emailtosend.email = activeuser.Email;
                                    emailtosend.broker = activeuser.Sys_Company.Name;
                                    emailtosend.resource = newebinar.Title;
                                    emailtosend.date = newebinar.Date.ToLongDateString() + " " + newebinar.Date.ToShortTimeString();
                                    emailtosend.time = newebinar.Date_end.ToLongDateString() + " " + newebinar.Date_end.ToShortTimeString();
                                    emailtosend.Send();

                                }

                            }
                            catch (Exception ex)
                            {

                            }


                        }
                        db.SaveChanges();
                    }

                }
                catch
                {

                }


                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }


        public ActionResult Postcomment(string comment, int id)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;


                Tb_Posts newpost = new Tb_Posts();
                newpost.Title = "Reply Post Broker";
                newpost.Content_post = comment;
                newpost.Lat = "";
                newpost.Lng = "";
                newpost.Start_at = DateTime.UtcNow;
                newpost.Finish_at = DateTime.UtcNow;
                newpost.Post_type = 2;
                newpost.ID_User = activeuser.ID_User;
                newpost.Username = activeuser.Name + " " + activeuser.LastName;
                newpost.Likes = 0;
                newpost.Comments = 0;
                newpost.User_likes = "";
                newpost.User_comments = "";
                newpost.created_at = DateTime.UtcNow;
                newpost.Active = true;
                newpost.Post_parent = id;
                newpost.Url_image = "";
                db.Tb_Posts.Add(newpost);



                var postparent = (from b in db.Tb_Posts where (b.ID_Post == id) select b).FirstOrDefault();
                if (postparent != null) {
                    postparent.Comments = postparent.Comments + 1;
                    db.Entry(postparent).State = EntityState.Modified;
                }


                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult DelPostcomment(int idpost, int idparent)
        {
            try
            {


                Tb_Posts post = (from a in db.Tb_Posts where (a.ID_Post == idpost) select a).FirstOrDefault();
                db.Entry(post).State = EntityState.Deleted;

                if (idparent != 0) {
                    var postparent = (from b in db.Tb_Posts where (b.ID_Post == idparent) select b).FirstOrDefault();
                    if (postparent != null)
                    {
                        postparent.Comments = postparent.Comments - 1;
                        db.Entry(postparent).State = EntityState.Modified;
                    }
                }



                db.SaveChanges();


                return RedirectToAction("Dashboard", "Portal", null);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return RedirectToAction("Dashboard", "Portal", null);

            }

        }



        public ActionResult Likepost(int id)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                Tb_Posts newpost = (from a in db.Tb_Posts where (a.ID_Post == id) select a).FirstOrDefault();
                List<int> TagIds = new List<int>();
                newpost.Likes = newpost.Likes + 1;
                if (newpost.User_likes == "") {
                    TagIds.Add(activeuser.ID_User);
                }
                else
                {
                    TagIds = newpost.User_likes.Split(',').Select(int.Parse).ToList();
                    if (!TagIds.Contains(activeuser.ID_User))
                    {
                        TagIds.Add(activeuser.ID_User);


                    }
                }
                newpost.User_likes = string.Join<int>(",", TagIds);
                db.Entry(newpost).State = EntityState.Modified;
                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult Dislikepost(int id)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                Tb_Posts newpost = (from a in db.Tb_Posts where (a.ID_Post == id) select a).FirstOrDefault();

                newpost.Likes = newpost.Likes - 1;
                if (newpost.Likes < 0) { newpost.Likes = 0; }

                List<int> TagIds = new List<int>();

                if (newpost.User_likes == "")
                {
                    newpost.User_likes = "";
                }
                else
                {
                    TagIds = newpost.User_likes.Split(',').Select(int.Parse).ToList();
                    if (TagIds.Contains(activeuser.ID_User))
                    {
                        TagIds.Remove(activeuser.ID_User);


                    }

                    if (TagIds.Count > 0) { newpost.User_likes = string.Join<int>(",", TagIds); } else { newpost.User_likes = ""; }


                }



                db.Entry(newpost).State = EntityState.Modified;
                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult Videos(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Videos";
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

                List<Tb_Videos> lstvideos = new List<Tb_Videos>();
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstvideos = (from a in db.Tb_Videos where (a.ID_Company == activeuser.ID_Company && a.Type != "broker" || a.ID_Company == 1) select a).ToList();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        lstvideos = (from a in db.Tb_Videos where (a.ID_Company == activeuser.ID_Company && a.Type != "broker") select a).ToList();
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {

                            lstvideos = (from a in db.Tb_Videos where (a.ID_Company == activeuser.ID_Company && a.Type != "broker" || a.ID_Company == 1) select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstvideos = (from a in db.Tb_Videos where (a.ID_Company == broker && a.Type != "broker") select a).ToList();
                        }
                    }

                }
                //VIDEO TYPE:

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

                var categories = (from a in db.Tb_Options where (a.Type == 3) select a).ToList();
                ViewBag.categories = categories;

                return View(lstvideos);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult Videos_management(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Videos";
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

                List<Tb_Videos> lstvideos = new List<Tb_Videos>();
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstvideos = (from a in db.Tb_Videos where (a.ID_Company == activeuser.ID_Company && a.Type != "broker") select a).ToList();
                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        lstvideos = (from a in db.Tb_Videos where (a.ID_Company == activeuser.ID_Company && a.Type != "broker") select a).ToList();
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {

                            lstvideos = (from a in db.Tb_Videos where (a.ID_Company == activeuser.ID_Company && a.Type != "broker") select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstvideos = (from a in db.Tb_Videos where (a.ID_Company == broker && a.Type != "broker") select a).ToList();
                        }
                    }



                }
                //VIDEO TYPE:

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


                var categories = (from a in db.Tb_Options where (a.Type == 3) select a).ToList();
                ViewBag.categories = categories;
                return View(lstvideos);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

        public ActionResult Marketing(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Marketing";
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

                List<Tb_Marketing> lstmarketing = new List<Tb_Marketing>();
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstmarketing = (from a in db.Tb_Marketing where (a.ID_Company == activeuser.ID_Company || a.ID_Company == 1) select a).ToList();
                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        lstmarketing = (from a in db.Tb_Marketing where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                    }
                    else
                    {
                        ViewBag.rol = "Admin";

                        if (broker == 0)
                        {

                            lstmarketing = (from a in db.Tb_Marketing where (a.ID_Company == activeuser.ID_Company || a.ID_Company == 1) select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstmarketing = (from a in db.Tb_Marketing where (a.ID_Company == broker) select a).ToList();
                        }
                    }


                }
                //VIDEO TYPE:

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

                var categories = (from a in db.Tb_Options where (a.Type == 1) select a).ToList();
                ViewBag.categories = categories;

                return View(lstmarketing);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

        public ActionResult Marketing_management(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Marketing";
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

                List<Tb_Marketing> lstmarketing = new List<Tb_Marketing>();

                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstmarketing = (from a in db.Tb_Marketing where (a.ID_Company == activeuser.ID_Company) select a).ToList();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        lstmarketing = (from a in db.Tb_Marketing where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                    }
                    else
                    {
                        ViewBag.rol = "Admin";

                        if (broker == 0)
                        {

                            lstmarketing = (from a in db.Tb_Marketing where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstmarketing = (from a in db.Tb_Marketing where (a.ID_Company == broker) select a).ToList();
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

                var categories = (from a in db.Tb_Options where (a.Type == 1) select a).ToList();
                ViewBag.categories = categories;

                return View(lstmarketing);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }


        [HttpPost]
        public ActionResult UploadDocument(string title, string category, string type)
        {
            var path = "";
            var fileName = "";
            string extension = "";
            string size = "";
            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    extension = Path.GetExtension(Request.Files[i].FileName).ToLower();
                    size = ConvertBytesToMegabytes(Request.Files[i].ContentLength).ToString("0.00");
                    fileName = Path.GetFileName(file.FileName);

                    if (type == "Documents")
                    {
                        path = Path.Combine(Server.MapPath("~/Content/Uploads/Resources/Documents/"), fileName);
                    }
                    else {
                        path = Path.Combine(Server.MapPath("~/Content/Uploads/Resources/Scripts/"), fileName);
                    }

                    file.SaveAs(path);
                }
            }
            catch
            {

            }

            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;


                Tb_Resources newresource = new Tb_Resources();
                newresource.Name = title;
                newresource.Description = "";
                if (type == "Documents")
                {
                    newresource.Url = "~/Content/Uploads/Resources/Documents/" + fileName;
                }
                else {
                    newresource.Url = "~/Content/Uploads/Resources/Scripts/" + fileName;
                }

                newresource.Type = type;


                newresource.Size = size + " MB";
                newresource.Times_downloaded = 0;
                newresource.Doc_type = category;
                newresource.Last_update = DateTime.UtcNow;
                newresource.Extension_file = extension;
                newresource.ID_Company = activeuser.ID_Company;


                db.Tb_Resources.Add(newresource);
                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        public class htmlModel
        {
            [AllowHtml]
            public string htmlformat { get; set; }
            public string title { get; set; }
            public string type { get; set; }
        }

        [HttpPost]
        public ActionResult UploadCampaign(htmlModel modelhtml)
        {


            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                Tb_Resources newresource = new Tb_Resources();
                newresource.Name = modelhtml.title;
                newresource.Description = "";

                newresource.Url = modelhtml.htmlformat;

                newresource.Type = modelhtml.type;


                newresource.Size = "";
                newresource.Times_downloaded = 0;
                newresource.Doc_type = "";
                newresource.Last_update = DateTime.UtcNow;
                newresource.Extension_file = "";
                newresource.ID_Company = activeuser.ID_Company;


                db.Tb_Resources.Add(newresource);
                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult Download_imageMarketing(int id)
        {

            var fileDB = (from a in db.Tb_Marketing where (a.ID_marketing == id) select a).FirstOrDefault();

            var path = fileDB.T_image;
            var file = Server.MapPath(path);


            return File(file, "image/jpeg");

        }

        [HttpPost]
        public ActionResult UploadVideo(string title, string url, string description, string category)
        {
            var path = "";
            var fileName = "";
            var date = DateTime.Now.Date;
            var min = DateTime.Now.Minute;
            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    fileName = Path.GetFileName(file.FileName);

                    path = Path.Combine(Server.MapPath("~/Content/Uploads/Images/Videos_img/"), date + "_" + min + fileName);
                    file.SaveAs(path);
                }
            }
            catch {

            }

            try {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;


                Tb_Videos newvideo = new Tb_Videos();
                newvideo.Name = title;
                newvideo.Description = description;
                newvideo.Url = url;
                newvideo.Category = category;
                newvideo.T_image = "~/Content/Uploads/Images/Videos_img/" + date + "_" + min + fileName;
                newvideo.Last_update = DateTime.UtcNow;
                newvideo.Active = 1;
                newvideo.Comments = 0;
                newvideo.Likes = 0;
                newvideo.Type = "";
                newvideo.ID_Company = activeuser.ID_Company;


                db.Tb_Videos.Add(newvideo);
                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex) {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public ActionResult UploadVideoBroker(string title, string url, string description, string category)
        {
            var path = "";
            var fileName = "";
            var date = DateTime.Now.Date;
            var min = DateTime.Now.Minute;
            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    fileName = Path.GetFileName(file.FileName);

                    path = Path.Combine(Server.MapPath("~/Content/Uploads/Images/Videos_img/"), date + "_" + min + fileName);
                    file.SaveAs(path);
                }
            }
            catch
            {

            }

            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;


                Tb_Videos newvideo = new Tb_Videos();
                newvideo.Name = title;
                newvideo.Description = description;
                newvideo.Url = url;
                newvideo.Category = category;
                newvideo.T_image = "~/Content/Uploads/Images/Videos_img/" + date + "_" + min + fileName;
                newvideo.Last_update = DateTime.UtcNow;
                newvideo.Active = 1;
                newvideo.Comments = 0;
                newvideo.Likes = 0;
                newvideo.Type = "broker";
                newvideo.ID_Company = activeuser.ID_Company;


                db.Tb_Videos.Add(newvideo);
                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public ActionResult EditVideo(int id, string title, string url, string description, string category)
        {
            var path = "";
            var fileName = "";
            var date = DateTime.Now.Date.Day;
            var min = DateTime.Now.Minute;
            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    fileName = Path.GetFileName(file.FileName);

                    path = Path.Combine(Server.MapPath("~/Content/Uploads/Images/Videos_img/"), date + "_" + min + fileName);
                    file.SaveAs(path);
                }
            }
            catch
            {

            }

            try
            {
                Tb_Videos editvideo = new Tb_Videos();
                editvideo = (from a in db.Tb_Videos where (a.ID_Video == id) select a).First();

                editvideo.Name = title;
                editvideo.Description = description;
                editvideo.Url = url;
                editvideo.Category = category;
                if (fileName == "") {

                }
                else
                {
                    editvideo.T_image = "~/Content/Uploads/Images/Videos_img/" + date + "_" + min + fileName;
                }

                editvideo.Last_update = DateTime.UtcNow;

                db.Entry(editvideo).State = EntityState.Modified;
                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }


        [HttpPost]
        public ActionResult EditNetwork(int id, string title, string url, string description, string category)
        {
            var path = "";
            var fileName = "";
            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    fileName = Path.GetFileName(file.FileName);

                    path = Path.Combine(Server.MapPath("~/Content/Uploads/Images/Network/"), fileName);
                    file.SaveAs(path);
                }
            }
            catch
            {

            }

            try
            {
                Tb_Network editnetwork = new Tb_Network();
                editnetwork = (from a in db.Tb_Network where (a.ID_Network == id) select a).First();
                editnetwork.Name = title;
                editnetwork.Description = description;
                editnetwork.Category = category;
                if (fileName == "")
                {

                }
                else
                {
                    editnetwork.T_image = "~/Content/Uploads/Images/Network/" + fileName;
                }

                editnetwork.Last_update = DateTime.UtcNow;

                db.Entry(editnetwork).State = EntityState.Modified;
                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public ActionResult UploadNetwork(string title, string url, string description, string category)
        {
            var path = "";
            var fileName = "";
            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    fileName = Path.GetFileName(file.FileName);

                    path = Path.Combine(Server.MapPath("~/Content/Uploads/Images/Network/"), fileName);
                    file.SaveAs(path);
                }
            }
            catch {

            }

            try {

                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                Tb_Network newnetwork = new Tb_Network();
                newnetwork.Name = title;
                newnetwork.Description = description;
                newnetwork.Url = url;
                newnetwork.Category = category;
                newnetwork.T_image = "~/Content/Uploads/Images/Network/" + fileName;
                newnetwork.Last_update = DateTime.UtcNow;
                newnetwork.Active = 1;
                newnetwork.Comments = 0;
                newnetwork.Likes = 0;
                newnetwork.Type = "";
                newnetwork.ID_Company = activeuser.ID_Company;


                db.Tb_Network.Add(newnetwork);
                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex) {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }


        [HttpPost]
        public ActionResult UploadImage(string title, string description, string category)
        {
            var path = "";
            var fileName = "";
            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    fileName = Path.GetFileName(file.FileName);

                    path = Path.Combine(Server.MapPath("~/Content/Uploads/Images/Marketing_img/"), fileName);
                    file.SaveAs(path);
                }
            }
            catch
            {

            }

            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                Tb_Marketing newimage = new Tb_Marketing();
                newimage.Name = title;
                newimage.Description = description;
                newimage.Url = "";
                newimage.Category = category;
                newimage.T_image = "~/Content/Uploads/Images/Marketing_img/" + fileName;
                newimage.Last_update = DateTime.UtcNow;
                newimage.Active = 1;
                newimage.Comments = 0;
                newimage.Likes = 0;
                newimage.Type = "";
                newimage.ID_Company = activeuser.ID_Company;


                db.Tb_Marketing.Add(newimage);
                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public ActionResult EditImage(int id, string title, string description, string category)
        {
            var path = "";
            var fileName = "";
            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    fileName = Path.GetFileName(file.FileName);

                    path = Path.Combine(Server.MapPath("~/Content/Uploads/Images/Marketing_img/"), fileName);
                    file.SaveAs(path);
                }
            }
            catch
            {

            }

            try
            {
                Tb_Marketing editimage = new Tb_Marketing();
                editimage = (from a in db.Tb_Marketing where (a.ID_marketing == id) select a).FirstOrDefault();
                editimage.Name = title;
                editimage.Description = description;
                editimage.Category = category;
                if (fileName == "")
                {

                }
                else
                {
                    editimage.T_image = "~/Content/Uploads/Images/Marketing_img/" + fileName;
                }
                editimage.Last_update = DateTime.UtcNow;
                db.Entry(editimage).State = EntityState.Modified;

                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }


        [HttpPost]
        public ActionResult Uploadfile(int[] ids)
        {
            var path = "";
            var fileName = "";
            string extension = "";
            string size = "";
            try
            {
                if (Request.Files.Count > 0)
                {
                    for (int x = 0; x < ids.Length; x++)
                    {
                        var id = ids[x];
                        var detail = (from a in db.Tb_Docpackages_details where (a.ID_Detail == id) select a).FirstOrDefault();
                        var headerpack = (from b in db.Tb_Docpackages where (b.ID_docpackage == detail.ID_docpackage) select b).FirstOrDefault();


                        if (detail.uploaded == true)
                        {
                            string fullPath = Request.MapPath(detail.URL);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }
                            detail.uploaded = false;
                            detail.Extension = "";
                            detail.Size = "";
                            headerpack.Total_docsf = headerpack.Total_docsf - 1;
                            headerpack.Last_update = DateTime.UtcNow;
                        }
                        Random rnd = new Random();

                        var file = Request.Files[x];
                        extension = Path.GetExtension(Request.Files[x].FileName).ToLower();
                        size = ConvertBytesToMegabytes(Request.Files[x].ContentLength).ToString("0.00");
                        fileName = DateTime.Now.Hour.ToString() + rnd.Next(52).ToString() + DateTime.Now.Day + rnd.Next(3981) + extension;     // creates a number between 0 and 51;//Path.GetFileName(file.FileName);
                        path = Path.Combine(Server.MapPath("~/Content/Uploads/DocumentsPackages/"), fileName);
                        file.SaveAs(path);


                        detail.URL = "~/Content/Uploads/DocumentsPackages/" + fileName;
                        detail.Extension = extension;
                        detail.Size = size;
                        headerpack.Total_docsf = headerpack.Total_docsf + 1;
                        headerpack.Last_update = DateTime.UtcNow;
                        detail.uploaded = true;
                        db.Entry(detail).State = EntityState.Modified;
                        db.Entry(headerpack).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    var result = "SUCCESS";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }


                else {
                    var result = "NO DATA";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }




            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult Video_watch(int id, int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Videos";
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

                //VIDEO TYPE:
                Tb_Videos lstvideos = new Tb_Videos();

                lstvideos = (from a in db.Tb_Videos where (a.ID_Video == id) select a).FirstOrDefault();

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
                return View(lstvideos);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }


        public ActionResult Videos_broker(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Videos";
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

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;

                List<Tb_Videos> lstvideos = new List<Tb_Videos>();
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstvideos = (from a in db.Tb_Videos where (a.Active == 1 && a.Type == "broker") select a).ToList();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";

                        lstvideos = (from a in db.Tb_Videos where (a.Type == "broker") select a).ToList();
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {

                            lstvideos = (from a in db.Tb_Videos where (a.Active == 1 && a.Type == "broker") select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstvideos = (from a in db.Tb_Videos where (a.Type == "broker") select a).ToList();
                        }
                    }

                }
                //VIDEO TYPE:

                ViewBag.selbroker = broker;
                //VIDEO TYPE:

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

                var categories = (from a in db.Tb_Options where (a.Type == 4) select a).ToList();
                ViewBag.categories = categories;

                return View(lstvideos);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult Videosbroker_management(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Videos";
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

                List<Tb_Videos> lstvideos = new List<Tb_Videos>();
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstvideos = (from a in db.Tb_Videos where (a.Type == "Broker") select a).ToList();
                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        lstvideos = (from a in db.Tb_Videos where (a.Type == "Broker") select a).ToList();
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {

                            lstvideos = (from a in db.Tb_Videos where (a.Type == "Broker") select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstvideos = (from a in db.Tb_Videos where (a.Type == "Broker") select a).ToList();
                        }
                    }



                }
                //VIDEO TYPE:

                ViewBag.selbroker = broker;
                //VIDEO TYPE:

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


                var categories = (from a in db.Tb_Options where (a.Type == 4) select a).ToList();
                ViewBag.categories = categories;
                return View(lstvideos);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

        public ActionResult Videobroker_watch(int id, int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Videos";
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

                //VIDEO TYPE:
                Tb_Videos lstvideos = new Tb_Videos();

                lstvideos = (from a in db.Tb_Videos where (a.ID_Video == id) select a).FirstOrDefault();
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
                return View(lstvideos);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }


        [HttpGet]
        public ActionResult newPackage(string title, int idpackage, string category, int customer, int property)
        {
            //Metodo para que los agentes agreguen paquetes
            try
            {
                Tb_Docpackages newpackage = new Tb_Docpackages();
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                newpackage.Description = title;
                newpackage.Category = category;
                newpackage.Last_update = DateTime.UtcNow;
                newpackage.Total_docs = 0;
                newpackage.Total_docsf = 0;
                newpackage.ID_User = activeuser.ID_User;
                newpackage.ID_Company = activeuser.ID_Company;
                newpackage.Active = true;
                newpackage.original = false;
                newpackage.Finished = false;
                newpackage.Sent = false;
                newpackage.ID_Process = property;

                var customerfromProp = (from t in db.Tb_Process where (t.ID_Process == property) select t).FirstOrDefault();
                var cust = (from t in db.Tb_Customers where (t.ID_Customer == customerfromProp.ID_Customer) select t).FirstOrDefault();
                newpackage.ID_Customer = customerfromProp.ID_Customer;
                newpackage.ID_Property = customerfromProp.ID_Process;


                //buscamos los detalles
                List<Tb_Docpackages_details> lsttosave = new List<Tb_Docpackages_details>();
                Tb_Docpackages_details packdetails = new Tb_Docpackages_details();

                var details = (from a in db.Tb_docCategies where (a.ID_Category == idpackage) select a).ToList();

                newpackage.Total_docs = details.Count();
                db.Tb_Docpackages.Add(newpackage);
                db.SaveChanges();

                foreach (var item in details) {
                    packdetails.original = false;
                    packdetails.ID_docpackage = newpackage.ID_docpackage;
                    packdetails.Title = item.Title;
                    packdetails.mandatory = item.Mandatory;
                    packdetails.Extension = "";
                    packdetails.Description = title;
                    packdetails.URL = "";
                    packdetails.Notes = "";
                    packdetails.Size = "";
                    packdetails.uploaded = false;
                    packdetails.sent = false;

                    lsttosave.Add(packdetails);
                    packdetails = new Tb_Docpackages_details();
                }
                db.Tb_Docpackages_details.AddRange(lsttosave);
                db.SaveChanges();

                CustomerCRMDashboard customerCRM = new CustomerCRMDashboard();
                customerCRM.package = newpackage;
                customerCRM.property = customerfromProp;
                customerCRM.customer = cust;
                customerCRM.pack_Det = lsttosave;

                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));

                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                }
                else
                {
                    if (r.Contains("Admin"))
                    {
                        ViewBag.rol = "Admin";
                    }
                    else
                    {
                        ViewBag.rol = "SA";
                    }
                }


                return PartialView("~/Views/Shared/PartialTables/DocumentsTemplate.cshtml", customerCRM);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }


        public ActionResult Resources(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Resources";
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

                List<Tb_Resources> lstresources = new List<Tb_Resources>();

                //RESOURCE TYPE: 1-Documents | 2- Scripts
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstresources = (from a in db.Tb_Resources where (a.ID_Company == activeuser.ID_Company && (a.Type != "Documents Broker" || a.Type != "Scripts Broker" || a.Type != "Email Campaign Broker" || a.Type != "Text Campaign Broker") || a.ID_Company == 1) select a).ToList();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        lstresources = (from a in db.Tb_Resources where (a.ID_Company == activeuser.ID_Company && (a.Type != "Documents Broker" || a.Type != "Scripts Broker" || a.Type != "Email Campaign Broker" || a.Type != "Text Campaign Broker")) select a).ToList();

                    }
                    else
                    {
                        ViewBag.rol = "Admin";

                        if (broker == 0)
                        {

                            lstresources = (from a in db.Tb_Resources where (a.ID_Company == activeuser.ID_Company && (a.Type != "Documents Broker" || a.Type != "Scripts Broker" || a.Type != "Email Campaign Broker" || a.Type != "Text Campaign Broker") || a.ID_Company == 1) select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstresources = (from a in db.Tb_Resources where (a.ID_Company == broker && (a.Type != "Documents Broker" || a.Type != "Scripts Broker" || a.Type != "Email Campaign Broker" || a.Type != "Text Campaign Broker")) select a).ToList();
                        }
                    }


                }

                //Resources for Brokers
                List<Tb_Resources> lstresourcesBroker = new List<Tb_Resources>();

                //RESOURCE TYPE: 1-Documents | 2- Scripts
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    //lstresources = (from a in db.Tb_Resources where ((a.Type == "Documents Broker" || a.Type == "Scripts Broker" || a.Type == "Email Campaign Broker" || a.Type == "Text Campaign Broker")) select a).ToList();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {

                    }
                    else
                    {
                        ViewBag.rol = "Admin";

                        if (broker == 0)
                        {

                            lstresourcesBroker = (from a in db.Tb_Resources where ((a.Type == "Documents Broker" || a.Type == "Scripts Broker" || a.Type == "Email Campaign Broker" || a.Type == "Text Campaign Broker")) select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            //lstresourcesBroker = (from a in db.Tb_Resources where ((a.Type == "Documents Broker" || a.Type == "Scripts Broker" || a.Type == "Email Campaign Broker" || a.Type == "Text Campaign Broker")) select a).ToList();
                        }
                    }


                }
                ViewBag.resourcesbroker = lstresourcesBroker;
                ////

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

                var categories = (from a in db.Tb_Options where (a.Type == 5) select a).ToList();
                ViewBag.categories = categories;

                return View(lstresources);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }


        public ActionResult Resources_management(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Resources";
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


                List<Tb_Resources> lstresources = new List<Tb_Resources>();
                //RESOURCE TYPE: 1-Documents | 2- Scripts
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstresources = (from a in db.Tb_Resources where (a.ID_Company == activeuser.ID_Company && (a.Type != "Documents Broker" || a.Type != "Scripts Broker" || a.Type != "Email Campaign Broker" || a.Type != "Text Campaign Broker")) select a).ToList();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        lstresources = (from a in db.Tb_Resources where (a.ID_Company == activeuser.ID_Company && (a.Type != "Documents Broker" || a.Type != "Scripts Broker" || a.Type != "Email Campaign Broker" || a.Type != "Text Campaign Broker")) select a).ToList();

                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {

                            lstresources = (from a in db.Tb_Resources where (a.ID_Company == activeuser.ID_Company && (a.Type != "Documents Broker" || a.Type != "Scripts Broker" || a.Type != "Email Campaign Broker" || a.Type != "Text Campaign Broker")) select a).ToList();

                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstresources = (from a in db.Tb_Resources where (a.ID_Company == broker && (a.Type != "Documents Broker" || a.Type != "Scripts Broker" || a.Type != "Email Campaign Broker" || a.Type != "Text Campaign Broker")) select a).ToList();
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
                return View(lstresources);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

        public ActionResult Resources_broker(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Resources";
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

                List<Tb_Resources> lstresources = new List<Tb_Resources>();

                //RESOURCE TYPE: 1-Documents | 2- Scripts
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstresources = (from a in db.Tb_Resources where ((a.Type == "Documents Broker" || a.Type == "Scripts Broker" || a.Type == "Email Campaign Broker" || a.Type == "Text Campaign Broker")) select a).ToList();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        return RedirectToAction("Dashboard", "Portal", new { broker = brokersel.ID_Company });
                    }
                    else
                    {
                        ViewBag.rol = "Admin";

                        if (broker == 0)
                        {

                            lstresources = (from a in db.Tb_Resources where ((a.Type == "Documents Broker" || a.Type == "Scripts Broker" || a.Type == "Email Campaign Broker" || a.Type == "Text Campaign Broker")) select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstresources = (from a in db.Tb_Resources where ((a.Type == "Documents Broker" || a.Type == "Scripts Broker" || a.Type == "Email Campaign Broker" || a.Type == "Text Campaign Broker")) select a).ToList();
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

                var categories = (from a in db.Tb_Options where (a.Type == 6) select a).ToList();
                ViewBag.categories = categories;

                return View(lstresources);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }


        public ActionResult Resourcesbroker_management(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "Resources";
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


                List<Tb_Resources> lstresources = new List<Tb_Resources>();
                //RESOURCE TYPE: 1-Documents | 2- Scripts
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstresources = (from a in db.Tb_Resources where (a.ID_Company == activeuser.ID_Company && (a.Type == "Documents Broker" || a.Type == "Scripts Broker" || a.Type == "Email Campaign Broker" || a.Type == "Text Campaign Broker")) select a).ToList();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        lstresources = (from a in db.Tb_Resources where ((a.Type == "Documents Broker" || a.Type == "Scripts Broker" || a.Type == "Email Campaign Broker" || a.Type == "Text Campaign Broker")) select a).ToList();

                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {

                            lstresources = (from a in db.Tb_Resources where (a.ID_Company == activeuser.ID_Company && (a.Type == "Documents Broker" || a.Type == "Scripts Broker" || a.Type == "Email Campaign Broker" || a.Type == "Text Campaign Broker")) select a).ToList();

                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstresources = (from a in db.Tb_Resources where ((a.Type == "Documents Broker" || a.Type == "Scripts Broker" || a.Type == "Email Campaign Broker" || a.Type == "Text Campaign Broker")) select a).ToList();
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
                return View(lstresources);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

       
        public ActionResult PGR_network(int category=0, string name = "",int broker=0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "PGR Network";
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
                List<Tb_Network> lstnetwork = new List<Tb_Network>();
                
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    if (name.Equals(""))
                    {
                        lstnetwork = (from a in db.Tb_Network where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                    }
                    else
                    {
                        lstnetwork = (from a in db.Tb_Network where (a.ID_Company == activeuser.ID_Company && a.Name.Contains(name)) select a).ToList();
                    }
                   


                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        lstnetwork = (from a in db.Tb_Network where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                        if (name.Equals(""))
                        {
                            lstnetwork = (from a in db.Tb_Network where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                        }
                        else 
                        {
                            lstnetwork = (from a in db.Tb_Network where (a.ID_Company == activeuser.ID_Company && a.Name.Contains(name)) select a).ToList();
                        }

                        
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {
                            if (name.Equals(""))
                            {
                                lstnetwork = (from a in db.Tb_Network where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                            }
                            else
                            {
                                lstnetwork = (from a in db.Tb_Network where (a.ID_Company == activeuser.ID_Company && a.Name.Contains(name)) select a).ToList();
                            }
                           

                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            if (name.Equals(""))
                            {
                                lstnetwork = (from a in db.Tb_Network where (a.ID_Company == broker) select a).ToList();
                            }
                            else
                            {
                                lstnetwork = (from a in db.Tb_Network where (a.ID_Company == broker && a.Name.Contains(name)) select a).ToList();
                            }
                           


                        }
                    }
                   



                }
                //VIDEO TYPE:


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

                if (category == 0)
                {
                    var categories = (from a in db.Tb_Options where (a.Type == 2) select a).ToList();
                    ViewBag.categories = categories;
                }
                else
                {
                    var categories = (from a in db.Tb_Options where (a.ID_option == category) select a).ToList();
                    ViewBag.categories = categories;
                }
                var reviews = (from e in db.Tb_Reviews select e).ToList();
                ViewBag.reviews = reviews;
                var categorieslist = (from a in db.Tb_Options where (a.Type == 2) select a).ToList();
                ViewBag.categoryList = categorieslist;

                return View(lstnetwork);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult PGR_network_management(int broker=0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Portal";
                ViewData["Page"] = "PGR Network";
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

                List<Tb_Network> lstnetwork = new List<Tb_Network>();
                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    lstnetwork = (from a in db.Tb_Network where (a.ID_Company == activeuser.ID_Company) select a).ToList();

                }
                else
                {
                    if (r.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company && usd.Roles.Contains("Admin")) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        lstnetwork = (from a in db.Tb_Network where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                    }
                    else
                    {
                        ViewBag.rol = "Admin";

                        if (broker == 0)
                        {

                            lstnetwork = (from a in db.Tb_Network where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";
                            lstnetwork = (from a in db.Tb_Network where (a.ID_Company == broker) select a).ToList();
                        }
                    }
                    

                }
                //VIDEO TYPE:

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

                var categories = (from a in db.Tb_Options where (a.Type == 2) select a).ToList();
                ViewBag.categories = categories;

                return View(lstnetwork);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

        public ActionResult Showpdf(int id)
        {

            var fileDB = (from a in db.Tb_Resources where (a.ID_Resource == id) select a).FirstOrDefault();

            var path = fileDB.Url;
            var file = Server.MapPath(path);

            
            return File(file, "application/pdf");
     
        }



        public ActionResult ShowCampaign(int id)
        {

            var fileDB = (from a in db.Tb_Resources where (a.ID_Resource == id) select a).FirstOrDefault();

            var path = fileDB.Url;


            ViewBag.formathtml = path;

            return View();

        }

        public ActionResult DownloadDoc(int id)
        {

            var fileDB = (from a in db.Tb_Resources where (a.ID_Resource == id) select a).FirstOrDefault();

            var path = fileDB.Url;
            var file = Server.MapPath(path);

           //file has multiple support for a phisical file byte[] or a route string to points where the file is located.
            return File(file, System.Net.Mime.MediaTypeNames.Application.Octet, fileDB.Name + fileDB.Extension_file);

        }
        
        public ActionResult DownloadDocpdfmarketing(int id)
        {

            var fileDB = (from a in db.Tb_Marketing where (a.ID_marketing == id) select a).FirstOrDefault();

            var path = fileDB.T_image;
            int lastIndex = path.LastIndexOf("/");
            var file = Server.MapPath(path);
            byte[] fileBytes = System.IO.File.ReadAllBytes(file);

            var filename = path.Substring(lastIndex +1);
          

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);


        }

        
        public ActionResult Showpdf_docspackage(int id)
        {

            var fileDB = (from a in db.Tb_Docpackages_details where (a.ID_Detail == id) select a).FirstOrDefault();

            var path = fileDB.URL;
            var file = Server.MapPath(path);

            return File(file, "application/pdf");

        }

        //Delete section


        public ActionResult DeleteMarketing(int id)
        {
            try {
                Tb_Marketing tb_Marketing = db.Tb_Marketing.Find(id);
                var urlimg = tb_Marketing.T_image;
                db.Tb_Marketing.Remove(tb_Marketing);
                db.SaveChanges();
                //Eliminamos imagen
                string fullPath = Request.MapPath(urlimg);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }


                TempData["exito"] = "Image deleted successfully.";
                return RedirectToAction("Marketing_management", "Portal");
            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("Marketing_management", "Portal");
            }

       
        }


        public ActionResult DeleteVideo(int id)
        {
            try
            {
                Tb_Videos tb_Videos = db.Tb_Videos.Find(id);
                db.Tb_Videos.Remove(tb_Videos);
                db.SaveChanges();

                TempData["exito"] = "Video deleted successfully.";
                return RedirectToAction("Videos_management", "Portal");
            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("Videos_management", "Portal");
            }


        }
        public ActionResult DeleteVideoBroker(int id)
        {
            try
            {
                Tb_Videos tb_Videos = db.Tb_Videos.Find(id);
                db.Tb_Videos.Remove(tb_Videos);
                db.SaveChanges();

                TempData["exito"] = "Video deleted successfully.";
                return RedirectToAction("Videosbroker_management", "Portal");
            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("Videosbroker_management", "Portal");
            }


        }
        public ActionResult DeleteNetwork(int id)
        {
            try
            {
                Tb_Network tb_network = db.Tb_Network.Find(id);
                db.Tb_Network.Remove(tb_network);
                db.SaveChanges();

                TempData["exito"] = "PGR Network deleted successfully.";
                return RedirectToAction("PGR_network_management", "Portal");
            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("PGR_network_management", "Portal");
            }


        }
        public ActionResult DeletePackage(int id, int iduser,int broker)
        {
            try
            {
                Tb_Docpackages tb_Docpackages = db.Tb_Docpackages.Find(id);
                db.Tb_Docpackages.Remove(tb_Docpackages);
                db.SaveChanges();

                var result = "SUCCESS";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult DeleteWebinar(int id)
        {
            try
            {
                Tb_Webinars tb_Webinars = db.Tb_Webinars.Find(id);
                db.Tb_Webinars.Remove(tb_Webinars);
                db.SaveChanges();

                TempData["exito"] = "Booking deleted successfully.";
                return RedirectToAction("Reservations_management", "Portal", new { broker = 1 });
            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("Reservations_management", "Portal", new { broker = 1});
            }


        }


        public ActionResult DeleteResource(int id)
        {
            try
            {
                Tb_Resources tb_Resources = db.Tb_Resources.Find(id);
                db.Tb_Resources.Remove(tb_Resources);
                db.SaveChanges();

                TempData["exito"] = "Resource deleted successfully.";
                return RedirectToAction("Resources_management", "Portal");
            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("Resources_management", "Portal");
            }


        }

        public ActionResult DeleteResourceBroker(int id)
        {
            try
            {
                Tb_Resources tb_Resources = db.Tb_Resources.Find(id);
                db.Tb_Resources.Remove(tb_Resources);
                db.SaveChanges();

                TempData["exito"] = "Resource deleted successfully.";
                return RedirectToAction("Resourcesbroker_management", "Portal");
            }
            catch (Exception ex)
            {
                TempData["advertencia"] = "Something went wrong." + ex.Message;
                return RedirectToAction("Resourcesbroker_management", "Portal");
            }


        }

        public ActionResult DownloadDocPackage(int id)
        {


            var fileDB = (from a in db.Tb_Docpackages_details where (a.ID_Detail == id) select a).FirstOrDefault();

            var path = fileDB.URL;
            var file = Server.MapPath(path);

            return File(file, System.Net.Mime.MediaTypeNames.Application.Octet, fileDB.Title + fileDB.Extension);

        }


        public class Routes_calendar
        {
            public string title { get; set; }
            public string url { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public string className { get; set; }
            public string time { get; set; }
            public string category { get; set; }


        }

        public ActionResult GetExistEmail(string email)
        {
            try
            {
                var lstusers = (from a in db.Sys_Users where (a.Email == email) select a).ToList();

                if (lstusers.Count > 0)
                {
                    return Json("si", JsonRequestBehavior.AllowGet);
                }
                else {
                    return Json("no", JsonRequestBehavior.AllowGet);
                }

               

            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult GetEvents(DateTime startf, DateTime endf)
        {
            try
            {
                var lstwebinars = (from a in db.Tb_Webinars where ((a.Date >= startf && a.Date <= endf) && a.Active == 1) select a).ToList();

                List<Routes_calendar> rutaslst = new List<Routes_calendar>();
                foreach (var item in lstwebinars)
                {
                    Routes_calendar rt = new Routes_calendar();

                    rt.title = item.Title.ToUpper();
                    rt.url = item.Url;
                    rt.category = item.Category;
                    rt.start = item.Date.ToString("yyyy-MM-dd");
                    rt.time = item.Date.ToShortTimeString();
                    rt.end = item.Date.AddDays(1).ToString("yyyy-MM-dd");
                    if (item.Category == "Meeting")
                    {
                        rt.className = "block b-t b-t-2x b-secondary";//"#2081d6";
                    }

                    else
                    {
                        rt.className = "block b-t b-t-2x b-info";//"#2081d6";
                    }

                    rutaslst.Add(rt);
                }
                //}
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result = javaScriptSerializer.Serialize(rutaslst);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
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
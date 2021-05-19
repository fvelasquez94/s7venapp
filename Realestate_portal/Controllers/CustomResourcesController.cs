using Newtonsoft.Json;
using Realestate_portal.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace Realestate_portal.Controllers
{
    public class CustomResourcesController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();
        // GET: CustomResources
        public ActionResult Index()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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


                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        
        }


        
        public ActionResult Step_one()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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
                List<Tb_TemplatesCatalogo> lsttemplates = new List<Tb_TemplatesCatalogo>();
                lsttemplates = (from a in db.Tb_TemplatesCatalogo where (a.ID_Company == activeuser.ID_Company) select a).ToList();

                return View(lsttemplates);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

      


      
        public ActionResult Shop()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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
                List<Tb_TemplatesCatalogo> lsttemplates = new List<Tb_TemplatesCatalogo>();
                lsttemplates = (from a in db.Tb_TemplatesCatalogo where (a.ID_Company==activeuser.ID_Company && a.Resource!="") select a ).ToList();

                return View(lsttemplates);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

       


       
        public ActionResult Editor(int id)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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

                var template = (from a in db.Tb_TemplatesCatalogo where (a.ID_Template == id) select a).FirstOrDefault();

                int preloadhtml = 0;
                if (template.htmltext != "") {
                    preloadhtml = 1;
                }
                ViewBag.preloadhtml = preloadhtml;
                ViewBag.htmlcode = ReplaceNewlines(template.htmltext,"");

                return View(template);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }



        string ReplaceNewlines(string blockOfText, string replaceWith)
        {
            return blockOfText.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
        }

        public ActionResult BusinessCard()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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


                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }


     
        [HttpPost]
        public ActionResult Agregardetalle([Bind(Include = "Resource,Style,Size,Company_name,Name,Title,Cellphone,Address,Email,Website,Facebook_url,Quantity,GroupQty,Price,ID_order,Status")] crTb_OrderDetails crTb_OrderDetailsObject)
        {
            //active
            //ordered
            //delivered
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            var existe = (from a in db.crTb_Orders where (a.Status == "active") select a).FirstOrDefault();

            
            if (crTb_OrderDetailsObject.Style == null) { crTb_OrderDetailsObject.Style = "";  }
            if (crTb_OrderDetailsObject.Size == null) { crTb_OrderDetailsObject.Size = "";  }
            if (crTb_OrderDetailsObject.Company_name == null) { crTb_OrderDetailsObject.Company_name = "";  }
            if (crTb_OrderDetailsObject.Name == null) { crTb_OrderDetailsObject.Name = "";  }
            if (crTb_OrderDetailsObject.Title == null) { crTb_OrderDetailsObject.Title = ""; }
            if (crTb_OrderDetailsObject.Cellphone == null) { crTb_OrderDetailsObject.Cellphone = "";  }
            if (crTb_OrderDetailsObject.Address == null) { crTb_OrderDetailsObject.Address = "";  }
            if (crTb_OrderDetailsObject.Email == null) { crTb_OrderDetailsObject.Email = "";  }
            if (crTb_OrderDetailsObject.Website == null) { crTb_OrderDetailsObject.Website = "";  }
            if (crTb_OrderDetailsObject.Facebook_url == null) { crTb_OrderDetailsObject.Facebook_url = "";  }
            if (crTb_OrderDetailsObject.Quantity == 0) { crTb_OrderDetailsObject.Quantity = 0;  }
            if (crTb_OrderDetailsObject.GroupQty == 0) { crTb_OrderDetailsObject.GroupQty = 0;  }
            if (crTb_OrderDetailsObject.Price == 0) { crTb_OrderDetailsObject.Price = 0;  }
            
            
            if(existe != null)
            {
                crTb_OrderDetailsObject.ID_order = existe.ID_order;
                crTb_OrderDetailsObject.Status = "active";
                crTb_OrderDetailsObject.ID_Company = activeuser.ID_Company;
                db.crTb_OrderDetails.Add(crTb_OrderDetailsObject);
                db.SaveChanges();
                return RedirectToAction("Check_out", "CustomResources");

            }
            else
            {
                crTb_Orders neworder = new crTb_Orders();

                neworder.Date = DateTime.UtcNow;
                neworder.ID_user = activeuser.ID_User;
                neworder.Status = "active";
                neworder.ID_Company = activeuser.ID_Company;
                db.crTb_Orders.Add(neworder);
                db.SaveChanges();

                crTb_OrderDetailsObject.ID_order = neworder.ID_order;
                crTb_OrderDetailsObject.Status = "active";
                crTb_OrderDetailsObject.ID_Company = activeuser.ID_Company;
                db.crTb_OrderDetails.Add(crTb_OrderDetailsObject);
                db.SaveChanges();
                return RedirectToAction("Check_out", "CustomResources");

            }
           
        }
       


      
     
        public ActionResult AgregardetalleShop(string Resources, string Style)
        {
            //active
            //ordered
            //delivered
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            var existe = (from a in db.crTb_Orders where (a.Status == "active") select a).FirstOrDefault();
            crTb_OrderDetails crTb_OrderDetailsObject = new crTb_OrderDetails();
            crTb_OrderDetailsObject.Style = Style;
            crTb_OrderDetailsObject.Resource = Resources;
            if (crTb_OrderDetailsObject.Style == null) { crTb_OrderDetailsObject.Style = ""; }
            if (crTb_OrderDetailsObject.Size == null) { crTb_OrderDetailsObject.Size = ""; }
            if (crTb_OrderDetailsObject.Company_name == null) { crTb_OrderDetailsObject.Company_name = ""; }
            if (crTb_OrderDetailsObject.Name == null) { crTb_OrderDetailsObject.Name = ""; }
            if (crTb_OrderDetailsObject.Title == null) { crTb_OrderDetailsObject.Title = ""; }
            if (crTb_OrderDetailsObject.Cellphone == null) { crTb_OrderDetailsObject.Cellphone = ""; }
            if (crTb_OrderDetailsObject.Address == null) { crTb_OrderDetailsObject.Address = ""; }
            if (crTb_OrderDetailsObject.Email == null) { crTb_OrderDetailsObject.Email = ""; }
            if (crTb_OrderDetailsObject.Website == null) { crTb_OrderDetailsObject.Website = ""; }
            if (crTb_OrderDetailsObject.Facebook_url == null) { crTb_OrderDetailsObject.Facebook_url = ""; }
            if (crTb_OrderDetailsObject.Quantity == 0) { crTb_OrderDetailsObject.Quantity = 0; }
            if (crTb_OrderDetailsObject.GroupQty == 0) { crTb_OrderDetailsObject.GroupQty = 0; }
            if (crTb_OrderDetailsObject.Price == 0) { crTb_OrderDetailsObject.Price = 0; }

            crTb_OrderDetailsObject.Price = 35;
            crTb_OrderDetailsObject.Quantity = 1;
            if (existe != null)
            {
                crTb_OrderDetailsObject.ID_order = existe.ID_order;
                crTb_OrderDetailsObject.Status = "active";
                crTb_OrderDetailsObject.ID_Company = activeuser.ID_Company;
                db.crTb_OrderDetails.Add(crTb_OrderDetailsObject);
                db.SaveChanges();
                return RedirectToAction("Check_out", "CustomResources");

            }
            else
            {
                crTb_Orders neworder = new crTb_Orders();

                neworder.Date = DateTime.UtcNow;
                neworder.ID_user = activeuser.ID_User;
                neworder.Status = "active";
                neworder.ID_Company = activeuser.ID_Company;
                db.crTb_Orders.Add(neworder);
                db.SaveChanges();

                crTb_OrderDetailsObject.ID_order = neworder.ID_order;
                crTb_OrderDetailsObject.Status = "active";
                crTb_OrderDetailsObject.ID_Company = activeuser.ID_Company;
                db.crTb_OrderDetails.Add(crTb_OrderDetailsObject);
                db.SaveChanges();
                return RedirectToAction("Check_out", "CustomResources");

            }

        }

      


      
        public ActionResult crearPlantilla()
        {

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            Tb_TemplatesCatalogo nuevaplantilla = new Tb_TemplatesCatalogo();
            nuevaplantilla.Broker_name = activeuser.Sys_Company.Name;
            nuevaplantilla.Template_name = "New template";
            nuevaplantilla.Resource = "";
            nuevaplantilla.Style = 1;
            nuevaplantilla.Price = 0;
            nuevaplantilla.Status = "";
            nuevaplantilla.visible = true;
            nuevaplantilla.Url_image = "";
            nuevaplantilla.original = true;
            nuevaplantilla.ID_Company = activeuser.ID_Company;
            nuevaplantilla.htmltext = "";
            db.Tb_TemplatesCatalogo.Add(nuevaplantilla);
            db.SaveChanges();

            return RedirectToAction("Editor", "CustomResources", new { id=nuevaplantilla.ID_Template});

        }

        
        public class htmlModel
        {
            [AllowHtml]
            public string htmlformat { get; set; }
            public string title { get; set; }
            public string type { get; set; }
            public int id_template { get; set; }
        }

     

        [HttpPost]
        public ActionResult guardarPlantilla(htmlModel plantilla)
        {
            try {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                Tb_TemplatesCatalogo nuevaplantilla = db.Tb_TemplatesCatalogo.Find(plantilla.id_template); 
                nuevaplantilla.Template_name = plantilla.title;
                nuevaplantilla.Resource = plantilla.type;
                nuevaplantilla.htmltext = plantilla.htmlformat;

                db.Entry(nuevaplantilla).State = EntityState.Modified;
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

      
        public ActionResult DoorHanger()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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


                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult Flyer()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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


                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult ForSaleSign()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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


                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult FrameSign()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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


                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult OpenHouseSign()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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


                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult Rider()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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


                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult Preview() {
            return View();
        }
        public ActionResult StandUpBanner()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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


                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult Check_out()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Custom Resources";
                ViewData["Page"] = "Index";
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
                crTb_Orders carrito = new crTb_Orders();
                carrito = (from a in db.crTb_Orders where (a.Status == "active") select a).FirstOrDefault();

                List<crTb_OrderDetails> compras = new List<crTb_OrderDetails>(); 
                compras = db.crTb_OrderDetails.Where(c => c.Status == "active").ToList();

                try
                {
                    if (carrito.ID_order != 0)
                    {
                        ViewBag.ID_order = carrito.ID_order;
                    }
                    else
                    {
                        ViewBag.ID_order = 0;
                    }
                }
                catch
                {

                }
               

                return View(compras);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

       
        public ActionResult deleteDetail(int id)
        {
            crTb_OrderDetails crTb_OrderDetailsObject = db.crTb_OrderDetails.Find(id);
            db.crTb_OrderDetails.Remove(crTb_OrderDetailsObject);
            db.SaveChanges();
            return RedirectToAction("Check_out");
        }

       

        public ActionResult actualizarEstado(int id)
        {
            var carrito = db.crTb_Orders.Find(id);
            if(carrito!= null)
            {
                carrito.Status = "ordered";
                db.Entry(carrito).State = EntityState.Modified;
                db.SaveChanges();
                db.Database.ExecuteSqlCommand("update crTb_OrderDetails set Status='ordered' where Status='active' and ID_order = {0}",carrito.ID_order);
            }
            
            return RedirectToAction("Index", "CustomResources");
        }

       
        public ActionResult Orders()
        {
             var ordenes= db.crTb_Orders.OrderByDescending(x => x.Date).ToList();         
            return View(ordenes);
        }
        
        public ActionResult orderDetails(int? id)
        {
            var detalles = db.crTb_OrderDetails.Where(x => x.ID_order == id);

            return View(detalles);
        }


    }
}
using Newtonsoft.Json;
using Realestate_portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Realestate_portal.Models.ViewModels.CRM;
using System.Web.Script.Serialization;
using System.IO;
using Realestate_portal.HttpPostedFile;

namespace Realestate_portal.Controllers
{
    public class CRMController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();

//global ajax variables for datatable calls 
      
     

        // GET: CRM
        public ActionResult Index()
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

                ViewBag.rol = "";


                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    ViewBag.rol = "Admin";
                }
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }


        public ActionResult Scheduler()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "CRM";
                ViewData["Page"] = "Scheduler";
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

                ViewBag.rol = "";


                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    ViewBag.rol = "Admin";


                }

                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }


        public ActionResult CRMDashboard(int broker = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "CRM";
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

                ViewBag.rol = "";

                int totalagents = 0;
                int totalteams = 0;
                int totaltasks = 0;
              

                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.teamleader = activeuser.Team_Leader;

                     var propertiesprojectedgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "ON CONTRACT") select f).ToList();
                    var propertiesgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "CLOSED") select f).ToList();
                    var totalproperties = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User) select f).Count();
                    var totalLeads = (from f in db.Tb_Customers where (f.ID_Company == activeuser.ID_Company && f.Lead) select f).Count();

                    decimal totalprojectedgains = 0;
                    decimal totalgains = 0;
                    if (propertiesprojectedgains.Count > 0) { totalprojectedgains = propertiesprojectedgains.Select(c => c.Commission_amount).Sum(); }
                    if (propertiesgains.Count > 0) { totalgains = propertiesgains.Select(c => c.Commission_amount).Sum(); }

                    ViewBag.totallistings = totalproperties;
                    ViewBag.totalleads = totalLeads;
                    ViewBag.totalgainsprojected = totalprojectedgains.ToString("N2");
                    ViewBag.totalgains = totalgains.ToString("N2");

                }
                else
                {
                    if (broker == 0)
                    {
                        ViewBag.rol = "Admin";
                        var companyusers = (from c in db.Sys_Users.Where(c => c.ID_Company == activeuser.ID_Company) select c).ToList();

                        decimal comission = 0;
                        decimal gains = 0;
                        int totalproperties = 0;
                        int totalleads = 0;
                       
                        totalleads = (from f in db.Tb_Customers where (f.ID_Company == activeuser.ID_Company && f.Lead) select f).Count();

                        totalagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company) select f).Count();
                        totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company) select f).Count();

                        foreach (var user in companyusers)
                        {
                            var listComission = (from f in db.Tb_Process.Where(f => f.ID_User == user.ID_User && f.Stage == "ON CONTRACT") select f).ToList();
                            if (listComission.Count > 0) { comission += listComission.Select(c => c.Commission_amount).Sum(); }

                            var listgains = (from f in db.Tb_Process where (f.ID_User == user.ID_User && f.Stage == "CLOSED") select f).ToList();
                            if (listgains.Count > 0) { gains += listgains.Select(c => c.Commission_amount).Sum(); }
                            totalproperties += (from f in db.Tb_Process where (f.ID_User == user.ID_User) select f).Count();
                        }
                        ViewBag.totalgainsprojected = comission.ToString("N2");
                        ViewBag.totalgains = gains.ToString("N2");
                        ViewBag.totallistings = totalproperties;
                        ViewBag.totalleads = totalleads;
                    }
                    else
                    {
                        ViewBag.rol = "SA";
                    }
                }
                ViewBag.totalagents = totalagents;
                ViewBag.totalteams = totalteams;
                ViewBag.totaltasks = totaltasks;

                ViewBag.selbroker = broker;
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        public ActionResult Tasks(int broker = 0, string token="")
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "CRM";
                ViewData["Page"] = "Tasks";
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

                ViewBag.rol = "";
                ViewBag.token = token;

                List<Tb_Tasks> lst_tasks = new List<Tb_Tasks>();

                if (r.Contains("Agent"))
                {
                    lst_tasks = (from a in db.Tb_Tasks where (a.ID_User == activeuser.ID_User) select a).ToList();

                }
                else
                {
                    lst_tasks = (from a in db.Tb_Tasks where (a.ID_User == activeuser.ID_User) select a).ToList();
                }

                ViewBag.selbroker = broker;
                return View(lst_tasks);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }
        public ActionResult Customers()
        {
            if (generalClass.checkSession())
            {
                //mover al login
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                ViewBag.notifications = lstAlerts;
                ViewBag.userID = activeuser.ID_User;
                ViewBag.userName = activeuser.Name + " " + activeuser.LastName;
                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.selbroker = 0;
                    ViewBag.teamleader = activeuser.Team_Leader;

                    var propertiesprojectedgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "ON CONTRACT") select f).ToList();
                    var propertiesgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "CLOSED") select f).ToList();
                    var totalproperties = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User) select f).Count();

                    decimal totalprojectedgains = 0;
                    decimal totalgains = 0;
                    if (propertiesprojectedgains.Count > 0) { totalprojectedgains = propertiesprojectedgains.Select(c => c.Commission_amount).Sum(); }
                    if (propertiesgains.Count > 0) { totalgains = propertiesgains.Select(c => c.Commission_amount).Sum(); }

                    ViewBag.totalcustomers = totalproperties;
                    ViewBag.totalgainsprojected = totalprojectedgains.ToString("N2");
                    ViewBag.totalgains = totalgains.ToString("N2");

                }
                else if (activeuser.Roles.Contains("SA"))
                {
                    ViewBag.rol = "SA";
                    ViewBag.selbroker = 1;
                }
                else if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";
                    ViewBag.selbroker = 0;
                    var companyusers = (from c in db.Sys_Users.Where(c => c.ID_Company == activeuser.ID_Company) select c).ToList();

                    decimal comission = 0;
                    decimal gains = 0;
                    int totalcustomer = 0;

                    foreach (var user in companyusers)
                    {
                        var listComission = (from f in db.Tb_Process.Where(f => f.ID_User == user.ID_User && f.Stage == "ON CONTRACT") select f).ToList();
                        if (listComission.Count > 0) { comission += listComission.Select(c => c.Commission_amount).Sum(); }

                        var listgains = (from f in db.Tb_Process where (f.ID_User == user.ID_User && f.Stage == "CLOSED") select f).ToList();
                        if (listgains.Count > 0) { gains += listgains.Select(c => c.Commission_amount).Sum(); }
                        totalcustomer += (from f in db.Tb_Process where (f.ID_User == user.ID_User) select f).Count();
                    }
                    ViewBag.totalgainsprojected = comission.ToString("N2");
                    ViewBag.totalgains = gains.ToString("N2");
                    ViewBag.totalcustomers = totalcustomer;
                }               


                return View();
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        public ActionResult CustomerAjax(string status, int broker=0)
        {
            if (generalClass.checkSession())
            {

               
        Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "CRM";
                ViewData["Page"] = "Customers";
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

                //local variables here we extract the data sent by the framework datatable
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("Columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                int pageSize, skip, recordsTotal;

                List<CustomerTableViewModel> tb_Customers;

            IQueryable<CustomerTableViewModel> query = (from c in db.Tb_Customers
                                                        orderby c.LastName ascending select new CustomerTableViewModel
                                                        {
                                                            Id = c.ID_Customer,
                                                            Name = c.LastName + " " + c.Name,
                                                            Marital_status = c.Marital_status,
                                                            Type = c.Type,
                                                            Email = c.Email,
                                                            Phone = c.Phone,
                                                            User_assigned = "",
                                                            Creation_date = c.Creation_date,
                                                            ID_Company = c.ID_Company,
                                                            Lead = c.Lead,
                                                            ID_User = 0,
                                                            Team = "",
                                                            DateString = "",
                                                        });
           

                IQueryable<CustomerTableViewModel> query2 = (from a in db.Tb_Customers
                                                            join c in db.Tb_Customers_Users on a.ID_Customer equals c.Id_Customer
                                                            join u in db.Sys_Users on c.Id_User equals u.ID_User
                                                            orderby a.LastName ascending
                                                            select new CustomerTableViewModel
                                                            {
                                                                Id = a.ID_Customer,
                                                                Name = a.LastName + " " + a.Name,
                                                                Marital_status = a.Marital_status,
                                                                Type = a.Type,
                                                                Email = a.Email,
                                                                Phone = a.Phone,
                                                                User_assigned = u.LastName + " " + u.Name,
                                                                Creation_date = a.Creation_date,
                                                                ID_Company = a.ID_Company,
                                                                Lead = a.Lead,
                                                                ID_User = c.Id_User,
                                                                Team = u.Leader_Name,
                                                                DateString = "",
                                                            });

              

                pageSize = !length.Equals("") ? Convert.ToInt32(length) : 0;
            skip = !start.Equals("") ? Convert.ToInt32(start) : 0;
            recordsTotal = 0;


                if (status!="All")
                {
                    query = query.Where(a => a.Marital_status.Equals(status));
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(a => a.Name.Contains(searchValue) || a.Phone.Contains(searchValue) || a.Email.Contains(searchValue)
                   || a.Type.Contains(searchValue) || a.Marital_status.Contains(searchValue) || a.User_assigned.Contains(searchValue)
                   );
                }

                //we check sortcolumn and sortcolumn direction
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                //to do order by it was necessary to add the reference or package from nugget System.Linq.Dynamic.Core
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }
           
                if (r.Contains("Agent"))
                {
                    if (activeuser.Team_Leader == true)
                    {
                   
                        query = query2.Where(a => a.Lead == false && a.ID_Company == activeuser.ID_Company && a.Team == activeuser.Leader_Name).OrderBy(l => l.Name);

                    }
                    else
                    {
                        query = query2.Where(a => a.Lead == false && a.ID_Company == activeuser.ID_Company && a.ID_User == activeuser.ID_User).OrderBy(l => l.Name);
                    }
                  
                        

                    var propertiesprojectedgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "ON CONTRACT") select f).ToList();
                    var propertiesgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "CLOSED") select f).ToList();
                    var totalproperties = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User) select f).Count();

                    decimal totalprojectedgains = 0;
                    decimal totalgains = 0;
                    if (propertiesprojectedgains.Count > 0) { totalprojectedgains = propertiesprojectedgains.Select(c => c.Commission_amount).Sum(); }
                    if (propertiesgains.Count > 0) { totalgains = propertiesgains.Select(c => c.Commission_amount).Sum(); }

                    ViewBag.totalcustomers = totalproperties;
                    ViewBag.totalgainsprojected = totalprojectedgains.ToString("N2");
                    ViewBag.totalgains = totalgains.ToString("N2");
                }
                else
                {
                    if (broker == 0)
                    {
                        query = query.Where(a => a.Lead == false && a.ID_Company == activeuser.ID_Company);
                      
                        
                        var companyusers = (from c in db.Sys_Users.Where(c => c.ID_Company == activeuser.ID_Company) select c).ToList();

                        decimal comission = 0;
                        decimal gains = 0;
                        int totalcustomer = 0;

                        foreach (var user in companyusers)
                        {
                            var listComission = (from f in db.Tb_Process.Where(f => f.ID_User == user.ID_User && f.Stage == "ON CONTRACT") select f).ToList();
                            if (listComission.Count > 0) { comission += listComission.Select(c => c.Commission_amount).Sum(); }

                            var listgains = (from f in db.Tb_Process where (f.ID_User == user.ID_User && f.Stage == "CLOSED") select f).ToList();
                            if (listgains.Count > 0) { gains += listgains.Select(c => c.Commission_amount).Sum(); }
                            totalcustomer += (from f in db.Tb_Process where (f.ID_User == user.ID_User) select f).Count();
                        }
                        ViewBag.totalgainsprojected = comission.ToString("N2");
                        ViewBag.totalgains = gains.ToString("N2");
                        ViewBag.totalcustomers = totalcustomer;
                    }
                    else
                    {
                        query = query.Where(a => a.Lead == false && a.ID_Company == broker);
                    }
                }
                recordsTotal = query.Count();                
                tb_Customers = query.Skip(skip).Take(pageSize).ToList();
                foreach (var cus in tb_Customers)
                {

                    if (!r.Contains("Agent"))
                    {
                        var user_assigned = (from c in db.Tb_Customers_Users where (c.Id_Customer == cus.Id) select c).ToList();

                        foreach (var user in user_assigned)
                        {
                            var ua = (from u in db.Sys_Users where (u.ID_User == user.Id_User) select u).FirstOrDefault();
                            if (ua != null)
                            {
                                cus.User_assigned = cus.User_assigned + "   " + ua.LastName + " " + ua.Name;
                                cus.Team = cus.Team + "   " + ua.Leader_Name;
                            }

                        }
                    }
                    
                    cus.DateString = cus.Creation_date.ToShortDateString();


                }
                if (activeuser.Team_Leader == true)
                {
                    tb_Customers = GetTeamLeads(tb_Customers);
                }
              

                


                return  Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = tb_Customers});

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

        public List<CustomerTableViewModel> GetTeamLeads(List<CustomerTableViewModel> tb_Customers) 
        {
            List<CustomerTableViewModel> teamLeads = new List<CustomerTableViewModel>() ;
            int last_id = 0;
            string user_assigned = "";

            foreach (var item in tb_Customers)
            {
                if (last_id != item.Id)
                {
                    user_assigned = "";
                    teamLeads.Add(item);
                   
                    user_assigned = item.User_assigned;
                }
                else
                {
                    user_assigned = user_assigned + "    " + item.User_assigned;

                    foreach (var leads in teamLeads)
                    {
                        if (leads.Id == item.Id)
                        {
                            leads.User_assigned = user_assigned;
                        }
                    }

                }
                last_id = item.Id;
            }
            return teamLeads;
        }


        public ActionResult Leads()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //NOTIFICATIONS
                List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                ViewBag.notifications = lstAlerts;
                //HEADER DATA
                ViewBag.activeuser = activeuser;
                ViewBag.company = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();
                //ROLES
                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.selbroker = 0;
                }
                else if (activeuser.Roles.Contains("SA"))
                {
                    ViewBag.rol = "SA";
                    ViewBag.selbroker = 1;
                }
                else if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";
                    ViewBag.selbroker = 0;
                }
                List<LeadsMain> leads = new List<LeadsMain>();




                leads = (from a in db.Tb_Customers
                         where (a.Lead == true && a.ID_Company == activeuser.ID_Company)
                         select new LeadsMain
                         {
                             ID_lead = a.ID_Customer,
                             Name = a.LastName + " " + a.Name,
                             Marital_status = a.Marital_status,
                             Type = a.Type,
                             Email = a.Email,
                             Phone = a.Phone,
                             Creation_date = a.Creation_date,
                             ID_Company = a.ID_Company,
                             Lead = a.Lead,
                             Team = (from t in db.Tb_WorkTeams where (a.ID_team == t.ID_team) select t.Name).FirstOrDefault(),
                             Agents = (from cu in db.Tb_Customers_Users
                                       join u in db.Sys_Users on cu.Id_User equals u.ID_User
                                       where ((cu.ID_team == a.ID_team || cu.Id_Customer == a.ID_Customer) && cu.Id_User!=4) select new TeamsModel_Users {
                                           Id_User = cu.Id_User, Name = u.Name + " " + u.LastName, Email = u.Email, Url_image = u.Image }).Distinct().ToList(),
                      
                         }).ToList();

                return View(leads);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }


        public ActionResult Teams(string token = "")
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
                ViewBag.token = token;
                //ROLES
                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.selbroker = 0;
                }
                else if (activeuser.Roles.Contains("SA"))
                {
                    ViewBag.rol = "SA";
                    ViewBag.selbroker = 1;
                }
                else if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";
                    ViewBag.selbroker = 0;
                }


                List<TeamsModel> teams = new List<TeamsModel>();
                List<Sys_Users> agents = new List<Sys_Users>();
                List<Tb_Customers> leads = new List<Tb_Customers>();

                teams = (from a in db.Tb_WorkTeams
                         //join c in db.Tb_Customers_Users on a.ID_team equals c.ID_team //si es directo a usuario
                         select new TeamsModel
                         {
                             ID_team = a.ID_team,
                             Name=a.Name,
                             Active=a.Active,
                             Creation_date=a.Creation_date,
                             Last_update=a.Last_update,
                             Description=a.Description,
                             ID_Company=a.ID_Company,
                             Users=(from u in db.Tb_Customers_Users join d in db.Sys_Users on u.Id_User equals d.ID_User where(u.ID_team==a.ID_team) select new TeamsModel_Users {
                                 Name=d.Name + " " + d.LastName, Id_User=u.Id_User, Email=d.Email, Url_image=d.Image
                             }).ToList(),
                             Leads=(from l in db.Tb_Customers where (l.ID_team==a.ID_team) select new TeamsModel_Leads { Id_Lead=l.ID_Customer, Name=l.Name + " " + l.LastName}).ToList()
                           

                         }).ToList();

                agents = db.Sys_Users.Where(c => c.Roles.Contains("Agent") && c.Active && c.ID_User != 4).ToList();
                leads = db.Tb_Customers.Where(c => c.Lead && c.Active ).ToList();
                ViewBag.agents = agents;
                ViewBag.leads = leads;
                return View(teams);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        public ActionResult LeadsAjax(string status, int broker = 0)
        {
            if (generalClass.checkSession())
            {

                Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "CRM";
                ViewData["Page"] = "Leads";
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

                //local variables here we extract the data sent by the framework datatable
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var sortColumn = Request.Form.GetValues("Columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                int pageSize, skip, recordsTotal;
                List<CustomerTableViewModel> tb_Customers;
                CustomerViewModel custVM = new CustomerViewModel();
                IQueryable<CustomerTableViewModel> query = (from a in db.Tb_Customers orderby a.LastName ascending
                                                            select new CustomerTableViewModel
                                                            {
                                                                Id = a.ID_Customer,
                                                                Name = a.LastName + " " + a.Name,
                                                                Marital_status = a.Marital_status,
                                                                Type = a.Type,
                                                                Email = a.Email,
                                                                Phone = a.Phone,
                                                                User_assigned = "",
                                                                Creation_date = a.Creation_date,
                                                                ID_Company = a.ID_Company,
                                                                Lead = a.Lead,
                                                                ID_User = 0,
                                                                DateString = "",
                                                            });

                pageSize = !length.Equals("") ? Convert.ToInt32(length) : 0;
                skip = !start.Equals("") ? Convert.ToInt32(start) : 0;
                recordsTotal = 0;


                if (status != "All")
                {
                    query = query.Where(a => a.Marital_status.Equals(status));
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(a => a.Name.Contains(searchValue) || a.Phone.Contains(searchValue) || a.Email.Contains(searchValue)
                   || a.Type.Contains(searchValue) || a.Marital_status.Contains(searchValue) || a.User_assigned.Contains(searchValue) 
                   );
                }

               

                //we check sortcolumn and sortcolumn direction
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
                {
                    //to do order by it was necessary to add the reference or package from nugget System.Linq.Dynamic.Core
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }

                if (r.Contains("Agent"))
                {
                    query = query.Where(a => a.ID_User == activeuser.ID_User && a.Lead == true);
                    var propertiesprojectedgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "ON CONTRACT") select f).ToList();
                    var propertiesgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "CLOSED") select f).ToList();
                    var totalproperties = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User) select f).Count();

                    decimal totalprojectedgains = 0;
                    decimal totalgains = 0;
                    if (propertiesprojectedgains.Count > 0) { totalprojectedgains = propertiesprojectedgains.Select(c => c.Commission_amount).Sum(); }
                    if (propertiesgains.Count > 0) { totalgains = propertiesgains.Select(c => c.Commission_amount).Sum(); }

                    ViewBag.totalcustomers = totalproperties;
                    ViewBag.totalgainsprojected = totalprojectedgains.ToString("N2");
                    ViewBag.totalgains = totalgains.ToString("N2");
                }
                else
                {
                    if (broker == 0)
                    {
                        query = query.Where(a => a.Lead == true && a.ID_Company == activeuser.ID_Company).OrderBy(l => l.Name);
                        var companyusers = (from c in db.Sys_Users.Where(c => c.ID_Company == activeuser.ID_Company) select c).ToList();

                        decimal comission = 0;
                        decimal gains = 0;
                        int totalcustomer = 0;

                        foreach (var user in companyusers)
                        {
                           var listComission = (from f in db.Tb_Process.Where(f => f.ID_User == user.ID_User && f.Stage == "ON CONTRACT") select f).ToList();
                            if(listComission.Count > 0) { comission += listComission.Select(c => c.Commission_amount).Sum(); }
                           
                          var listgains = (from f in db.Tb_Process where (f.ID_User == user.ID_User && f.Stage == "CLOSED") select f).ToList();
                            if (listgains.Count > 0) { gains += listgains.Select(c => c.Commission_amount).Sum(); }
                            totalcustomer += (from f in db.Tb_Process where (f.ID_User == user.ID_User) select f).Count();
                        }
                        ViewBag.totalgainsprojected = comission.ToString("N2");
                        ViewBag.totalgains = gains.ToString("N2");
                        ViewBag.totalcustomers = totalcustomer;
                    }
                    else
                    {
                        query = query.Where(a => a.Lead == true && a.ID_Company == broker);
                    }
                }

                tb_Customers = query.Skip(skip).Take(pageSize).ToList();
                foreach (var cus in tb_Customers)
                {
                    cus.DateString = cus.Creation_date.ToShortDateString();
                }
                recordsTotal = tb_Customers.Count();


                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = tb_Customers });

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }



        public ActionResult EditCustomer()
        {
            
            return View();
        }

        public ActionResult CRMDashboardCopy()
        {
            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
           
            List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
            ViewBag.notifications = lstAlerts;
            ViewBag.userID = activeuser.ID_User;
            ViewBag.userName = activeuser.Name + " " + activeuser.LastName;
            return View();
        }

        public ActionResult Properties(int broker = 0)
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
                //FIN HEADER

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;

                ViewBag.rol = "";
                IQueryable<Tb_Process> Tb_Process;

                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    Tb_Process = db.Tb_Process.Where(a => a.ID_User == activeuser.ID_User).Include(t => t.Tb_Customers);
                }
                else
                {

                    ViewBag.rol = "Admin";

                    if (broker == 0)
                    {

                        var clientes = db.Tb_Customers.Where(c => c.ID_Company == activeuser.ID_Company).Select(c => c.ID_Customer).ToArray();
                        Tb_Process = db.Tb_Process.Where(t => clientes.Contains(t.ID_Customer)).Include(t => t.Tb_Customers);
                    }
                    else
                    {
                        ViewBag.rol = "SA";
                        var clientes = db.Tb_Customers.Where(c => c.ID_Company == broker).Select(c => c.ID_Customer).ToArray();
                        Tb_Process = db.Tb_Process.Where(t => clientes.Contains(t.ID_Customer)).Include(t => t.Tb_Customers);
                    }


                }
                foreach (var item in Tb_Process)
                {
                    var agent = db.Sys_Users.Where(a => a.ID_User == item.ID_User).Select(a => a).FirstOrDefault();
                    item.Loan_Officer_name = agent.Name + " " + agent.LastName;
                }

                ViewBag.selbroker = broker;
                var propertiesprojectedgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "ON CONTRACT") select f).ToList();
                var propertiesgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "CLOSED") select f).ToList();
                var totalproperties = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User) select f).Count();

                decimal totalprojectedgains = 0;
                decimal totalgains = 0;
                if (propertiesprojectedgains.Count > 0) { totalprojectedgains = propertiesprojectedgains.Select(c => c.Commission_amount).Sum(); }
                if (propertiesgains.Count > 0) { totalgains = propertiesgains.Select(c => c.Commission_amount).Sum(); }

                ViewBag.totalcustomers = totalproperties;
                ViewBag.totalgainsprojected = totalprojectedgains;
                ViewBag.totalgains = totalgains;


                return View(Tb_Process.ToList());

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        [HttpPost]
        public ActionResult UpdateByAjaxSideBard(int? id,int broker, string stage)
        {
            Tb_Customers tb_Customers =(from a in db.Tb_Customers.Where(a=> a.ID_Customer==id) select a).AsNoTracking().FirstOrDefault();
       
            tb_Customers.Marital_status = stage;
         

            db.Entry(tb_Customers).State = EntityState.Modified;
            db.SaveChanges();

            

            //Sys_Notifications newnotification = new Sys_Notifications();
            //newnotification.Active = true;
            //newnotification.Date = DateTime.UtcNow;
            //newnotification.Title = "New Customer assigned.";
            //newnotification.Description = "Customer: " + tb_Customers.Name + " " + tb_Customers.LastName + ".";
            //newnotification.ID_user = UserID;
            //db.Sys_Notifications.Add(newnotification);
            //db.SaveChanges();

            return null;
        }


        public ActionResult CustomerDashboard(int? id, int broker=0)
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
                List<Tb_Notes> notes = (from a in db.Tb_Notes.Where(a => a.ID_Customer == id) orderby a.Date select a).ToList();
                List<Tb_Process> properties = (from a in db.Tb_Process.Where(a => a.ID_Customer == id) select a).ToList();
                
                ViewBag.ID_Company = new SelectList(db.Sys_Company, "ID_Company", "Name", tb_Customers.ID_Company);
                var lstsource = (from o in db.Tb_Source where (o.Id_Company == activeuser.ID_Company || o.Id_Company == null) select o).ToList();
                ViewBag.lstSource = lstsource;
                var lststatus = (from t in db.Tb_Status where (t.Id_Company == activeuser.ID_Company || t.Id_Company == null) select t).ToList();
                ViewBag.lstStatus = lststatus;
                var lstLeadDocs = (from doc in db.Tb_LeadDocs where (doc.Id_Customer == id) select doc).ToList();
                ViewBag.leadDocs = lstLeadDocs;
                ViewBag.rol = "";
                ViewBag.customer = id;
                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;


                if (r.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.ID_User = new SelectList((from t in db.Sys_Users
                                                      where (t.Active == true)
                                                      orderby t.LastName ascending
                                                      // where (t.Roles.Contains("Agent"))
                                                      select new
                                                      {
                                                          ID = t.ID_User,
                                                          FullName = t.Name + " " + t.LastName
                                                      }), "ID", "FullName");

                    

                    if (activeuser.Team_Leader == true)
                    {
                        ViewBag.userslist = (from u in db.Sys_Users where (u.Sys_Company.ID_Company == activeuser.ID_Company && (u.ID_User == activeuser.ID_User || u.Id_Leader == activeuser.ID_User) && u.Active == true) orderby u.LastName ascending select u).ToList();
                    }
                    else
                    {
                        ViewBag.userslist = (from u in db.Sys_Users where (u.Sys_Company.ID_Company == activeuser.ID_Company && u.ID_User == activeuser.ID_User && u.Active == true) orderby u.LastName ascending select u).ToList();

                    }

                    var propertiesprojectedgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "ON CONTRACT") select f).ToList();
                    var propertiesgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "CLOSED") select f).ToList();
                    var totalproperties = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User) select f).Count();

                    decimal totalprojectedgains = 0;
                    decimal totalgains = 0;
                    if (propertiesprojectedgains.Count > 0) { totalprojectedgains = propertiesprojectedgains.Select(c => c.Commission_amount).Sum(); }
                    if (propertiesgains.Count > 0) { totalgains = propertiesgains.Select(c => c.Commission_amount).Sum(); }

                    ViewBag.totalcustomers = totalproperties;
                    ViewBag.totalgainsprojected = totalprojectedgains.ToString("N2");
                    ViewBag.totalgains = totalgains.ToString("N2"); ;

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
                                                          where (t.Active == true)
                                                          orderby t.LastName ascending
                                                          // where (t.Roles.Contains("Agent"))
                                                          select new
                                                          {
                                                              ID = t.ID_User,
                                                              FullName = t.Name + " " + t.LastName
                                                          }), "ID", "FullName");
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {
                            ViewBag.ID_User = new SelectList((from t in db.Sys_Users
                                                              where ((t.ID_Company == activeuser.ID_Company || t.ID_User == 4) && t.Active == true)
                                                              orderby t.LastName ascending
                                                              select new
                                                              {
                                                                  ID = t.ID_User,
                                                                  FullName = t.Name + " " + t.LastName
                                                              }), "ID", "FullName");

                            var companyusers = (from c in db.Sys_Users.Where(c => c.ID_Company == activeuser.ID_Company) select c).ToList();

                            decimal comission = 0;
                            decimal gains = 0;
                            int totalcustomer = 0;

                            foreach (var user in companyusers)
                            {
                                var listComission = (from f in db.Tb_Process.Where(f => f.ID_User == user.ID_User && f.Stage == "ON CONTRACT") select f).ToList();
                                if (listComission.Count > 0) { comission += listComission.Select(c => c.Commission_amount).Sum(); }

                                var listgains = (from f in db.Tb_Process where (f.ID_User == user.ID_User && f.Stage == "CLOSED") select f).ToList();
                                if (listgains.Count > 0) { gains += listgains.Select(c => c.Commission_amount).Sum(); }
                                totalcustomer += (from f in db.Tb_Process where (f.ID_User == user.ID_User) select f).Count();
                            }
                            ViewBag.totalgainsprojected = comission.ToString("N2");
                            ViewBag.totalgains = gains.ToString("N2");
                            ViewBag.totalcustomers = totalcustomer;
                        }
                        else
                        {
                            ViewBag.rol = "SA";

                        }
                    }
                    ViewBag.userslist = (from u in db.Sys_Users where (u.Sys_Company.ID_Company == activeuser.ID_Company && (u.Roles == "Agent" || u.Roles == "Admin") && u.Active == true) orderby u.LastName ascending select u).ToList();
                }

                ViewBag.selbroker = broker;
                
                CustomerCRMDashboard dcustomer = new CustomerCRMDashboard();
                dcustomer.properties = properties;
                dcustomer.notes = notes;
                dcustomer.customer = tb_Customers;
                dcustomer.property = properties.FirstOrDefault();

                Tb_Docpackages Id_doc = (from a in db.Tb_Docpackages.Where(a => a.ID_Customer == dcustomer.customer.ID_Customer) select a).FirstOrDefault();

                List<Tb_Docpackages_details> lstpackages = new List<Tb_Docpackages_details>();
                if (Id_doc !=null)
                {
                    lstpackages = (from a in db.Tb_Docpackages_details where (a.ID_docpackage == Id_doc.ID_docpackage && a.original == false) select a).ToList();
                }
                

                dcustomer.pack_Det = lstpackages;
                dcustomer.package = Id_doc;
                return View("CustomerDashboard", dcustomer);

            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
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
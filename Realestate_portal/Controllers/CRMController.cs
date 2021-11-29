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
using Realestate_portal.Models.ViewModels;
using System.Globalization;

namespace Realestate_portal.Controllers
{
    public class CRMController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();

//global ajax variables for datatable calls merge
      
     

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

        // GET: Brokers
        public ActionResult Brokers(int broker = 0, string token = "")
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
                //FIN HEADER


                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();

                ViewBag.selbroker = broker;

                return View(lstCompanies);
            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

        }


        public ActionResult CRMDashboard(string fstartd, string fendd, int broker = 0, int agent=0)
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
                    //FILTROS VARIABLES
                    DateTime filtrostartdate;
                    DateTime filtroenddate;
                    ////filtros de fecha 
                    var firstDayOfMonth = new DateTime(DateTime.Today.Year, 1, 1);
                    var lastDayOfMonth = new DateTime(DateTime.Today.Year, 12, 31);

                if (fstartd == null || fstartd == "") { filtrostartdate = firstDayOfMonth; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                    if (fendd == null || fendd == "") { filtroenddate = lastDayOfMonth; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                    var startDate = filtrostartdate;
                var endDate = filtroenddate;
                //importante
                var months = MonthsBetween(startDate, endDate);
                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();
                List<GainsReport> lstgainsreport = new List<GainsReport>();
                ViewBag.rol = "";

                int totalagents = 0;
                int totalteams = 0;
                int totaltasks = 0;
   
                List<Tb_Process> lstlistings = new List<Tb_Process>();
                List<Tb_Customers> lstleads = new List<Tb_Customers>();
                List<Sys_Users> lstagents = new List<Sys_Users>();

                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.teamleader = activeuser.Team_Leader;

                    if (activeuser.Team_Leader)
                    {
                        var assigned = (from f in db.Tb_Customers_Users where (f.Id_User == activeuser.ID_User && f.ID_team != 0) select f.ID_team).ToArray();

                        if (assigned.Length > 0)
                        {
                            var equipo = (from e in db.Tb_Customers_Users where (assigned.Contains(e.ID_team)) select e.Id_User).ToArray();

                            //Verificamos si uso filtro por agente

                            if (agent != 0)
                            {
                                var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == agent) select c.Id_Customer).Distinct().ToArray();

                                lstlistings = (from f in db.Tb_Process where (f.ID_User == agent && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).ToList();

                                lstagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company && equipo.Contains(f.ID_User)) select f).ToList();

                                totalagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company && equipo.Contains(f.ID_User)) select f).Count();
                                totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company && f.ID_User== agent) select f).Count();

                                var totalproperties = (from f in db.Tb_Process where (f.ID_User == agent && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).Count();
                                lstleads = (from f in db.Tb_Customers where (leadsassigned.Contains(f.ID_Customer) && f.Lead) select f).ToList();
                                totalteams = (from f in db.Tb_WorkTeams where (assigned.Contains(f.ID_team)) select f).Count();

                                if (lstlistings.Count > 0)
                                {
                                    //reporte
                                    if (months.Count() > 0)
                                    {
                                        if (months.Count() == 1)
                                        { //como solo mostraria un mes la grafica no se desplegaria correctamente
                                            var actual = months.FirstOrDefault();
                                            var fechaactual = new DateTime(actual.Year, actual.montint, 1);
                                            var fechaant = fechaactual.AddMonths(-1);
                                            var fechapost = fechaactual.AddMonths(1);
                                            GainsReport datacero = new GainsReport();
                                            datacero.monthyear = fechaant.ToString("MMMM") + " - " + fechaant.Year;

                                            datacero.projected = 0;
                                            datacero.gains = 0;

                                            lstgainsreport.Add(datacero);


                                        }
                                    }


                                    foreach (var item in months)
                                    {
                                        GainsReport data = new GainsReport();
                                        data.monthyear = item.Month + " - " + item.Year;

                                        var StartDay = new DateTime(item.Year, item.montint, 1);
                                        var EndDay = StartDay.AddMonths(1).AddDays(-1);

                                        data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                                        data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                                        lstgainsreport.Add(data);

                                        if (months.Count() == 1)
                                        {
                                            //para finalizar la grafica
                                            GainsReport datacero2 = new GainsReport();
                                            datacero2.monthyear = StartDay.AddMonths(1).ToString("MMMM") + " - " + StartDay.AddMonths(1).Year;

                                            datacero2.projected = 0;
                                            datacero2.gains = 0;

                                            lstgainsreport.Add(datacero2);
                                        }
                                    }
                                }
                            }
                            else {
                                var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == activeuser.ID_User || equipo.Contains(c.Id_User)) select c.Id_Customer).Distinct().ToArray();

                                lstlistings = (from f in db.Tb_Process where ((f.ID_User == activeuser.ID_User || equipo.Contains(f.ID_User)) && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).ToList();

                                lstagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company && equipo.Contains(f.ID_User)) select f).ToList();

                                totalagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company && equipo.Contains(f.ID_User)) select f).Count();
                                totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company && equipo.Contains(f.ID_User)) select f).Count();

                                var totalproperties = (from f in db.Tb_Process where ((f.ID_User == activeuser.ID_User || equipo.Contains(f.ID_User)) && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).Count();
                                lstleads = (from f in db.Tb_Customers where (leadsassigned.Contains(f.ID_Customer) && f.Lead) select f).ToList();
                                totalteams = (from f in db.Tb_WorkTeams where (assigned.Contains(f.ID_team)) select f).Count();

                                if (lstlistings.Count > 0)
                                {
                                    //reporte
                                    if (months.Count() > 0)
                                    {
                                        if (months.Count() == 1)
                                        { //como solo mostraria un mes la grafica no se desplegaria correctamente
                                            var actual = months.FirstOrDefault();
                                            var fechaactual = new DateTime(actual.Year, actual.montint, 1);
                                            var fechaant = fechaactual.AddMonths(-1);
                                            var fechapost = fechaactual.AddMonths(1);
                                            GainsReport datacero = new GainsReport();
                                            datacero.monthyear = fechaant.ToString("MMMM") + " - " + fechaant.Year;

                                            datacero.projected = 0;
                                            datacero.gains = 0;

                                            lstgainsreport.Add(datacero);


                                        }
                                    }


                                    foreach (var item in months)
                                    {
                                        GainsReport data = new GainsReport();
                                        data.monthyear = item.Month + " - " + item.Year;

                                        var StartDay = new DateTime(item.Year, item.montint, 1);
                                        var EndDay = StartDay.AddMonths(1).AddDays(-1);

                                        data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                                        data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                                        lstgainsreport.Add(data);

                                        if (months.Count() == 1)
                                        {
                                            //para finalizar la grafica
                                            GainsReport datacero2 = new GainsReport();
                                            datacero2.monthyear = StartDay.AddMonths(1).ToString("MMMM") + " - " + StartDay.AddMonths(1).Year;

                                            datacero2.projected = 0;
                                            datacero2.gains = 0;

                                            lstgainsreport.Add(datacero2);
                                        }
                                    }
                                }
                            }
                          


                        }
                        else {
                            var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == activeuser.ID_User) select c.Id_Customer).Distinct().ToArray();

                            lstlistings = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).ToList();

                            var totalproperties = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).Count();
                            lstleads = (from f in db.Tb_Customers where (leadsassigned.Contains(f.ID_Customer) && f.Lead) select f).ToList();
                            totalteams = (from f in db.Tb_WorkTeams select f).Count();
                            totalagents = 0;
                            totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company && f.ID_User==activeuser.ID_User) select f).Count();
                            if (lstlistings.Count > 0)
                            {
                                //reporte
                                if (months.Count() > 0)
                                {
                                    if (months.Count() == 1)
                                    { //como solo mostraria un mes la grafica no se desplegaria correctamente
                                        var actual = months.FirstOrDefault();
                                        var fechaactual = new DateTime(actual.Year, actual.montint, 1);
                                        var fechaant = fechaactual.AddMonths(-1);
                                        var fechapost = fechaactual.AddMonths(1);
                                        GainsReport datacero = new GainsReport();
                                        datacero.monthyear = fechaant.ToString("MMMM") + " - " + fechaant.Year;

                                        datacero.projected = 0;
                                        datacero.gains = 0;

                                        lstgainsreport.Add(datacero);


                                    }
                                }


                                foreach (var item in months)
                                {
                                    GainsReport data = new GainsReport();
                                    data.monthyear = item.Month + " - " + item.Year;

                                    var StartDay = new DateTime(item.Year, item.montint, 1);
                                    var EndDay = StartDay.AddMonths(1).AddDays(-1);

                                    data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                                    data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                                    lstgainsreport.Add(data);

                                    if (months.Count() == 1)
                                    {
                                        //para finalizar la grafica
                                        GainsReport datacero2 = new GainsReport();
                                        datacero2.monthyear = StartDay.AddMonths(1).ToString("MMMM") + " - " + StartDay.AddMonths(1).Year;

                                        datacero2.projected = 0;
                                        datacero2.gains = 0;

                                        lstgainsreport.Add(datacero2);
                                    }
                                }
                            }

                        }

                    }
                    else {
                     var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == activeuser.ID_User) select c.Id_Customer).Distinct().ToArray();

                    lstlistings = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).ToList();

                    var totalproperties = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).Count();
                   lstleads = (from f in db.Tb_Customers where (leadsassigned.Contains(f.ID_Customer) && f.Lead) select f).ToList();
                    totalteams = (from f in db.Tb_WorkTeams select f).Count();

                        totalagents = 0;
                        totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company && f.ID_User == activeuser.ID_User) select f).Count();

                        if (lstlistings.Count > 0)
                    {
                        //reporte
                        if (months.Count() > 0)
                        {
                            if (months.Count() == 1)
                            { //como solo mostraria un mes la grafica no se desplegaria correctamente
                                var actual = months.FirstOrDefault();
                                var fechaactual = new DateTime(actual.Year, actual.montint, 1);
                                var fechaant = fechaactual.AddMonths(-1);
                                var fechapost = fechaactual.AddMonths(1);
                                GainsReport datacero = new GainsReport();
                                datacero.monthyear = fechaant.ToString("MMMM") + " - " + fechaant.Year;

                                datacero.projected = 0;
                                datacero.gains = 0;

                                lstgainsreport.Add(datacero);


                            }
                        }


                        foreach (var item in months)
                        {
                            GainsReport data = new GainsReport();
                            data.monthyear = item.Month + " - " + item.Year;

                            var StartDay = new DateTime(item.Year, item.montint, 1);
                            var EndDay = StartDay.AddMonths(1).AddDays(-1);

                            data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                            data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                            lstgainsreport.Add(data);

                            if (months.Count() == 1)
                            {
                                //para finalizar la grafica
                                GainsReport datacero2 = new GainsReport();
                                datacero2.monthyear = StartDay.AddMonths(1).ToString("MMMM") + " - " + StartDay.AddMonths(1).Year;

                                datacero2.projected = 0;
                                datacero2.gains = 0;

                                lstgainsreport.Add(datacero2);
                            }
                        }
                    }
                    }

   
                }
                else
                {
                    if (activeuser.Roles.Contains("Admin"))
                    {
                        ViewBag.rol = "Admin";

                        if (agent != 0)
                        {
                            var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == agent) select c.Id_Customer).Distinct().ToArray();
                            lstleads = (from f in db.Tb_Customers where (f.ID_Company == activeuser.ID_Company && leadsassigned.Contains(f.ID_Customer) ) select f).ToList();

                            lstagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company) select f).ToList();

                            totalagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company) select f).Count();
                            totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company && f.ID_User== agent) select f).Count();
                            totalteams = (from f in db.Tb_WorkTeams where (f.ID_Company == activeuser.ID_Company) select f).Count();

                            var idscustomer = lstleads.Select(c => c.ID_Customer).Distinct().ToArray();

                            lstlistings = (from f in db.Tb_Process where (f.ID_User==agent && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).ToList();


                            if (lstlistings.Count > 0)
                            {
                                //reporte
                                if (months.Count() > 0)
                                {
                                    if (months.Count() == 1)
                                    { //como solo mostraria un mes la grafica no se desplegaria correctamente
                                        var actual = months.FirstOrDefault();
                                        var fechaactual = new DateTime(actual.Year, actual.montint, 1);
                                        var fechaant = fechaactual.AddMonths(-1);
                                        var fechapost = fechaactual.AddMonths(1);
                                        GainsReport datacero = new GainsReport();
                                        datacero.monthyear = fechaant.ToString("MMMM") + " - " + fechaant.Year;

                                        datacero.projected = 0;
                                        datacero.gains = 0;

                                        lstgainsreport.Add(datacero);


                                    }
                                }


                                foreach (var item in months)
                                {
                                    GainsReport data = new GainsReport();
                                    data.monthyear = item.Month + " - " + item.Year;

                                    var StartDay = new DateTime(item.Year, item.montint, 1);
                                    var EndDay = StartDay.AddMonths(1).AddDays(-1);

                                    data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                                    data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                                    lstgainsreport.Add(data);

                                    if (months.Count() == 1)
                                    {
                                        //para finalizar la grafica
                                        GainsReport datacero2 = new GainsReport();
                                        datacero2.monthyear = StartDay.AddMonths(1).ToString("MMMM") + " - " + StartDay.AddMonths(1).Year;

                                        datacero2.projected = 0;
                                        datacero2.gains = 0;

                                        lstgainsreport.Add(datacero2);
                                    }
                                }
                            }
                        }
                        else
                        {
                            lstleads = (from f in db.Tb_Customers where (f.ID_Company == activeuser.ID_Company && f.Lead) select f).ToList();

                        lstagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company) select f).ToList();

                        totalagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company) select f).Count();
                        totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company) select f).Count();
                        totalteams = (from f in db.Tb_WorkTeams where (f.ID_Company == activeuser.ID_Company) select f).Count();

                        var idscustomer = lstleads.Select(c => c.ID_Customer).Distinct().ToArray();

                        lstlistings = (from f in db.Tb_Process where (idscustomer.Contains(f.ID_Customer) && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).ToList();


                        if (lstlistings.Count > 0)
                        {
                            //reporte
                            if (months.Count() > 0)
                            {
                                if (months.Count() == 1) { //como solo mostraria un mes la grafica no se desplegaria correctamente
                                    var actual = months.FirstOrDefault();
                                    var fechaactual = new DateTime(actual.Year, actual.montint, 1);
                                    var fechaant = fechaactual.AddMonths(-1);
                                    var fechapost = fechaactual.AddMonths(1);
                                    GainsReport datacero = new GainsReport();
                                    datacero.monthyear = fechaant.ToString("MMMM") + " - " + fechaant.Year;

                                    datacero.projected = 0;
                                    datacero.gains = 0;

                                    lstgainsreport.Add(datacero);


                                }
                            }


                            foreach (var item in months)
                            {
                                GainsReport data = new GainsReport();
                                data.monthyear = item.Month + " - " + item.Year;

                                var StartDay = new DateTime(item.Year, item.montint, 1);
                                var EndDay = StartDay.AddMonths(1).AddDays(-1);

                                data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                                data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                                lstgainsreport.Add(data);

                                if (months.Count() == 1)
                                {
                                    //para finalizar la grafica
                                    GainsReport datacero2 = new GainsReport();
                                    datacero2.monthyear = StartDay.AddMonths(1).ToString("MMMM") + " - " + StartDay.AddMonths(1).Year;

                                    datacero2.projected = 0;
                                    datacero2.gains = 0;

                                    lstgainsreport.Add(datacero2);
                                }
                            }
                        }
                    }

                    }
                    else
                    {
                        ViewBag.rol = "SA";

                        if (agent != 0)
                        {
                            var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == agent) select c.Id_Customer).Distinct().ToArray();
                            lstleads = (from f in db.Tb_Customers where (f.ID_Company == activeuser.ID_Company && leadsassigned.Contains(f.ID_Customer)) select f).ToList();

                            lstagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company) select f).ToList();

                            totalagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company) select f).Count();
                            totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company && f.ID_User == agent) select f).Count();
                            totalteams = (from f in db.Tb_WorkTeams where (f.ID_Company == activeuser.ID_Company) select f).Count();

                            var idscustomer = lstleads.Select(c => c.ID_Customer).Distinct().ToArray();

                            lstlistings = (from f in db.Tb_Process where (f.ID_User == agent && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).ToList();


                            if (lstlistings.Count > 0)
                            {
                                //reporte
                                if (months.Count() > 0)
                                {
                                    if (months.Count() == 1)
                                    { //como solo mostraria un mes la grafica no se desplegaria correctamente
                                        var actual = months.FirstOrDefault();
                                        var fechaactual = new DateTime(actual.Year, actual.montint, 1);
                                        var fechaant = fechaactual.AddMonths(-1);
                                        var fechapost = fechaactual.AddMonths(1);
                                        GainsReport datacero = new GainsReport();
                                        datacero.monthyear = fechaant.ToString("MMMM") + " - " + fechaant.Year;

                                        datacero.projected = 0;
                                        datacero.gains = 0;

                                        lstgainsreport.Add(datacero);


                                    }
                                }


                                foreach (var item in months)
                                {
                                    GainsReport data = new GainsReport();
                                    data.monthyear = item.Month + " - " + item.Year;

                                    var StartDay = new DateTime(item.Year, item.montint, 1);
                                    var EndDay = StartDay.AddMonths(1).AddDays(-1);

                                    data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                                    data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                                    lstgainsreport.Add(data);

                                    if (months.Count() == 1)
                                    {
                                        //para finalizar la grafica
                                        GainsReport datacero2 = new GainsReport();
                                        datacero2.monthyear = StartDay.AddMonths(1).ToString("MMMM") + " - " + StartDay.AddMonths(1).Year;

                                        datacero2.projected = 0;
                                        datacero2.gains = 0;

                                        lstgainsreport.Add(datacero2);
                                    }
                                }
                            }
                        }
                        else
                        {
                            lstleads = (from f in db.Tb_Customers where (f.ID_Company == activeuser.ID_Company && f.Lead) select f).ToList();

                            lstagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company) select f).ToList();

                            totalagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company) select f).Count();
                            totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company) select f).Count();
                            totalteams = (from f in db.Tb_WorkTeams where (f.ID_Company == activeuser.ID_Company) select f).Count();

                            var idscustomer = lstleads.Select(c => c.ID_Customer).Distinct().ToArray();

                            lstlistings = (from f in db.Tb_Process where (idscustomer.Contains(f.ID_Customer) && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).ToList();


                            if (lstlistings.Count > 0)
                            {
                                //reporte
                                if (months.Count() > 0)
                                {
                                    if (months.Count() == 1)
                                    { //como solo mostraria un mes la grafica no se desplegaria correctamente
                                        var actual = months.FirstOrDefault();
                                        var fechaactual = new DateTime(actual.Year, actual.montint, 1);
                                        var fechaant = fechaactual.AddMonths(-1);
                                        var fechapost = fechaactual.AddMonths(1);
                                        GainsReport datacero = new GainsReport();
                                        datacero.monthyear = fechaant.ToString("MMMM") + " - " + fechaant.Year;

                                        datacero.projected = 0;
                                        datacero.gains = 0;

                                        lstgainsreport.Add(datacero);


                                    }
                                }


                                foreach (var item in months)
                                {
                                    GainsReport data = new GainsReport();
                                    data.monthyear = item.Month + " - " + item.Year;

                                    var StartDay = new DateTime(item.Year, item.montint, 1);
                                    var EndDay = StartDay.AddMonths(1).AddDays(-1);

                                    data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                                    data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                                    lstgainsreport.Add(data);

                                    if (months.Count() == 1)
                                    {
                                        //para finalizar la grafica
                                        GainsReport datacero2 = new GainsReport();
                                        datacero2.monthyear = StartDay.AddMonths(1).ToString("MMMM") + " - " + StartDay.AddMonths(1).Year;

                                        datacero2.projected = 0;
                                        datacero2.gains = 0;

                                        lstgainsreport.Add(datacero2);
                                    }
                                }
                            }
                        }

                    }
                }

                //List<AgentsProperties_ViewDashboard> lstAgentes = new List<AgentsProperties_ViewDashboard>();

                //lstAgentes = db.Sys_Users.Where(t => t.ID_User != 4 && t.Roles.Contains("Agent") && t.ID_Company == activeuser.ID_Company).Select(c => new AgentsProperties_ViewDashboard
                //{
                //    ID_User = c.ID_User,
                //    Active = c.Active,
                //    Brokerage_name = c.Brokerage_name,
                //    Email = c.Email,
                //    ID_Company = c.ID_Company,
                //    Image = c.Image,
                //    LastName = c.LastName,
                //    Main_telephone = c.Main_telephone,
                //    Member_since = c.Member_since,
                //    My_License = c.My_License,
                //    Name = c.Name,
                //    properties = (from det in db.Tb_Process
                //                  where (det.ID_User == c.ID_User)
                //                  select det).Count()

                //}).OrderByDescending(t => t.properties).Take(3).ToList();

                //ViewBag.bestsellers = lstAgentes;

                ViewBag.leads = lstleads;
                ViewBag.totalagents = totalagents;
                ViewBag.totalteams = totalteams;
                ViewBag.totaltasks = totaltasks;
                ViewBag.agents = lstagents;
                ViewBag.gainsreport_dates = lstgainsreport.Select(c=>c.monthyear).ToArray();
                ViewBag.gainsreport_projected = lstgainsreport.Select(c=>c.projected).ToArray();
                ViewBag.gainsreport_gains = lstgainsreport.Select(c=>c.gains).ToArray();
                ViewBag.selbroker = broker;
                ViewBag.agent = agent;
                if (lstlistings.Count == 0) {
                    Tb_Process newblank = new Tb_Process();
                    lstlistings.Add(newblank);
                }
                return View(lstlistings);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }


        public ActionResult CRMDashboard_Teams(string fstartd, string fendd, int broker = 0, int team = 0)
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
                //FILTROS VARIABLES
                DateTime filtrostartdate;
                DateTime filtroenddate;
                ////filtros de fecha 
                var firstDayOfMonth = new DateTime(DateTime.Today.Year, 1, 1);
                var lastDayOfMonth = new DateTime(DateTime.Today.Year, 12, 31);

                if (fstartd == null || fstartd == "") { filtrostartdate = firstDayOfMonth; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = lastDayOfMonth; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                var startDate = filtrostartdate;
                var endDate = filtroenddate;
                //importante
                var months = MonthsBetween(startDate, endDate);
                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();
                List<GainsReport> lstgainsreport = new List<GainsReport>();
                ViewBag.rol = "";

                int totalagents = 0;
                int totalteams = 0;
                int totaltasks = 0;

                List<Tb_Process> lstlistings = new List<Tb_Process>();
                List<Tb_Customers> lstleads = new List<Tb_Customers>();
                List<Tb_WorkTeams> lstteams = new List<Tb_WorkTeams>();

                    if (activeuser.Roles.Contains("Admin"))
                    {
                        ViewBag.rol = "Admin";

                        if (team != 0)
                        {
                        //filtramos equipo
                            var leadsassigned = (from c in db.Tb_Customers_Users where (c.ID_team == team) select c.Id_Customer).Distinct().ToArray();
                            var agentsassigned = (from c in db.Tb_Customers_Users where (c.ID_team == team) select c.Id_User).Distinct().ToArray();
                            lstleads = (from f in db.Tb_Customers where (f.ID_Company == activeuser.ID_Company && leadsassigned.Contains(f.ID_Customer)) select f).ToList();

                            lstteams = (from a in db.Tb_WorkTeams where (a.ID_Company == activeuser.ID_Company) select a).ToList();

                        totalagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company && f.Roles.Contains("Agent")) select f).Count();
                            totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company && agentsassigned.Contains(f.ID_User)) select f).Count();
                            totalteams = (from f in db.Tb_WorkTeams where (f.ID_Company == activeuser.ID_Company) select f).Count();

                            var idscustomer = lstleads.Select(c => c.ID_Customer).Distinct().ToArray();

                            lstlistings = (from f in db.Tb_Process where (agentsassigned.Contains(f.ID_User) && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).ToList();


                            if (lstlistings.Count > 0)
                            {
                                //reporte
                                if (months.Count() > 0)
                                {
                                    if (months.Count() == 1)
                                    { //como solo mostraria un mes la grafica no se desplegaria correctamente
                                        var actual = months.FirstOrDefault();
                                        var fechaactual = new DateTime(actual.Year, actual.montint, 1);
                                        var fechaant = fechaactual.AddMonths(-1);
                                        var fechapost = fechaactual.AddMonths(1);
                                        GainsReport datacero = new GainsReport();
                                        datacero.monthyear = fechaant.ToString("MMMM") + " - " + fechaant.Year;

                                        datacero.projected = 0;
                                        datacero.gains = 0;

                                        lstgainsreport.Add(datacero);


                                    }
                                }


                                foreach (var item in months)
                                {
                                    GainsReport data = new GainsReport();
                                    data.monthyear = item.Month + " - " + item.Year;

                                    var StartDay = new DateTime(item.Year, item.montint, 1);
                                    var EndDay = StartDay.AddMonths(1).AddDays(-1);

                                    data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                                    data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                                    lstgainsreport.Add(data);

                                    if (months.Count() == 1)
                                    {
                                        //para finalizar la grafica
                                        GainsReport datacero2 = new GainsReport();
                                        datacero2.monthyear = StartDay.AddMonths(1).ToString("MMMM") + " - " + StartDay.AddMonths(1).Year;

                                        datacero2.projected = 0;
                                        datacero2.gains = 0;

                                        lstgainsreport.Add(datacero2);
                                    }
                                }
                            }
                        }
                        else
                        {

                        //filtramos por todos los equipos del broker 
                        var teams = (from a in db.Tb_WorkTeams where (a.ID_Company == activeuser.ID_Company) select (int?)a.ID_team).ToArray();
                        var leadsassigned = (from c in db.Tb_Customers_Users where (teams.Contains(c.ID_team)) select c.Id_Customer).Distinct().ToArray();
                        var agentsassigned = (from c in db.Tb_Customers_Users where (teams.Contains(c.ID_team)) select c.Id_User).Distinct().ToArray();
                        lstleads = (from f in db.Tb_Customers where (f.ID_Company == activeuser.ID_Company && leadsassigned.Contains(f.ID_Customer)) select f).ToList();

                        lstteams = (from a in db.Tb_WorkTeams where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                        totalagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company && f.Roles.Contains("Agent")) select f).Count();
                        totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company && agentsassigned.Contains(f.ID_User)) select f).Count();
                        totalteams = (from f in db.Tb_WorkTeams where (f.ID_Company == activeuser.ID_Company) select f).Count();

                        var idscustomer = lstleads.Select(c => c.ID_Customer).Distinct().ToArray();

                        lstlistings = (from f in db.Tb_Process where (agentsassigned.Contains(f.ID_User) && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).ToList();

                        if (lstlistings.Count > 0)
                            {
                                //reporte
                                if (months.Count() > 0)
                                {
                                    if (months.Count() == 1)
                                    { //como solo mostraria un mes la grafica no se desplegaria correctamente
                                        var actual = months.FirstOrDefault();
                                        var fechaactual = new DateTime(actual.Year, actual.montint, 1);
                                        var fechaant = fechaactual.AddMonths(-1);
                                        var fechapost = fechaactual.AddMonths(1);
                                        GainsReport datacero = new GainsReport();
                                        datacero.monthyear = fechaant.ToString("MMMM") + " - " + fechaant.Year;

                                        datacero.projected = 0;
                                        datacero.gains = 0;

                                        lstgainsreport.Add(datacero);


                                    }
                                }


                                foreach (var item in months)
                                {
                                    GainsReport data = new GainsReport();
                                    data.monthyear = item.Month + " - " + item.Year;

                                    var StartDay = new DateTime(item.Year, item.montint, 1);
                                    var EndDay = StartDay.AddMonths(1).AddDays(-1);

                                    data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                                    data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                                    lstgainsreport.Add(data);

                                    if (months.Count() == 1)
                                    {
                                        //para finalizar la grafica
                                        GainsReport datacero2 = new GainsReport();
                                        datacero2.monthyear = StartDay.AddMonths(1).ToString("MMMM") + " - " + StartDay.AddMonths(1).Year;

                                        datacero2.projected = 0;
                                        datacero2.gains = 0;

                                        lstgainsreport.Add(datacero2);
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        ViewBag.rol = "SA";

                        if (team != 0)
                        {
                        //filtramos equipo
                        var leadsassigned = (from c in db.Tb_Customers_Users where (c.ID_team == team) select c.Id_Customer).Distinct().ToArray();
                        var agentsassigned = (from c in db.Tb_Customers_Users where (c.ID_team == team) select c.Id_User).Distinct().ToArray();
                        lstleads = (from f in db.Tb_Customers where (f.ID_Company == activeuser.ID_Company && leadsassigned.Contains(f.ID_Customer)) select f).ToList();

                        lstteams = (from a in db.Tb_WorkTeams where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                        totalagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company && f.Roles.Contains("Agent")) select f).Count();
                        totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company && agentsassigned.Contains(f.ID_User)) select f).Count();
                        totalteams = (from f in db.Tb_WorkTeams where (f.ID_Company == activeuser.ID_Company) select f).Count();

                        var idscustomer = lstleads.Select(c => c.ID_Customer).Distinct().ToArray();

                        lstlistings = (from f in db.Tb_Process where (agentsassigned.Contains(f.ID_User) && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).ToList();


                        if (lstlistings.Count > 0)
                            {
                                //reporte
                                if (months.Count() > 0)
                                {
                                    if (months.Count() == 1)
                                    { //como solo mostraria un mes la grafica no se desplegaria correctamente
                                        var actual = months.FirstOrDefault();
                                        var fechaactual = new DateTime(actual.Year, actual.montint, 1);
                                        var fechaant = fechaactual.AddMonths(-1);
                                        var fechapost = fechaactual.AddMonths(1);
                                        GainsReport datacero = new GainsReport();
                                        datacero.monthyear = fechaant.ToString("MMMM") + " - " + fechaant.Year;

                                        datacero.projected = 0;
                                        datacero.gains = 0;

                                        lstgainsreport.Add(datacero);


                                    }
                                }


                                foreach (var item in months)
                                {
                                    GainsReport data = new GainsReport();
                                    data.monthyear = item.Month + " - " + item.Year;

                                    var StartDay = new DateTime(item.Year, item.montint, 1);
                                    var EndDay = StartDay.AddMonths(1).AddDays(-1);

                                    data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                                    data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                                    lstgainsreport.Add(data);

                                    if (months.Count() == 1)
                                    {
                                        //para finalizar la grafica
                                        GainsReport datacero2 = new GainsReport();
                                        datacero2.monthyear = StartDay.AddMonths(1).ToString("MMMM") + " - " + StartDay.AddMonths(1).Year;

                                        datacero2.projected = 0;
                                        datacero2.gains = 0;

                                        lstgainsreport.Add(datacero2);
                                    }
                                }
                            }
                        }
                        else
                        {
                        //filtramos por todos los equipos del broker 
                        var teams = (from a in db.Tb_WorkTeams where (a.ID_Company == activeuser.ID_Company) select (int?)a.ID_team).ToArray();
                        var leadsassigned = (from c in db.Tb_Customers_Users where (teams.Contains(c.ID_team)) select c.Id_Customer).Distinct().ToArray();
                        var agentsassigned = (from c in db.Tb_Customers_Users where (teams.Contains(c.ID_team)) select c.Id_User).Distinct().ToArray();
                        lstleads = (from f in db.Tb_Customers where (f.ID_Company == activeuser.ID_Company && leadsassigned.Contains(f.ID_Customer)) select f).ToList();

                        lstteams = (from a in db.Tb_WorkTeams where (a.ID_Company == activeuser.ID_Company) select a).ToList();
                        totalagents = (from f in db.Sys_Users where (f.ID_Company == activeuser.ID_Company && f.Roles.Contains("Agent")) select f).Count();
                        totaltasks = (from f in db.Tb_Tasks where (f.ID_Company == activeuser.ID_Company && agentsassigned.Contains(f.ID_User)) select f).Count();
                        totalteams = (from f in db.Tb_WorkTeams where (f.ID_Company == activeuser.ID_Company) select f).Count();

                        var idscustomer = lstleads.Select(c => c.ID_Customer).Distinct().ToArray();

                        lstlistings = (from f in db.Tb_Process where (agentsassigned.Contains(f.ID_User) && f.Creation_date >= startDate && f.Creation_date <= endDate) select f).ToList();


                        if (lstlistings.Count > 0)
                            {
                                //reporte
                                if (months.Count() > 0)
                                {
                                    if (months.Count() == 1)
                                    { //como solo mostraria un mes la grafica no se desplegaria correctamente
                                        var actual = months.FirstOrDefault();
                                        var fechaactual = new DateTime(actual.Year, actual.montint, 1);
                                        var fechaant = fechaactual.AddMonths(-1);
                                        var fechapost = fechaactual.AddMonths(1);
                                        GainsReport datacero = new GainsReport();
                                        datacero.monthyear = fechaant.ToString("MMMM") + " - " + fechaant.Year;

                                        datacero.projected = 0;
                                        datacero.gains = 0;

                                        lstgainsreport.Add(datacero);


                                    }
                                }


                                foreach (var item in months)
                                {
                                    GainsReport data = new GainsReport();
                                    data.monthyear = item.Month + " - " + item.Year;

                                    var StartDay = new DateTime(item.Year, item.montint, 1);
                                    var EndDay = StartDay.AddMonths(1).AddDays(-1);

                                    data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                                    data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                                    lstgainsreport.Add(data);

                                    if (months.Count() == 1)
                                    {
                                        //para finalizar la grafica
                                        GainsReport datacero2 = new GainsReport();
                                        datacero2.monthyear = StartDay.AddMonths(1).ToString("MMMM") + " - " + StartDay.AddMonths(1).Year;

                                        datacero2.projected = 0;
                                        datacero2.gains = 0;

                                        lstgainsreport.Add(datacero2);
                                    }
                                }
                            }
                        }

                    }
                
                ViewBag.leads = lstleads;
                ViewBag.totalagents = totalagents;
                ViewBag.totalteams = totalteams;
                ViewBag.totaltasks = totaltasks;
                ViewBag.teams = lstteams;
                ViewBag.gainsreport_dates = lstgainsreport.Select(c => c.monthyear).ToArray();
                ViewBag.gainsreport_projected = lstgainsreport.Select(c => c.projected).ToArray();
                ViewBag.gainsreport_gains = lstgainsreport.Select(c => c.gains).ToArray();
                ViewBag.selbroker = broker;
                ViewBag.team = team;
                if (lstlistings.Count == 0)
                {
                    Tb_Process newblank = new Tb_Process();
                    lstlistings.Add(newblank);
                }
                return View(lstlistings);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }


        public static IEnumerable<(string Month, int montint, int Year)> MonthsBetween(
        DateTime startDate,
        DateTime endDate)
        {
            DateTime iterator;
            DateTime limit;

            if (endDate > startDate)
            {
                iterator = new DateTime(startDate.Year, startDate.Month, 1);
                limit = endDate;
            }
            else
            {
                iterator = new DateTime(endDate.Year, endDate.Month, 1);
                limit = startDate;
            }

            var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
            while (iterator <= limit)
            {
                yield return (
                    dateTimeFormat.GetMonthName(iterator.Month),iterator.Month,
                    iterator.Year
                );

                iterator = iterator.AddMonths(1);
            }
        }

        public ActionResult Tasks(int broker = 0, string token="")
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
                //FIN HEADER

                ViewBag.rol = "";
       

                List<TasksView> lst_tasks = new List<TasksView>();
                List<Sys_Users> agents = new List<Sys_Users>();
                List<Tb_Customers> leads = new List<Tb_Customers>();
                if (activeuser.Roles.Contains("Agent"))
                {
                    if (activeuser.Team_Leader)
                    {
                        var assigned = (from f in db.Tb_Customers_Users where (f.Id_User == activeuser.ID_User && f.ID_team != 0) select f.ID_team).ToArray();

                        if (assigned.Length > 0)
                        {
                            var equipo = (from e in db.Tb_Customers_Users where (assigned.Contains(e.ID_team)) select e.Id_User).ToArray();
                            var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == activeuser.ID_User || equipo.Contains(c.Id_User)) select c.Id_Customer).Distinct().ToArray();

                            lst_tasks = (from a in db.Tb_Tasks
                                         where (a.ID_User == activeuser.ID_User || equipo.Contains(a.ID_User))
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
                                             Customer = a.Customer,
                                             Name = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.Name).FirstOrDefault(),
                                             Lastname = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.LastName).FirstOrDefault()
                                         }).ToList();
                            agents = db.Sys_Users.Where(c => equipo.Contains(c.ID_User) && c.Active && c.ID_User != 4).ToList();

                            agents.Add(activeuser);

                            leads = (from a in db.Tb_Customers
                                     where (a.Lead == true && leadsassigned.Contains(a.ID_Customer))
                                     select a).ToList();
                        }
                        else {
                            lst_tasks = (from a in db.Tb_Tasks
                                         where (a.ID_User == activeuser.ID_User)
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
                                             Customer = a.Customer,
                                             Name = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.Name).FirstOrDefault(),
                                             Lastname = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.LastName).FirstOrDefault()
                                         }).ToList();
                            agents.Add(activeuser);

                            var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == activeuser.ID_User) select c.Id_Customer).Distinct().ToArray();



                            leads = (from a in db.Tb_Customers
                                     where (a.Lead == true && leadsassigned.Contains(a.ID_Customer))
                                     select a).ToList();
                        }
                            
                    }
                    else {
                        lst_tasks = (from a in db.Tb_Tasks
                                     where (a.ID_User == activeuser.ID_User)
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
                                         Customer = a.Customer,
                                         Name = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.Name).FirstOrDefault(),
                                         Lastname = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.LastName).FirstOrDefault()
                                     }).ToList();
                        agents.Add(activeuser);

                        var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == activeuser.ID_User) select c.Id_Customer).Distinct().ToArray();



                        leads = (from a in db.Tb_Customers
                                 where (a.Lead == true && leadsassigned.Contains(a.ID_Customer))
                                 select a).ToList();
                    }
              
                }
                else 
                {
                    if (activeuser.Roles.Contains("Admin"))
                    {
                        ViewBag.rol = "Admin";
                        lst_tasks = (from a in db.Tb_Tasks
                                     where(a.ID_Company==activeuser.ID_Company)
                                     select new TasksView
                                     {
                                         ID_Company = a.ID_Company,
                                         Description = a.Description,
                                         Finished = a.Finished,
                                         ID_task = a.ID_task,
                                         ID_User = a.ID_User,
                                         Lastupdate = a.Createdat,
                                         Title = a.Title,
                                         Customer = a.Customer,
                                         Url_image = (from b in db.Sys_Users where (b.ID_User == a.ID_User) select b.Image).FirstOrDefault(),
                                         Name = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.Name).FirstOrDefault(),
                                         Lastname = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.LastName).FirstOrDefault()
                                     }).ToList();
                        agents = db.Sys_Users.Where(c => c.Roles.Contains("Agent") && c.ID_Company==activeuser.ID_Company && c.Active && c.ID_User != 4).ToList();


                        leads = (from a in db.Tb_Customers
                                 where (a.Lead == true && a.ID_Company==activeuser.ID_Company)
                                 select a).ToList();
                    }
                    else {
                        ViewBag.rol = "SA";
                      
                        lst_tasks = (from a in db.Tb_Tasks
                                     where (a.ID_Company == activeuser.ID_Company)
                                     select new TasksView
                                     {
                                         ID_Company = a.ID_Company,
                                         Description = a.Description,
                                         Finished = a.Finished,
                                         ID_task = a.ID_task,
                                         ID_User = a.ID_User,
                                         Lastupdate = a.Createdat,
                                         Title = a.Title,
                                         Customer = a.Customer,
                                         Url_image = (from b in db.Sys_Users where (b.ID_User == a.ID_User) select b.Image).FirstOrDefault(),
                                         Name = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.Name).FirstOrDefault(),
                                         Lastname = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.LastName).FirstOrDefault()
                                     }).ToList();
                        agents = db.Sys_Users.Where(c => c.Roles.Contains("Agent") && c.ID_Company == activeuser.ID_Company && c.Active && c.ID_User != 4).ToList();


                        leads = (from a in db.Tb_Customers
                                 where (a.Lead == true && a.ID_Company == activeuser.ID_Company)
                                 select a).ToList();
                    }
                        
                }

                ViewBag.agents = agents;
                ViewBag.selbroker = broker;
                ViewBag.leads = leads;
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


        public ActionResult Leads(string fstartd, string fendd, string token = "")
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
                ViewBag.token = token;
                //FILTROS VARIABLES
                DateTime filtrostartdate;
                DateTime filtroenddate;
                List<LeadsMain> leads = new List<LeadsMain>();
                ////filtros de fecha 
                var firstDayOfMonth = new DateTime(DateTime.Today.Year, 1, 1);
                var lastDayOfMonth = new DateTime(DateTime.Today.Year, 12, 31);

                if (fstartd == null || fstartd == "") { filtrostartdate = firstDayOfMonth; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = lastDayOfMonth; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                var startDate = filtrostartdate;
                var endDate = filtroenddate;
                //importante
      
                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();
                //ROLES
                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.selbroker = 0;

                    if (activeuser.Team_Leader == true)
                    {

                        var assigned = (from f in db.Tb_Customers_Users where (f.Id_User == activeuser.ID_User && f.ID_team != 0) select f.ID_team).ToArray();

                        if (assigned.Length > 0)
                        {

                            var equipo = (from e in db.Tb_Customers_Users where (assigned.Contains(e.ID_team)) select e.Id_User).ToArray();

                            var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == activeuser.ID_User || equipo.Contains(c.Id_User)) select c.Id_Customer).Distinct().ToArray();



                            leads = (from a in db.Tb_Customers
                                     where (a.Lead == true && leadsassigned.Contains(a.ID_Customer) && a.Creation_date >= startDate && a.Creation_date <= endDate)
                                     select new LeadsMain
                                     {
                                         ID_lead = a.ID_Customer,
                                         Name = a.Name + " " + a.LastName,
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
                                                   where ((cu.Id_Customer == a.ID_Customer) && cu.Id_User != 4)
                                                   select new TeamsModel_Users
                                                   {
                                                       Id_User = cu.Id_User,
                                                       Name = u.Name,
                                                       Lastname = u.LastName,
                                                       Email = u.Email,
                                                       Id_Team=cu.ID_team,
                                                       Url_image = u.Image
                                                   }).Distinct().ToList(),

                                     }).ToList();

                        }
                        else {
                            var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == activeuser.ID_User) select c.Id_Customer).Distinct().ToArray();



                            leads = (from a in db.Tb_Customers
                                     where (a.Lead == true && leadsassigned.Contains(a.ID_Customer) && a.Creation_date >= startDate && a.Creation_date <= endDate)
                                     select new LeadsMain
                                     {
                                         ID_lead = a.ID_Customer,
                                         Name = a.Name + " " + a.LastName,
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
                                                   where ((cu.Id_User == activeuser.ID_User || cu.Id_Customer == a.ID_Customer) && cu.Id_User != 4)
                                                   select new TeamsModel_Users
                                                   {
                                                       Id_User = cu.Id_User,
                                                       Name = u.Name,
                                                       Lastname = u.LastName,
                                                       Email = u.Email,
                                                       Id_Team = cu.ID_team,
                                                       Url_image = u.Image
                                                   }).Distinct().ToList(),

                                     }).ToList();
                        }
                    }
                    else {
                        var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == activeuser.ID_User) select c.Id_Customer).Distinct().ToArray();



                        leads = (from a in db.Tb_Customers
                                 where (a.Lead == true && leadsassigned.Contains(a.ID_Customer) && a.Creation_date >= startDate && a.Creation_date <= endDate)
                                 select new LeadsMain
                                 {
                                     ID_lead = a.ID_Customer,
                                     Name = a.Name + " " + a.LastName,
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
                                               where ((cu.Id_User == activeuser.ID_User || cu.Id_Customer == a.ID_Customer) && cu.Id_User != 4)
                                               select new TeamsModel_Users
                                               {
                                                   Id_User = cu.Id_User,
                                                   Name = u.Name,
                                                   Id_Team = cu.ID_team,
                                                   Lastname = u.LastName,
                                                   Email = u.Email,
                                                   Url_image = u.Image
                                               }).Distinct().ToList(),

                                 }).ToList();
                    }

             
                }
                else if (activeuser.Roles.Contains("SA"))
                {
                    ViewBag.rol = "SA";
                    ViewBag.selbroker = 1;
                  
                    leads = (from a in db.Tb_Customers
                             where (a.Lead == true && a.ID_Company == activeuser.ID_Company && a.Creation_date >= startDate && a.Creation_date <= endDate)
                             select new LeadsMain
                             {
                                 ID_lead = a.ID_Customer,
                                 Name = a.Name + " " + a.LastName,
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
                                           where ((cu.Id_Customer == a.ID_Customer) && cu.Id_User != 4)
                                           select new TeamsModel_Users
                                           {
                                               Id_User = cu.Id_User,
                                               Name = u.Name,
                                               Id_Team = cu.ID_team,
                                               Lastname = u.LastName,
                                               Email = u.Email,
                                               Url_image = u.Image
                                           }).Distinct().ToList(),

                             }).ToList();

         

                }
                else if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";
                    ViewBag.selbroker = 0;

                    leads = (from a in db.Tb_Customers
                             where (a.Lead == true && a.ID_Company==activeuser.ID_Company && a.Creation_date >= startDate && a.Creation_date <= endDate)
                             select new LeadsMain
                             {
                                 ID_lead = a.ID_Customer,
                                 Name = a.Name + " " + a.LastName,
                                 Marital_status = a.Marital_status,
                                 Type = a.Type,
                                 Email = a.Email,
                                 ID_team=a.ID_team,
                                 Phone = a.Phone,
                                 Creation_date = a.Creation_date,
                                 ID_Company = a.ID_Company,
                                 Lead = a.Lead,
                                 Team = (from t in db.Tb_WorkTeams where (a.ID_team == t.ID_team) select t.Name).FirstOrDefault(),
                                 Agents = (from cu in db.Tb_Customers_Users
                                           join u in db.Sys_Users on cu.Id_User equals u.ID_User
                                           where ((cu.Id_Customer == a.ID_Customer) && cu.Id_User != 4)
                                           select new TeamsModel_Users
                                           {
                                               Id_User = cu.Id_User,
                                               Name = u.Name,
                                               Id_Team = cu.ID_team,
                                               Lastname = u.LastName,
                                               Email = u.Email,
                                               Url_image = u.Image
                                           }).Distinct().ToList(),

                             }).ToList();
                }

                if (leads.Count > 0) {
                    foreach (var item in leads)
                    {
                        if (item.ID_team != null) {
                            if (item.ID_team != 0) {
                                //asignamos equipo
                                var agentsadd = (from cu in db.Tb_Customers_Users
                                                 join u in db.Sys_Users on cu.Id_User equals u.ID_User
                                                 where ((cu.Id_Customer == item.ID_lead || cu.ID_team==item.ID_team) && cu.Id_User != 4)
                                                 select new TeamsModel_Users
                                                 {
                                                     Id_User = cu.Id_User,
                                                     Name = u.Name,
                                                     Lastname = u.LastName,
                                                     Email = u.Email,
                                                     Url_image = u.Image,
                                                       Id_Team = cu.ID_team,
                                                 }).Distinct().ToList();

                                if (agentsadd.Count > 0) {
                                    item.Agents.AddRange(agentsadd);
                                }
                            }
                        }
                    }
                }
                


             

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

                List<TeamsModel> teams = new List<TeamsModel>();
                List<Sys_Users> agents = new List<Sys_Users>();
                List<Tb_Customers> leads = new List<Tb_Customers>();

                //ROLES
                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.selbroker = 0;
                    var assigned = (from f in db.Tb_Customers_Users where (f.Id_User == activeuser.ID_User && f.ID_team != 0) select f.ID_team).ToArray();
                    teams = (from a in db.Tb_WorkTeams
                                 //join c in db.Tb_Customers_Users on a.ID_team equals c.ID_team //si es directo a usuario
                             where (assigned.Contains(a.ID_team) && a.ID_Company == activeuser.ID_Company)
                             select new TeamsModel
                             {
                                 ID_team = a.ID_team,
                                 Name = a.Name,
                                 Active = a.Active,
                                 Creation_date = a.Creation_date,
                                 Last_update = a.Last_update,
                                 Description = a.Description,
                                 ID_Company = a.ID_Company,
                                 Users = (from u in db.Tb_Customers_Users
                                          join d in db.Sys_Users on u.Id_User equals d.ID_User
                                          where (u.ID_team == a.ID_team && u.Teamleader == false)
                                          select new TeamsModel_Users
                                          {
                                              Name = d.Name,
                                              Lastname = d.LastName,
                                              Id_User = u.Id_User,
                                              Id_Team = u.ID_team,
                                              Email = d.Email,
                                              Url_image = d.Image
                                          }).ToList(),
                                 Leads = (from l in db.Tb_Customers where (l.ID_team == a.ID_team) select new TeamsModel_Leads { Id_Lead = l.ID_Customer, Name = l.Name + " " + l.LastName }).ToList(),
                                 Teamleader = (from u in db.Tb_Customers_Users
                                               join d in db.Sys_Users on u.Id_User equals d.ID_User
                                               where (u.ID_team == a.ID_team && u.Teamleader == true)
                                               select new TeamsModel_Users
                                               {
                                                   Name = d.Name,
                                                   Lastname = d.LastName,
                                                   Id_User = u.Id_User,
                                                   Id_Team = u.ID_team,
                                                   Email = d.Email,
                                                   Url_image = d.Image
                                               }).ToList()


                             }).ToList();

                    var equipo = (from e in db.Tb_Customers_Users where (assigned.Contains(e.ID_team)) select e.Id_User).ToArray();
                    var leadsassigned = (from c in db.Tb_Customers_Users where (c.Id_User == activeuser.ID_User || equipo.Contains(c.Id_User)) select c.Id_Customer).Distinct().ToArray();

                    agents = db.Sys_Users.Where(c => equipo.Contains(c.ID_User) && c.ID_User != 4).ToList();
                    leads = db.Tb_Customers.Where(c => c.Lead && c.Active && c.ID_Company == activeuser.ID_Company && leadsassigned.Contains(c.ID_Customer)).ToList();
                }
                else if (activeuser.Roles.Contains("SA"))
                {
                    ViewBag.rol = "SA";
                    ViewBag.selbroker = 1;


                    teams = (from a in db.Tb_WorkTeams
                                 //join c in db.Tb_Customers_Users on a.ID_team equals c.ID_team //si es directo a usuario
                             where(a.ID_Company==activeuser.ID_Company)
                             select new TeamsModel
                             {
                                 ID_team = a.ID_team,
                                 Name = a.Name,
                                 Active = a.Active,
                                 Creation_date = a.Creation_date,
                                 Last_update = a.Last_update,
                                 Description = a.Description,
                                 ID_Company = a.ID_Company,
                                 Users = (from u in db.Tb_Customers_Users
                                          join d in db.Sys_Users on u.Id_User equals d.ID_User
                                          where (u.ID_team == a.ID_team && u.Teamleader == false)
                                          select new TeamsModel_Users
                                          {
                                              Name = d.Name,
                                              Lastname = d.LastName,
                                              Id_User = u.Id_User,
                                              Id_Team = u.ID_team,
                                              Email = d.Email,
                                              Url_image = d.Image
                                          }).ToList(),
                                 Leads = (from l in db.Tb_Customers where (l.ID_team == a.ID_team) select new TeamsModel_Leads { Id_Lead = l.ID_Customer, Name = l.Name + " " + l.LastName }).ToList(),
                                 Teamleader = (from u in db.Tb_Customers_Users
                                               join d in db.Sys_Users on u.Id_User equals d.ID_User
                                               where (u.ID_team == a.ID_team && u.Teamleader == true)
                                               select new TeamsModel_Users
                                               {
                                                   Name = d.Name,
                                                   Lastname = d.LastName,
                                                   Id_User = u.Id_User,
                                                   Id_Team = u.ID_team,
                                                   Email = d.Email,
                                                   Url_image = d.Image
                                               }).ToList()


                             }).ToList();

                    agents = db.Sys_Users.Where(c => c.Roles.Contains("Agent") && c.Active && c.ID_User != 4 && c.ID_Company==activeuser.ID_Company).ToList();
                    leads = db.Tb_Customers.Where(c => c.Lead && c.Active && c.ID_Company==activeuser.ID_Company).ToList();

                }
                else if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";
                    ViewBag.selbroker = 0;

                    teams = (from a in db.Tb_WorkTeams
                             where (a.ID_Company == activeuser.ID_Company)
                             //join c in db.Tb_Customers_Users on a.ID_team equals c.ID_team //si es directo a usuario
                             select new TeamsModel
                             {
                                 ID_team = a.ID_team,
                                 Name = a.Name,
                                 Active = a.Active,
                                 Creation_date = a.Creation_date,
                                 Last_update = a.Last_update,
                                 Description = a.Description,
                                 ID_Company = a.ID_Company,
                                 Users = (from u in db.Tb_Customers_Users
                                          join d in db.Sys_Users on u.Id_User equals d.ID_User
                                          where (u.ID_team == a.ID_team && u.Teamleader == false)
                                          select new TeamsModel_Users
                                          {
                                              Name = d.Name,
                                              Lastname = d.LastName,
                                              Id_User = u.Id_User,
                                              Id_Team = u.ID_team,
                                              Email = d.Email,
                                              Url_image = d.Image
                                          }).ToList(),
                                 Leads = (from l in db.Tb_Customers where (l.ID_team == a.ID_team) select new TeamsModel_Leads { Id_Lead = l.ID_Customer, Name = l.Name + " " + l.LastName }).ToList(),
                                 Teamleader = (from u in db.Tb_Customers_Users
                                               join d in db.Sys_Users on u.Id_User equals d.ID_User
                                               where (u.ID_team == a.ID_team && u.Teamleader == true)
                                               select new TeamsModel_Users
                                               {
                                                   Name = d.Name,
                                                   Lastname = d.LastName,
                                                   Id_User = u.Id_User,
                                                   Id_Team = u.ID_team,
                                                   Email = d.Email,
                                                   Url_image = d.Image
                                               }).ToList()


                             }).ToList();

                    agents = db.Sys_Users.Where(c => c.Roles.Contains("Agent") && c.Active && c.ID_User != 4 && c.ID_Company == activeuser.ID_Company).ToList();
                    leads = db.Tb_Customers.Where(c => c.Lead && c.Active && c.ID_Company == activeuser.ID_Company).ToList();

                }


       


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



        public ActionResult Properties(int broker = 0)
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

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;

                ViewBag.rol = "";
                IQueryable<Tb_Process> Tb_Process;

                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    if (activeuser.Team_Leader)
                    {
                        var assigned = (from f in db.Tb_Customers_Users where (f.Id_User == activeuser.ID_User && f.ID_team != 0) select f.ID_team).ToArray();

                        if (assigned.Length > 0)
                        {
                            var equipo = (from e in db.Tb_Customers_Users where (assigned.Contains(e.ID_team)) select e.Id_User).ToArray();
                            Tb_Process = db.Tb_Process.Where(a => equipo.Contains(a.ID_User) || a.ID_User == activeuser.ID_User).Include(t => t.Tb_Customers);
                        }
                        else
                        {
                            Tb_Process = db.Tb_Process.Where(a => a.ID_User == activeuser.ID_User).Include(t => t.Tb_Customers);
                        }
                    }
                    else
                    {
                        Tb_Process = db.Tb_Process.Where(a => a.ID_User == activeuser.ID_User).Include(t => t.Tb_Customers);
                    }
                   
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
                        var clientes = db.Tb_Customers.Where(c => c.ID_Company == activeuser.ID_Company).Select(c => c.ID_Customer).ToArray();
                        Tb_Process = db.Tb_Process.Where(t => clientes.Contains(t.ID_Customer)).Include(t => t.Tb_Customers);
                    }


                }
                var usuarios = db.Sys_Users.ToList();
                foreach (var item in Tb_Process) {
                    item.Attorneys_name = usuarios.Where(c => c.ID_User == item.ID_User).Select(c => c.Name + " " + c.LastName).FirstOrDefault();
                }

                ViewBag.selbroker = broker;
     


                return View(Tb_Process.ToList());

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }
        public ActionResult PropertiesFiltered(string properties,int broker = 0)
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

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;

                ViewBag.rol = "";
                List<Tb_Process> Tb_Process= new List<Tb_Process>();

                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {

                    ViewBag.rol = "Admin";

                    if (broker == 0)
                    {

  
                    }
                    else
                    {
      
                    }


                }
                var props = properties.Split(',').Select(int.Parse);

           

                var usuarios = db.Sys_Users.ToList();
                if (properties!="")
                {
                    Tb_Process = db.Tb_Process.Where(t => props.Contains(t.ID_Process)).ToList();
                    foreach (var item in Tb_Process)
                    {
                        item.Attorneys_name = usuarios.Where(c => c.ID_User == item.ID_User).Select(c => c.Name + " " + c.LastName).FirstOrDefault();
                    }
                }
                

                ViewBag.selbroker = broker;



                return View(Tb_Process.ToList());

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
        }

        [HttpPost]
        public ActionResult UpdateByAjaxSideBard(int? id, string stage)
        {
            try
            {
                Tb_Customers tb_Customers = (from a in db.Tb_Customers.Where(a => a.ID_Customer == id) select a).AsNoTracking().FirstOrDefault();

                tb_Customers.Marital_status = stage;


                db.Entry(tb_Customers).State = EntityState.Modified;
                db.SaveChanges();


                var result = "Success";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex) {
                var result = "error";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
         
        }
        public ActionResult Agents(int broker = 0, string token="")
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
                ViewBag.token = token;
                ViewBag.selbroker = broker;
             
 
                List<AgentsView> lstAgentes = new List<AgentsView>();


                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.teamleader = activeuser.Team_Leader;

                    if (activeuser.Team_Leader == true)
                    {

                        var assigned = (from f in db.Tb_Customers_Users where (f.Id_User == activeuser.ID_User && f.ID_team!=0) select f.ID_team).ToArray();

                        if (assigned.Length>0) {

                            var equipo = (from e in db.Tb_Customers_Users where (assigned.Contains(e.ID_team)) select e.Id_User).ToArray();

                            lstAgentes = db.Sys_Users.Where(t => equipo.Contains(t.ID_User)).Select(c => new AgentsView
                            {
                                ID_User = c.ID_User,
                                Active = c.Active,
                                Address = c.Address,
                                Brokerage_name = c.Brokerage_name,
                                Email = c.Email,
                                ID_Company = c.ID_Company,
                                Image = c.Image,
                                LastName = c.LastName,
                                Main_telephone = c.Main_telephone,
                                Secundary_telephone = c.Secundary_telephone,
                                Last_login = c.Last_login,
                                Team_Leader = c.Team_Leader,
                                Member_since = c.Member_since,
                                My_License = c.My_License,
                                Name = c.Name,
                                Position = c.Position,
                                State = c.State,
                                //Leads = (from det in db.Tb_Customers_Users join ag in db.Tb_Customers on det.Id_Customer equals ag.ID_Customer where (det.Id_User == c.ID_User) select new LeadsAgents { ID_lead = det.Id_Customer, Name = ag.Name + " " + ag.LastName }).ToList(),
                                Teams = (from det in db.Tb_Customers_Users
                                         join wt in db.Tb_WorkTeams on det.ID_team equals wt.ID_team
                                         where (det.Id_User == c.ID_User && det.ID_team != null && det.ID_team != 0)

                                         select new TeamsAgents
                                         {
                                             id_team = wt.ID_team,
                                             Name = wt.Name
                                         }).ToList()

                            }).OrderBy(t => t.LastName).ToList();
                        }

                    }
                }
                else
                {
                    ViewBag.rol = "Admin";
                    if (activeuser.Roles.Contains("Admin"))
                    {
                        // se utiliza id = 4 para registros no asignados
                        lstAgentes = db.Sys_Users.Where(t => t.ID_User != 4 && t.Roles.Contains("Agent") && t.ID_Company == activeuser.ID_Company).Select(c=> new AgentsView {
                            ID_User=c.ID_User,
                            Team_Leader = c.Team_Leader,
                            Active =c.Active, Address=c.Address, Brokerage_name=c.Brokerage_name, Email=c.Email, ID_Company=c.ID_Company, Image=c.Image, LastName=c.LastName, Main_telephone=c.Main_telephone,
                            Secundary_telephone=c.Secundary_telephone, Last_login=c.Last_login, Member_since=c.Member_since, My_License=c.My_License, Name=c.Name, Position=c.Position, State=c.State,
                            //Leads = (from det in db.Tb_Customers_Users join ag in db.Tb_Customers on det.Id_Customer equals ag.ID_Customer where(det.Id_User==c.ID_User) select new LeadsAgents { ID_lead=det.Id_Customer, Name=ag.Name + " " +ag.LastName }).ToList(),
                            Teams=(from det in db.Tb_Customers_Users join wt in db.Tb_WorkTeams on det.ID_team equals wt.ID_team
                                   where (det.Id_User == c.ID_User && det.ID_team != null && det.ID_team != 0)

                                   select new TeamsAgents
                                   {
                                       id_team = wt.ID_team,
                                       Name = wt.Name
                                   }).ToList()
                            
                        }).OrderBy(t => t.LastName).ToList();
                    }
                    else
                    {
                        // se utiliza id = 4 para registros no asignados
                        ViewBag.rol = "SA";
                        // se utiliza id = 4 para registros no asignados
                        lstAgentes = db.Sys_Users.Where(t => t.ID_User != 4 && t.Roles.Contains("Agent") && t.ID_Company == activeuser.ID_Company).Select(c => new AgentsView
                        {
                            ID_User = c.ID_User,
                            Team_Leader = c.Team_Leader,
                            Active = c.Active,
                            Address = c.Address,
                            Brokerage_name = c.Brokerage_name,
                            Email = c.Email,
                            ID_Company = c.ID_Company,
                            Image = c.Image,
                            LastName = c.LastName,
                            Main_telephone = c.Main_telephone,
                            Secundary_telephone = c.Secundary_telephone,
                            Last_login = c.Last_login,
                            Member_since = c.Member_since,
                            My_License = c.My_License,
                            Name = c.Name,
                            Position = c.Position,
                            State = c.State,
                            //Leads = (from det in db.Tb_Customers_Users join ag in db.Tb_Customers on det.Id_Customer equals ag.ID_Customer where(det.Id_User==c.ID_User) select new LeadsAgents { ID_lead=det.Id_Customer, Name=ag.Name + " " +ag.LastName }).ToList(),
                            Teams = (from det in db.Tb_Customers_Users
                                     join wt in db.Tb_WorkTeams on det.ID_team equals wt.ID_team
                                     where (det.Id_User == c.ID_User && det.ID_team != null && det.ID_team != 0)

                                     select new TeamsAgents
                                     {
                                         id_team = wt.ID_team,
                                         Name = wt.Name
                                     }).ToList()

                        }).OrderBy(t => t.LastName).ToList();

                    }


                }

  
                return View(lstAgentes);
            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

        }

        public ActionResult Agents_properties(string fstartd, string fendd, int broker = 0, string token = "")
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
                ViewBag.token = token;
                ViewBag.selbroker = broker;
                //FILTROS VARIABLES
                DateTime filtrostartdate;
                DateTime filtroenddate;
                ////filtros de fecha 
                var firstDayOfMonth = new DateTime(DateTime.Today.Year, 1, 1);
                var lastDayOfMonth = new DateTime(DateTime.Today.Year, 12, 31);

                if (fstartd == null || fstartd == "") { filtrostartdate = firstDayOfMonth; } else { filtrostartdate = Convert.ToDateTime(fstartd); }
                if (fendd == null || fendd == "") { filtroenddate = lastDayOfMonth; } else { filtroenddate = Convert.ToDateTime(fendd).AddHours(23).AddMinutes(59); }

                var startDate = filtrostartdate;
                var endDate = filtroenddate;

                ViewBag.filtrofechastart = filtrostartdate.ToShortDateString();
                ViewBag.filtrofechaend = filtroenddate.ToShortDateString();

                List<AgentsProperties_View> lstAgentes = new List<AgentsProperties_View>();


                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.teamleader = activeuser.Team_Leader;

                    // se utiliza id = 4 para registros no asignados
                    lstAgentes = db.Sys_Users.Where(t => t.ID_User != 4 && t.Roles.Contains("Agent") && t.ID_Company == activeuser.ID_Company).Select(c => new AgentsProperties_View
                    {
                        ID_User = c.ID_User,
                        Active = c.Active,
                        Brokerage_name = c.Brokerage_name,
                        Email = c.Email,
                        ID_Company = c.ID_Company,
                        Image = c.Image,
                        LastName = c.LastName,
                        Main_telephone = c.Main_telephone,
                        Member_since = c.Member_since,
                        My_License = c.My_License,
                        Name = c.Name,
                        Leads = (from det in db.Tb_Customers_Users join ag in db.Tb_Customers on det.Id_Customer equals ag.ID_Customer where (det.Id_User == c.ID_User) select new LeadsAgents { ID_lead = det.Id_Customer, Name = ag.Name + " " + ag.LastName }).ToList(),
                        properties = (from det in db.Tb_Process
                                      where (det.ID_User == c.ID_User && det.Creation_date >= startDate && det.Creation_date <= endDate)

                                      select new PropertiesAgents
                                      {
                                          id_process = det.ID_Process,
                                          address = det.Address,
                                          stage = det.Stage
                                      }).ToList()

                    }).OrderBy(t => t.LastName).ToList();
                }
                else
                {
                    ViewBag.rol = "Admin";
                    if (activeuser.Roles.Contains("Admin"))
                    {
                        // se utiliza id = 4 para registros no asignados
                        lstAgentes = db.Sys_Users.Where(t => t.ID_User != 4 && t.Roles.Contains("Agent") && t.ID_Company == activeuser.ID_Company).Select(c => new AgentsProperties_View
                        {
                     ID_User = c.ID_User,
                     Active = c.Active,
                     Brokerage_name = c.Brokerage_name,
                     Email = c.Email,
                     ID_Company = c.ID_Company,
                     Image = c.Image,
                     LastName = c.LastName,
                     Main_telephone = c.Main_telephone,
                     Member_since = c.Member_since,
                     My_License = c.My_License,
                     Name = c.Name,
                     Leads = (from det in db.Tb_Customers_Users join ag in db.Tb_Customers on det.Id_Customer equals ag.ID_Customer where (det.Id_User == c.ID_User) select new LeadsAgents { ID_lead = det.Id_Customer, Name = ag.Name + " " + ag.LastName }).ToList(),
                     properties = (from det in db.Tb_Process
                                   where (det.ID_User == c.ID_User && det.Creation_date >= startDate && det.Creation_date <= endDate)

                                   select new PropertiesAgents
                                   {
                                       id_process = det.ID_Process,
                                       address = det.Address,
                                       stage = det.Stage
                                   }).ToList()

                 }).OrderBy(t => t.LastName).ToList();

                    }
                    else
                    {
                        // se utiliza id = 4 para registros no asignados
                        ViewBag.rol = "SA";
                        // se utiliza id = 4 para registros no asignados
                        lstAgentes = db.Sys_Users.Where(t => t.ID_User != 4 && t.Roles.Contains("Agent") && t.ID_Company == activeuser.ID_Company).Select(c => new AgentsProperties_View
                        {
                            ID_User = c.ID_User,
                            Active = c.Active,
                            Brokerage_name = c.Brokerage_name,
                            Email = c.Email,
                            ID_Company = c.ID_Company,
                            Image = c.Image,
                            LastName = c.LastName,
                            Main_telephone = c.Main_telephone,
                            Member_since = c.Member_since,
                            My_License = c.My_License,
                            Name = c.Name,
                            Leads = (from det in db.Tb_Customers_Users join ag in db.Tb_Customers on det.Id_Customer equals ag.ID_Customer where (det.Id_User == c.ID_User) select new LeadsAgents { ID_lead = det.Id_Customer, Name = ag.Name + " " + ag.LastName }).ToList(),
                            properties = (from det in db.Tb_Process
                                          where (det.ID_User == c.ID_User && det.Creation_date >= startDate && det.Creation_date <= endDate)

                                          select new PropertiesAgents
                                          {
                                              id_process = det.ID_Process,
                                              address = det.Address,
                                              stage = det.Stage
                                          }).ToList()

                        }).OrderBy(t => t.LastName).ToList();

                    }


                }


                return View(lstAgentes);
            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

        }

        public ActionResult CustomerDashboard(int? id, int broker=0, string token="")
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
                ViewBag.token = token;


                Tb_Customers tb_Customers = db.Tb_Customers.Find(id);
                ViewBag.lead = tb_Customers;
                List<NotesView> notes = (from a in db.Tb_Notes.Where(a => a.ID_Customer == id) orderby a.Date select new NotesView {
                    ID_Customer=a.ID_Customer, Created_By=a.Created_By, Date=a.Date, Text=a.Text, ID_note=a.ID_note, Url_image=(from us in db.Sys_Users where(us.ID_User==a.ID_User) select us.Image).FirstOrDefault()
                }).ToList();
                ViewBag.notes = notes;
                List<Tb_Process> properties = (from a in db.Tb_Process.Where(a => a.ID_Customer == id) select a).ToList();
                ViewBag.listings = properties;
                List<Tb_LeadDocs> otherDocs = (from doc in db.Tb_LeadDocs where (doc.Id_Customer == id) select doc).ToList();
                ViewBag.otherDocs = otherDocs;
                List<Tb_Source> lstSource = (from o in db.Tb_Source where (o.Id_Company==activeuser.ID_Company) select o).ToList();
                ViewBag.lstsource = lstSource;
                List<Tb_Status> lststatus = (from t in db.Tb_Status where (t.Id_Company == activeuser.ID_Company) select t).ToList();
                ViewBag.lststatus = lststatus;
                List<Tb_Docpackages> lstdocpack = (from a in db.Tb_Docpackages.Where(a => a.ID_Customer == tb_Customers.ID_Customer) select a).ToList();
                ViewBag.docpackages = lstdocpack;
                Tb_WorkTeams Team = new Tb_WorkTeams();
                List<TeamsModel_Users> Agents = new List<TeamsModel_Users>();
                List<Sys_Users> agents2 = new List<Sys_Users>();


                agents2 = db.Sys_Users.Where(c => c.Roles.Contains("Agent") && c.Active && c.ID_User != 4 && c.ID_Company == activeuser.ID_Company).ToList();
                ViewBag.agentsadd = agents2;
                List<TasksView> lst_tasks = new List<TasksView>();
 
                    lst_tasks = (from a in db.Tb_Tasks
                                 where (a.ID_Customer==id)
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
                                     Customer = a.Customer,
                                     Name = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.Name).FirstOrDefault(),
                                     Lastname = (from c in db.Sys_Users where (c.ID_User == a.ID_User) select c.LastName).FirstOrDefault()
                                 }).ToList();

                ViewBag.tasks = lst_tasks;
                    List<GainsReport> lstgainsreport = new List<GainsReport>();
                if (tb_Customers.ID_team != null)
                {
                    if (tb_Customers.ID_team != 0)
                    {
                        Team = (from t in db.Tb_WorkTeams where (t.ID_team == tb_Customers.ID_team) select t).FirstOrDefault();
                        Agents = (from cu in db.Tb_Customers_Users
                                                         join u in db.Sys_Users on cu.Id_User equals u.ID_User
                                                         where ((cu.ID_team == tb_Customers.ID_team || cu.Id_Customer == tb_Customers.ID_Customer) && cu.Id_User != 4)
                                                         select new TeamsModel_Users
                                                         {
                                                             Id_User = cu.Id_User,
                                                             Name = u.Name,
                                                             Lastname= u.LastName,
                                                             Id_Team = cu.ID_team,
                                                             Email = u.Email,
                                                             Url_image = u.Image
                                                         }).Distinct().ToList();
                    }
                    else
                    {
                        Agents = (from cu in db.Tb_Customers_Users
                                  join u in db.Sys_Users on cu.Id_User equals u.ID_User
                                  where ((cu.Id_Customer == tb_Customers.ID_Customer) && cu.Id_User != 4)
                                  select new TeamsModel_Users
                                  {
                                      Id_User = cu.Id_User,
                                      Name = u.Name,
                                      Lastname = u.LastName,
                                      Id_Team = cu.ID_team,
                                      Email = u.Email,
                                      Url_image = u.Image
                                  }).Distinct().ToList();
                    }
                }
                else
                {
                    Agents = (from cu in db.Tb_Customers_Users
                              join u in db.Sys_Users on cu.Id_User equals u.ID_User
                              where ((cu.Id_Customer == tb_Customers.ID_Customer) && cu.Id_User != 4)
                              select new TeamsModel_Users
                              {
                                  Id_User = cu.Id_User,
                                  Name = u.Name,
                                  Id_Team = cu.ID_team,
                                  Lastname = u.LastName,
                                  Email = u.Email,
                                  Url_image = u.Image
                              }).Distinct().ToList();

                }
                ViewBag.Team = Team;
                ViewBag.Agents = Agents;

                List<Tb_Process> lstlistings = new List<Tb_Process>();

                //ROLES
                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";
                    ViewBag.selbroker = 0;

                    var propertiesprojectedgains = (from f in db.Tb_Process where (f.Stage == "ON CONTRACT" && f.ID_Customer==tb_Customers.ID_Customer) select f).ToList();
                    var propertiesgains = (from f in db.Tb_Process where (f.Stage == "CLOSED" && f.ID_Customer == tb_Customers.ID_Customer) select f).ToList();
                    var totalproperties = (from f in db.Tb_Process where (f.ID_Customer == tb_Customers.ID_Customer) select f).Count();

                    decimal totalprojectedgains = 0;
                    decimal totalgains = 0;
                    if (propertiesprojectedgains.Count > 0) { totalprojectedgains = propertiesprojectedgains.Select(c => c.Commission_amount).Sum(); }
                    if (propertiesgains.Count > 0) { totalgains = propertiesgains.Select(c => c.Commission_amount).Sum(); }


                    lstlistings = (from f in db.Tb_Process where (f.ID_Customer == id ) select f).ToList();


                    if (lstlistings.Count > 0)
                    {

                        var staritem = lstlistings.OrderBy(c => c.Creation_date).FirstOrDefault();
                        var enditem = lstlistings.OrderByDescending(c => c.Creation_date).FirstOrDefault();

                        var startdate = new DateTime(staritem.Creation_date.Year, staritem.Creation_date.Month, 1, 0, 0, 0);
                        var enddate = new DateTime(enditem.Creation_date.Year, enditem.Creation_date.Month, enditem.Creation_date.AddMonths(1).AddDays(-1).Day, 0, 0, 0);

                        var months = MonthsBetween(startdate, enddate);

                        foreach (var item in months)
                        {
                            GainsReport data = new GainsReport();
                            data.monthyear = item.Month + " - " + item.Year;

                            var StartDay = new DateTime(item.Year, item.montint, 1);
                            var EndDay = StartDay.AddMonths(1).AddDays(-1);

                            data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                            data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                            lstgainsreport.Add(data);

                        }
                    }

                    ViewBag.totalcustomers = totalproperties;
                    ViewBag.totalgainsprojected = totalprojectedgains.ToString("N2");
                    ViewBag.totalgains = totalgains.ToString("N2");
                }
                else if (activeuser.Roles.Contains("SA"))
                {
                    ViewBag.rol = "SA";
            
                    ViewBag.selbroker = 0;


                    decimal comission = 0;
                    decimal gains = 0;
                    int totalcustomer = 0;


                    var listComission = (from f in db.Tb_Process.Where(f => f.Stage == "ON CONTRACT" && f.ID_Customer == tb_Customers.ID_Customer) select f).ToList();
                    if (listComission.Count > 0) { comission = listComission.Select(c => c.Commission_amount).Sum(); }

                    var listgains = (from f in db.Tb_Process where (f.Stage == "CLOSED" && f.ID_Customer == tb_Customers.ID_Customer) select f).ToList();
                    if (listgains.Count > 0) { gains = listgains.Select(c => c.Commission_amount).Sum(); }
                    totalcustomer = (from f in db.Tb_Process where (f.ID_Customer == tb_Customers.ID_Customer) select f).Count();

                    ViewBag.totalgainsprojected = comission.ToString("N2");
                    ViewBag.totalgains = gains.ToString("N2");
                    ViewBag.totalcustomers = totalcustomer;

                    lstlistings = (from f in db.Tb_Process where (f.ID_Customer == id) select f).ToList();


                    if (lstlistings.Count > 0)
                    {

                        var staritem = lstlistings.OrderBy(c => c.Creation_date).FirstOrDefault();
                        var enditem = lstlistings.OrderByDescending(c => c.Creation_date).FirstOrDefault();

                        var startdate = new DateTime(staritem.Creation_date.Year, staritem.Creation_date.Month, 1, 0, 0, 0);
                        var enddate = new DateTime(enditem.Creation_date.Year, enditem.Creation_date.Month, enditem.Creation_date.AddMonths(1).AddDays(-1).Day, 0, 0, 0);

                        var months = MonthsBetween(startdate, enddate);

                        foreach (var item in months)
                        {
                            GainsReport data = new GainsReport();
                            data.monthyear = item.Month + " - " + item.Year;

                            var StartDay = new DateTime(item.Year, item.montint, 1);
                            var EndDay = StartDay.AddMonths(1).AddDays(-1);

                            data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                            data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                            lstgainsreport.Add(data);

                        }
                    }
                }
                else if (activeuser.Roles.Contains("Admin"))
                {
                    ViewBag.rol = "Admin";
                    ViewBag.selbroker = 0;

          
                    decimal comission = 0;
                    decimal gains = 0;
                    int totalcustomer = 0;

                 
                        var listComission = (from f in db.Tb_Process.Where(f => f.Stage == "ON CONTRACT" && f.ID_Customer == tb_Customers.ID_Customer) select f).ToList();
                        if (listComission.Count > 0) { comission = listComission.Select(c => c.Commission_amount).Sum(); }

                        var listgains = (from f in db.Tb_Process where (f.Stage == "CLOSED" && f.ID_Customer == tb_Customers.ID_Customer) select f).ToList();
                        if (listgains.Count > 0) { gains = listgains.Select(c => c.Commission_amount).Sum(); }
                        totalcustomer = (from f in db.Tb_Process where (f.ID_Customer == tb_Customers.ID_Customer) select f).Count();
                    
                    ViewBag.totalgainsprojected = comission.ToString("N2");
                    ViewBag.totalgains = gains.ToString("N2");
                    ViewBag.totalcustomers = totalcustomer;

                    lstlistings = (from f in db.Tb_Process where (f.ID_Customer==id) select f).ToList();


                    if (lstlistings.Count > 0)
                    {

                        var staritem = lstlistings.OrderBy(c => c.Creation_date).FirstOrDefault();
                        var enditem = lstlistings.OrderByDescending(c => c.Creation_date).FirstOrDefault();

                        var startdate = new DateTime(staritem.Creation_date.Year, staritem.Creation_date.Month, 1,0,0,0);
                        var enddate = new DateTime(enditem.Creation_date.Year, enditem.Creation_date.Month, enditem.Creation_date.AddMonths(1).AddDays(-1).Day, 0,0,0);

                        var months = MonthsBetween(startdate, enddate);

                        foreach (var item in months)
                        {
                            GainsReport data = new GainsReport();
                            data.monthyear = item.Month + " - " + item.Year;

                            var StartDay = new DateTime(item.Year, item.montint, 1);
                            var EndDay = StartDay.AddMonths(1).AddDays(-1);

                            data.projected = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "ON CONTRACT") select f.Commission_amount).Sum();
                            data.gains = (from f in lstlistings where ((f.Creation_date >= StartDay && f.Creation_date <= EndDay) && f.Stage == "CLOSED") select f.Commission_amount).Sum();

                            lstgainsreport.Add(data);

                        }
                    }


                }


                ViewBag.gainsreport_dates = lstgainsreport.Select(c => c.monthyear).ToArray();
                ViewBag.gainsreport_projected = lstgainsreport.Select(c => c.projected).ToArray();
                ViewBag.gainsreport_gains = lstgainsreport.Select(c => c.gains).ToArray();

                return View("CustomerDashboard", tb_Customers);

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

        public ActionResult DownloadDoclead(int id)
        {

            var fileDB = (from a in db.Tb_LeadDocs where (a.Id_Document == id) select a).FirstOrDefault();

            var path = fileDB.Url;
            var file = Server.MapPath(path);

            //file has multiple support for a phisical file byte[] or a route string to points where the file is located.
            return File(file, System.Net.Mime.MediaTypeNames.Application.Octet, fileDB.Title + fileDB.Extension);

        }

        public ActionResult Showpdflead(int id)
        {

            var fileDB = (from a in db.Tb_LeadDocs where (a.Id_Document == id) select a).FirstOrDefault();

            var path = fileDB.Url;
            var file = Server.MapPath(path);


            return File(file, "application/pdf");

        }

    }
}
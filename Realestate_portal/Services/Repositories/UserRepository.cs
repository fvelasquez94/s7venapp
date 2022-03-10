using Realestate_portal.Models;
using Realestate_portal.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Realestate_portal.Models.ViewModels.SysCompany;

namespace Realestate_portal.Services.Repositories
{
    public class UserRepository : Controller, IUser 
    {
        private Realstate_agentsEntities db;
        private IGeneral general;
        public UserRepository(Realstate_agentsEntities _db, IGeneral _general)
        {
            db = _db;
            general = _general;
        }

        //
        // Summary:
        //     gets the users defined in the Session as Sys_USers object
        public Sys_Users GetUser()
        {
        
            return general.GetUser();
        }


        public string GetUserRole()
        {
            var activeuser = GetUser();

            if (activeuser.Roles.Contains("SA"))
            {
               return "SA";
            }
            else if (activeuser.Roles.Contains("Admin"))
            {
                return  "Admin";
            }
            else
            {
                return  "Agent";
            }
        }

        public int GetUserId()
        {
         return GetUser().ID_User;
        }


        public Company GetUserCompany()
        {
            var userCompanyId = GetUser().ID_Company;
            return (Company)db.Sys_Company.Where(c => c.ID_Company == userCompanyId).Select(c => new Company() {Id=c.ID_Company, Logo=c.Logo, Name=c.Name }).FirstOrDefault();
        }

        public List<Sys_Notifications> SetUserNotifications()
        {
            var activeuser = GetUser();
            DateTime now = DateTime.Today;
            List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
            return lstAlerts;
        }

        public (int properties,string projectedGains, string gains) SetProjectedGains()
        {
            var activeuser = GetUser();
            var properties = 0;
            var projectedGains = "";
            var gain="";

            if (GetUserRole().Contains("Agent"))
            {                
                var propertiesprojectedgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "ON CONTRACT") select f).ToList();
                var propertiesgains = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User && f.Stage == "CLOSED") select f).ToList();
                var totalproperties = (from f in db.Tb_Process where (f.ID_User == activeuser.ID_User) select f).Count();

                decimal totalprojectedgains = 0;
                decimal totalgains = 0;
                if (propertiesprojectedgains.Count > 0) { totalprojectedgains = propertiesprojectedgains.Select(c => c.Commission_amount).Sum(); }
                if (propertiesgains.Count > 0) { totalgains = propertiesgains.Select(c => c.Commission_amount).Sum(); }
                
                properties = totalproperties;
                projectedGains = totalprojectedgains.ToString("N2");
                gain = totalgains.ToString("N2");
            }
            else
            {
                if (GetUserRole().Contains("Admin"))
                {                   
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

                     properties = totalcustomer;
                     projectedGains = comission.ToString("N2");
                     gain = gains.ToString("N2");                 
                }
                else
                {
                    properties = 0;
                    projectedGains = "0.00";
                    gain = "0.00";
                }
            }           
            return (properties, projectedGains, gain);
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
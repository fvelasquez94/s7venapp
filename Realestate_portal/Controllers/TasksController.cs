using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Postal;
using Realestate_portal.Controllers.SendGridAPI;
using Realestate_portal.Controllers.TwilioAPI;
using Realestate_portal.Models;

namespace Realestate_portal.Controllers
{
    public class TasksController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private SendSMS SMSclass = new SendSMS();

        // GET: Tasks
        public ActionResult Index()
        {
            return View(db.Tb_Tasks.ToList());
        }

        // GET: Tasks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Tasks tb_Tasks = db.Tb_Tasks.Find(id);
            if (tb_Tasks == null)
            {
                return HttpNotFound();
            }
            return View(tb_Tasks);
        }

        // GET: Tasks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tasks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCD([Bind(Include = "ID_task,Title,Description,Finished,Createdat,Lastupdate,ID_Customer,ID_User,Username,ID_Company")] Tb_Tasks tb_Tasks)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //tb_Tasks.Createdat = DateTime.UtcNow;
                tb_Tasks.Lastupdate = DateTime.UtcNow;
                tb_Tasks.ID_Company = activeuser.ID_Company;
                tb_Tasks.Username = "";
                tb_Tasks.Customer = "";
                if (tb_Tasks.ID_Customer != 0)
                {
                    var cust = db.Tb_Customers.Where(c => c.ID_Customer == tb_Tasks.ID_Customer).FirstOrDefault();
                    if (cust != null)
                    {
                        tb_Tasks.Customer = cust.Name + " " + cust.LastName;
                    }
                }

                var agent = (from a in db.Sys_Users where (a.ID_User == tb_Tasks.ID_User) select a).FirstOrDefault();

                if (ModelState.IsValid)
                {
                    db.Tb_Tasks.Add(tb_Tasks);
                    db.SaveChanges();
                    //Colocamos notificacion
                    Sys_Notifications newnotification = new Sys_Notifications();
                    newnotification.Active = true;
                    newnotification.Date = DateTime.UtcNow;
                    newnotification.Title = "New task assigned.";
                    newnotification.Description = tb_Tasks.Title;
                    newnotification.ID_user = tb_Tasks.ID_User;
                    db.Sys_Notifications.Add(newnotification);
                    db.SaveChanges();

                    //Send the email
                    //dynamic semail = new Email("NewNotification_task");
                    //semail.To = agent.Email.ToString();
                    //semail.From = "support@s7ven.co";
                    //semail.user = agent.Name + " " + agent.LastName;
                    //semail.title = tb_Tasks.Title;
                    //semail.Description = tb_Tasks.Description;
                    //semail.Assignedby = activeuser.Name + " " + activeuser.LastName;

                    //semail.Send();
                    SendEmail emailnot = new SendEmail();
                    //Email formato cliente
                    var resut = emailnot.SendEmail_newTask(agent.Email.ToString(), tb_Tasks.Title, tb_Tasks.Description);

                    //Send SMS notification
                    if (agent.Main_telephone != "")
                    {
                        SendSMS sendsms = new SendSMS();
                        var result = sendsms.sendSMSTrilio("Hello, you have a new task assigned in our S7VEN platform. " + tb_Tasks.Title, agent.Main_telephone);
                    }
                    return RedirectToAction("CustomerDashboard", "CRM", new { id=tb_Tasks.ID_Customer, token = "success" });
                }
                else
                {

                    return RedirectToAction("CustomerDashboard", "CRM", new { id = tb_Tasks.ID_Customer, token = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("CustomerDashboard", "CRM", new { id = tb_Tasks.ID_Customer, token = "error" });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_task,Title,Description,Finished,Createdat,Lastupdate,ID_Customer,ID_User,Username,ID_Company")] Tb_Tasks tb_Tasks)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //tb_Tasks.Createdat = DateTime.UtcNow;
                tb_Tasks.Lastupdate = DateTime.UtcNow;
                tb_Tasks.ID_Company = activeuser.ID_Company;
                tb_Tasks.Username = "";
                tb_Tasks.Customer = "";
                if (tb_Tasks.ID_Customer != 0) {
                    var cust = db.Tb_Customers.Where(c => c.ID_Customer == tb_Tasks.ID_Customer).FirstOrDefault();
                    if (cust != null) {
                        tb_Tasks.Customer = cust.Name + " " + cust.LastName;
                    }
                }

                var agent = (from a in db.Sys_Users where (a.ID_User == tb_Tasks.ID_User) select a).FirstOrDefault();

                if (ModelState.IsValid)
                {
                    db.Tb_Tasks.Add(tb_Tasks);
                    db.SaveChanges();
                    //Colocamos notificacion
                    Sys_Notifications newnotification = new Sys_Notifications();
                    newnotification.Active = true;
                    newnotification.Date = DateTime.UtcNow;
                    newnotification.Title = "New task assigned.";
                    newnotification.Description = tb_Tasks.Title;
                    newnotification.ID_user = tb_Tasks.ID_User;
                    db.Sys_Notifications.Add(newnotification);
                    db.SaveChanges();

                    //Send the email
                    SendEmail emailnot = new SendEmail();
                    //Email formato cliente
                    var resut = emailnot.SendEmail_newTask(agent.Email.ToString(), tb_Tasks.Title, tb_Tasks.Description);


                    //Send SMS notification
                    if (agent.Main_telephone != "")
                    {
                        SendSMS sendsms = new SendSMS();
                        var result = sendsms.sendSMSTrilio("Hello, you have a new task assigned in our S7VEN platform. " + tb_Tasks.Title, agent.Main_telephone);
                    }
               
                    return RedirectToAction("Tasks", "CRM", new { token = "success" });
                }
                else
                {

                    return RedirectToAction("Tasks", "CRM", new { token = "error" });
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Tasks", "CRM", new { token = "error" });
            }

        }

        // GET: Tasks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Tasks tb_Tasks = db.Tb_Tasks.Find(id);
            if (tb_Tasks == null)
            {
                return HttpNotFound();
            }
            return View(tb_Tasks);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_task,Title,Description,Finished,Createdat,ID_Customer,Lastupdate,ID_User,Username,ID_Company")] Tb_Tasks tb_Tasks)
        {
            tb_Tasks.Customer = "";
            if (tb_Tasks.ID_Customer != 0)
            {
                var cust = db.Tb_Customers.Where(c => c.ID_Customer == tb_Tasks.ID_Customer).FirstOrDefault();
                if (cust != null)
                {
                    tb_Tasks.Customer = cust.Name + " " + cust.LastName;
                }
            }
            if (ModelState.IsValid)
            {
                db.Entry(tb_Tasks).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tb_Tasks);
        }

        // GET: Tasks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Tasks tb_Tasks = db.Tb_Tasks.Find(id);
            if (tb_Tasks == null)
            {
                return HttpNotFound();
            }
            return View(tb_Tasks);
        }

        // POST: Tasks/Delete/5

        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Tb_Tasks tb_Tasks = db.Tb_Tasks.Find(id);
                db.Tb_Tasks.Remove(tb_Tasks);
                db.SaveChanges();


                var result = "Success";
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }


        public ActionResult UpdateTask(int id, bool value)
        {
            try
            {
                Tb_Tasks tb_Tasks = db.Tb_Tasks.Find(id);
                tb_Tasks.Finished = value;
                db.Entry(tb_Tasks).State=EntityState.Modified;
                db.SaveChanges();


                var result = "Success";
                return Json(result, JsonRequestBehavior.AllowGet);

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

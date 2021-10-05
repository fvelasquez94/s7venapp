using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Postal;
using Realestate_portal.Models;

namespace Realestate_portal.Controllers
{
    public class TasksController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();

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
        public ActionResult Create([Bind(Include = "ID_task,Title,Description,Finished,Createdat,Lastupdate,ID_User,Username,ID_Company")] Tb_Tasks tb_Tasks)
        {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //tb_Tasks.Createdat = DateTime.UtcNow;
                tb_Tasks.Lastupdate = DateTime.UtcNow;
                tb_Tasks.ID_Company = activeuser.ID_Company;
                tb_Tasks.Username = "";


                var agent = (from a in db.Sys_Users where (a.ID_User == tb_Tasks.ID_User) select a).FirstOrDefault();

                if (ModelState.IsValid)
                {
                    db.Tb_Tasks.Add(tb_Tasks);
                    db.SaveChanges();

                    //Send the email
                    dynamic semail = new Email("NewNotification_task");
                    semail.To = agent.Email.ToString();
                    semail.From = "support@s7ven.co";
                    semail.user = agent.Name + " " + agent.LastName;
                    semail.title = tb_Tasks.Title;
                    semail.Description = tb_Tasks.Description;
                    semail.Assignedby = activeuser.Name + " " + activeuser.LastName;

                    semail.Send();

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
        public ActionResult Edit([Bind(Include = "ID_task,Title,Description,Finished,Createdat,Lastupdate,ID_User,Username,ID_Company")] Tb_Tasks tb_Tasks)
        {
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

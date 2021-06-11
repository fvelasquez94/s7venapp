using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Realestate_portal.Models;

namespace Realestate_portal.Controllers
{
    public class Tb_StatusController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();

        // GET: Tb_Status
        public ActionResult Index()
        {
            return View(db.Tb_Status.ToList());
        }

        // GET: Tb_Status/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Status tb_Status = db.Tb_Status.Find(id);
            if (tb_Status == null)
            {
                return HttpNotFound();
            }
            return View(tb_Status);
        }

        // GET: Tb_Status/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tb_Status/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id_Status,Stage_name")] Tb_Status tb_Status)
        {
            if (ModelState.IsValid)
            {
                db.Tb_Status.Add(tb_Status);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tb_Status);
        }
        [HttpPost]
        public ActionResult CreateStatus(string mstatus)
        {
            try
            {
                Sys_Users activeUser = Session["activeUser"] as Sys_Users;
                Tb_Status newStatus = new Tb_Status();
                newStatus.Stage_name = mstatus;
                newStatus.Id_Company = activeUser.ID_Company;
                db.Tb_Status.Add(newStatus);
                db.SaveChanges();
                var result = "SUCCESS";
                return Json(result);
            }
            catch (Exception ex)
            {

                var result = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        // GET: Tb_Status/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Status tb_Status = db.Tb_Status.Find(id);
            if (tb_Status == null)
            {
                return HttpNotFound();
            }
            return View(tb_Status);
        }

        // POST: Tb_Status/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_Status,Stage_name")] Tb_Status tb_Status)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_Status).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tb_Status);
        }

        // GET: Tb_Status/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Status tb_Status = db.Tb_Status.Find(id);
            if (tb_Status == null)
            {
                return HttpNotFound();
            }
            return View(tb_Status);
        }

        // POST: Tb_Status/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tb_Status tb_Status = db.Tb_Status.Find(id);
            db.Tb_Status.Remove(tb_Status);
            db.SaveChanges();
            return RedirectToAction("Index");
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

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
    public class Tb_SourceController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();

        // GET: Tb_Source
        public ActionResult Index()
        {
            return View(db.Tb_Source.ToList());
        }

        // GET: Tb_Source/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Source tb_Source = db.Tb_Source.Find(id);
            if (tb_Source == null)
            {
                return HttpNotFound();
            }
            return View(tb_Source);
        }

        // GET: Tb_Source/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tb_Source/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id_Source,Source_name,Id_Company")] Tb_Source tb_Source)
        {
            if (ModelState.IsValid)
            {
                db.Tb_Source.Add(tb_Source);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tb_Source);
        }
        [HttpPost]
        public ActionResult CreateSource(string source) {
            try
            {
                Sys_Users activeUser = Session["activeUser"] as Sys_Users;
                Tb_Source newSource = new Tb_Source();
                newSource.Source_name = source;
                newSource.Id_Company = activeUser.ID_Company;
                db.Tb_Source.Add(newSource);
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

        // GET: Tb_Source/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Source tb_Source = db.Tb_Source.Find(id);
            if (tb_Source == null)
            {
                return HttpNotFound();
            }
            return View(tb_Source);
        }

        // POST: Tb_Source/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_Source,Source_name,Id_Company")] Tb_Source tb_Source)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_Source).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tb_Source);
        }

        // GET: Tb_Source/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Source tb_Source = db.Tb_Source.Find(id);
            if (tb_Source == null)
            {
                return HttpNotFound();
            }
            return View(tb_Source);
        }

        // POST: Tb_Source/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tb_Source tb_Source = db.Tb_Source.Find(id);
            db.Tb_Source.Remove(tb_Source);
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

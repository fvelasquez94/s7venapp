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
    public class TeamsController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();

        // GET: Teams
        public ActionResult Index()
        {
            return View(db.Tb_WorkTeams.ToList());
        }

        // GET: Teams/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_WorkTeams tb_WorkTeams = db.Tb_WorkTeams.Find(id);
            if (tb_WorkTeams == null)
            {
                return HttpNotFound();
            }
            return View(tb_WorkTeams);
        }

        // GET: Teams/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Teams/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_team,Name,Description,ID_Company,Active,Creation_date,Last_update")] Tb_WorkTeams tb_WorkTeams)
        {
            if (ModelState.IsValid)
            {
                db.Tb_WorkTeams.Add(tb_WorkTeams);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tb_WorkTeams);
        }

        // GET: Teams/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_WorkTeams tb_WorkTeams = db.Tb_WorkTeams.Find(id);
            if (tb_WorkTeams == null)
            {
                return HttpNotFound();
            }
            return View(tb_WorkTeams);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_team,Name,Description,ID_Company,Active,Creation_date,Last_update")] Tb_WorkTeams tb_WorkTeams)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_WorkTeams).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tb_WorkTeams);
        }

        // GET: Teams/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_WorkTeams tb_WorkTeams = db.Tb_WorkTeams.Find(id);
            if (tb_WorkTeams == null)
            {
                return HttpNotFound();
            }
            return View(tb_WorkTeams);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tb_WorkTeams tb_WorkTeams = db.Tb_WorkTeams.Find(id);
            db.Tb_WorkTeams.Remove(tb_WorkTeams);
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Realestate_portal.Controllers.BlobStorage;
using Realestate_portal.Models;

namespace Realestate_portal.Controllers
{
    public class LeadDocsController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        UploadService imageService = new UploadService();

        // GET: LeadDocs
        public ActionResult Index()
        {
            var tb_LeadDocs = db.Tb_LeadDocs.Include(t => t.Tb_Customers);
            return View(tb_LeadDocs.ToList());
        }

        // GET: LeadDocs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_LeadDocs tb_LeadDocs = db.Tb_LeadDocs.Find(id);
            if (tb_LeadDocs == null)
            {
                return HttpNotFound();
            }
            return View(tb_LeadDocs);
        }

        [HttpPost]
        public async Task<ActionResult> UploadNewDocument(HttpPostedFileBase photo)
        {
            //subimos imagen
            try
            {
                var imageUrl = await imageService.UploadImageAsync(photo);

                var result = imageUrl;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception errr)
            {
                var result = "";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public async Task<ActionResult> DeleteDocumentUploaded(string filename)
        {
            //eliminamos imagen

            var imageUrl = await imageService.DeleteImageAsync(filename); //

            var result = imageUrl;
            return Json(result, JsonRequestBehavior.AllowGet);

        }


        // GET: LeadDocs/Create
        public ActionResult Create()
        {
            ViewBag.Id_Customer = new SelectList(db.Tb_Customers, "ID_Customer", "Name");
            return View();
        }

        // POST: LeadDocs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id_Document,Title,Url,Size,Id_Customer,Extension,Upload_Date")] Tb_LeadDocs tb_LeadDocs, int Id_CustomerDoc)
        {

            try { 
            tb_LeadDocs.Size = "0";
            tb_LeadDocs.Extension = "";
            tb_LeadDocs.Upload_Date = DateTime.UtcNow;
                tb_LeadDocs.Id_Customer = Id_CustomerDoc;
                db.Tb_LeadDocs.Add(tb_LeadDocs);
                db.SaveChanges();

            return RedirectToAction("CustomerDashboard", "CRM", new { id = Id_CustomerDoc, token = "success" });

        }
            catch (Exception ex)
            {
                return RedirectToAction("CustomerDashboard", "CRM", new { id = Id_CustomerDoc, token = "error" });
            }


        }

        // GET: LeadDocs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_LeadDocs tb_LeadDocs = db.Tb_LeadDocs.Find(id);
            if (tb_LeadDocs == null)
            {
                return HttpNotFound();
            }
            ViewBag.Id_Customer = new SelectList(db.Tb_Customers, "ID_Customer", "Name", tb_LeadDocs.Id_Customer);
            return View(tb_LeadDocs);
        }

        // POST: LeadDocs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id_Document,Title,Url,Size,Id_Customer,Extension,Upload_Date")] Tb_LeadDocs tb_LeadDocs)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tb_LeadDocs).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Id_Customer = new SelectList(db.Tb_Customers, "ID_Customer", "Name", tb_LeadDocs.Id_Customer);
            return View(tb_LeadDocs);
        }

        // GET: LeadDocs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_LeadDocs tb_LeadDocs = db.Tb_LeadDocs.Find(id);
            if (tb_LeadDocs == null)
            {
                return HttpNotFound();
            }
            return View(tb_LeadDocs);
        }

        // POST: LeadDocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tb_LeadDocs tb_LeadDocs = db.Tb_LeadDocs.Find(id);
            db.Tb_LeadDocs.Remove(tb_LeadDocs);
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

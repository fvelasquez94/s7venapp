using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Realestate_portal.Models;
using Realestate_portal.Services.Contracts;

namespace Realestate_portal.Controllers
{
    public class PostsController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        private clsGeneral generalClass = new clsGeneral();

        private Imarket repo;

        public PostsController(Imarket _repo)
        {
            repo = _repo;
        }

        public PostsController()
        {
        }


        // GET: Posts
        public ActionResult Index( int broker=0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
                //HEADER DATA
                ViewBag.activeuser = activeuser;
                ViewBag.userCompany = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();

                ViewBag.rol = "";

                //Filtros SA

                var lstCompanies = (from a in db.Sys_Company select a).ToList();
                ViewBag.lstCompanies = lstCompanies;

                List<Tb_Posts> lstPost = new List<Tb_Posts>();

                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    if (activeuser.Roles.Contains("SA") && broker == 0)
                    {
                        ViewBag.rol = "SA";
                        ViewBag.userdata = (from usd in db.Sys_Users where (usd.ID_Company == activeuser.ID_Company) select usd).FirstOrDefault();
                        var brokersel = (from b in db.Sys_Users where (b.ID_Company == activeuser.ID_Company && b.Roles.Contains("Admin")) select b).FirstOrDefault();
                        RedirectToAction("Dashboard", "Portal", new { broker = brokersel.ID_Company });
                    }
                    else
                    {
                        ViewBag.rol = "Admin";
                        if (broker == 0)
                        {

                            lstPost = db.Tb_Posts.Where(t => t.ID_Company == activeuser.ID_Company).Select(t => t).ToList();
                        }
                        else
                        {
                            ViewBag.rol = "SA";

                            lstPost = db.Tb_Posts.Where(t => t.ID_Company == broker).Select(t => t).ToList();
                        }
                    }
                   


                }
                //FIN HEADER
                ViewBag.selbroker = broker;
                
                return View(lstPost);

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

     
        }

        // GET: Posts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tb_Posts tb_Posts = db.Tb_Posts.Find(id);
            if (tb_Posts == null)
            {
                return HttpNotFound();
            }
            return View(tb_Posts);
        }

        // GET: Posts/Create
        public ActionResult Create()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
                //HEADER DATA
                ViewBag.activeuser = activeuser;
                ViewBag.userCompany = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();

                ViewBag.rol = "";


                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    ViewBag.rol = "Admin";


                }
                //FIN HEADER

                ViewBag.ID_User = new SelectList(db.Sys_Users, "ID_User", "Name");
                return View();

            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }
         
        }

        // POST: Posts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_Post,Title,Content_post,Url_image,Lat,Lng,Start_at,Finish_at,Post_type,ID_User,Username,Likes,User_likes,Comments,User_comments,created_at,Post_parent,Active")] Tb_Posts tb_Posts)
        {
            if (tb_Posts.Url_image == null) { tb_Posts.Url_image = ""; }
            if (tb_Posts.Lat == null) { tb_Posts.Lat = ""; }
            if (tb_Posts.Lng == null) { tb_Posts.Lng = ""; }
            tb_Posts.Start_at = DateTime.UtcNow;
            tb_Posts.Finish_at = DateTime.UtcNow;
            tb_Posts.created_at = DateTime.UtcNow;

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            tb_Posts.ID_User = activeuser.ID_User;
            tb_Posts.Username = activeuser.Name + " " + activeuser.LastName;
            tb_Posts.Post_type = 1;
            tb_Posts.Likes = 0;
            tb_Posts.User_likes = "";
            tb_Posts.User_comments = "";
            tb_Posts.Comments = 0;
            tb_Posts.Post_parent = 0;
            tb_Posts.Active = true;
            tb_Posts.ID_Company = activeuser.ID_Company;

     
                db.Tb_Posts.Add(tb_Posts);
                //Actualizamos todos los post a false
                db.Database.ExecuteSqlCommand("update Tb_Posts set Active=0 where Post_type=1 and ID_company={0}",activeuser.ID_Company);

                db.SaveChanges();



            try
            {//Enviamos notificaciones  a todos los agentes del broker
                var agents = (from a in db.Sys_Users where (a.Active == true && a.ID_Company== activeuser.ID_Company) select a).ToList();

                if (agents.Count > 0)
                {
                    foreach (var item in agents)
                    {
                        Sys_Notifications newnotification = new Sys_Notifications();
                        newnotification.Active = true;
                        newnotification.Date = DateTime.UtcNow;
                        newnotification.Title = "New post from Broker.";
                        newnotification.Description = "A new post from your Broker was added";
                        newnotification.ID_user = item.ID_User;
                        db.Sys_Notifications.Add(newnotification);
                    }
                    db.SaveChanges();
                }

            }
            catch
            {

            }

            return RedirectToAction("Index", "Posts");
            

        }

        // GET: Posts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
                //HEADER DATA
                ViewBag.activeuser = activeuser;
                ViewBag.userCompany = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();

                ViewBag.rol = "";


                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    ViewBag.rol = "Admin";


                }
                //FIN HEADER

                Tb_Posts tb_Posts = db.Tb_Posts.Find(id);

                ViewBag.ID_User = new SelectList(db.Sys_Users, "ID_User", "Name", tb_Posts.ID_User);
                return View(tb_Posts);
            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }


        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_Post,Title,Content_post,Url_image,Lat,Lng,Start_at,Finish_at,Post_type,ID_User,Username,Likes,User_likes,Comments,User_comments,created_at,Post_parent,Active")] Tb_Posts tb_Posts)
        {
            if (tb_Posts.Url_image == null) { tb_Posts.Url_image = ""; }
            if (tb_Posts.Lat == null) { tb_Posts.Lat = ""; }
            if (tb_Posts.Lng == null) { tb_Posts.Lng = ""; }
            if (tb_Posts.User_likes == null) { tb_Posts.User_likes = ""; }
            if (tb_Posts.User_comments == null) { tb_Posts.User_comments = ""; }

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            tb_Posts.ID_Company = activeuser.ID_Company;

            db.Entry(tb_Posts).State = EntityState.Modified;
                db.SaveChanges();
                TempData["exito"] = "Data saved successfully.";
                return RedirectToAction("Index");

        }

        // GET: Posts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Sys_Notifications> lstAlerts = (from a in db.Sys_Notifications where (a.ID_user == activeuser.ID_User && a.Active == true) select a).OrderByDescending(x => x.Date).Take(4).ToList();
                ViewBag.notifications = lstAlerts; ViewBag.CartItems = repo.GetCartCount();
                //HEADER DATA
                ViewBag.activeuser = activeuser;
                ViewBag.userCompany = db.Sys_Company.Where(c => c.ID_Company == activeuser.ID_Company).FirstOrDefault();

                ViewBag.rol = "";


                if (activeuser.Roles.Contains("Agent"))
                {
                    ViewBag.rol = "Agent";

                }
                else
                {
                    ViewBag.rol = "Admin";


                }
                //FIN HEADER
                Tb_Posts tb_Posts = db.Tb_Posts.Find(id);
                return View(tb_Posts);
            }
            else
            {

                return RedirectToAction("Login", "Portal", new { access = false });

            }

        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tb_Posts tb_Posts = db.Tb_Posts.Find(id);
            db.Tb_Posts.Remove(tb_Posts);
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

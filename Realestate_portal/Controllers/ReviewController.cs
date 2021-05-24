using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Postal;
using Realestate_portal.Models;
using Realestate_portal.Models.ViewModels;

namespace Realestate_portal.Controllers
{
    public class ReviewController : Controller
    {
        private Realstate_agentsEntities db = new Realstate_agentsEntities();
        // GET: Review
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
       
        public ActionResult GetReview(int idNetwork) {

            Sys_Users activeuser = Session["activeUser"] as Sys_Users;
            try
            {
                var lstreview = (from a in db.Tb_Reviews.Where(c => c.Id_Network == idNetwork) orderby a.Id_Review descending
                                 select new ReviewView
                                 {

                                     Id_Network = a.Id_Network,
                                     Id_Review = a.Id_Review,
                                     Comment = a.Comment,
                                     Date_Review = (DateTime)a.Date_Review,
                                     Review_Rate = a.Rate_Review,
                                     User = a.Name_User

                                 }).ToList();


                foreach (var item in lstreview)
                {
                    item.Date = item.Date_Review.ToShortDateString();
                }
                return Json(lstreview);
            }
            catch (Exception ex)
            {

                return Json("ERROR" + ex);
            }
            
        }
        [HttpPost]
        public ActionResult AddReview(string comment, int network, int rate) {
            //if (customer == null) { customer = 0; }
            //if (process == null) { process = 0; }
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                Tb_Reviews newReview = new Tb_Reviews();
                newReview.Date_Review = DateTime.UtcNow;
                newReview.Id_Network = network;
                newReview.Rate_Review = rate;
                newReview.Comment = comment;
                newReview.Name_User = activeuser.Name + " " + activeuser.LastName;
                db.Tb_Reviews.Add(newReview);
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

        public ActionResult DeleteReview(int review) {
            try
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                Tb_Reviews newReview = new Tb_Reviews();
                newReview.Id_Review = review;
                db.Tb_Reviews.Remove(newReview);
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
    }
}
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
        public ActionResult Create([Bind(Include = "ID_team,Name,Description,ID_Company,Active,Creation_date,Last_update")] Tb_WorkTeams tb_WorkTeams, int[] agents, int[] idcustomer, int leader)
        {

            try
            {
                var idcompany = 1;
                var idcustomersel = 0;
                tb_WorkTeams.Description = "";
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                if (activeuser != null) { idcompany = activeuser.ID_Company; }
                tb_WorkTeams.ID_Company = idcompany;
                tb_WorkTeams.Active = true;
                tb_WorkTeams.Creation_date = DateTime.UtcNow;
                tb_WorkTeams.Last_update = DateTime.UtcNow;
                db.Tb_WorkTeams.Add(tb_WorkTeams);
                db.SaveChanges();

                if(idcustomer!=null)
                {
                    if (idcustomer.Length > 0)
                    {
                        foreach(var cust in idcustomer)
                        {
                            //Agregamos prop a cliente
                            var customer = db.Tb_Customers.Where(c => c.ID_Customer == cust).FirstOrDefault();
                            if (customer != null)
                            {
                                //idcustomersel = customer.ID_Customer;
                                customer.ID_team = tb_WorkTeams.ID_team;
                                db.Entry(customer).State = EntityState.Modified;
                            }
                        }
                        db.SaveChanges();
                    }

                }


                //guardamos agentes
                if (agents != null)
                {
                    if (agents.Length > 0)
                    {
                        foreach (var user in agents)
                        {
                            //Agregamos agentes a equipo
                            Tb_Customers_Users newteamuser = new Tb_Customers_Users();
                            newteamuser.Id_Customer = idcustomersel;
                            newteamuser.Id_User = user;
                            newteamuser.ID_team = tb_WorkTeams.ID_team;
                            newteamuser.Teamleader = false;
                            db.Tb_Customers_Users.Add(newteamuser);
                        }
                        db.SaveChanges();
                    }
                }


             
                    //Agregamos Team leader
                    Tb_Customers_Users newteamuserleader = new Tb_Customers_Users();
                    newteamuserleader.Id_Customer = idcustomersel;
                    newteamuserleader.Id_User = leader;
                    newteamuserleader.ID_team = tb_WorkTeams.ID_team;
                    newteamuserleader.Teamleader = true;
                    db.Tb_Customers_Users.Add(newteamuserleader);

                //Le cambiamos la propiedad para que tenga mas acceso
                try
                {
                    var usuario = db.Sys_Users.Find(leader);
                    usuario.Team_Leader = true;
                    db.Entry(usuario).State = EntityState.Modified;
                }
                catch {

                }
              

                    db.SaveChanges();
                

                return RedirectToAction("Teams", "CRM", new { token = "success" });
            }
            catch(Exception ex)
            {
                return RedirectToAction("Teams", "CRM", new { token = "error" });
            }

          

          
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
    
        public ActionResult Edit(int ID_team, string NameEdit, string DescriptionEdit, int[] agentsEdit, int[] idcustomerEdit, string leaderEdit)
        {
            try {
                var idcustomersel = 0;
                Tb_WorkTeams tb_WorkTeams = db.Tb_WorkTeams.Find(ID_team);
                tb_WorkTeams.Name = NameEdit;
                DescriptionEdit = "";
                tb_WorkTeams.Description = DescriptionEdit;
                tb_WorkTeams.Last_update = DateTime.UtcNow;

                db.Entry(tb_WorkTeams).State = EntityState.Modified;
                db.SaveChanges();


                //Eliminamos agentes viejos
                var agents = db.Tb_Customers_Users.Where(c => c.ID_team == ID_team).ToList();
                if (agents.Count > 0)
                {
                    db.Tb_Customers_Users.RemoveRange(agents);
                    db.SaveChanges();
                }


                //volvemos a correr proceso de guardado
                if (idcustomerEdit != null)
                {
                    if (idcustomerEdit.Length > 0)
                    {
                        foreach (var cust in idcustomerEdit)
                        {
                            //Agregamos prop a cliente
                            var customer = db.Tb_Customers.Where(c => c.ID_Customer == cust).FirstOrDefault();
                            if (customer != null)
                            {
                                //idcustomersel = customer.ID_Customer;
                                customer.ID_team = tb_WorkTeams.ID_team;
                                db.Entry(customer).State = EntityState.Modified;
                            }
                        }
                        db.SaveChanges();
                    }

                }


                //guardamos agentes
                if (agentsEdit != null)
                {
                    if (agentsEdit.Length > 0)
                    {
                        foreach (var user in agentsEdit)
                        {
                            //Agregamos agentes a equipo
                            Tb_Customers_Users newteamuser = new Tb_Customers_Users();
                            newteamuser.Id_Customer = idcustomersel;
                            newteamuser.Id_User = user;
                            newteamuser.ID_team = tb_WorkTeams.ID_team;
                            newteamuser.Teamleader = false;
                            db.Tb_Customers_Users.Add(newteamuser);
                        }
                        db.SaveChanges();
                    }
                }
                 if(leaderEdit != null)
                {
                    //Agregamos Team leader
                    Tb_Customers_Users newteamuserleader = new Tb_Customers_Users();
                    newteamuserleader.Id_Customer = idcustomersel;
                    newteamuserleader.Id_User = Convert.ToInt32(leaderEdit);
                    newteamuserleader.ID_team = tb_WorkTeams.ID_team;
                    newteamuserleader.Teamleader = true;
                    db.Tb_Customers_Users.Add(newteamuserleader);

                    //Le cambiamos la propiedad para que tenga mas acceso
                    try
                    {
                        var usuario = db.Sys_Users.Find(Convert.ToInt32(leaderEdit));
                        usuario.Team_Leader = true;
                        db.Entry(usuario).State = EntityState.Modified;
                    }
                    catch
                    {

                    }
                }
    
                db.SaveChanges();

                return RedirectToAction("Teams", "CRM", new { token = "success" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Teams", "CRM", new { token = "error" });
            }


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

        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Tb_WorkTeams tb_WorkTeams = db.Tb_WorkTeams.Find(id);
                db.Tb_WorkTeams.Remove(tb_WorkTeams);
                db.SaveChanges();

                //Quitamos prop a clientes
                var customer = db.Tb_Customers.Where(c => c.ID_team == id).ToList();
                if (customer.Count>0)
                {
                    foreach(var cust in customer)
                    {
                        cust.ID_team = 0;
                        db.Entry(cust).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                 
                }

                var agents = db.Tb_Customers_Users.Where(c => c.ID_team == id).ToList();
                if (agents.Count > 0)
                {
                    db.Tb_Customers_Users.RemoveRange(agents);
                    db.SaveChanges();
                }
             

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

using Newtonsoft.Json;
using Realestate_portal.Cors;
using Realestate_portal.Models;
using Realestate_portal.Models.ViewModels.Market;
using Realestate_portal.Services.Contracts;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Realestate_portal.Controllers
{
    public class MarketController : Controller
    {
        // GET: Marketing
        private Imarket repo;
        private IUser user;
        private IGeneral general;
        private IEmail email;      

        public MarketController()
        {
        }
       
        public MarketController(Imarket _repo, IUser _user, IGeneral _general, IEmail _email)
        {
            repo = _repo;
            user = _user;
            general = _general;
            email = _email;           
        }
        public ActionResult Index(int broker=0)
        {
            if (general.checkSession())
            {

                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();          
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
         

          
        }


        public ActionResult Brands(int broker = 0)
        {
            if (general.checkSession())
            {

                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                return View(repo.GetBrandstCategories());
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        public ActionResult JoinBrand(string name, string company, string email)
        {
            if (this.email.JoinBrandEmail("JoinBrand", "User wishes to join our brands", name, company, email))
            {
                return Json( new { message= "Success" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { message = "Error" }, JsonRequestBehavior.AllowGet);
            }

        }



            [HttpGet]
        public ActionResult Categories(int id, bool? child, int subcategory=0)
        {
            if (general.checkSession())
            {

                if (child != null)
                {
                    return RedirectToAction("Companies", "Market", new { id = id, subcategory = true });
                }
         //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                ViewBag.company = id;
               
                List<Template_type> types = null;

                if (subcategory != 0)
                {
                    ViewBag.pgr = true;
                    ViewBag.sub = subcategory;
                    types = repo.getCategories(id, subcategory);
                }
                else
                {
                    ViewBag.pgr = false;
                    ViewBag.sub = subcategory;
                    types = repo.getCategories(id);
                }
              
                return View(types);                
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        public ActionResult Companies(int id, bool? subcategory)
        {
            if (general.checkSession())
            {

                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                ViewBag.company = id;
                List<template_subcategories> sub = new List<template_subcategories>();

                if ((bool)subcategory)
                {
                     sub = repo.getSubcategories(id);
                }

              
   

                return View(sub);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }


        [HttpGet]
        public ActionResult Template(bool pgr, int subcategory=0, int type=0, int company=0)
        {
            if (general.checkSession())
            {
             

                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                ViewBag.company = company;
                ViewBag.pgr = pgr;
                ViewBag.type = type;
                ViewBag.subcategory = subcategory;
                if (subcategory != 0)
                {
                    ViewBag.pgr = true;
                    return View(repo.getTemplatesLayout(company, type, subcategory));
                }

                return View(repo.getTemplatesLayout(company, type));
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }


        [HttpGet]
        public ActionResult Shop(int id=0)
        {
            if (general.checkSession())
            {
                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                ViewBag.company = id;
                return View(repo.GetTemplatesTypesByCompany(id));
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }


        [HttpGet]
        public ActionResult SizesAndMaterials(string id, int company, bool pgr, int subcategory)
        {
            if (general.checkSession())
            {

                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                Template_Layout temp = repo.GetTemplateLayout(id);
                ViewBag.Name = temp.Name;
                ViewBag.Path = temp.Path;
                ViewBag.width = temp.Width;
                ViewBag.height = temp.Height;               
                ViewBag.dimensions = repo.GetTemplateDimensions(id);
                ViewBag.type = temp.type;
                ViewBag.company = company;
                ViewBag.pgr = pgr;
                ViewBag.subcategory = subcategory;

                return View();
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        [HttpGet]
            public JsonResult GetQuantyAndDetails(string type, int typeId)
        {
            var info = repo.getTypeInfoByType(type, typeId);

            return Json(info,JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetPricesByType(string type, int typeId)
        {
            var info = repo.getTypeInfoByType(type, typeId);

            return Json(info.Prices, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTemplateAsPDF(string img, string width = "500px", string height = "500px")
        {
            if (general.checkSession())
            {
                try
                {


                    string path = repo.SavePDFToTemplate(img);
                    if (path != null)
                    {
                        Rotativa.Options.Margins margin = new Rotativa.Options.Margins(0, 20, 0, 20);
                        var actionResult = new ActionAsPdf("TemplatePDF", new { path = path, width = width, height = height }) { FileName = "template.pdf", CustomSwitches = "--disable-smart-shrinking", PageSize = Rotativa.Options.Size.A3, PageMargins = margin };
                        var byteArray = actionResult.BuildFile(ControllerContext);
                        var pdfa = Convert.ToBase64String(byteArray);



                        return Json(new { msg = "success", pdf = pdfa });
                    }

                    else
                    {
                        return Json( new { msg = "error", error = "No path" });
                    }
                }catch(Exception e)
                {
                    return Json(new {msg = "error", error = e.Message + "path " });
                }

            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }


        [HttpPost]
        
        public ActionResult TemplateToPDF(string img, string width="500px", string height="500px")
        {           
            try
            {
            
                var path  =  repo.SavePDFToTemplate(HttpUtility.UrlDecode(img));
                /*    Rotativa.Options.Margins margin = new Rotativa.Options.Margins(0, 20, 0, 20);
                   var actionResult = new ActionAsPdf("TemplatePDF", new { path = path, width = width, height = height }) { FileName = "template.pdf", CustomSwitches = "--disable-smart-shrinking", PageSize = Rotativa.Options.Size.A3, PageMargins = margin };
                   var byteArray = actionResult.BuildFile(ControllerContext);
                   var pdfa = Convert.ToBase64String(byteArray);                    
             */
                return Json(path);
            }
            catch (Exception e)
            {
                return Json(new { msg = "errorr", error = e.InnerException.ToString() });

            }
           
        }


        private void SaveByteArrayAsImage(string fullOutputPath, string base64String)
        {
            var base64 = base64String.Substring(base64String.LastIndexOf(',') + 1);


            byte[] imageBytes = Convert.FromBase64String(base64);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);

            image.Save(fullOutputPath, ImageFormat.Jpeg);

            //   File.WriteAllBytes(fullOutputPath+".png", bytes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TemplateCreate(Saved_Templates template, int subcategory, bool pgr, int type, int company)
        {
            if (general.checkSession())
            {
                repo.AddMaterials(template,user.GetUser().ID_User);

                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                var templateCanvas = repo.GetTemplateLabels(template.Templateid);
                ViewBag.type =type;
                ViewBag.company = company;
                ViewBag.pgr = pgr;
                ViewBag.subcategory = subcategory;

                return View(templateCanvas);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }


        [HttpGet]
        public JsonResult GetTemplateColors(string id)
        {
            var info = repo.GetTemplateColorsById(id);

            return Json(info, JsonRequestBehavior.AllowGet);
        }


        

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public ActionResult EditMaterials(SavedTemplateModel template, int savedId, string templateid)
        {
            if (general.checkSession())
            {

                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                ViewBag.id = savedId;              
                ViewBag.type = template.Template_Type;

                if (template.dimension == null)
                {
                    template.dimension = repo.GetDimensionId((int)repo.GetTemplateTypeById(template.Templateid), template.Material);
                }

                if (repo.EditSavedTemplateMaterials(template))
                {
                    return View("TemplateEdit", repo.GetLabelsToEditSavedTemplate(template.Id));
                }
                else
                {
                    ViewBag.error = "Template could not be edited, please try again";
                    return View("SizesAndMaterialsEdit", template);
                }

              
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        [HttpGet]
        public ActionResult EditTemplate(string templateId, int id = 0)
        {
            if (general.checkSession())
            {
                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                var template = repo.GetTemplateToUpdateMaterial(id);
                Template_Layout temp = repo.GetTemplateLayout(template.Templateid);
                ViewBag.width = temp.Width;
                ViewBag.height = temp.Height;               
                ViewBag.dimensions = repo.GetTemplateDimensions(template.Templateid);
                var info= repo.getTypeInfoByType(template.Material,(int)temp.type);
                ViewBag.amount = info.Quantity;
                ViewBag.description = info.Description;              
                ViewBag.type = temp.type;
                ViewBag.id = id;
                ViewBag.templateid = templateId;

                if (template == null)
                {
                    ViewBag.error = "that template was not found";
                    return View("ShoppingCart");
                }
                else
                {
                    return View("SizesAndMaterialsEdit", template);
                }
              
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        [HttpGet]
        public ActionResult ShoppingCart()
        {
            if (general.checkSession())
            {
                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                var templates= repo.GetViewModelForCart();
                ViewBag.total = templates.Templates.Sum(t => t.Price);                              
                return View(templates);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        [HttpGet]
        public ActionResult Checkout()
        {
            if (general.checkSession())
            {
                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                ViewBag.CartItems = repo.GetCartCount();
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(Billing_Shipping_details info)
        {
            if (general.checkSession())
            {
                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                 repo.SaveBillingDetails(info);
                ViewBag.shipping = repo.GetShippingOptions();
                var shipping = 24.95M;
                ViewBag.total = repo.GetTotalprice() + shipping;


                return View(info);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }



        public ActionResult EditBillingInfo(string first_name,string last_name, string b_Adress, string b_suite,
        string city, string state, string zip_code, string email, string phone)
        {
            try
            {
                if (repo.UpdateBillingDetails(first_name,last_name,last_name,b_suite,city,state,zip_code,email,phone))
                {
                    return Json("Success");
                }
                else
                {
                    return Json("Error");
                }
            }
            catch(Exception e)
            {
                return Json("Error");
            }                    
        }

        public ActionResult EditShippingInfo(string s_name, string s_adress, string s_suite,
       string s_city, string s_state, string s_zip_code, string s_phone)
        {
            try
            {
               if(repo.UpdateShippingDetails(s_name, s_adress, s_suite, s_city, s_state, s_zip_code, s_phone))
                {
                    return Json("Success");
                }
                else
                {
                    return Json("Error");
                }

               
            }
            catch (Exception e)
            {
                return Json("Error");
            }
        }


        [HttpPost]        
        public ActionResult AddToCart(string img, List<saved_labels> list, int edited, string background, string comment="")
        {
            if (general.checkSession())
            {

                if (repo.CompleteSavedTemplate(list, img, edited,comment, background))
                {
                    return Json("SUCCESS");
                }
                else
                {
                    return Json("Error");
                }

                
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }



        [HttpPost]
        public ActionResult AddToCartLogo(string img, List<saved_labels> list, string file, int filetype, int edited, string background, string comment = "")
        {
            if (general.checkSession())
            {

                if (repo.CompleteSavedTemplate(list, img, file, filetype, edited, comment, background))
                {
                    return Json("SUCCESS");
                }
                else
                {
                    return Json("Error");
                }


            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        [HttpPost]

        public ActionResult AddToCartPhoto(string img, List<saved_labels> list, string file, int filetype, int edited, string background, string comment = "")
        {
            if (general.checkSession())
            {

                if (repo.CompleteSavedTemplate(list, img, file,filetype, edited, comment, background))
                {
                    return Json("SUCCESS");
                }
                else
                {
                    return Json("Error");
                }


            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }


        [HttpPost]
        public ActionResult AddToCartPhotoLogo(string img, List<saved_labels> list, string logo, string photo, int edited, string background, string comment = "")
        {
            if (general.checkSession())
            {
                if (repo.CompleteSavedTemplate(list, img, photo, logo, edited, comment, background))
                {
                    return Json("SUCCESS");
                }
                else
                {
                    return Json("Error");
                }
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }


        

        
        public ActionResult DeleteTemplate(int id)
        {
            if (general.checkSession())
            {

                if (repo.deleteSavedTemplate(id))
                {

                    return Json("SUCCESS", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }


            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        public ActionResult updatePrice(int id, int amount)
        {           
                var result = repo.UpdateTemplatePrice(id, amount);

                return Json(new { price = result }, JsonRequestBehavior.AllowGet);                                              
        }
       

        [HttpGet]       
        public ActionResult Payment(string dump)
        {
            if (general.checkSession())
            {
                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                ViewBag.CartItems = repo.GetCartCount();
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        [HttpGet]
        public ActionResult PurchaseHistory()
        {
            if (general.checkSession())
            {
                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                var orders = repo.GetCompletedOrders();
                return View(orders);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }


        [HttpGet]
        public ActionResult OrderDetails(string OrderID,bool nw=false)
        {
            if (general.checkSession())
            {

                //settting all user information information
                ViewBag.notifications = user.SetUserNotifications();
                ViewBag.activeuser = user.GetUser();
                ViewBag.userCompany = user.GetUserCompany();
                ViewBag.rol = user.GetUserRole();

                //setting all market info
                ViewBag.CartItems = repo.GetCartCount();
                var order = repo.GetOrderIdByPaymentIntentId(OrderID, user.GetUser().ID_User);
                var templates = repo.TemplatesCompletedByOrderId(order.id);
                ViewBag.purchasedate = order.creation_date.ToShortDateString();
                ViewBag.paymentid = order.payment_intent_id;
                ViewBag.total = order.ttl_price;
                ViewBag.orderid = order.id;
                var dimension = templates.Select(t => t.dimension).ToArray();
                ViewBag.descriptions = repo.GetDescriptionBydimensionId(dimension);
                if (nw)
                {
                    if (!repo.OrderCompleted(OrderID))
                    {
                        ViewBag.purchased = "You purchase was completed successfully, an email has been sent, with the details";
                        string username = user.GetUser().Name + " " + user.GetUser().LastName;
                        var receipt = repo.GetReceiptDetail(OrderID);
                        var billing = repo.completeBillingDetails(receipt.order_id);
                        email.SendEmailWithFile("TemplateConfirmation", billing.email, $"PGR Purchase confirmation order number #{receipt.order_id}", receipt.location);
                    }
                }
                return View(templates);
            }
            else
            {
                return RedirectToAction("Login", "Portal", new { access = false });
            }
        }

        public ActionResult TemplatePDF(string path, string width, string height)
        {            
            TempPDF pdfile = new TempPDF();
            pdfile.Src = path;
            pdfile.Height = height;
            pdfile.Width = width;
            return View(pdfile);
        }


        public ActionResult Print(string id,int userid)
        {
            var order = repo.GetOrderIdByPaymentIntentId(id, userid);
            var templates = repo.TemplatesCompletedByOrderId(order.id);
            ViewBag.purchasedate = order.creation_date.ToShortDateString();
            ViewBag.total = order.ttl_price;
            ViewBag.orderid = order.id;
            var dimension = templates.Select(t => t.dimension).ToArray();
            ViewBag.descriptions = repo.GetDescriptionBydimensionId(dimension);
            return View("Receipt",templates);
        }       

     
        public ActionResult CompleteOrder(paymentIntent intent)
        {
            var result = repo.PaymentSucceded(intent.id);
            var userid = user.GetUser().ID_User;
            if (result!="Error")
            {
                var order = repo.GetOrderIdByPaymentIntentId(result, userid);              
                Rotativa.Options.Margins margin = new Rotativa.Options.Margins(0, 20, 0, 20);
                var actionResult = new ActionAsPdf("Print",new {id=result,userid= userid }) { FileName = $"Receipt_{result}.pdf", CustomSwitches = "--disable-smart-shrinking", PageSize = Rotativa.Options.Size.A3, PageMargins = margin };
                var byteArray = actionResult.BuildFile(ControllerContext);
                var path = Server.MapPath(Path.Combine("~/Content/Receipts", $"Receipt_{ result }.pdf"));
                using(var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(byteArray, 0, byteArray.Length);
                    fileStream.Close();
                }                

                Receipts receipt = new Receipts()
                {
                    file_name = $"Receipt_{ result }.pdf",
                    location = path,
                    order_id = order.id,
                    user_id = user.GetUser().ID_User
                };
                repo.CreateReceipt(receipt);

                return Json(result);
            }
            else
            {
                return Json("Error");
            }
          
        }

        public class paymentIntent
        {
            [JsonProperty("id")]
            public string id { get; set; }
        }

        public ActionResult Receipt(string OrderID)
        {
            var userid = user.GetUser().ID_User;
            var order = repo.GetOrderIdByPaymentIntentId(OrderID,userid);           
            ViewBag.purchasedate = order.creation_date.ToShortDateString();
            ViewBag.total = order.ttl_price;
            ViewBag.orderid = order.id;
            Rotativa.Options.Margins margin = new Rotativa.Options.Margins(0, 20, 0, 20);
            return new ActionAsPdf("Print", new { id=OrderID,userid=userid}) {FileName= "Receipt.pdf", CustomSwitches = "--disable-smart-shrinking", PageSize =Rotativa.Options.Size.A3, PageMargins=margin };
        }
    }
}
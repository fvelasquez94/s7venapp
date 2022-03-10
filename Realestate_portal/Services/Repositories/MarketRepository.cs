using Microsoft.AspNetCore.Mvc;
using Realestate_portal.Models;
using Realestate_portal.Models.Enum;
using Realestate_portal.Models.ViewModels.Market;
using Realestate_portal.Services.Contracts;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Realestate_portal.Services.Repositories
{
    public class MarketRepository : Imarket      
    {
        private static List<Saved_Templates>  saved;
        private Realstate_agentsEntities _db;
        private IUser user;

        public MarketRepository(Realstate_agentsEntities db, IUser _user)
        {
            _db = db;
            user = _user;
        }

        //gets all categories of templates
        public List<Template_type>  getCategories(int category, int subcategory=0 )
        {
            Market_Categ_type Cat_type = null;
            List<Template_type> types = new List<Template_type>();
            Template_type type = null;
            if (subcategory != 0)
            {
                 Cat_type = _db.Market_Categ_type.Where(c => c.category == category && c.subcategory == subcategory).FirstOrDefault();
            }
            else
            {
                 Cat_type= _db.Market_Categ_type.Where(c => c.category == category).FirstOrDefault();
            }

            if (Cat_type.yard_signs != 0)
            {
                type = _db.Template_type.Where(t => t.id == Cat_type.yard_signs).FirstOrDefault();
                types.Add(type);
            }

            if (Cat_type.business_card != 0)
            {
                type = _db.Template_type.Where(t => t.id == Cat_type.business_card).FirstOrDefault();
                types.Add(type);
            }

            if (Cat_type.closing_signs != 0)
            {
                type = _db.Template_type.Where(t => t.id == Cat_type.closing_signs).FirstOrDefault();
                types.Add(type);
            }

            if (Cat_type.open_house != 0)
            {
                type = _db.Template_type.Where(t => t.id == Cat_type.open_house).FirstOrDefault();
                types.Add(type);
            }


            if (Cat_type.riders != 0)
            {
                type = _db.Template_type.Where(t => t.id == Cat_type.riders).FirstOrDefault();
                types.Add(type);
            }

            if (Cat_type.for_sale_panels != 0)
            {
                type = _db.Template_type.Where(t => t.id == Cat_type.for_sale_panels).FirstOrDefault();
                types.Add(type);
            }

            return types;


        }


        public List<template_subcategories> getSubcategories(int id)
        {
            return _db.template_subcategories.Where(s => s.category_type == id).ToList();
        }

        //get the id of category of the template using the templateid
        public int getCategoryIdByTemplateId(string id)
        {
            var template= (from t in _db.Template_Details where t.id == id select t).FirstOrDefault();

            return (from c in _db.Template_categories where c.id == template.category select c.id).FirstOrDefault();
        }

        //it returns a list of template_layout with the name and path of the template
        public List<Template_Layout> getTemplatesLayout(int company,int type, int subcategory=0)
        {
            List<Template_Layout> list = new List<Template_Layout>();
            if (subcategory != 0)
            {
                 list = (from c in _db.Template_Details where c.category == company && c.type == type && c.subcategories==subcategory select new Template_Layout { Name = c.id, Path = c.path }).ToList();

            }
            else
            {
                 list = (from c in _db.Template_Details where c.category == company && c.type == type select new Template_Layout { Name = c.id, Path = c.path }).ToList();

            }

            return list;
        }

        //it returns an object template_details 
        public Template_Details GetTemplateDetailsById(string id)
        {
            return (from t in _db.Template_Details where t.id == id select t).FirstOrDefault();
        }


        //get modelview templates to be sent to the shopping cart
        public CartViewModel GetViewModelForCart()
        {
            var id = user.GetUser().ID_User;

            CartViewModel viewModel = new CartViewModel();
            var templates = (from t in _db.Saved_Templates
                             join c in _db.Template_categories on t.category equals c.id
                            where t.user_id==id && t.completed==false select new SavedTemplateModel
                             {
                                Id=t.id,
                                 Templateid = t.Templateid,
                                 Category = c.category,
                                 Amount = t.amount,
                                 CreationDate = t.Creation_date,
                                 Holes = t.Holes,
                                 Material = t.Material,
                                 Price = t.price,
                                 size = t.Size,
                                 Location=t.location,
                                 Template_Type=t.type,
                                 dimension=t.dimension
                                 
                             }).ToList();


            var dimension = templates.Select(t => t.dimension).ToArray();

            viewModel.Amounts = GetPricesForSavedTemplates(dimension);

            viewModel.Templates = templates;
           
            var labels = (from l in _db.saved_labels where l.userid == id select l).ToList();

            viewModel.Labels = labels;

            return viewModel;
        }



        //it returns an object template_layout with the name and path of the template
        public Template_Layout GetTemplateLayout(string id)
        {
            return (from c in _db.Template_Details where c.id == id select new Template_Layout {Name=c.id,Path=c.path,Width=c.width,Height=c.height,type=c.type }).FirstOrDefault();
        }

        //get the template to add the materials which will be saved later it will reset it when call 
        public Saved_Templates getTemplateToUpdateMaterials()
        {
            if (saved == null)
            {
                saved = new List<Saved_Templates>();
            }
            Saved_Templates template = new Saved_Templates();
            return template;
        }

        //return template saved to static list as cart by id
        public Saved_Templates getTemplateSavedToCart(string id)
        {
            return (from s in saved where s.Templateid == id select s).FirstOrDefault();
        }

        //it deletes the templated saved to the static list
        public bool removedTempleteSavedInCart(string id)
        {
            try
            {
                var template = (from s in saved where s.Templateid == id select s).FirstOrDefault();
                saved.Remove(template);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        //it adds the materials from sizes and materials also user id, creation date and category then it adds it static list
        public bool AddMaterials(Saved_Templates ToSave, int userid)
        {
            try
            {
                var template = getTemplateToUpdateMaterials();                
                template.label_num = 0;              
                template.Laminate = ToSave.Laminate;
                template.Material = ToSave.Material;
                template.Holes = ToSave.Holes;
                template.Size = ToSave.Size;
                template.amount = ToSave.amount;
                template.clearcoat = ToSave.clearcoat;
                template.Templateid = ToSave.Templateid;
                template.user_id = userid;
                template.price = ToSave.price;
                template.Creation_date = DateTime.Now;
                template.dimension = ToSave.dimension;
                template.category = getCategoryIdByTemplateId(ToSave.Templateid);
                saved.Add(template);
                return true;

            }catch(Exception e)
            {
                return false;
            }           
        }

        //it update the template saved in cart
        public bool UpdateMaterials(Saved_Templates ToSave)
        {
            try
            {
                var template = getTemplateSavedToCart(ToSave.Templateid);
                template.Laminate = ToSave.Laminate;
                template.Material = ToSave.Material;
                template.Holes = ToSave.Holes;
                template.Size = ToSave.Size;
                template.amount = ToSave.amount;
                template.clearcoat = ToSave.clearcoat;
                template.Creation_date = DateTime.Now;               

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }


        public string SavePDFToTemplate(string img)
        {
            Random random = new Random();

            int number = random.Next(1, 150);

            string name = "Template"+DateTime.Now.ToOADate().ToString()+ number + ".pdf";
                       
            var location = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/TemplatePDF/"), name);

            SaveByteArrayAsImage(location, img);

            return location;

        }



        // it takes the phisical file and the saved template in static list to save it to a path in server
        //then it saves it to db and removes the item from static list
        public bool CompleteSavedTemplate(List<saved_labels> labels, string img, int edited, string comment, string background)
        {
            var userID = user.GetUser().ID_User;
            try
            {
                if (edited == 0) { 
                saved_labels label = (saved_labels)labels.ToArray().GetValue(0);

                var template = (from t in saved
                                where t.Templateid == label.templateid && t.user_id== userID
                                select t).FirstOrDefault();

                Random random = new Random();

                int number = random.Next(1, 150);

                template.label_num = labels.Count;
                
                template.Comments = comment;

                template.path=background;

                template.description = label.templateid + user.GetUser().Name + user.GetUser().LastName + user.GetUser().ID_User + DateTime.Now.ToOADate() + number +".jpg";         
                
                template.location = "Content/images/marketing/savedTemplates/" + template.description;

                var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/savedTemplates/"), template.description);

                SaveByteArrayAsImage(path,img);             

                template.completed = false;

                _db.Saved_Templates.Add(template);

                _db.SaveChanges();

                saved.Remove(template);

                var id = template.id;
                var userid = user.GetUser().ID_User;

                foreach(var lab in labels)
                {
                    lab.saved_template = id;
                    lab.userid = userid;
                    
                    _db.saved_labels.Add(lab);
                }

                _db.SaveChanges();
                }

                if (edited == 1)
                {
                    saved_labels label = (saved_labels)labels.ToArray().GetValue(0);

                    var id = Convert.ToInt32(label.templateid);

                    var template = (from t in _db.Saved_Templates
                                    where t.id ==id 
                                    select t).FirstOrDefault();

                    Random random = new Random();

                    int number = random.Next(1, 150);

                    var existingPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/savedTemplates/"), template.description);
                    if(File.Exists(existingPath))
                    {
                        File.Delete(existingPath);
                    }

                    template.description = template.Templateid + user.GetUser().Name + user.GetUser().LastName + user.GetUser().ID_User + DateTime.Now.ToOADate() + number + ".jpg";

                   
                    template.location = "Content/images/marketing/savedTemplates/" + template.description;

                    var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/savedTemplates/"), template.description);

                    SaveByteArrayAsImage(path, img);

                    template.Creation_date = DateTime.Now;

                    template.Comments = comment;

                    template.path = background;


                    var savedLab = (from l in _db.saved_labels where l.saved_template == label.saved_template select l).ToList();


                    foreach (var lab in labels)
                    {                        
                       foreach(var save in savedLab)
                        {
                            if (lab.labelid == save.labelid)
                            {
                                save.text = lab.text;
                            }
                        }
                    }

                    _db.SaveChanges();



                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public bool CompleteSavedTemplate(List<saved_labels> labels, string img, string photo, string logo, int edited,string comment, string background)
        {
            try
            {
                if (edited == 0)
                {
                    saved_labels label = (saved_labels)labels.ToArray().GetValue(0);

                var template = (from t in saved
                                where t.Templateid == label.templateid
                                select t).FirstOrDefault();

                Random random = new Random();

                int number = random.Next(1, 150);

                template.label_num = labels.Count;             

                template.Comments = comment;

                template.path=background;

                template.description = label.templateid + user.GetUser().Name + user.GetUser().LastName + user.GetUser().ID_User + DateTime.Now.ToOADate() + number + ".jpg";

                template.location = "Content/images/marketing/savedTemplates/" + template.description;

                var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/savedTemplates/"), template.description);

                SaveByteArrayAsImage(path, img);

                template.photo_path = "Content/images/marketing/photos/" + template.description;

                path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/photos/"), template.description);

                SaveByteArrayAsImage(path, photo);

                template.logo_path = "Content/images/marketing/logos/" + template.description;

                path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/logos/"), template.description);

                SaveByteArrayAsImage(path, logo);

                template.completed = false;

                _db.Saved_Templates.Add(template);

                _db.SaveChanges();

                saved.Remove(template);

                var id = template.id;
                var userid = user.GetUser().ID_User;

                foreach (var lab in labels)
                {
                    lab.saved_template = id;
                    lab.userid = userid;
                    _db.saved_labels.Add(lab);
                }

                _db.SaveChanges();
            }

                if (edited == 1)
                {
                    saved_labels label = (saved_labels)labels.ToArray().GetValue(0);

                    var id = Convert.ToInt32(label.templateid);

                    var template = (from t in _db.Saved_Templates
                                    where t.id == id
                                    select t).FirstOrDefault();

                    Random random = new Random();

                    int number = random.Next(1, 150);

                    var existingPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/savedTemplates/"), template.description);
                    if (File.Exists(existingPath))
                    {
                        File.Delete(existingPath);
                    }

                   var description = template.Templateid + user.GetUser().Name + user.GetUser().LastName + user.GetUser().ID_User + DateTime.Now.ToOADate() + number + ".jpg";                   

                    var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/savedTemplates/"), description);

                    SaveByteArrayAsImage(path, img);

                     existingPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/photos/"), template.description);
                    if (File.Exists(existingPath))
                    {
                        File.Delete(existingPath);
                    }

                    template.photo_path = "Content/images/marketing/photos/" + description;

                    path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/photos/"), description);

                    SaveByteArrayAsImage(path, photo);

                    existingPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/logos/"), template.description);
                    if (File.Exists(existingPath))
                    {
                        File.Delete(existingPath);
                    }

                    template.logo_path = "Content/images/marketing/logos/" + description;

                    path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/logos/"), description);

                    SaveByteArrayAsImage(path, logo);

                    template.description = description;
                    template.location = "Content/images/marketing/savedTemplates/" + description;

                    template.Creation_date = DateTime.Now;

                    template.Comments = comment;

                    template.path = background;


                    var savedLab = (from l in _db.saved_labels where l.saved_template == label.saved_template select l).ToList();


                    foreach (var lab in labels)
                    {
                        foreach (var save in savedLab)
                        {
                            if (lab.labelid == save.labelid)
                            {
                                save.text = lab.text;
                            }
                        }
                    }

                    _db.SaveChanges();
                }


                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public bool CompleteSavedTemplate(List<saved_labels> labels, string img, string file, int filetype, int edited, string comment, string background)
        {
            try
            {
                if (edited == 0)
                {
                    saved_labels label = (saved_labels)labels.ToArray().GetValue(0);

                    var template = (from t in saved
                                    where t.Templateid == label.templateid
                                    select t).FirstOrDefault();

                    Random random = new Random();

                    int number = random.Next(1, 150);

                    template.label_num = labels.Count;

                    template.Comments = comment;

                    template.path = background;

                    template.description = label.templateid + user.GetUser().Name + user.GetUser().LastName + user.GetUser().ID_User + DateTime.Now.ToOADate() + number + ".jpg";

                    template.location = "Content/images/marketing/savedTemplates/" + template.description;

                    var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/savedTemplates/"), template.description);

                    SaveByteArrayAsImage(path, img);

                    if (filetype == 1)
                    {

                        template.photo_path = "Content/images/marketing/photos/" + template.description;

                        path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/photos/"), template.description);

                        SaveByteArrayAsImage(path, file);
                    }
                    else if (filetype == 0)
                    {
                        template.logo_path = "Content/images/marketing/logos/" + template.description;

                        path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/logos/"), template.description);

                        SaveByteArrayAsImage(path, file);
                    }

                    template.completed = false;

                    _db.Saved_Templates.Add(template);

                    _db.SaveChanges();

                    saved.Remove(template);

                    var id = template.id;
                    var userid = user.GetUser().ID_User;

                    foreach (var lab in labels)
                    {
                        lab.saved_template = id;
                        lab.userid = userid;
                        _db.saved_labels.Add(lab);
                    }

                    _db.SaveChanges();

                }

                if (edited == 1)
                {
                    saved_labels label = (saved_labels)labels.ToArray().GetValue(0);

                    var id = Convert.ToInt32(label.templateid);

                    var template = (from t in _db.Saved_Templates
                                    where t.id == id
                                    select t).FirstOrDefault();

                    Random random = new Random();

                    int number = random.Next(1, 150);

                    var existingPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/savedTemplates/"), template.description);
                    if (File.Exists(existingPath))
                    {
                        File.Delete(existingPath);
                    }

                    var description = template.Templateid + user.GetUser().Name + user.GetUser().LastName + user.GetUser().ID_User + DateTime.Now.ToOADate() + number + ".jpg";

                    var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/savedTemplates/"), description);

                    SaveByteArrayAsImage(path, img);

                    

                    if (filetype == 1)
                    {

                         existingPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/photos/"), template.description);
                        if (File.Exists(existingPath))
                        {
                            File.Delete(existingPath);
                        }

                        template.photo_path = "Content/images/marketing/photos/" + description;

                        path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/photos/"), description);

                        SaveByteArrayAsImage(path, file);
                    }
                    else if (filetype == 0)
                    {
                        existingPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/logos/"), template.description);
                        if (File.Exists(existingPath))
                        {
                            File.Delete(existingPath);
                        }

                        template.logo_path = "Content/images/marketing/logos/" + description;

                        path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/images/marketing/logos/"), description);

                        SaveByteArrayAsImage(path, file);
                    }

                    template.description = description;

                    template.location = "Content/images/marketing/savedTemplates/" + description;

                    template.Creation_date = DateTime.Now;

                    template.Comments = comment;

                    template.path = background;


                    var savedLab = (from l in _db.saved_labels where l.saved_template == label.saved_template select l).ToList();


                    foreach (var lab in labels)
                    {
                        foreach (var save in savedLab)
                        {
                            if (lab.labelid == save.labelid)
                            {
                                save.text = lab.text;
                            }
                        }
                    }

                    _db.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        //trim al data extract from entries in table lable in db
        public labels TrimLabelItem(labels label)
        {
            if (!string.IsNullOrEmpty(label.template))
            {
                label.template = label.template.Trim();
            }

            if (!string.IsNullOrEmpty(label.name))
            {
                label.name = label.name.Trim();
            }
            if (!string.IsNullOrEmpty(label.description))
            {
                label.description = label.description.Trim();
            }
            if (!string.IsNullOrEmpty(label.p_top))
            {
                label.p_top = label.p_top.Trim();
            }

            if (!string.IsNullOrEmpty(label.p_right))
            {
                label.p_right = label.p_right.Trim();
            }

            if (!string.IsNullOrEmpty(label.p_left))
            {
                label.p_left = label.p_left.Trim();
            }
            if (!string.IsNullOrEmpty(label.p_bottom))
            {
                label.p_bottom = label.p_bottom.Trim();
            }
            if (!string.IsNullOrEmpty(label.Font))
            {
                label.Font = label.Font.Trim();
            }
            if (!string.IsNullOrEmpty(label.Color))
            {
                label.Color = label.Color.Trim();
            }
            if (!string.IsNullOrEmpty(label.size))
            {
                label.size = label.size.Trim();
            }
            if (!string.IsNullOrEmpty(label.width))
            {
                label.width = label.width.Trim();
            }
            if (!string.IsNullOrEmpty(label.height))
            {
                label.height = label.height.Trim();
            }
            if (!string.IsNullOrEmpty(label.pic_bottom))
            {
                label.pic_bottom = label.pic_bottom.Trim();
            }
            if (!string.IsNullOrEmpty(label.pic_left))
            {
                label.pic_left = label.pic_left.Trim();
            }
           

            return label;
        }


        //it gets the labes to be sent to create template
        public TemplateCanvas GetTemplateLabels(string id)
        {
            List<labels> labels = (from l in _db.labels where l.template == id select l).ToList();
            List<labels> Trimlabels = new List<labels>();

            TemplateCanvas canvas = new TemplateCanvas();
            Template_Details details = (from d in _db.Template_Details where d.id == id select d).FirstOrDefault();
            canvas.Details = details;

            foreach (var item in labels)
            {
                Trimlabels.Add(TrimLabelItem(item));
            }

                foreach (var item in Trimlabels)
            {              


                if (item.name == "Header")
                {
                    canvas.Header = item;
                }

                if (item.name == "Facebook")
                {
                    canvas.Facebook = item;
                }

                if (item.name == "Instagram")
                {
                    canvas.Instagram = item;
                }

                if (item.name == "Company")
                {
                    canvas.Company = item;
                }

                if (item.name == "Agent")
                {
                    canvas.Agent = item;
                }

                if (item.name == "Phone")
                {
                    canvas.Phone = item;
                }

                if (item.name == "Website")
                {
                    canvas.Website = item;
                }

                if (item.name == "Photo")
                {
                    canvas.Photo = item;
                }

                if (item.name == "Logo")
                {
                    canvas.Logo = item;
                }

                if (item.name == "Title")
                {
                    canvas.Title = item;
                }

                if (item.name == "Email")
                {
                    canvas.Email = item;
                }

                if (item.name == "Other")
                {
                    canvas.Other = item;
                }

                if (item.name == "Message")
                {
                    canvas.Message = item;
                }

                if (item.name == "Adress_1")
                {
                    canvas.Adress_1 = item;
                }

                if (item.name == "Adress_2")
                {
                    canvas.Adress_2 = item;
                }


            }
            return canvas;
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

        public bool deleteSavedTemplate(int id)
        {
            var labels = (from l in _db.saved_labels where l.saved_template == id select l).ToList();
            var template = (from t in _db.Saved_Templates where t.id == id select t).FirstOrDefault();
            var path = Path.Combine(HttpContext.Current.Server.MapPath("~/"), template.location);
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                _db.saved_labels.RemoveRange((IEnumerable<saved_labels>)labels);
                _db.Saved_Templates.Remove(template);

                _db.SaveChanges();
                return true;
            }catch(Exception e)
            {
                return true;
            }
        }

        public SavedTemplateModel GetTemplateToUpdateMaterial(int id)
        {
            var savedTemplate = (from s in _db.Saved_Templates where s.id == id select
                                 new SavedTemplateModel
                                 {
                                     Id = s.id,
                                     Templateid = s.Templateid,                                    
                                     Amount = s.amount,                                    
                                     Holes = s.Holes,
                                     Material = s.Material,
                                     Price = s.price,
                                     size = s.Size,
                                     Location = s.location                                                                   
                                 }
                                 ).FirstOrDefault();
            var template = (from t in _db.Saved_Templates where t.Templateid == savedTemplate.Templateid select t).FirstOrDefault();           

            return savedTemplate;

        }


        public decimal UpdateTemplatePrice(int id, int amount)
        {
            var dimension = _db.Saved_Templates.Where(t => t.id == id).FirstOrDefault();
            var price = _db.template_pricing.Where(p => p.dimensions == dimension.dimension && p.quantity == amount).Select(p => p.price).FirstOrDefault();

            dimension.price = price;
            dimension.amount = amount;
            _db.Entry(dimension).State = EntityState.Modified;
            _db.SaveChanges();

            return price; 
        }

        public  bool EditSavedTemplateMaterials(SavedTemplateModel template)
        {
            var savedTemplate = (from t in _db.Saved_Templates where t.id == template.Id select t).FirstOrDefault();

          
            try
            {
                if (template.Price == null)
                template.Price = savedTemplate.price;

                if (savedTemplate.Material == template.Material &&
                savedTemplate.price == template.Price &&
                savedTemplate.amount == template.Amount)
                {
                    return true; 
                }

                savedTemplate.Material = template.Material;
                savedTemplate.price = template.Price;                
                savedTemplate.amount = template.Amount;

                                 
                if (_db.SaveChanges() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                return false;
            }
          

        }

        public TemplateCanvas GetLabelsToEditSavedTemplate(int id)
        {
            var savedLabels = (from l in _db.saved_labels where l.saved_template == id select l).ToList();
            var savedTemplate= (from t in _db.Saved_Templates where t.id == id select t).FirstOrDefault();
            saved_labels savedlab =(saved_labels) savedLabels.ToArray().GetValue(0);
            var labels= (from l in _db.labels where l.template == savedlab.templateid select l).ToList();
            var template= (from t in _db.Template_Details where t.id == savedlab.templateid select t).FirstOrDefault();
            List<labels> Trimlabels = new List<labels>();

            var canvas = new TemplateCanvas();

            foreach (var item in labels)
            {
                Trimlabels.Add(TrimLabelItem(item));
            }

            foreach (var item in Trimlabels)
            {
                foreach(var it in savedLabels)
                {
                    if (item.id == it.labelid)
                    {
                        item.description = it.text;
                        item.size = it.font_size;
                        item.Color = it.color;
                    }
                }

                if (item.name == "Header")
                {                   
                    canvas.Header = item;
                }

                if (item.name == "Company")
                {
                    canvas.Company = item;
                }

                if (item.name == "Agent")
                {
                    canvas.Agent = item;
                }

                if (item.name == "Phone")
                {
                    canvas.Phone = item;
                }

                if (item.name == "Website")
                {
                    canvas.Website = item;
                }

                if (item.name == "Photo")
                {
                    canvas.Photo = item;
                }

                if (item.name == "Logo")
                {
                    canvas.Logo = item;
                }

                if (item.name == "Title")
                {
                    canvas.Title = item;
                }

                if (item.name == "Email")
                {
                    canvas.Email = item;
                }

                if (item.name == "Other")
                {
                    canvas.Other = item;
                }

                if (item.name == "Message")
                {
                    canvas.Message = item;
                }

                if (item.name == "Adress_1")
                {
                    canvas.Adress_1 = item;
                }

                if (item.name == "Adress_2")
                {
                    canvas.Adress_2 = item;
                }
                if (item.name == "Facebook")
                {
                    canvas.Facebook = item;
                }

                if (item.name == "Instagram")
                {
                    canvas.Instagram = item;
                }

            }





            canvas.Details = template;

            canvas.Details.path = savedTemplate.path;


            if (!string.IsNullOrEmpty(savedTemplate.photo_path))
            {
                canvas.Details.pic_path = savedTemplate.photo_path;
            }

            if (!string.IsNullOrEmpty(savedTemplate.logo_path))
            {
                canvas.Details.logo_path = savedTemplate.logo_path;
            }

            canvas.Details.id = savedTemplate.id.ToString();

            return canvas;




        }


        public int GetCartCount()
        {
            int id = user.GetUser().ID_User;
            return (from c in _db.Saved_Templates where c.user_id ==id  && c.completed == false
                    select c).Count();
        }

       public List<Template_Type> GetTemplatesTypesByCompany(int id)
        {
            return (from t in _db.T_Company_Type join ty in _db.Template_type on t.type equals ty.id
                    join c in _db.Template_categories on t.company equals c.id
                    where t.company==id select 
                    new Template_Type {Id=t.id, Company=c.category, Path=t.path,Type=ty.type, TypeId=ty.id, CompanyId=c.id}).ToList();
        }


        public bool SaveBillingDetails(Billing_Shipping_details info)
        {
         
            try
            {
                info.completed = false;
                info.userid = user.GetUser().ID_User;
                info.country = "United States";
                info.s_country = "United States";

                var billinglist = _db.Billing_Shipping_details.Where(b => b.completed == false && b.userid == info.userid).ToList();
                if (billinglist.Count > 0)
                {
                    _db.Billing_Shipping_details.RemoveRange(billinglist);
                }

                _db.Billing_Shipping_details.Add(info);
                if (_db.SaveChanges() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }catch(Exception e)
            {
                return false;
            }
        }

        public bool UpdateBillingDetails(string first_name, string last_name, string b_Adress, string b_suite,
        string city, string state, string zip_code, string email, string phone)
        {
            int id = user.GetUser().ID_User;
            try
            {
                Billing_Shipping_details _info = (from i in _db.Billing_Shipping_details where i.completed == false && i.userid ==id select i).FirstOrDefault();
                _info.first_name = first_name;
                _info.last_name = last_name;
                _info.b_Adress = b_Adress;
                _info.s_suite = b_suite;
                _info.city = city;
                _info.state = state;
                _info.zip_code = zip_code;
                _info.email = email;
                _info.phone = phone;

                if (_db.SaveChanges() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch(Exception e)
            {
                return false;
            }
           

        }

        public bool UpdateShippingDetails(string s_name, string s_adress, string s_suite,
       string s_city, string s_state, string s_zip_code, string s_phone)
        {
            int id = user.GetUser().ID_User;
            try
            {
                Billing_Shipping_details _info = (from i in _db.Billing_Shipping_details where i.completed == false && i.userid ==id select i).FirstOrDefault();
                _info.s_name = s_name;
                _info.s_adress = s_adress;
                _info.s_suite = s_suite;
                _info.s_city = s_city;
                _info.s_state = s_state;
                _info.s_zip_code = s_zip_code;
                _info.s_phone = s_phone;

                if (_db.SaveChanges() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<Shipping_Details> GetShippingOptions()
        {
            return  _db.Shipping_Details.ToList();
        }

        public string GetEmailFromUncompletedBilling(int id)
        {
            return _db.Billing_Shipping_details.Where(u => u.userid == id && u.completed ==false).Select(u => u.email).FirstOrDefault();
        }

        public decimal? GetOrderAmount(int id)
        {
            return _db.Saved_Templates.Where(u => u.user_id == id && u.completed == false).Sum(u => u.price);
        }

        public int GetOrderCount(int id)
        {
            return _db.Saved_Templates.Where(u => u.user_id == id && u.completed == false).Count();
        }


        public bool CreatePaymentIntent(Payment_Intent intent)
        {
            var id = user.GetUser().ID_User;
            try
            {
                var pending = _db.Payment_Intent.Where(p => p.status == OrderStatus.pending && p.user_id==id).FirstOrDefault();
                if (pending == null)
                {
                    _db.Payment_Intent.Add(intent);
                }
                else
                {
                    pending.payment_intent_id = intent.payment_intent_id;
                }
               
                if (_db.SaveChanges() > 0)
                {
                    return true;
                }

                return false;
            }catch(Exception e)
            {
                return false;
            }
        }


       public int CreateOrder(marketing_orders order, int id)
        {
            try
            {
                var ord = _db.marketing_orders.Where(o => o.status == OrderStatus.pending && o.user_id == id).FirstOrDefault();
                if (ord == null)
                {
                    _db.marketing_orders.Add(order);
                    _db.SaveChanges();
                    return order.id;
                }
                else
                {
                    ord.payment_intent_id = order.payment_intent_id;

                    if (_db.SaveChanges() > 0)
                    {
                        return ord.id;
                    }else
                    {
                        return -1;
                    }
                   
                }
                             
            }
            catch (Exception e)
            {
                return -1;
            }
        }


        public string PaymentSucceded(string id)
        {
            var userid = user.GetUser().ID_User;
            var order = _db.marketing_orders.Where(o => o.payment_intent_id == id && o.user_id==userid && o.status==OrderStatus.pending).FirstOrDefault();          
            order.status = OrderStatus.succeded;
            var intent = _db.Payment_Intent.Where(p => p.payment_intent_id == id).FirstOrDefault();
            intent.status= OrderStatus.succeded;

          
            var templates = _db.Saved_Templates.Where(s => s.user_id == userid && s.completed == false).ToList();

            foreach(var item in templates)
            {
                item.completed = true;
                item.orderid = order.id;

                var order_Details = new Marketing_order_details()
                {
                    order_id = order.id,
                    template = Convert.ToString(item.id),
                    items = item.amount,
                    unit_price = Convert.ToDecimal(item.price),
                    creation_date = DateTime.Now
                };

                _db.Marketing_order_details.Add(order_Details);

            }


            if (_db.SaveChanges() > 0)
            {              
                return id;
            }

            return "Error";
        }

        public marketing_orders GetOrderIdByPaymentIntentId(string id,int userid)
        {
           
            return _db.marketing_orders.Where(o => o.payment_intent_id == id && o.status == OrderStatus.succeded && o.user_id == userid).FirstOrDefault();                   
        }


        public List<Saved_Templates> TemplatesCompletedByOrderId(int id)
        {
           return _db.Saved_Templates.Where(t => t.completed == true && t.orderid == id).Include(t=>t.Template_dimensions).ToList();     
        }

        public List<signs_description> GetDescriptionBydimensionId(int?[] dimension)
        {
            List<signs_description> list = new List<signs_description>();
            foreach(var item in dimension)
            {
                var description = _db.signs_description.Where(s => s.dimension == item).FirstOrDefault();
                list.Add(description);
            }

            return list;


        }


        public bool CreateReceipt(Receipts receipt)
        {
            try
            {
                _db.Receipts.Add(receipt);

                if (_db.SaveChanges() > 0)
                {
                    return true;
                }

                return false;
            }
            catch(Exception e)
            {
                return false;
            }
           

        }

        public Receipts GetReceiptDetail(string id)
        {
            var order = GetOrderIdByPaymentIntentId(id,user.GetUser().ID_User);
            var receipt = _db.Receipts.Where(r => r.order_id == order.id).FirstOrDefault();
            return receipt;

        }


        public Billing_Shipping_details GetUncompletedBilling_Shipping_Details()
        {
            var userid = user.GetUser().ID_User;
            var billing = _db.Billing_Shipping_details.Where(b => b.completed == false && b.userid == userid).FirstOrDefault();
            return billing;
        }

        public Billing_Shipping_details completeBillingDetails(int id)
        {
            var billing = GetUncompletedBilling_Shipping_Details();
            billing.orderid = id;
            billing.completed = true;
            _db.SaveChanges();
            return billing;
        }

        public List<marketing_orders> GetCompletedOrders()
        {
            var userid = user.GetUser().ID_User;
            var list = _db.marketing_orders.Where(o => o.user_id == userid && o.status==OrderStatus.succeded).ToList();

            return list;
            
        }

        public bool OrderCompleted(string paymentIntent)
        {
          var userId = user.GetUser().ID_User;
          var orderId = GetOrderIdByPaymentIntentId(paymentIntent, userId).id;
          var completed =  _db.Billing_Shipping_details.Where(b => b.orderid == orderId).Select(b => b.completed).FirstOrDefault();
            if (completed == null)
            {
                completed = false;
            }
            if ((bool)completed)
            {
                return true;
            }

            return false;
        }

       public List<string> GetTemplateDimensions(string id)
        {
            var type = GetTemplateTypeById(id);

            if(type!=null && type > 0)
            {
                var list = _db.Template_dimensions.Where(d => d.type == type).Select(d => d.dimensions).ToList();

                return list;
            }
            else
            {
                return null;
            }
               

       
        }

        public int? GetTemplateTypeById(string id)
        {
          var type=  GetTemplateDetailsById(id).type;
            if (type != null && type>0)
            {
                return type;
            }
            else
            {
                return -1;
            }
        }


       public int GetDimensionId(int type, string dimension)
        {
            return _db.Template_dimensions.Where(d => d.dimensions == dimension && d.type == type).Select(d => d.id).FirstOrDefault();
        }

        public TypeInfo getTypeInfoByType(string type, int typeId)
        {

            var dimensions = _db.Template_dimensions.Where(d => d.dimensions == type && d.type==typeId).FirstOrDefault();

            var typeid = dimensions.id;

            var info = new TypeInfo();

            info.Prices = _db.template_pricing.Where(p => p.dimensions == typeid).Select(p=>p.price).ToList();
            
            info.Quantity= _db.template_pricing.Where(p => p.dimensions == typeid).Select(p => p.quantity).ToList();

            info.Description= _db.signs_description.Where(p => p.dimension == typeid).Select(p => p.description).FirstOrDefault();

            info.dimension = dimensions.id;

            return info;

        }


        public List<AmountsByType> GetPricesForSavedTemplates(int?[] dimension)
        {
            var amountXtype = new AmountsByType();
            List<AmountsByType> amounts = new List<AmountsByType>();           

          foreach(var item in dimension)
            {
                amountXtype.amounts = _db.template_pricing.Include(t => t.dimensions).Where(t => t.Template_dimensions.id == item)
                    .Select(t => t.quantity).ToArray();
                amountXtype.type = item;
                amounts.Add(amountXtype);
                amountXtype = new AmountsByType();
            }

            return amounts;
        }

        public decimal? GetTotalprice()
        {
            var userid = user.GetUser().ID_User;

            return _db.Saved_Templates.Where(t => t.user_id == userid && t.completed == false).Sum(t => t.price);
        }

        public Colors GetTemplateColorsById(string id)
        {
            var colors = new Colors();

            colors.tempcolors = (from t in _db.Template_colors
                                 where t.templateid == id
                                 select new TemplateColors
                                 {
                                     id = t.id,
                                     color = t.color,
                                     templateid = t.templateid,
                                     path = t.path
                                 }).ToList();

            colors.labcolors = (from t in _db.label_color
                                 where t.templateid == id
                                 select new LabelColors
                                 {                                     
                                     color = t.color,
                                     background=t.background,
                                     labelid=t.labelid,
                                     name=t.label_name,
                                     templateid = t.templateid                                     
                                 }).ToList();

            return colors;
        }


        public List<Template_categories> GetBrandstCategories()
        {
            return _db.Template_categories.Where(c=>c.id!=7).ToList();

        }
    }
}
using Realestate_portal.Models;
using Realestate_portal.Models.ViewModels.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realestate_portal.Services.Contracts
{
   public interface Imarket
    {
        List<Template_type> getCategories(int category, int subcategory = 0);

        List<template_subcategories> getSubcategories(int id);

        List<Template_Layout> getTemplatesLayout(int type, int company, int subcategory=0);

        Template_Details GetTemplateDetailsById(string id);

        int getCategoryIdByTemplateId(string id);
      
        Template_Layout GetTemplateLayout(string id);

         Saved_Templates getTemplateToUpdateMaterials();

        bool AddMaterials(Saved_Templates ToSave, int userid);

        bool UpdateMaterials(Saved_Templates ToSave);

         Saved_Templates getTemplateSavedToCart(string id);

         bool removedTempleteSavedInCart(string id);

        bool CompleteSavedTemplate(List<saved_labels> labels, string img, int edited,string comment, string background);

        bool CompleteSavedTemplate(List<saved_labels> labels, string img,string file,int filetype, int edited, string comment, string background);

        bool CompleteSavedTemplate(List<saved_labels> labels, string img, string photo, string logo, int edited, string comment, string background);

        TemplateCanvas GetTemplateLabels(string id);

        labels TrimLabelItem(labels label);

        CartViewModel GetViewModelForCart();

        bool deleteSavedTemplate(int id);

        SavedTemplateModel GetTemplateToUpdateMaterial(int id);

        bool EditSavedTemplateMaterials(SavedTemplateModel template);

        TemplateCanvas GetLabelsToEditSavedTemplate(int id);

        int GetCartCount();

        List<Template_Type> GetTemplatesTypesByCompany(int id);

        bool SaveBillingDetails(Billing_Shipping_details info);

        bool UpdateBillingDetails(string first_name, string last_name, string b_Adress, string b_suite,
        string city, string state, string zip_code, string email, string phone);

        bool UpdateShippingDetails(string s_name, string s_adress, string s_suite,
       string s_city, string s_state, string s_zip_code, string s_phone);

        List<Shipping_Details> GetShippingOptions();

        string GetEmailFromUncompletedBilling(int id);
        decimal? GetOrderAmount(int id);

        int GetOrderCount(int id);

        bool CreatePaymentIntent(Payment_Intent intent);

        int CreateOrder(marketing_orders order, int id);

        string PaymentSucceded(string id);

        marketing_orders GetOrderIdByPaymentIntentId(string id,int userid);

        List<Saved_Templates> TemplatesCompletedByOrderId(int id);

        bool CreateReceipt(Receipts receipt);

        Receipts GetReceiptDetail(string id);

        Billing_Shipping_details GetUncompletedBilling_Shipping_Details();

        Billing_Shipping_details completeBillingDetails(int id);

        List<marketing_orders> GetCompletedOrders();

        List<string> GetTemplateDimensions(string id);

        int? GetTemplateTypeById(string id);

        TypeInfo getTypeInfoByType(string type, int typeId);

        int GetDimensionId(int type, string dimension);

        decimal UpdateTemplatePrice(int id, int amount);

        List<AmountsByType> GetPricesForSavedTemplates(int?[] dimension);

        decimal? GetTotalprice();

        List<signs_description> GetDescriptionBydimensionId(int?[] dimension);

        Colors  GetTemplateColorsById(string id);


        List<Template_categories> GetBrandstCategories();


        bool OrderCompleted(string paymentIntent);

        string SavePDFToTemplate(string img);
    }
}

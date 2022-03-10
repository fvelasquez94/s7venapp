using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Realestate_portal.Models;
using Realestate_portal.Models.Enum;
using Realestate_portal.Services.Contracts;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Realestate_portal.Controllers
{

    public class PaymentIntentApiController : Controller
    {

        public PaymentIntentApiController()
        {
        }

        private IUser _user;
        private Imarket _market;
        private int userId;

        public PaymentIntentApiController(IUser user, Imarket market)
        {
            _user = user;
            _market = market;
            userId = _user.GetUser().ID_User;
        }
        
        public System.Web.Mvc.ActionResult Create(PaymentIntentCreateRequest request)
        {
            StripeConfiguration.ApiKey = ConfigurationManager.AppSettings["StripeKey_test"].ToString();
            var paymentIntents = new PaymentIntentService();
            var paymentIntent = paymentIntents.Create(new PaymentIntentCreateOptions
            {
                Amount = Convert.ToInt64(CalculateOrderAmount(request.Items)),
                Currency = "usd",               
                ReceiptEmail=_market.GetEmailFromUncompletedBilling(userId),                           
            });

            var orderid = _market.CreateOrder(new marketing_orders() { 
            user_id=userId,
            items= _market.GetOrderCount(userId),
            ttl_price=(decimal) CalculateOrderAmount(request.Items),
            taxes=0.00M,
            shipping = GetShippingFee(request.Items),
            creation_date=DateTime.Now,
            payment_intent_id=paymentIntent.Id,
            status=OrderStatus.pending
            },userId);

            

            Payment_Intent intent = new Payment_Intent()
            {
                payment_intent_id = paymentIntent.Id,
                price = Convert.ToDecimal(CalculateOrderAmount(request.Items)),
                user_id = userId,
                shipping = GetShippingFee(request.Items),
                status = OrderStatus.pending,
                items = _market.GetOrderCount(userId),
                orderid = orderid
            };

            _market.CreatePaymentIntent(intent);

            return Json(new { clientSecret = paymentIntent.ClientSecret });
        }
        private decimal? CalculateOrderAmount(Item[] request)
        {

           var shipping= 24.95M;
            var total = _market.GetTotalprice() + shipping;

            return total;
        }

        private decimal GetShippingFee(Item[] items)
        {
            // return items[0].Shipping;

            return 24.95M;
        }
        public class Item
        {          
            [JsonProperty("shipping")]
            public decimal Shipping { get; set; }
        }
        public class PaymentIntentCreateRequest
        {
            [JsonProperty("items")]
            public Item[] Items { get; set; }
        }
    }
}
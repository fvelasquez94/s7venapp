using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Realestate_portal.Controllers.TwilioAPI
{
    public class SendSMS
    {
        // Find your Account SID and Auth Token at twilio.com/console
        // and set the environment variables. See http://twil.io/secure
        static string accountSid = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_SID"];
        static string authToken = ConfigurationManager.AppSettings["TWILIO_AUTH_TOKEN"];
        static string phonenumber = ConfigurationManager.AppSettings["TWILIO_Phone"];
        public string sendSMSTrilio (string messagenotification, string number="")
        {

            TwilioClient.Init(accountSid, authToken);
           
            var message = MessageResource.Create(
                body: messagenotification,
                from: new Twilio.Types.PhoneNumber(phonenumber),
                to: new Twilio.Types.PhoneNumber(number)
            );

            Console.WriteLine(message.Sid);
            return message.Sid;
        }
    }
}
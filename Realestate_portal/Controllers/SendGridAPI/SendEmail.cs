using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Realestate_portal.Controllers.SendGridAPI
{

    public class SendEmail
    {
        // apik key SendGrid V.2.00
        static string apikey = ConfigurationManager.AppSettings["SendGrid_APIK"];
        static string fromemail = "support@s7ven.co";
        static string idtemplate_NewAgentBroker = "d-7d3c9591721b4fc896f0b968e76d6ff0";
        static string idtemplate_NewTask = "d-fd86398b2b164fa28421c877b5575be9";
        static string idtemplate_NewLead = "d-78f979719e6845368e7bf9c1f2cfd075";

        //New agent, new Broker
        public async Task<string> SendEmail_newAgentBroker(string toemail, string password)
        {
            try
            {
                //Enviar correo
                var sendGridClient = new SendGridClient(apikey);
                var sendGridMessage = new SendGridMessage();
                sendGridMessage.SetFrom(fromemail, "Support");
                sendGridMessage.AddTo(toemail);
                //The Template Id
                sendGridMessage.SetTemplateId(idtemplate_NewAgentBroker);
                //Body o datos variables
                sendGridMessage.SetTemplateData(new
                {
                    email = toemail,
                    pass = password
                });

                var response = await sendGridClient.SendEmailAsync(sendGridMessage);

                return "Success";
            }
            catch(Exception ex)
            {

                return "Error: " + ex.Message;
            }

        }

        //New task
        public async Task<string> SendEmail_newTask(string toemail, string title, string description)
        {
            try
            {
                //Enviar correo
                var sendGridClient = new SendGridClient(apikey);
                var sendGridMessage = new SendGridMessage();
                sendGridMessage.SetFrom(fromemail, "Support");
                sendGridMessage.AddTo(toemail);
                //The Template Id
                sendGridMessage.SetTemplateId(idtemplate_NewTask);
                //Body o datos variables
                sendGridMessage.SetTemplateData(new
                {
                    title = title,
                    description = description
                });

                var response = await sendGridClient.SendEmailAsync(sendGridMessage);

                return "Success";
            }
            catch (Exception ex)
            {

                return "Error: " + ex.Message;
            }

        }

        public async Task<string> SendEmail_newLead(string toemail, string leadname, string phone, string address, string state)
        {
            try
            {
                //Enviar correo
                var sendGridClient = new SendGridClient(apikey);
                var sendGridMessage = new SendGridMessage();
                sendGridMessage.SetFrom(fromemail, "Support");
                sendGridMessage.AddTo(toemail);
                //The Template Id
                sendGridMessage.SetTemplateId(idtemplate_NewLead);
                //Body o datos variables
                sendGridMessage.SetTemplateData(new
                {
                    leadname = leadname,
                    phone=phone,
                    address=address,
                    state=state
                });

                var response = await sendGridClient.SendEmailAsync(sendGridMessage);

                return "Success";
            }
            catch (Exception ex)
            {

                return "Error: " + ex.Message;
            }

        }
    }
}
using Postal;
using Realestate_portal.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;

namespace Realestate_portal.Services.Repositories
{
    public class EmailRepository : Controller,IDisposable, IEmail
    {

        private readonly string From = "support@s7ven.co"; 


        //emailtosend is a Viewbag property, it is a dinamic value used to render on the HTML templates,
        //and by the email system, yo must add them on the Views/Emails/template.cshtml file where
        //you will create the template as @Viewbag.To @Viewbag.subject as you have them below
        public  bool SendSimpleEmail(string Template, string To, string Subject)
        {
            try
            {
                dynamic emailtosend = new Email(Template);                
                    emailtosend.To = To;
                    emailtosend.From = this.From;
                    emailtosend.subject = Subject;
                    emailtosend.username = "heyner palacios";
                    emailtosend.Send();
                
                return true;

            } catch(Exception e)
            {
                return false;
            }
        }

        public bool JoinBrandEmail(string Template, string Subject, string name, string company, string email)
        {
            try
            {
                dynamic emailtosend = new Email(Template);
                emailtosend.To = this.From;
                emailtosend.From = this.From;
                emailtosend.subject = Subject;
                emailtosend.username = "S7ven Support";
                emailtosend.name = name;
                emailtosend.company = company;
                emailtosend.email = email;

                emailtosend.Send();

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SendSimpleEmailWithUsername(string username,string Template, string To, string Subject)
        {
            try
            {
                dynamic emailtosend = new Email(Template);
                emailtosend.To = To;
                emailtosend.From = this.From;
                emailtosend.subject = Subject;
                emailtosend.username = username;
                emailtosend.Send();

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SendEmailWithCC(string Template, string To, string Subject, string cc)
        {
            try
            {
                dynamic emailtosend = new Email(Template);                
                    emailtosend.To = To;
                    emailtosend.From = this.From;
                    emailtosend.subject = Subject;
                    emailtosend.cc = cc;
                    emailtosend.Send();
                
                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SendEmailWithFile(string Template, string To, string Subject, string Attachment)
        {
            try
            {
                dynamic emailtosend = new Email(Template);                                
                    emailtosend.To = To;
                    emailtosend.From = this.From;
                    emailtosend.subject = Subject;
                    emailtosend.Attach(new Attachment(Attachment));
                    emailtosend.Send();                
                

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SendfullEmail(string Template, string To, string cc, string Subject, string Attachment)
        {
            try
            {
                dynamic emailtosend = new Email(Template);                
                    emailtosend.To = To;
                    emailtosend.From = this.From;
                    emailtosend.subject = Subject;
                    emailtosend.cc = cc;
                    emailtosend.Attach(new Attachment(Attachment));
                    emailtosend.Send();                

                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
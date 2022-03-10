using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realestate_portal.Services.Contracts
{
    public interface IEmail
    {
        bool SendSimpleEmailWithUsername(string username,string Template, string To, string Subject);
        
        bool SendSimpleEmail(string Template,  string To, string Subject);

         bool SendEmailWithCC(string Template, string To, string Subject, string cc);

         bool SendEmailWithFile(string Template, string To, string Subject, string Attachment);

         bool SendfullEmail(string Template, string To, string cc, string Subject, string Attachment);

         bool JoinBrandEmail(string Template, string Subject, string name, string company, string email);
    }
}

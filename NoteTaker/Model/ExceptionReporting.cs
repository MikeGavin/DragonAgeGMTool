using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace Scrivener.Model
{
    public static class ExceptionReporting
    {
        public static void Email(Exception e)
        {
            // Create the Outlook application.
           var oApp = new Outlook.Application();
           // Create a new mail item.
           var oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
           oMsg.Display(false);
           // Add a recipient.
           oMsg.To = "edtechsupport@ecotoh.org";
           //Subject line
           oMsg.Subject = "[Scrivener] Exception Report.";
           //add the body of the email
           oMsg.Body = string.Format("Exception: {0}", e.ToString());
           oMsg.Attachments.Add(string.Format(@"c:\Temp\Scrivener Logs\{0}.log", DateTime.Today.ToString("yyyy-MM-dd")));
           // Send.
           ((Outlook._MailItem)oMsg).Send();
       }
    }
}


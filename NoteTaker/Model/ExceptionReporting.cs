using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace Scrivener.Model
{
    public static class ExceptionReporting
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private static string GetPublishedVersion()
        {
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                return System.Deployment.Application.ApplicationDeployment.CurrentDeployment.
                    CurrentVersion.ToString();
            }
            return string.Format("Not network deployed.{0}Executing Assembly: {1}", Environment.NewLine, Assembly.GetExecutingAssembly().GetName().Version);
        }

        public static void Email(Exception e)
        {
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                // Create the Outlook application.
                var oApp = new Outlook.Application();
                // Create a new mail item.
                var oMsg = (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);
                oMsg.Display(false);
                // Add a recipient.
                oMsg.To = "edtechsupport@ecotoh.org";
                // Add a CC
                oMsg.CC = "michael.gavin@ecotoh.org";
                //Subject line
                oMsg.Subject = string.Format("[Scrivener] {0}", e.Message);
                //add the body of the email
                oMsg.Body = string.Format("{0}{1}{2}", GetPublishedVersion(), Environment.NewLine, e.ToString());
                oMsg.Attachments.Add(string.Format(@"{1}\Scrivener\Logs\{0}.log", DateTime.Today.ToString("yyyy-MM-dd"), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
                // Send.
                ((Outlook._MailItem)oMsg).Send();
            }
            else
            {
                log.Error("Not network deployed or would have generated email for exception {0}", e);
            }
       }
    }
}


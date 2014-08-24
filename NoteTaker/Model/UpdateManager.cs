using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public class UpdateManager
    {
        public UpdateManager()
        {
            FileSystemWatcher program = new FileSystemWatcher(@"\\fs1\EdTech\Scrivener", "*.exe");
            program.NotifyFilter = NotifyFilters.LastWrite;
            program.EnableRaisingEvents = true;
            program.Changed += program_Changed;

        }

        private void program_Changed(object sender, FileSystemEventArgs e)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                var ad = ApplicationDeployment.CurrentDeployment;
                ad.CheckForUpdateCompleted += ad_CheckForUpdateCompleted;
                //ad.CheckForUpdateProgressChanged += ad_CheckForUpdateProgressChanged;
                ad.CheckForUpdateAsync();
            }
        }

        void ad_CheckForUpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void ad_CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}

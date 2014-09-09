using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public class UpdateManager
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        public event EventHandler<System.ComponentModel.AsyncCompletedEventArgs> UpdateComplete;
        private void OnUpdateComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            log.Info("Update Complete");
            if (UpdateComplete != null) { UpdateComplete(sender, e); }
        }

        private DateTime lastRead = DateTime.MinValue;
        private ApplicationDeployment ad;
        public UpdateManager()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ad = ApplicationDeployment.CurrentDeployment;
                var uri = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.UpdateLocation.LocalPath.ToLower().Replace("scrivener.application", string.Empty);
                log.Debug("Update Location: {0}", uri);
                FileSystemWatcher program = new FileSystemWatcher(uri, "*.exe");
                program.NotifyFilter = NotifyFilters.LastWrite;
                program.EnableRaisingEvents = true;
                program.Changed += program_Changed;
                ad.CheckForUpdateCompleted += ad_CheckForUpdateCompleted;
                ad.UpdateCompleted += OnUpdateComplete;
            }
        }

        private void program_Changed(object sender, FileSystemEventArgs e)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);
                if (lastWriteTime != lastRead)
                {
                    log.Debug("File Changed: {0}", e.FullPath);
                    try
                    {
                        ad.CheckForUpdateAsync();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message);
                    }
                }
            }
        }

        void ad_CheckForUpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void ad_CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            log.Info("Update Check Complete");
            if (e.UpdateAvailable == true)
            {
                log.Info("Update Available");             
                try
                {
                    ad.UpdateAsync();
                }
                catch(Exception ex)
                {
                    log.Error(ex);   
                }
            }
            log.Info("Update Unvailable");
        }

    }
}

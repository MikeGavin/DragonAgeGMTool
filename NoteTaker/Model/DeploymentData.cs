using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public class DeploymentData
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public string Mode { get; protected set; }
        public string SettingsFolder { get; protected set; }        
        public bool NetworkDeployed { get; protected set; }
        private Uri _updateLocation;
        public Uri UpdateLocation { get { return _updateLocation ?? (_updateLocation = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.UpdateLocation); } }
        private string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        public DeploymentData(string baseFolder)
        {
            DeploymentCheck(baseFolder);
            CreateSettingsFolder(SettingsFolder);
        }

        private void DeploymentCheck(string baseFolder)
        {
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                NetworkDeployed = true;                

                log.Debug("uri.LocalPath: {0}", UpdateLocation.LocalPath.ToString());

                if (UpdateLocation.LocalPath.Contains(appName + "Dev"))
                {
                    Mode = "development";
                    SettingsFolder = BuildSettingsPath(baseFolder, ".Development");
                }
                else
                {
                    Mode = "production";
                    SettingsFolder = BuildSettingsPath(baseFolder);
                }                
            }
            else
            {
                NetworkDeployed = false;
                Mode = "debug";
                SettingsFolder = BuildSettingsPath(baseFolder, ".Debug"); 
            }
        }

        private string BuildSettingsPath(string baseFolder, string branch = "")
        {
            return Path.Combine(baseFolder, string.Format("{0}{1}", appName, branch));
        }

        private void CreateSettingsFolder(string settingsFolder)
        {
            //check for settings folder. Create if missing.            
            if (!Directory.Exists(settingsFolder))
            {
                Directory.CreateDirectory(settingsFolder);
            }
        }
    }
}

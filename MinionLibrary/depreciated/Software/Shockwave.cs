using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minion.Process;

namespace Minion.Software
{
    public class Shockwave : SilentProcess
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private bool x64;

        public Shockwave(string ip, bool bits) : base(ip)
        {
            x64 = bits;
        }

        public bool Install_MSI()
        {
            log.Info("Installing Shockwave using MSI...");
            if (SilentPA(Settings.Shockwave.Default.install_msi, Settings.Shockwave.Default.msi_location, Settings.Shockwave.Default.remotedir) == true)
            {
                log.Info("Shockwave Install-MSI process compleated, verifying...");
                if (GetVersion() != "NOT INSTALLED")
                {
                    log.Info("Shockwave has been successfuly installed");
                    return true;
                }
            }
            log.Error("Shockwave MSI install failed");
            return false;
        }

        public bool Uninstall_APP()
        {
            log.Info("Uninstalling Shockwave using App...");
            if (SilentPA(Settings.Shockwave.Default.uninstall_app, Settings.Shockwave.Default.uninstaller_location, Settings.Shockwave.Default.remotedir) == true)
            {
                log.Info("Shockwave Uninstall-App process compleated, verifying...");
                if (GetVersion() == "NOT INSTALLED")
                {
                    log.Info("Shockwave has been successfuly uninstalled");
                    return true;
                }
            }
            log.Error("Shockwave App Uninstall failed");
            return false;
        }

        public bool Uninstall_WMIC()
        {
            log.Info("Uninstalling Shockwave using WMIC...");
            if (SilentPA(Settings.Shockwave.Default.uninstall_WMIC, true) == true)
            {
                log.Info("Shockwave Uninstall-WMIC process compleated, verifying...");
                if (GetVersion() == "NOT INSTALLED")
                {
                    log.Info("Shockwave has been successfuly uninstalled");
                    return true;
                }
            }
            log.Error("Shockwave WMIC Uninstall failed");
            return false;
        }

        public string GetVersion()
        {
            log.Info("Acquiring Shockwave Version...");
            Version = "Updating";
            _tools.PAExec("-s REG query \"hklm\\Software\\Adobe\\Shockwave 12\\currentupdateversion\"");

            if (_tools.StandardError.Contains("ERROR"))
            {
                Version = "NOT INSTALLED";
                log.Warn("Shockwave is not installed");
            }
            else
            {
                Version = _tools.StandardOutput.Replace("\r\r\nHKEY_LOCAL_MACHINE\\Software\\Adobe\\Shockwave 12\\currentupdateversion\r\r\n    (Default)    REG_SZ    ", string.Empty).Trim(null);
                log.Info("Shockwave version: " + Version);
            }

            return Version;
        }
    }
}
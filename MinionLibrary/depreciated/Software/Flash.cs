using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minion.Process;

namespace Minion.Software
{
    public class Flash : SilentProcess
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private bool x64;

        public Flash(string ip, bool bits) : base(ip)
        {
            x64 = bits;
        }

        public bool Install()
        {
            log.Info("Installing Flash using standard installer...");
            if (SilentPA(Settings.Flash.Default.install, true) == true)
            {
                log.Info("Flash install process compleated, verifying...");
                if (GetVersion() != "NOT INSTALLED")
                {
                    log.Info("Flash has been successfuly installed");
                    return true;
                }
            }
            log.Error("Flash install failed");
            return false;
        }

        public bool Uninstall_APP()
        {
            log.Info("Uninstalling Flash using App...");
            if (SilentPA(Settings.Flash.Default.uninstall_APP, true) == true)
            {
                log.Info("Flash Uninstall-App process compleated, verifying...");
                if (GetVersion() == "NOT INSTALLED")
                {
                    log.Info("Flash has been successfuly uninstalled");
                    return true;
                }
            }
            log.Error("Flash Uninstall-App failed");
            return false;
        }
        public bool Uninstall_WMIC()
        {
            log.Info("Uninstalling Flash using WMIC...");
            if (SilentPA(Settings.Flash.Default.uninstall_WMIC, true) == true)
            {
                log.Info("Flash Uninstall-WMIC process compleated, verifying...");
                if (GetVersion() == "NOT INSTALLED")
                {
                    log.Info("Flash has been successfuly uninstalled");
                    return true;
                }
            }
            log.Error("Flash Unistall_WMIC failed");
            return false;
        }

        public string GetVersion()
        {
            log.Info("Acquiring Flash Version...");
            Version = "Updating";
            _tools.PAExec(@"-accepteula -s REG query hklm\Software\Macromedia\FlashPlayerActiveX /v Version");
            if (_tools.StandardError.Contains("The system was unable to find the specified registry key or value"))
            {
                log.Warn("flash is not installed");
                Version = "NOT INSTALLED";
            }
            else
            {
                Version = _tools.StandardOutput.Replace("\r\r\nHKEY_LOCAL_MACHINE\\Software\\Macromedia\\FlashPlayerActiveX\r\r\n    Version    REG_SZ    ", string.Empty).Trim(null);
                log.Info("Flash version: " + Version);
            }
            return Version;
        }
    }
}
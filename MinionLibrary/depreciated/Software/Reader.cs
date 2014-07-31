using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minion.Process;

namespace Minion.Software
{
    public class Reader : SilentProcess
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private bool x64;

        public Reader(string ip, bool bits) : base(ip)
        {
            x64 = bits;
        }

        public bool Install()
        {
            log.Info("Installing Reader using standard installer...");
            if (SilentPA(Settings.Reader.Default.install, Settings.Reader.Default.Install_Location, Settings.General.Default.remote_folder) == true)
            {
                log.Info("Reader install process compleated, verifying...");
                if (GetVersion() != "NOT INSTALLED")
                {
                    log.Info("Reader has been successfuly installed");
                    return true;
                }
            }
            log.Error("Reader MSI install failed");
            return false;
        }

        public bool Uninstall_APP()
        {
            log.Info("Uninstalling Reader using App...");
            if (SilentPA(Settings.Reader.Default.uninstall_APP, Settings.Reader.Default.Uninstall_Location, Settings.General.Default.remote_folder) == true)
            {
                log.Info("Reader Uninstall-App process compleated, verifying...");
                if (GetVersion() == "NOT INSTALLED")
                {
                    log.Info("Reader has been successfuly uninstalled");
                    return true;
                }
            }
            log.Error("Reader Uninstall-App failed");
            return false;
        }
        public bool Uninstall_WMIC()
        {
            log.Info("Uninstalling Reader using WMIC...");
            if (SilentPA(Settings.Reader.Default.uninstall_WMIC, true) == true)
            {
                log.Info("Reader Uninstall-WMIC process compleated, verifying...");
                if (GetVersion() == "NOT INSTALLED")
                {
                    log.Info("Reader has been successfuly uninstalled");
                    return true;
                }
            }
            log.Error("Reader Unistall_WMIC failed");
            return false;
        }

        public string GetVersion()
        {
            string result = string.Empty;
            string argument = string.Empty;
            log.Info("Acquiring Reader Version...");
            Version = "Updating";
            //_tools.Copy(Settings.General.Default.sigcheck, Settings.General.Default.remote_folder);
            if (x64 == true)
                argument = @"Program Files (x86)";
            else
                argument = @"Program Files";

            _tools.PAExec(string.Format(@"wmic datafile where name='C:\\{0}\\Adobe\\Reader 11.0\\Reader\\AcroRd32.exe' get version", argument));
            if (_tools.StandardOutput.Contains("No Instance(s) Available"))
            {
                log.Debug("Reader 11 is not installed");
                _tools.PAExec(string.Format(@"wmic datafile where name='C:\\{0}\\Adobe\\Reader 10.0\\Reader\\AcroRd32.exe' get version", argument));
                if (_tools.StandardOutput.Contains("No Instance(s) Available"))
                {
                    log.Debug("Reader 10 is not installed");
                    result = "NOT INSTALLED";
                    log.Warn("Reader is not installed");
                }
                else
                {
                    result = _tools.StandardOutput.Remove(0,7).Trim(null);
                    log.Info("Reader version: " + result);
                }
            }
            else
            {
                result = _tools.StandardOutput.Remove(0, 7).Trim(null);
                log.Info("Reader version: " + result);
            }
            Version = result;
            return Version;
        }
    }
}
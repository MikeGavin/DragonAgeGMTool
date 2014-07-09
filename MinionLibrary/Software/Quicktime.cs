using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minion.Process;

namespace Minion.Software
{
    public class Quicktime : SilentProcess
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private bool x64;
        private string iP;

        public Quicktime(string ip, bool bits) : base(ip)
        {
            x64 = bits;
            iP = ip;
        }

        public bool Install()
        {
            //_tools.Copy(@"\\fs1\helpdesk\programs\quicktime\QuickTime.msi",@"\quicktime\QuickTime.msi");

            SilentPS(@"-i -s -h -accepteula msiexec /i c:\temp\QuickTime.msi /qn DESKTOP_SHORTCUTS=NO SCHEDULE_ASUW=0 ASUWINSTALLED=0", @"\\fs1\helpdesk\programs\quicktime\QuickTime.msi", Settings.General.Default.remote_folder);
            SilentPS(@"-i -s -h -accepteula msiexec /i c:\temp\AppleSoftwareUpdate.msi /qn", @"\\fs1\helpdesk\programs\quicktime\AppleSoftwareUpdate.msi", Settings.General.Default.remote_folder);
            SilentPS(@"-i -s -h -accepteula msiexec /i c:\temp\AppleApplicationSupport.msi /qn", @"\\fs1\helpdesk\programs\quicktime\AppleApplicationSupport.msi", Settings.General.Default.remote_folder);

            //_tools.PSExec(@"-i -s -h -accepteula msiexec /i c:\temp\quicktime\QuickTime.msi /qn DESKTOP_SHORTCUTS=NO SCHEDULE_ASUW=0 ASUWINSTALLED=0");
            //_tools.PSExec(@"-i -s -h -accepteula msiexec /i c:\temp\quicktime\AppleSoftwareUpdate.msi /qn");
            //_tools.PSExec(@"-i -s -h -accepteula msiexec /i c:\temp\quicktime\AppleApplicationSupport.msi /qn");

            log.Info("Disabling Apple update...");
            _tools.GenericProcess(@"c:\windows\sysnative\schtasks.exe", @"/delete /s \\" + iP + @" /tn ""Apple\AppleSoftwareUpdate"" /f");

                log.Info("Quicktime Install process compleated, verifying...");
                if (GetVersion() != "NOT INSTALLED")
                {
                    log.Info("Quicktime has been successfuly installed");
                    return true;
                }
            
            log.Error("Quicktime MSI install failed");
            return true;
        }

        //public bool Uninstall_APP()
        //{
        //    log.Info("Uninstalling Quicktime using App...");
        //    if (SilentPA(Settings.Quicktime.Default.uninstall_app, Settings.Quicktime.Default.uninstaller_location, Settings.Quicktime.Default.remotedir) == true)
        //    {
        //        log.Info("Quicktime Uninstall-App process compleated, verifying...");
        //        if (GetVersion() == "NOT INSTALLED")
        //        {
        //            log.Info("Quicktime has been successfuly uninstalled");
        //            return true;
        //        }
        //    }
        //    log.Error("Quicktime App Uninstall failed");
        //    return false;
        //}

        public bool Uninstall_WMIC()
        {
            log.Info("Uninstalling Quicktime using WMIC...");
            if (SilentPA(@"-accepteula -realtime -h wmic product where ""vendor like 'Apple%%'"" call uninstall /nointeractive", true) == true)
            {
                log.Info("Quicktime Uninstall-WMIC process compleated, verifying...");
                if (GetVersion() == "NOT INSTALLED")
                {
                    log.Info("Quicktime has been successfuly uninstalled");
                    return true;
                }
            }
            log.Error("Quicktime WMIC Uninstall failed");
            return false;
        }

        public string GetVersion()
        {
            string result = string.Empty;
            string argument = string.Empty;
            log.Info("Acquiring Quicktime Version...");
            Version = "Updating";
            //_tools.Copy(Settings.General.Default.sigcheck, Settings.General.Default.remote_folder);
            if (x64 == true)
                argument = @"Program Files (x86)";
            else
                argument = @"Program Files";

            //_tools.PAExec(string.Format(@"-accepteula -h c:\temp\sigcheck.exe -accepteula -n ""C:\{0}\Quicktime\QuickTimePlayer.exe""", argument));
            _tools.PAExec(string.Format(@"wmic datafile where name='C:\\{0}\\Quicktime\\QuickTimePlayer.exe' get version", argument));
            if (_tools.StandardError.Contains("No Instance(s) Available."))
            {
                result = "NOT INSTALLED";
                log.Warn("Quicktime is not installed");
            }
            else
            {
                result = _tools.StandardOutput.Remove(0,7).Trim();
                log.Info("Quicktime version: " + result);
            }

            Version = result;
            return Version;
        }
    }
}
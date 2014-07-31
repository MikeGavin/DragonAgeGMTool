using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minion.Process;
using System.Collections.ObjectModel;

namespace Minion.Software
{
    public class Java : SilentProcess
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private bool x64;
        
        public Java(string ip, bool bits) : base(ip)
        {
            x64 = bits;
            Kills.Add("javaws.exe");
            Kills.Add("jusched.exe");
        }

        public bool Install_MSI(string Version)
        {
            string test = string.Empty;
            bool result = false;
            log.Info(string.Format("Installing Java {0} using MSI...",Version));
            if (Version == "55")
            {
                result = SilentPA(Settings.Java.Default.MSI_55_Argument, Settings.Java.Default.MSI_55_Location, Settings.Java.Default.MSI_55_Target);
                Java51Plus_FIX();
            }
            if (Version == "51")
            {
                result = SilentPA(Settings.Java.Default.MSI_51_Argument, Settings.Java.Default.MSI_51_Location, Settings.Java.Default.MSI_51_Target);
                Java51Plus_FIX();
            }
            if (Version == "45")
                result = SilentPA(Settings.Java.Default.MSI_45_Argument, Settings.Java.Default.MSI_45_Location, Settings.Java.Default.MSI_45_Target);
            if (Version == "40")
                result = SilentPA(Settings.Java.Default.MSI_40_Argument, Settings.Java.Default.MSI_40_Location, Settings.Java.Default.MSI_40_Target);

            
            if (result == true)
            {              
                log.Info("Java MSI Install process complete, verifying");
                test = GetVersion();
                if (test != "NOT INSTALLED" && test != "ERROR")
                {
                    log.Info(String.Format("Java {0} successfully installed", Version));
                    return true;
                }
            }
            log.Error(string.Format("Install-{0}-MSI failed", Version));
            return false;
        }

        public bool Install_Manual(string Version)
        {
            log.Info("Starting Java Manual Install, complete process with user to continue...");
            string test;
            string command = string.Empty;
            if (Version == "51")
            {
                command = Settings.Java.Default.Install_51_Manual;
                Java51Plus_FIX();
            }
            if (Version == "45")
                command = Settings.Java.Default.Install_45_Manual;
            if (Version == "40")
                command = Settings.Java.Default.Install_40_Manual;

            if (SilentPA(command, true) == true)
            {
                log.Info("Java Manual Install process complete, verifying...");
                test = GetVersion();
                if (test != "NOT INSTALLED" && test != "ERROR")
                {
                    log.Info(String.Format("Java {0} successfully installed", Version));
                    return true;
                }
            }
            log.Error(string.Format("Install-{0}-MSI failed", Version));
            return false;
        }


        public bool Uninstall_RA()
        {
            string temp;
            log.Info("EXTERMINATING Java using JavaRA...");
            Uninstall_FIX();
            if (x64 == true)
            {
                log.Error("JavaRA will not work on 64 bit systems at this time.");
                return false;
            }
            else
                temp = Settings.Java.Default.RA_Uninstall;
            
            if (SilentPA(temp, Settings.Java.Default.RA_Location, Settings.Java.Default.RA_Target) == true)
            {
                //SilentPS(Settings.Java.Default.RA_Purge, false);
                //SilentPS(Settings.Java.Default.RA_Purge, false);
                log.Info("Java Uninstall-RA process complete, verifying...");
                if (GetVersion() == "NOT INSTALLED")
                {
                    log.Info("Java successfully uninstalled");
                    return true;
                }
            }
            log.Error("Uninstall-RA process failed");
            return false;
        }

        public bool Uninstall()
        {
            bool result = false;

            //check installed version and uninstall it.
            if (Version.Contains("55") == true)
            {
                log.Info("Uninstalling Java 55");
                result = SilentPA(Settings.Java.Default.uninstall_55, true);
            }
            else if (Version.Contains("51") == true)
            {
                log.Info("Uninstalling Java 51");
                result = SilentPA(Settings.Java.Default.uninstall_51, true);
            }
            else if (Version.Contains("45") == true)
            {
                log.Info("Uninstalling Java 45");
                result = SilentPA(Settings.Java.Default.uninstall_45, true);
            }
            else if (Version.Contains("40") == true)
            {
                log.Info("Uninstalling Java 40");
                result = SilentPA(Settings.Java.Default.uninstall_40, true);
            }
            else if (Version.Contains("25") == true)
            {
                log.Info("Uninstalling Java 25");
                result = SilentPA(Settings.Java.Default.uninstall_25, true);
            }
            else if (Version.Contains("21") == true)
            {
                log.Info("Uninstalling Java 21");
                result = SilentPA(Settings.Java.Default.uninstall_21, true);
            }
            else if (Version.Contains("17") == true)
            {
                log.Info("Uninstalling Java 17");
                result = SilentPA(Settings.Java.Default.uninstall_17, true);
            }
            else if (Version.Contains("16") == true)
            {
                log.Info("Uninstalling Java 16");
                result = SilentPA(Settings.Java.Default.uninstall_16, true);
            }
            else if (Version.Contains("15") == true)
            {
                log.Info("Uninstalling Java 15");
                result = SilentPA(Settings.Java.Default.uninstall_15, true);
            }
            else if (Version.Contains("14") == true)
            {
                log.Info("Uninstalling Java 14");
                result = SilentPA(Settings.Java.Default.uninstall_14, true);
            }
            else if (Version.Contains("13") == true)
            {
                log.Info("Uninstalling Java 13");
                result = SilentPA(Settings.Java.Default.uninstall_13, true);
            }
            else if (Version.Contains("12") == true)
            {
                log.Info("Uninstalling Java 12");
                result = SilentPA(Settings.Java.Default.uninstall_12, true);
            }
            else if (Version.Contains("11") == true)
            {
                log.Info("Uninstalling Java 11");
                result = SilentPA(Settings.Java.Default.uninstall_11, true);
            }
            else if (Version.Contains("10") == true)
            {
                log.Info("Uninstalling Java 10");
                result = SilentPA(Settings.Java.Default.uninstall_10, true);
            }
            //if no installed version listing search using WMIC
            else
            {
                log.Info("No GUDI string availalbe for specific uninstall. Using WMIC SEARCH... This can take a while...");
                Uninstall_FIX();
                result = SilentPA(Settings.Java.Default.uninstall_SEARCH, true);
            }


            //result evaulation
            if (result == true)
            {
                log.Info("Java Uninstall-WMIC process complete, verifying uninstall");
                if (GetVersion() == "NOT INSTALLED")
                {
                    log.Info("Java successfully uninstalled");
                    return true;
                }
            }
            log.Error("Uninstall-WMIC process failed");
            return false;
            
        }

        private void Java51Plus_FIX()
        {
            log.Info(string.Format("Running deployment fixes", Version));
            //special exception list for colaberate when using java 51
            if (Version.Contains("51"))
            {   
                _tools.PAExec(@"-accepteula -to 30 -s c:\temp\Java_51_MSI\DeploymentFix.bat");
            }
            if (Version.Contains("55"))
            {
                _tools.PAExec(@"-accepteula -to 30 -s c:\temp\Java_55_MSI\DeploymentFix.bat");
            }
        }

        private void Uninstall_FIX()
        {
            //special for successful java uninstall if install is broken
            string temp;
            string From = @"\\fs1\Helpdesk\TOOLS\Software\Java\installer.dll";
            if (x64 == true) //Check both program files on remote for java.
            {   
                
                temp = @"\Program Files (x86)\Java\jre7\bin";
            }
            else
            {
                temp = @"\Program Files\Java\jre7\bin";
            }
            log.Info("Pushing necessaary DLL to allow uninstalling corrupt installs. User still may get error box they must click ok on.");
            _tools.Copy(From, temp);
        }

        public string GetVersion()
        {
            Version = "Updating";
            log.Info("Acquiring Java Version...");
            string result = "0";
            //RunKills = false;
            string command;
            if (x64 == true)
            {
                command = (@"""c:\Program Files (x86)\Java\jre7\bin\java.exe"" -version");
            }
            else
            {
                command = (@"""c:\Program Files\Java\jre7\bin\java.exe"" -version");
            }

            if ( _tools.PAExec(command) == true)
            {
                bool loop = true;
                while (loop == true)
                {
                    if (_tools.StandardError.Contains("java version") == true)
                    {
                        result = _tools.StandardError.Split(new char[] { '\"', '\"' })[1];
                        log.Info("Java version: " + result);
                        loop = false;
                    }
                    else
                    {
                        log.Debug("Version lookup failed, trying again");
                        _tools.PSExec(command);
                    }
                }
            }
            
            else if (_tools.StandardError.Contains("-9"))
            {
                result = "NOT INSTALLED";
                log.Warn("Java is not installed");
            }
            else
            {
                result = "ERROR";
                log.Warn("Java version lookup returned error");
            }
            Version = result;
            return Version;
        }


    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Reflection;
using System.Data.OleDb;

namespace Minion
{
    public class RemoteMachine : INotifyPropertyChanged //Class is used to sstore and process properties for remote machines
    {

        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private string _LibraryVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string LibraryVersion { get { return _LibraryVersion  ; } }
       

        public RemoteMachine()
        {
            //var tool = new Tools(ip);
            //bool ipv4 = tool.IPv4_Check();
            //bool ping = tool.PingTest();
            //if ((ipv4 == false) | (ping = false))
            //    throw new ArgumentOutOfRangeException(@"[Invalid IP Address]");
            //else
                //IP = ip;         
 
            //string connectionString = @"Provider=Microsoft Office 12.0 Access Database Engine OLE DB Provider;" + string.Format(@"Data source= {0}\Resources\software.accdb", Environment.CurrentDirectory);

            //string queryString = "SELECT * FROM [SQL Agent Unique ID Test Load]";
            //try
            //{
            //    using (OleDbConnection connection = new OleDbConnection(connectionString))
            //    {
            //        OleDbCommand command = new OleDbCommand(queryString, connection);
            //        connection.Open();
            //        OleDbDataReader reader = command.ExecuteReader();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //    log.Fatal(ex);
            //}
     
           
        }
        
        #region Property Changed

        internal void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }

        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region PC Properties

        private string _IP;
        public string IP { get { return _IP; } set { _IP = value; RaisePropertyChanged(); } }
        private string _Name;
        public string Name { get { return _Name; } set { _Name = value; RaisePropertyChanged(); } }
        private string _ChipStyle;
        public string ChipStyle { get { return _ChipStyle; } set { _ChipStyle = value; RaisePropertyChanged(); } }
        private string _RAM;
        public string RAM { get { return _RAM; } set { _RAM = value; RaisePropertyChanged(); } }
        private string _OSBit;
        public string OSBit { get { return _OSBit; } set { _OSBit = value; RaisePropertyChanged(); } }
        private string _IEVersion;
        public string IEVersion { get { return _IEVersion; } set { _IEVersion = value; RaisePropertyChanged(); } }
        private bool x64;
        private Tools _tools;
        private Software.Java _Java;
        public Software.Java Java { get { return _Java; } set { _Java = value; RaisePropertyChanged(); } }
        private Software.Flash _Flash;
        public Software.Flash Flash { get { return _Flash; } set { _Flash = value; RaisePropertyChanged(); } }
        private Software.Shockwave _Shockwave;
        public Software.Shockwave Shockwave { get { return _Shockwave; } set { _Shockwave = value; RaisePropertyChanged(); } }
        private Software.Reader _Reader;
        public Software.Reader Reader { get { return _Reader; } set { _Reader = value; RaisePropertyChanged(); } }
        private Software.Quicktime _Quicktime;
        public Software.Quicktime Quicktime { get { return _Quicktime; } set { _Quicktime = value; RaisePropertyChanged(); } }


        #endregion

        #region ECOT Properties

        private string _CurrentUser;
        public string CurrentUser { get { return _CurrentUser; } set { _CurrentUser = value; RaisePropertyChanged(); } }
        private string _BOD;
        public string BOD { get { return _BOD; } set { _BOD = value; RaisePropertyChanged(); } }
        private string _Image;
        public string Image { get { return _Image; } set { _Image = value; RaisePropertyChanged(); } }
        private string _VPN;
        public string VPN { get { return _VPN; } set { _VPN = value; RaisePropertyChanged(); } }

        #endregion

        private static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        private string GetVPN(string ip)
        {
            
            int n = 0;
            int c = 1;
            int vpncount = 1;
            while (n <= 31)
            {
                if (ip.Contains(string.Format("10.39.{0}.", n)))
                {
                    VPN = string.Format("0{0}", vpncount.ToString());
                    return VPN;
                }
                else
                {
                    n++;
                    if (c++ == 4)
                    {
                        c = 1;
                        vpncount++;
                    }
                }
            }
            VPN = "N/A";
            return VPN;
        }

        public bool GetInfo(string TargetIP)
        {

            var tool = new Tools(TargetIP);
            log.Info("Verifying IP Address...");
            if ((tool.IPv4_Check() == false) || (tool.PingTest() == false))
            {
                return false;
            }
            else
            {
                VPN = RAM = OSBit = Image = BOD = IP = CurrentUser = Name = "Updating";
                IP = TargetIP;
                log.Info("Acquiring VPN info");
                GetVPN(IP);
                log.Debug("VPN: " + VPN);

                _tools = new Tools(IP);
                log.Info("Acquiring remote machine properities...");
                _tools.PAExec(@"-accepteula -s -realtime wmic computersystem get systemtype, username, totalphysicalmemory /format:csv");

                string Info = _tools.StandardOutput.Replace("\r\r\r\nNode,SystemType,TotalPhysicalMemory,UserName\r\r\r\n", string.Empty).Trim(null);
                string[] InfoArray = Info.Split(',');
                Name = InfoArray[0];
                ChipStyle = InfoArray[1];
                RAM = BytesToString(Convert.ToInt64(InfoArray[2]));
                CurrentUser = InfoArray[3].ToLower().Replace("ecotoh\\", string.Empty);
                log.Debug(string.Format("Name: {0} ChipStyle: {1} RAM: {2} CurrentUser {3}", Name, ChipStyle, RAM, CurrentUser));
                _tools.PAExec(@"-accepteula -s -realtime wmic os get OSArchitecture /format:csv");
                Info = _tools.StandardOutput.Replace("\r\r\r\nNode,OSArchitecture\r\r\r\n", string.Empty).Trim(null);
                InfoArray = Info.Split(',');
                OSBit = InfoArray[1];
                log.Debug(string.Format("OSBit: {0}", OSBit));
                if (OSBit == "64-bit") { x64 = true; }
                else { x64 = false; }

                log.Info("Acquiring remote machine properities...");

                if (Name.Contains("ECT-") == true) //Test for student machine only. If not student skip as there is no version or bod for teachers.
                {
                    log.Info("Acquiring Image information...");
                    _tools.PAExec(@"-accepteula -s -dfr REG query hklm\Software\ecot /v ""Image Version""");
                    Image = _tools.StandardOutput.Replace("\r\r\nHKEY_LOCAL_MACHINE\\Software\\ecot\r\r\n    Image Version    REG_SZ    ", string.Empty).TrimEnd(null);
                    log.Debug("Image: " + Image);

                    log.Info("Acquiring BOD...");
                    string f = @"\\" + IP + @"\c$\Image_Files\Bginfo\Born on date.txt";
                    using (StreamReader r = new StreamReader(f, System.Text.Encoding.ASCII))
                    {
                        BOD = r.ReadToEnd();
                        log.Info("BOD:" + BOD);
                    }
                }
                else
                {
                    log.Info("Not a student PC");
                    BOD = "N/A";
                    Image = "N/A";
                }

                _tools.PAExec(@"WMIC DATAFILE WHERE ""Name='c:\\program files\\internet explorer\\iexplore.exe'"" GET Version");
                IEVersion = _tools.StandardOutput.Replace("Version          \r\r\r\n", string.Empty).Trim();
                log.Debug("IE: " + IEVersion);
                Java = new Software.Java(IP, x64);
                Flash = new Software.Flash(IP, x64);
                Shockwave = new Software.Shockwave(IP, x64);
                Reader = new Software.Reader(IP, x64);
                Quicktime = new Software.Quicktime(IP, x64);
            }

            //Java.GetVersion();
            //Flash.GetVersion();
            //Shockwave.GetVersion();

            return true;
        }

        public bool OpenDameware()
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = (@"C:\Program Files\SolarWinds\DameWare Mini Remote Control 10.0 x64\DWRCC.exe"),
                Arguments = (@"-c: -h: -a:1 -x -m:" + IP),
            };

            if (System.IO.File.Exists(@"C:\Program Files\SolarWinds\DameWare Mini Remote Control 10.0 x64\DWRCC.exe"))
            {
                log.Info("Launching Dameware 10 x64");
                System.Diagnostics.Process.Start(startInfo);
            }
            else if (System.IO.File.Exists(@"C:\Program Files\SolarWinds\DameWare Mini Remote Control 9.0\DWRCC.exe"))
            {
                log.Info("Launching Dameware 9");
                startInfo.FileName = @"C:\Program Files\SolarWinds\DameWare Mini Remote Control 9.0\DWRCC.exe";
                System.Diagnostics.Process.Start(startInfo);

            }
            else if (System.IO.File.Exists(@"C:\Program Files\SolarWinds\DameWare Mini Remote Control 8.0\DWRCC.exe"))
            {
                log.Info("Launching Dameware 8");
                startInfo.FileName = @"C:\Program Files\SolarWinds\DameWare Mini Remote Control 8.0\DWRCC.exe";
                System.Diagnostics.Process.Start(startInfo);
            }
            else
            {
                log.Error("Could Not Find DameWare installed!");
                MessageBox.Show("Could Not Find DameWare!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public void OpenCShare()
        {
            log.Info("Opening C-Share");
            try
            {
                System.Diagnostics.Process.Start("explorer", @"\\" + IP + @"\c$\");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void OpenHDrive()
        {
            log.Info(string.Format("Opening H drive for {0}", CurrentUser));
            try
            {
                System.Diagnostics.Process.Start("explorer", @"\\fs2\students\" + CurrentUser);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }

        public void JNLPAssoication()
        {
            log.Info("Trying JNLP fix, this will add javaws.exe to the list but will not override another current default");
            try
            {
                _tools.PAExec(@"cmd /c assoc .jnlp=<JNLPFILE>");
                if (x64 == true) 
                _tools.PAExec(@"ftype jnlpfile=""c:\Program Files (x86)\Java\jre7\bin\javaws.exe"" ""%1""");
                else
                _tools.PAExec(@"ftype jnlpfile=""c:\Program Files\Java\jre7\bin\javaws.exe"" ""%1""");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void FixBackGround()
        {
            //_tools.PSExec(@"reg add HKU\.DEFAULT\Software\Sysinternals\BGInfo /v EulaAccepted /t REG_DWORD /d 1 /f");
            _tools.PAExec(@"-i 1 -s -accepteula c:\image_files\bginfo\bginfo.exe /nolicprompt c:\image_files\bginfo\wallpaper.bgi /timer:0 /silent");
            //Fix theme too
            _tools.PAExec(@"-i 1 -s -accepteula cmd.exe /c c:\windows\resources\themes\ecot.theme");
            log.Info("Ran background fix");
        }

        public void FileCleanup()
        {
            log.Info("Starting remote file cleanup");
            //int filenumber, directorynumber, errornumber;
            //filenumber = directorynumber = errornumber = 0;
            string f = @"resources\FileDeleteList.txt";

            // 1
            // Declare new List.
            List<string> lines = new List<string>();

            // 2
            // Use using StreamReader for disposing.
            using (StreamReader r = new StreamReader(f, System.Text.Encoding.ASCII))
            {
                // 3
                // Use while != null pattern for loop
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    // 4
                    // Insert logic here.
                    // ...
                    // "line" is a line in the file. Add it to our List.
                    if (line.Contains("USERNAME")) { line = line.Replace("USERNAME", CurrentUser); }
                    lines.Add(string.Format("cmd /C RMDIR /S /Q {0}", line));
                }
            }

            // 5
            // Use lines to delete remote dirs.
            foreach (string s in lines)
            {
                log.Info("Deleting remote folder: " + s.Replace("cmd /C RMDIR /S /Q ", string.Empty));
                _tools.PAExec(s);
            }
            _tools.Copy(@"\\fs1\HelpDesk\SHORTCUTS\Clear_IE_Cache.lnk", string.Format(@"\Users\{0}\Desktop\", CurrentUser));
            log.Info("File Cleanup Complete, run 'Clear_IE_Cache' on desktop");
        }

        public void ProfileWipe()
        {
            _tools.Copy(@"\\fs1\HelpDesk\TOOLS\3rdParty\Delprof2 1.5.4", @"\temp\Delprof2_1.5.4\");
            _tools.GenericProcess(@"c:\windows\sysnative\schtasks.exe", @"/delete /s \\" + IP + @" /tn ""Profile wipe"" /f");
            _tools.GenericProcess(@"c:\windows\sysnative\schtasks.exe", @"/delete /s \\" + IP + @" /tn ""remove wipe"" /f");
            log.Info("Creating Tasks for profile wipe...");
            _tools.GenericProcess(@"c:\windows\sysnative\schtasks.exe", @"/create /s \\" + IP + @" /sc onstart /delay 0000:10 /rl HIGHEST /ru SYSTEM /tn ""Profile wipe"" /tr ""c:\temp\Delprof2_1.5.4\delprof2.exe /u /id:""" + CurrentUser);
            _tools.GenericProcess(@"c:\windows\sysnative\schtasks.exe", @"/create /s \\" + IP + @"  /sc onlogon /ru SYSTEM /tn ""remove wipe"" /tr ""c:\temp\Delprof2_1.5.4\remove.bat""");
        }

        public void ProfileBackup()
        {
            log.Info(@"Backuping up \Documents & \Desktop stuff for " + CurrentUser);
            _tools.PAExec(string.Format(@"robocopy c:\users\{0}\Desktop c:\temp\profilebackup\{0}\Desktop /E /R:0", CurrentUser));
            _tools.PAExec(string.Format(@"robocopy c:\users\{0}\Documents c:\temp\profilebackup\{0}\Documents /E /R:0", CurrentUser));
        }

        public void Reboot()
        {
            log.Info(string.Format("Rebooting {0}", Name));
            _tools.PSShutdown(@" -r -t 01 -f");
        }
    }
}

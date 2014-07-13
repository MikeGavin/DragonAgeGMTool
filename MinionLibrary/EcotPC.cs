using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net;
using System.Threading;
using Minion.Lists;
using System.IO;


namespace Minion
{
    public class EcotPC : INotifyPropertyChanged, Minion.IEcotPC
    {
        //Logging System
        protected static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
              
        private List<Lists.EcotPCCommand> CommandList = new List<Lists.EcotPCCommand>() 
        { 
            new Lists.EcotPCCommand() { Name = "MachineInfo", Tool = "PAExec", Command = @"-accepteula -s -realtime wmic computersystem get systemtype, username, totalphysicalmemory /format:csv" },
            new Lists.EcotPCCommand() { Name = "Bittness", Tool = "PAExec", Command = @"-accepteula -s -realtime wmic os get OSArchitecture /format:csv" },
            new Lists.EcotPCCommand() { Name = "IE", Tool = "PAExec", Command = @"WMIC DATAFILE WHERE ""Name='c:\\program files\\internet explorer\\iexplore.exe'"" GET Version" },
            new Lists.EcotPCCommand() { Name = "Image", Tool = "PAExec", Command = @"-accepteula -s -dfr REG query hklm\Software\ecot /v ""Image Version""" },                   
        };

        #region Constructors and Deconstructors
        public EcotPC(IPAddress ipaddress)
        {
            if (Tool.IP.Ping(ipaddress) == true)
                _IPAddress = ipaddress;
            else
            {
                History = "IP is unreachable";
                throw new Exception("IP is unreachable");
            }

            #region Removed Database Code
            //SQLiteConnection minion = new SQLiteConnection(string.Format(@"Data Source={0}\Resources\Minion.sqlite", Environment.CurrentDirectory));
            //SQLiteCommand pullall = new SQLiteCommand();
            //pullall.CommandText = "SELECT * FROM Version";
            //pullall.Connection = minion;

            //try
            //{
            //    log.Debug(string.Format("Opening Database: {0}", minion.ConnectionString.ToString()));
            //    minion.Open();
            //    SQLiteDataReader reader = pullall.ExecuteReader();
            //    while (reader.Read())
            //        CommandList.Add(new Lists.EcotPCCommand() { Name = reader["Name"].ToString(), Tool = reader["Tool"].ToString(), Command = reader["Command"].ToString() });
            //    minion.Close();
            //    log.Debug(string.Format("Closing Database: {0}", minion.ConnectionString.ToString()));
            //}
            //catch (Exception e)
            //{
            //    log.Fatal(e);
            //}
            #endregion

            var token = tokenSource.Token;
            Task PingTesting = new Task( () => PingTest(token), token, TaskCreationOptions.LongRunning);
            PingTesting.Start();
            Task.Run(() => Get_MachineInfo());
            Task.Run(() => Get_OSBitness());
        }

        ~EcotPC()
        {
           // tokenSource.Cancel();
        }

        #endregion

        #region Helpers & Constant Tasks
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        protected void PingTest(CancellationToken token)
        {
            System.Net.NetworkInformation.Ping pingClass = new System.Net.NetworkInformation.Ping();
            
            while (token.IsCancellationRequested == false)
            {              
                System.Net.NetworkInformation.PingReply pingReply = pingClass.Send(IPAddress);
                //if ip is valid run checks else
                if (pingReply.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    if (IsOnline != true)
                    {
                        IsOnline = true;
                        History = string.Format("{0} is online!", IPAddress.ToString());
                        log.Info("{0} is online!", IPAddress.ToString());
                        RaiseOnlineChanged(EventArgs.Empty);
                    }
                }
                else
                {
                    if (IsOnline == true)
                    {
                        IsOnline = false;
                        History = string.Format("{0} has gone offline!", IPAddress.ToString());
                        log.Warn(string.Format("{0} has gone offline!", IPAddress.ToString()));
                        RaiseOnlineChanged(EventArgs.Empty);

                    }
                }
                Thread.Sleep(5000);
            }
        }

        protected static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        #endregion

        #region Events

        internal void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }

        }
        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaiseOnlineChanged(EventArgs e)
        {
            if (OnlineChanged != null) { OnlineChanged(this, e); }

        }
        public event EventHandler OnlineChanged;

        protected string _History;
        public string History { get { return _History; } protected set { _History += value + Environment.NewLine; RaisePropertyChanged(); } }

        #endregion

        #region Machine Properties
        protected IPAddress _IPAddress;
        public IPAddress IPAddress { get { return _IPAddress; } protected set { _IPAddress = value; RaisePropertyChanged(); } }

        protected string _Name;
        public string PCName { get { return _Name; } protected set { _Name = value; RaisePropertyChanged(); } }

        protected string _CurrentUser;
        public string CurrentUser { get { return _CurrentUser; } protected set { _CurrentUser = value; RaisePropertyChanged(); } }

        protected string _ChipStyle;
        public string ChipStyle { get { return _ChipStyle; } set { _ChipStyle = value; RaisePropertyChanged(); } }
        
        protected string _RAM;
        public string RAM { get { return _RAM; } set { _RAM = value; RaisePropertyChanged(); } }

        protected string _OSBit;
        public string OSBit { get { return _OSBit; } set { _OSBit = value; RaisePropertyChanged(); } }

        protected bool x64;

        protected bool _IsStaff;
        public bool IsStaff { get { return _IsStaff; } protected set { _IsStaff = value; RaisePropertyChanged(); } }
        
        protected bool _IsOnline;
        public bool IsOnline { get { return _IsOnline; } protected set { _IsOnline = value; RaisePropertyChanged(); } }
        #endregion

        #region Student Image Properties

        private string _BOD;
        public string BOD { get { return _BOD; } set { _BOD = value; RaisePropertyChanged(); } }
        private string _Image;
        public string Image { get { return _Image; } set { _Image = value; RaisePropertyChanged(); } }
        private string _VPN;
        public string VPN { get { return _VPN; } set { _VPN = value; RaisePropertyChanged(); } }

        #endregion

        #region Software Properties
        protected string _IEVersion;
        public string IEVersion { get { return _IEVersion; } set { _IEVersion = value; RaisePropertyChanged(); } }

        protected string _Java;
        public string Java { get { return _Java; } set { _Java = value; RaisePropertyChanged(); } }
        
        protected string _Flash;
        public string Flash { get { return _Flash; } set { _Flash = value; RaisePropertyChanged(); } }
        
        protected string _Shockwave;
        public string Shockwave { get { return _Shockwave; } set { _Shockwave = value; RaisePropertyChanged(); } }
        
        protected string _Reader;
        public string Reader { get { return _Reader; } set { _Reader = value; RaisePropertyChanged(); } }
        
        protected string _Quicktime;
        public string Quicktime { get { return _Quicktime; } set { _Quicktime = value; RaisePropertyChanged(); } }
        #endregion

        #region Get Machine Info Methods

        public async Task Get_MachineInfo()
        {
            var machineInfo = CommandList.Find(c => c.Name == "MachineInfo");
            Tool.PAExec paexec = new Tool.PAExec(IPAddress, machineInfo.Command);
            await paexec.Run();
            string Info = paexec.StandardOutput.Replace("\r\r\r\nNode,SystemType,TotalPhysicalMemory,UserName\r\r\r\n", string.Empty).Trim(null);
            string[] InfoArray = Info.Split(',');
            PCName = InfoArray[0];
            ChipStyle = InfoArray[1];
            RAM = BytesToString(Convert.ToInt64(InfoArray[2]));
            CurrentUser = InfoArray[3].ToLower().Replace("ecotoh\\", string.Empty);

            log.Debug("Name: {0} ChipStyle: {1} RAM: {2} CurrentUser {3}", PCName, ChipStyle, RAM, CurrentUser);

            if (PCName.Contains("ECT-"))
            { 
                IsStaff = false;
                await Get_StudentImageInfo();
            }
            else
            { 
                IsStaff = true;
                BOD = VPN = Image = "N/A";
            }
        }

        public async Task Get_OSBitness()
        {
            var machineInfo = CommandList.Find(c => c.Name == "Bittness");
            Tool.PAExec paexec = new Tool.PAExec(IPAddress, machineInfo.Command);
            await paexec.Run();
            string Info = paexec.StandardOutput.Replace("\r\r\r\nNode,OSArchitecture\r\r\r\n", string.Empty).Trim(null);
            string[] InfoArray = Info.Split(',');
            OSBit = InfoArray[1];
            if (OSBit == "64-bit") { x64 = true; }
            else { x64 = false; }

            log.Debug("OSBit: {0}", OSBit);
        }

        protected async Task Get_StudentImageInfo()
        {
            var machineInfo = CommandList.Find(c => c.Name == "Image");
            log.Info("Acquiring Image information...");
            var paexec = new Tool.PAExec(IPAddress, machineInfo.Command);
            await paexec.Run();
            Image = paexec.StandardOutput.Replace("\r\r\nHKEY_LOCAL_MACHINE\\Software\\ecot\r\r\n    Image Version    REG_SZ    ", string.Empty).TrimEnd(null);
            log.Debug("Image: " + Image);

            log.Info("Acquiring BOD...");
            string f = @"\\" + IPAddress.ToString() + @"\c$\Image_Files\Bginfo\Born on date.txt";
            using (StreamReader r = new StreamReader(f, System.Text.Encoding.ASCII))
            {
                BOD = r.ReadToEnd();
                log.Info("BOD:" + BOD);
            }

            Get_VPN();
        }

        protected string Get_VPN()
        {
            int n = 0;
            int c = 1;
            int vpncount = 1;
            while (n <= 31)
            {
                if (IPAddress.ToString().Contains(string.Format("10.39.{0}.", n)))
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

        #endregion

        #region Get Software Methods

        public async Task Get_IE()
        {
            IEVersion = "Updating...";
            var machineInfo = CommandList.Find(c => c.Name == "IE");
            Tool.PAExec paexec = new Tool.PAExec(IPAddress, machineInfo.Command);
            await paexec.Run();
            IEVersion = paexec.StandardOutput.Replace("Version          \r\r\r\n", string.Empty).Trim();
            log.Debug("IE: {0}", IEVersion);
        }

        public async Task Get_Java()
        {
            Java = "Updating...";
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

            var paexec = new Tool.PAExec(IPAddress, command);
            await paexec.Run();
            bool loop = true;
            //while (loop == true)
            //{
                if (paexec.StandardError.Contains("java version") == true)
                {
                    result = paexec.StandardError.Split(new char[] { '\"', '\"' })[1];
                    log.Info("Java version: " + result);
                    loop = false;
                }
                //else
                //{
                //    log.Debug("Version lookup failed, trying again");
                //    await paexec.Run();
                //}
            //}
            
            else if (paexec.StandardError.Contains("-9"))
            {
                result = "NOT INSTALLED";
                log.Warn("Java is not installed");
            }
            else
            {
                result = "ERROR";
                log.Warn("Java version lookup returned error");
            }
            Java = result;
        }

        public async Task Get_Flash()
        {
            log.Info("Acquiring Flash Version...");
            Flash = "Updating...";
            var paexec = new Tool.PAExec(IPAddress, @"-accepteula -s REG query hklm\Software\Macromedia\FlashPlayerActiveX /v Version");
            await paexec.Run();
            if (paexec.StandardError.Contains("The system was unable to find the specified registry key or value"))
            {
                log.Warn("flash is not installed");
                Flash = "NOT INSTALLED";
            }
            else
            {
                Flash = paexec.StandardOutput.Replace("\r\r\nHKEY_LOCAL_MACHINE\\Software\\Macromedia\\FlashPlayerActiveX\r\r\n    Version    REG_SZ    ", string.Empty).Trim(null);
                log.Info("Flash version: " + Flash);
            }
        }

        public async Task Get_Shockwave()
        {
            log.Info("Acquiring Shockwave Version...");
            Shockwave = "Updating...";
            var paexec = new Tool.PAExec(IPAddress, "-s REG query \"hklm\\Software\\Adobe\\Shockwave 12\\currentupdateversion\"");
            await paexec.Run();
            
            if (paexec.StandardError.Contains("ERROR"))
            {
                Shockwave = "NOT INSTALLED";
                log.Warn("Shockwave is not installed");
            }
            else
            {
                Shockwave = paexec.StandardOutput.Replace("\r\r\nHKEY_LOCAL_MACHINE\\Software\\Adobe\\Shockwave 12\\currentupdateversion\r\r\n    (Default)    REG_SZ    ", string.Empty).Trim(null);
                log.Info("Shockwave version: " + Shockwave);
            }
        }

        public async Task Get_Reader()
        {
            string argument = string.Empty;
            log.Info("Acquiring Reader Version...");
            Reader = "Updating...";
            //_tools.Copy(Settings.General.Default.sigcheck, Settings.General.Default.remote_folder);
            if (x64 == true)
                argument = @"Program Files (x86)";
            else
                argument = @"Program Files";

            var r11 = new Tool.PAExec(IPAddress, string.Format(@"wmic datafile where name='C:\\{0}\\Adobe\\Reader 11.0\\Reader\\AcroRd32.exe' get version", argument));
            await r11.Run();
                        
            if (r11.StandardOutput.Contains("No Instance(s) Available"))
            {
                log.Debug("Reader 11 is not installed");
                var r10 = new Tool.PAExec(IPAddress, string.Format(@"wmic datafile where name='C:\\{0}\\Adobe\\Reader 10.0\\Reader\\AcroRd32.exe' get version", argument));
                await r10.Run();

                if (r10.StandardOutput.Contains("No Instance(s) Available"))
                {
                    log.Debug("Reader 10 is not installed");
                    Reader = "NOT INSTALLED";
                    log.Warn("Reader is not installed");
                }
                else
                {
                    Reader = r10.StandardOutput.Remove(0, 7).Trim(null);
                    log.Info("Reader version: " + Reader);
                }
            }
            else
            {
                Reader = r11.StandardOutput.Remove(0, 7).Trim(null);
                log.Info("Reader version: " + Reader);
            }
        }

        public async Task Get_Quicktime()
        {
            string argument = string.Empty;
            log.Info("Acquiring Quicktime Version...");
            Quicktime = "Updating...";
            
            if (x64 == true)
                argument = @"Program Files (x86)";
            else
                argument = @"Program Files";

            var paexec = new Tool.PAExec(IPAddress, string.Format(@"wmic datafile where name='C:\\{0}\\Quicktime\\QuickTimePlayer.exe' get version", argument));
            await paexec.Run();
            if (paexec.StandardError.Contains("No Instance(s) Available."))
            {
                Quicktime = "NOT INSTALLED";
                log.Warn("Quicktime is not installed");
            }
            else
            {
                Quicktime = paexec.StandardOutput.Remove(0, 7).Trim();
                log.Info("Quicktime version: " + Quicktime);
            }
        }

        #endregion

        public async Task<bool> Command(RemoteCommandItem item)
        {
            History = "";
            return true;
        }

        /// <summary>
        /// Used to launch a local instance of dameware to connect to the remote machine.
        /// </summary>
        /// <returns>Returns false if dameware is not installed.</returns>
        public async Task<bool> OpenDameware()
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = (@"C:\Program Files\SolarWinds\DameWare Mini Remote Control 10.0 x64\DWRCC.exe"),
                Arguments = (@"-c: -h: -a:1 -x -m:" + IPAddress.ToString()),
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
            }
            else if (System.IO.File.Exists(@"C:\Program Files\SolarWinds\DameWare Mini Remote Control 8.0\DWRCC.exe"))
            {
                log.Info("Launching Dameware 8");
                startInfo.FileName = @"C:\Program Files\SolarWinds\DameWare Mini Remote Control 8.0\DWRCC.exe";
            }
            else
            {
                log.Error("Could Not Find DameWare installed!");
                return false;
            }
            Task.Run(() => System.Diagnostics.Process.Start(startInfo));
            return true;
        }

        /// <summary>
        /// Used to launch explorer of the remote machine c share
        /// </summary>
        /// <returns>Returns false if cshare cannot be opened.</returns>
        public async Task<bool> OpenCShare()
        {
            log.Info("Opening C-Share");
            try
            {
                var sp = new Tool.StandardProcess("explorer", @"\\" + IPAddress.ToString() + @"\c$\");
                await sp.Run();
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        public async Task<bool> OpenHDrive()
        {
            log.Info(string.Format("Opening H drive for {0}", CurrentUser));
            try
            {
                var sp = new Tool.StandardProcess("explorer", @"\\fs2\students\" + CurrentUser);
                await sp.Run();
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }

        }

        public async Task<bool> FixJNLPAssoication()
        {
            log.Info("Trying JNLP fix");
            try
            {
                var assoc = new Tool.PAExec(IPAddress, @"cmd /c assoc .jnlp=<JNLPFILE>");
                await assoc.Run();               
                var paexec = new Tool.PAExec(IPAddress, @"ftype jnlpfile=""c:\Program Files (x86)\Java\jre7\bin\javaws.exe"" ""%1""");
                await paexec.Run();
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        public async Task FixBackGround()
        {
            var bg = new Tool.PAExec(IPAddress, @"-i 1 -s -accepteula c:\image_files\bginfo\bginfo.exe /nolicprompt c:\image_files\bginfo\wallpaper.bgi /timer:0 /silent");
            await bg.Run();
            //Fix theme too
            var theme = new Tool.PAExec(IPAddress, @"-i 1 -s -accepteula cmd.exe /c c:\windows\resources\themes\ecot.theme");
            await theme.Run();      
        }

        public async Task FileCleanup()
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
                var paexec = new Tool.PAExec(IPAddress, s);
                await paexec.Run();
            }
            History = "File cleanup complete";
        }

        public async Task ProfileWipe_Enable()
        {
            await Task.Run(() => Tool.Files.Copy(@"\\fs1\HelpDesk\TOOLS\3rdParty\Delprof2 1.5.4", @"\temp\Delprof2_1.5.4\"));

            var add1 = new Tool.StandardProcess(@"c:\windows\sysnative\schtasks.exe", @"/create /s \\" + IPAddress.ToString() + @" /sc onstart /delay 0000:10 /rl HIGHEST /ru SYSTEM /tn ""Profile wipe"" /tr ""c:\temp\Delprof2_1.5.4\delprof2.exe /u /id:""" + CurrentUser);
            var add2 = new Tool.StandardProcess(@"c:\windows\sysnative\schtasks.exe", @"/create /s \\" + IPAddress.ToString() + @"  /sc onlogon /ru SYSTEM /tn ""remove wipe"" /tr ""c:\temp\Delprof2_1.5.4\remove.bat""");
            await add1.Run();
            await add2.Run();
            History = "Profile Wipe ENABELED on next boot.";
        }

        public async Task ProfileWipe_Disable()
        {
            var remove1 = new Tool.StandardProcess(@"c:\windows\sysnative\schtasks.exe", @"/delete /s \\" + IPAddress.ToString() + @" /tn ""Profile wipe"" /f");
            var remove2 = new Tool.StandardProcess(@"c:\windows\sysnative\schtasks.exe", @"/delete /s \\" + IPAddress.ToString() + @" /tn ""remove wipe"" /f");
            await remove1.Run();
            await remove2.Run();
            History = "Profile Wipe DISABELED";
        }

        protected async Task ProfileBackup()
        {           
            var desktop = new Tool.PAExec(IPAddress, string.Format(@"robocopy c:\users\{0}\Desktop c:\temp\profilebackup\{0}\Desktop /E /R:0", CurrentUser));            
            var documents = new Tool.PAExec(IPAddress, string.Format(@"robocopy c:\users\{0}\Documents c:\temp\profilebackup\{0}\Documents /E /R:0", CurrentUser));
            History = @"Backuping up \Desktop for " + CurrentUser;
            await desktop.Run();
            History = @"Backuping up \Documents for " + CurrentUser;
            await documents.Run();
            History = "Profile backup complete for " + CurrentUser;
        }

        public async Task Reboot()
        {
            log.Info(string.Format("Rebooting {0}", PCName));
            var reboot = new Tool.PSShutdown(IPAddress, @" -r -t 01 -f");
            await reboot.Run();
            History = "Sent reboot command.";
        }
    }



}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net;
using System.Threading;
using Minion.ListItems;
using System.IO;
using System.Collections.ObjectModel;


namespace Minion
{
    public enum log
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    public class EcotPC : INotifyPropertyChanged
    {
        #region Custom Logger
        /// <summary>
        /// This logger addition is used to allow for raising an event which passes the logged message 
        /// so that it can be captured by listeners for screen logging
        /// </summary>
        protected static NLog.Logger nlog = NLog.LogManager.GetCurrentClassLogger();
        protected void Log(Minion.log type, string message, params object[] args)
        {
            int count = 0;
            foreach (object item in args)
            {
                message = message.Replace(@"{" + count.ToString() + "}", item.ToString());
                count++;
            }

            if (type == log.Trace)
            {
                nlog.Trace(message);
            }
            else if (type == log.Debug)
            {
                nlog.Debug(message);
            }
            else if (type == log.Info)
            {
                nlog.Info(message);
            }
            else if (type == log.Warn)
            {
                nlog.Warn(message);
            }
            else if (type == log.Error)
            {
                nlog.Error(message);
            }
            else if (type == log.Fatal)
            {
                nlog.Fatal(message);
            }

            if (EventLogged != null) { EventLogged(this, new Minion.LogEventArgs(DateTime.Now, type, message)); }
        }
        protected void PassEventLogged(object sender, Minion.LogEventArgs e)
        {
            if (EventLogged != null) { EventLogged(this, e); }
        }
        public event EventHandler<Minion.LogEventArgs> EventLogged;
        #endregion
            
        #region Constructors and Deconstructors
        public EcotPC(IPAddress ipaddress)
        {
            if (Tool.IP.Ping(ipaddress) == true)
                _IPAddress = ipaddress;
            else
            {
                throw new Exception("IP is unreachable");
            }

            var token = tokenSource.Token;
            Task PingTesting = new Task( () => PingTest(token), token, TaskCreationOptions.LongRunning);
            PingTesting.Start();
            Task.Run(async () => await Get_OSBitness());            
            Task.Run(() => Get_MachineInfo());
            
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
                        Log(log.Info, "{0} is online!", IPAddress.ToString());
                        RaiseOnlineChanged(EventArgs.Empty);
                    }
                }
                else
                {
                    if (IsOnline == true)
                    {
                        IsOnline = false;
                        Log(log.Info, "{0} has gone offline!", IPAddress.ToString());
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

        protected void ListUpdate(ObservableCollection<ProperityItem> items, string val, [CallerMemberName] string prop = "")
        {
            var result = items.Where(p => p.Name == prop).FirstOrDefault();
            if (result == null)
            {
                items.Add(new ProperityItem { Name = prop, Info = val });
            }
            else
            {
                result.Info = val;
            }
        }
        #endregion

        #region Machine Properties
        //collection of all properties test
        protected ObservableCollection<ProperityItem> _machine = new ObservableCollection<ProperityItem>();
        public ObservableCollection<ProperityItem> Machine { get { return _machine; } set { _machine = value; RaisePropertyChanged(); } }

        //used to see if machine is active with passed tasked
        protected int _processing = 0;
        public int Processing { get { return _processing; } set { _processing = value; if (_processing > 0) { Active = true; } else { Active = false; } RaisePropertyChanged(); } }
        protected bool _active;
        public bool Active { get { return _active; } set { _active = value; RaisePropertyChanged(); } }

        protected IPAddress _IPAddress;
        public IPAddress IPAddress { get { return _IPAddress; } protected set { _IPAddress = value; RaisePropertyChanged(); } }

        protected string _Name;
        public string PCName { get { return _Name; } protected set { _Name = value; RaisePropertyChanged(); ListUpdate(Machine, value); } }

        protected string _CurrentUser;
        public string CurrentUser { get { return _CurrentUser; } protected set { _CurrentUser = value; RaisePropertyChanged(); ListUpdate(Machine, value); } }

        protected string _ChipStyle;
        public string ChipStyle { get { return _ChipStyle; } set { _ChipStyle = value; RaisePropertyChanged(); ListUpdate(Machine, value); } }
        
        protected string _RAM;
        public string RAM { get { return _RAM; } set { _RAM = value; RaisePropertyChanged(); ListUpdate(Machine, value); } }

        protected string _OSBit;
        public string OSBit { get { return _OSBit; } set { _OSBit = value; RaisePropertyChanged(); ListUpdate(Machine, value); } }

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

        protected string _java64;
        public string Java64 { get { return _java64; } set { _java64 = value; RaisePropertyChanged(); } }
        protected string _java32;
        public string Java32 { get { return _java32; } set { _java32 = value; RaisePropertyChanged(); } }


        protected string _Flash;
        public string Flash { get { return _Flash; } set { _Flash = value; RaisePropertyChanged(); } }
        
        protected string _Shockwave;
        public string Shockwave { get { return _Shockwave; } set { _Shockwave = value; RaisePropertyChanged(); } }
        
        protected string _Reader;
        public string Reader { get { return _Reader; } set { _Reader = value; RaisePropertyChanged(); } }
        
        protected string _Quicktime;
        public string Quicktime { get { return _Quicktime; } set { _Quicktime = value; RaisePropertyChanged(); } }
        #endregion

        #region Get Machine Info

        private List<ListItems.EcotPCCommand> CommandList = new List<ListItems.EcotPCCommand>() 
        { 
            new ListItems.EcotPCCommand() { Name = "MachineInfo", Tool = "PAExec", Command = @"-accepteula -s -realtime wmic computersystem get systemtype, username, totalphysicalmemory /format:csv" },
            new ListItems.EcotPCCommand() { Name = "Bittness", Tool = "PAExec", Command = @"-accepteula -s -realtime wmic os get OSArchitecture /format:csv" },
            new ListItems.EcotPCCommand() { Name = "IE", Tool = "PAExec", Command = @"WMIC DATAFILE WHERE ""Name='c:\\program files\\internet explorer\\iexplore.exe'"" GET Version" },
            new ListItems.EcotPCCommand() { Name = "Image", Tool = "PAExec", Command = @"-accepteula -s -dfr REG query hklm\Software\ecot /v ""Image Version""" },                   
        };

        public async Task Get_MachineInfo()
        {
            Processing++;
            var machineInfo = CommandList.Find(c => c.Name == "MachineInfo");
            Tool.PAExec paexec = new Tool.PAExec(IPAddress, machineInfo.Command);
            await paexec.Run();
            string Info = paexec.StandardOutput.Replace("\r\r\r\nNode,SystemType,TotalPhysicalMemory,UserName\r\r\r\n", string.Empty).Trim(null);
            string[] InfoArray = Info.Split(',');
            PCName = InfoArray[0];
            ChipStyle = InfoArray[1];
            RAM = BytesToString(Convert.ToInt64(InfoArray[2]));
            CurrentUser = InfoArray[3].ToLower().Replace("ecotoh\\", string.Empty);

            Log(log.Trace, "Name: {0} ChipStyle: {1} RAM: {2} CurrentUser {3}", PCName, ChipStyle, RAM, CurrentUser);

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
            Processing--;
        }

        public async Task Get_OSBitness()
        {
            Processing++;
            var machineInfo = CommandList.Find(c => c.Name == "Bittness");
            Tool.PAExec paexec = new Tool.PAExec(IPAddress, machineInfo.Command);
            await paexec.Run();
            string Info = paexec.StandardOutput.Replace("\r\r\r\nNode,OSArchitecture\r\r\r\n", string.Empty).Trim(null);
            string[] InfoArray = Info.Split(',');
            OSBit = InfoArray[1];
            if (OSBit == "64-bit") { x64 = true; }
            else { x64 = false; }

            Log(log.Trace, "OSBit: {0}", OSBit);
            Processing--;
        }

        protected async Task Get_StudentImageInfo()
        {           
            var machineInfo = CommandList.Find(c => c.Name == "Image");
            Log(log.Debug, "Acquiring Image information...");
            var paexec = new Tool.PAExec(IPAddress, machineInfo.Command);
            await paexec.Run();
            Image = paexec.StandardOutput.Replace("\r\r\nHKEY_LOCAL_MACHINE\\Software\\ecot\r\r\n    Image Version    REG_SZ    ", string.Empty).TrimEnd(null);
            Log(log.Trace, "Image: " + Image);

            Log(log.Debug, "Acquiring BOD...");
            string f = @"\\" + IPAddress.ToString() + @"\c$\Image_Files\Bginfo\Born on date.txt";
            using (StreamReader r = new StreamReader(f, System.Text.Encoding.ASCII))
            {
                BOD = r.ReadToEnd();
                Log(log.Trace, "BOD:" + BOD);
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

        public async Task<string> Get_IE()
        {            
            Processing++;
            IEVersion = "Updating...";
            var machineInfo = CommandList.Find(c => c.Name == "IE");
            Tool.PAExec paexec = new Tool.PAExec(IPAddress, machineInfo.Command);
            await paexec.Run();
            IEVersion = paexec.StandardOutput.Replace("Version", string.Empty).Trim();
            Log(log.Trace, "IE: {0}", IEVersion);
            Processing--;
            return IEVersion;
        }

        public async Task Get_Java()
        {
            Processing++;
            Java64 = Java32 = "Updating...";
            string command;
            string result = string.Empty;
            if (Directory.Exists(string.Format(@"\\{0}\c$\Program Files (x86)\", IPAddress)))
            {
                //Is 64 bit machine
                if (File.Exists(string.Format(@"\\{0}\c$\Program Files (x86)\Java\jre7\bin\java.exe", IPAddress)))
                {
                    command = @"""c:\Program Files (x86)\Java\jre7\bin\java.exe"" -version";
                    Java32 = await JavaVersion(command, result);
                }
                else
                {
                    Java32 = "NOT INSTALLED";
                }

                if (File.Exists(string.Format(@"\\{0}\c$\Program Files\Java\jre7\bin\java.exe", IPAddress)))
                {
                    command = @"""c:\Program Files\Java\jre7\bin\java.exe"" -version";
                    Java64 = await JavaVersion(command, result);
                }
                else
                {
                    Java64 = "NOT INSTALLED";
                }
            }
            else
            {
                Java64 = "N/A";
                if (File.Exists(string.Format(@"\\{0}\c$\Program Files\Java\jre7\bin\java.exe", IPAddress)))
                {
                    command = @"""c:\Program Files\Java\jre7\bin\java.exe"" -version";
                    Java32 = await JavaVersion(command, result);
                }
                else
                {
                    Java32 = "NOT INSTALLED";
                }
            }

            Log(log.Trace, "Java32 version: " + Java32);
            Log(log.Trace, "Java64 version: " + Java64);
            Processing--;
        }
        private async Task<string> JavaVersion(string command, string result)
        {
            var paexec = new Tool.PAExec(IPAddress, command);
            await paexec.Run();
            if (paexec.StandardError.Contains("java version") == true)
            {
                result = paexec.StandardError.Split(new char[] { '\"', '\"' })[1];
            }
            else
            {
                result = "ERROR";
            }
            return result;
        }

        public async Task<string> Get_Flash()
        {
            Processing++;
            Flash = "Updating...";
            var paexec = new Tool.PAExec(IPAddress, @"-accepteula -s REG query hklm\Software\Macromedia\FlashPlayerActiveX /v Version");
            await paexec.Run();
            if (paexec.StandardError.Contains("The system was unable to find the specified registry key or value"))
            {
                Flash = "NOT INSTALLED";
            }
            else
            {
                Flash = paexec.StandardOutput.Replace("\r\r\nHKEY_LOCAL_MACHINE\\Software\\Macromedia\\FlashPlayerActiveX\r\r\n    Version    REG_SZ    ", string.Empty).Trim(null);
            }
            Processing--;
            Log(log.Trace, "Flash version: " + Flash);
            return Flash;
        }

        public async Task<string> Get_Shockwave()
        {
            Processing++;
            Shockwave = "Updating...";
            List<string> paths = new List<string>();
            if (Directory.Exists(string.Format(@"\\{0}\c$\Program Files (x86)\", IPAddress)))
            {
                //Is 64 bit machine              
                paths.Add(@"C:\\Windows\\SysWOW64\\Adobe\\Shockwave 12\\swinit.exe");
                paths.Add(@"C:\\Windows\\SysWOW64\\Adobe\\Shockwave 11\\swinit.exe");
            }
            else
            {
                paths.Add(@"C:\\Windows\\System32\\Adobe\\Shockwave 12\\swinit.exe");
                paths.Add(@"C:\\Windows\\System32\\Adobe\\Shockwave 11\\swinit.exe");
            }
            Shockwave = await WMICVersion(paths);
            Processing--;
            return Shockwave;

            //var paexec = new Tool.PAExec(IPAddress, "-s REG query \"hklm\\Software\\Adobe\\Shockwave 12\\currentupdateversion\"");
            //await paexec.Run();
            
            //if (paexec.StandardError.Contains("ERROR"))
            //{
            //    Shockwave = "NOT INSTALLED";
            //}
            //else
            //{
            //    Shockwave = paexec.StandardOutput.Replace("\r\r\nHKEY_LOCAL_MACHINE\\Software\\Adobe\\Shockwave 12\\currentupdateversion\r\r\n    (Default)    REG_SZ    ", string.Empty).Trim(null);
            //}
            
            //Log(log.Trace, "Shockwave version: " + Shockwave);
            //return Shockwave;
        }

        private async Task<string> WMICVersion(List<string> paths)
        {
            foreach (string path in paths)
            {
                var file = new Tool.PAExec(IPAddress, string.Format(@"wmic datafile where name='{0}' get version", path));
                await file.Run();

                if (!file.StandardError.Contains("No Instance(s) Available") && !file.StandardError.Contains("Invalid query"))
                {
                    return file.StandardOutput.Remove(0, 7).Trim(null);
                }
            }
            return "NOT INSTALLED";

        }

        public async Task<string> Get_Reader()
        {
            Processing++;
            string argument = string.Empty;
            Reader = "Updating...";
            //_tools.Copy(Settings.General.Default.sigcheck, Settings.General.Default.remote_folder);
            if (x64 == true)
                argument = @"Program Files (x86)";
            else
                argument = @"Program Files";

            var r11 = new Tool.PAExec(IPAddress, string.Format(@"wmic datafile where name='C:\\{0}\\Adobe\\Reader 11.0\\Reader\\AcroRd32.exe' get version", argument));
            await r11.Run();
                        
            if (r11.StandardError.Contains("No Instance(s) Available"))
            {
                var r10 = new Tool.PAExec(IPAddress, string.Format(@"wmic datafile where name='C:\\{0}\\Adobe\\Reader 10.0\\Reader\\AcroRd32.exe' get version", argument));
                await r10.Run();

                if (r10.StandardError.Contains("No Instance(s) Available"))
                {
                    Reader = "NOT INSTALLED";
                }
                else
                {
                    Reader = r10.StandardOutput.Remove(0, 7).Trim(null);
                }
            }
            else
            {
                Reader = r11.StandardOutput.Remove(0, 7).Trim(null);
                
            }
            Processing--;
            Log(log.Debug, "Reader version: " + Reader);
            return Reader;
        }

        public async Task<string> Get_Quicktime()
        {
            Processing++;
            string argument = string.Empty;
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
            }
            else
            {
                Quicktime = paexec.StandardOutput.Remove(0, 7).Trim();
            }
            Processing--;
            Log(log.Trace, "Quicktime version: " + Quicktime);
            return Quicktime;
        }

        #endregion

        public async Task<bool> Command(MinionCommandItem item)
        {
            Processing++;
            if (item.Version != null || item.Version == string.Empty)
                Log(log.Info, "Running {0} of {1} Version {2}", item.Action, item.Name, item.Version);
            else
                Log(log.Info,"Running {0} of {1}", item.Action, item.Name);

            Minion.Tool.PAExec paexec;
            if (string.IsNullOrEmpty(item.CopyFrom))
                paexec = new Minion.Tool.PAExec(IPAddress, item.Command);
            else if (string.IsNullOrEmpty(item.CopyTo))
                paexec = new Minion.Tool.PAExec(IPAddress, item.Command, item.CopyFrom);
            else
                paexec = new Minion.Tool.PAExec(IPAddress, item.Command, item.CopyFrom, item.CopyTo);

            paexec.EventLogged += PassEventLogged;
            await paexec.Run();
            paexec.EventLogged -= PassEventLogged;
            Log(log.Info, "{0} of {1} exited", item.Action, item.Name);
            Processing--;
            return true;
        }

        #region Tools

        public async Task Kill_Defaultss()
        {
            Processing++;
            var gettasks = new Tool.PAExec(IPAddress, @"-accepteula -realtime -s tasklist");
            await gettasks.Run();
            //Looks at tasks returned and only kills open tasks. Needs to be changed to a list to shorten code.
            if (gettasks.StandardOutput.Contains("iexplore.exe"))
            {
                Log(log.Info, "Killing Task(s) iexplore.exe");
                var kill = new Tool.PSKill(IPAddress, "iexplore.exe");
                await kill.Run();
            }
            if (gettasks.StandardOutput.Contains("msiexec.exe"))
            {
                Log(log.Info, "Killing Task(s) javaws.exe");
                var kill = new Tool.PSKill(IPAddress, "javaws.exe");
                await kill.Run();
            }
            if (gettasks.StandardOutput.Contains("javaws.exe"))
            {
                Log(log.Info, "Killing Task(s) javaws.exe");
                var kill = new Tool.PSKill(IPAddress, "javaws.exe");
                await kill.Run();
            }
            if (gettasks.StandardOutput.Contains("jusched.exe"))
            {
                Log(log.Info, "Killing Task(s) jusched.exe");
                var kill = new Tool.PSKill(IPAddress, "jusched.exe");
                await kill.Run();
            }

            //string test = Environment.CurrentDirectory.ToString() + @"\Resources\defaultkills.bat";
            //var kills = new Minion.Tool.PAExec(IPAddress, @"-s c:\temp\defaultkills.bat", test, @"\Temp\", true);
            
            //await kills.Run();
            Processing--;
        }
  
        /// <summary>
        /// Used to launch a local instance of dameware to connect to the remote machine.
        /// </summary>
        /// <returns>Returns false if dameware is not installed.</returns>
        public async Task<bool> OpenDameware()
        {
            Processing++;
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                Arguments = (@"-c: -h: -a:1 -x -m:" + IPAddress.ToString()),
            };

            if (System.IO.File.Exists(@"C:\Program Files\SolarWinds\DameWare Mini Remote Control 10.0 x64\DWRCC.exe"))
            {
                Log(log.Debug, "Launching Dameware 10 x64");
                startInfo.FileName = (@"C:\Program Files\SolarWinds\DameWare Mini Remote Control 10.0 x64\DWRCC.exe");
            }
            else if (System.IO.File.Exists(@"C:\Program Files\SolarWinds\DameWare Mini Remote Control 9.0\DWRCC.exe"))
            {
                Log(log.Debug, "Launching Dameware 9");
                startInfo.FileName = @"C:\Program Files\SolarWinds\DameWare Mini Remote Control 9.0\DWRCC.exe";
            }
            else if (System.IO.File.Exists(@"C:\Program Files\SolarWinds\DameWare Mini Remote Control 8.0\DWRCC.exe"))
            {
                Log(log.Debug, "Launching Dameware 8");
                startInfo.FileName = @"C:\Program Files\SolarWinds\DameWare Mini Remote Control 8.0\DWRCC.exe";
            }
            else
            {
                Log(log.Error, "Could Not Find DameWare installed!");
                Processing--;
                return false;
            }
            await Task.Run(() => System.Diagnostics.Process.Start(startInfo));
            Processing--;
            return true;
        }

        /// <summary>
        /// Used to launch explorer of the remote machine c share
        /// </summary>
        /// <returns>Returns false if cshare cannot be opened.</returns>
        public async Task<bool> OpenCShare()
        {
            Processing++;
            Log(log.Info, "Opening C-Share");
            try
            {
                var sp = new Tool.StandardProcess(@"c:\Windows\", "explorer.exe", @"\\" + IPAddress.ToString() + @"\c$\");
                await sp.Run();
                Processing--;
                return true;
            }
            catch (Exception ex)
            {
                Log(log.Error, ex.ToString());
                Processing--;
                return false;
            }
        }

        public async Task<bool> OpenHDrive()
        {
            Log(log.Info, "Opening H drive for {0}", CurrentUser);
            try
            {
                var sp = new Tool.StandardProcess(@"c:\Windows\", "explorer", @"\\fs2\students\" + CurrentUser);
                await sp.Run();
                return true;
            }
            catch (Exception ex)
            {
                Log(log.Error, ex.ToString());
                return false;
            }

        }

        public async Task<bool> FixJNLPAssoication()
        {
            Log(log.Info, "Trying JNLP fix");
            Processing++;
            try
            {
                var assoc = new Tool.PAExec(IPAddress, @"cmd /c assoc .jnlp=<JNLPFILE>");
                await assoc.Run();
                var paexec = new Tool.PAExec(IPAddress, @"ftype jnlpfile=""c:\Program Files (x86)\Java\jre7\bin\javaws.exe"" ""%1""");
                await paexec.Run();
                Processing--;
                return true;
            }
            catch (Exception ex)
            {
                Log(log.Error, ex.ToString());
                Processing--;
                return false;
            }
        }

        public async Task FixBackGround()
        {
            Processing++;
            var bg = new Tool.PAExec(IPAddress, @"-i 1 -s -accepteula c:\image_files\bginfo\bginfo.exe /nolicprompt c:\image_files\bginfo\wallpaper.bgi /timer:0 /silent");
            await bg.Run();
            //Fix theme too
            var theme = new Tool.PAExec(IPAddress, @"-i 1 -s -accepteula cmd.exe /c c:\windows\resources\themes\ecot.theme");
            await theme.Run();
            Processing--;
        }

        public async Task FileCleanup()
        {
            Processing++;
            Log(log.Info, "Starting remote file cleanup");
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
                Log(log.Info, "Deleting remote folder: " + s.Replace("cmd /C RMDIR /S /Q ", string.Empty));
                var paexec = new Tool.PAExec(IPAddress, s);
                await paexec.Run();
            }
            var x = new Tool.Files();

            x.Copy(@"\\fs1\HelpDesk\SHORTCUTS\Clear_IE_Cache.lnk", string.Format(@"\\{0}\c$\Users\{1}\Desktop\",IPAddress, CurrentUser));
            Log(log.Info, "Copied Clear_IE_Cache.lnk to Remote Desktop for clearing cache in IE. ");
            Processing--;
            Log(log.Info, "File cleanup complete");
        }

        public async Task ProfileWipe_Enable()
        {
            Processing++;
            
            var UserProfileServiceFix = new Tool.PAExec(IPAddress, @"-accepteula -realtime -s icacls c:\Users\Default\*  /inheritance:e /T /C");
            await UserProfileServiceFix.Run();

            var file = new Tool.Files();
            file.EventLogged += PassEventLogged;
            await Task.Run(() => file.Copy(@"\\fs1\HelpDesk\TOOLS\3rdParty\Delprof2 1.5.4", string.Format(@"\\{0}\c$\temp\Delprof2_1.5.4\", IPAddress)));
            file.EventLogged -= PassEventLogged;

            var add1 = new Tool.StandardProcess(@"c:\windows\system32\", "schtasks.exe", @"/create /s \\" + IPAddress.ToString() + @" /sc onstart /delay 0000:10 /rl HIGHEST /ru SYSTEM /tn ""Profile wipe"" /tr ""c:\temp\Delprof2_1.5.4\delprof2.exe /u /id:""" + CurrentUser);
            var add2 = new Tool.StandardProcess(@"c:\windows\system32\", "schtasks.exe", @"/create /s \\" + IPAddress.ToString() + @"  /sc onlogon /ru SYSTEM /tn ""remove wipe"" /tr ""c:\temp\Delprof2_1.5.4\remove.bat""");
            await add1.Run();
            await add2.Run();
            Processing--;
            Log(log.Info, "Profile Wipe ENABELED on next boot.");
        }

        public async Task ProfileWipe_Disable()
        {
            Processing++;
            var remove1 = new Tool.StandardProcess(@"c:\windows\system32\", "schtasks.exe", @"/delete /s \\" + IPAddress.ToString() + @" /tn ""Profile wipe"" /f");
            var remove2 = new Tool.StandardProcess(@"c:\windows\system32\", "schtasks.exe", @"/delete /s \\" + IPAddress.ToString() + @" /tn ""remove wipe"" /f");
            await remove1.Run();
            await remove2.Run();
            Processing--;
            Log(log.Info, "Profile Wipe DISABELED");
        }

        public async Task ProfileBackup()
        {
            Processing++;
            var desktop = new Tool.PAExec(IPAddress, string.Format(@"robocopy c:\users\{0}\Desktop c:\temp\profilebackup\{0}\Desktop /E /R:0", CurrentUser));
            var documents = new Tool.PAExec(IPAddress, string.Format(@"robocopy c:\users\{0}\Documents c:\temp\profilebackup\{0}\Documents /E /R:0", CurrentUser));
            Log(log.Info, @"Backuping up \Desktop for " + CurrentUser);
            await desktop.Run();
            Log(log.Info, @"Backuping up \Documents for " + CurrentUser);
            await documents.Run();
            Log(log.Info, string.Format("Profile backup complete for {0}", CurrentUser));
            Processing--;
        }

        public async Task Reboot()
        {
            Processing++;
            
            var reboot = new Tool.PSShutdown(IPAddress, @" -r -t 01 -f");
            await reboot.Run();
            Processing--;
            Log(log.Info, string.Format("Rebooting {0}", PCName));
        }
    } 
        #endregion



}

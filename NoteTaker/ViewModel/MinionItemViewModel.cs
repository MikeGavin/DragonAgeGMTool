using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Runtime.CompilerServices;

namespace Scrivener.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MinionItemViewModel : ViewModelBase
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        
        #region Events
        public event EventHandler<Model.MinionArgs> NoteWrite;
        private void RaiseNoteWrite(string message)
        {
            if (NoteWrite != null) { NoteWrite(this, new Model.MinionArgs(message)); }
        }
        public event EventHandler RequestClose;
        void RaiseRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        } 
        #endregion

        //Constructor
        public MinionItemViewModel(IPAddress IP, Minion.MinionCommands commands)
        {
            Machine = new Minion.EcotPC(IP);
            MinionCommands = commands;
        }
        
        public string Title { get { return Machine.IPAddress.ToString(); } }
        
        public Minion.EcotPC Machine { get; protected set; }

        private Minion.MinionCommands _minionCommands;
        public Minion.MinionCommands MinionCommands { get { return _minionCommands; } protected set { _minionCommands = value; } }

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new RelayCommand(RaiseRequestClose)); } }
        
        #region GetVersionCommands

        private RelayCommand _getIECommand;
        public RelayCommand GetIECommand { get { return _getIECommand ?? (_getIECommand = new RelayCommand(async () => await Machine.Get_IE())); } }

        private RelayCommand _getJavaCommand;
        public RelayCommand GetJavaCommand { get { return _getJavaCommand ?? (_getJavaCommand = new RelayCommand(async () => await Machine.Get_Java())); } }

        private RelayCommand _getFlashCommand;
        public RelayCommand GetFlashCommand { get { return _getFlashCommand ?? (_getFlashCommand = new RelayCommand(async () => await Machine.Get_Flash())); } }

        private RelayCommand _getShockwaveCommand;
        public RelayCommand GetShockwaveCommand { get { return _getShockwaveCommand ?? (_getShockwaveCommand = new RelayCommand(async () => await Machine.Get_Shockwave())); } }

        private RelayCommand _getReaderCommand;
        public RelayCommand GetReaderCommand { get { return _getReaderCommand ?? (_getReaderCommand = new RelayCommand(async () => await Machine.Get_Reader())); } }

        private RelayCommand _getQuicktimeCommand;
        public RelayCommand GetQuicktimeCommand { get { return _getQuicktimeCommand ?? (_getQuicktimeCommand = new RelayCommand(async () => await Machine.Get_Quicktime())); } }

        #endregion

        #region ToolCommands

        private RelayCommand _openDamewareCommand;
        public RelayCommand OpenDamewareCommand { get { return _openDamewareCommand ?? (_openDamewareCommand = new RelayCommand(async () => await Machine.OpenDameware())); } }

        private RelayCommand _openCShareCommand;
        public RelayCommand OpenCShareCommand { get { return _openCShareCommand ?? (_openCShareCommand = new RelayCommand(async () => await Machine.OpenCShare())); } }

        private RelayCommand<string> _remoteStartCommand;
        public RelayCommand<string> RemoteStartCommand { get { return _remoteStartCommand ?? (_remoteStartCommand = new RelayCommand<string>(async (param) => await RemoteStart(param))); } }
        public async Task RemoteStart(string command)
        {
            Minion.Tool.PSExec psexec = new Minion.Tool.PSExec(Machine.IPAddress, string.Format("-accepteula -i -s -h -d {0}", command));
            await psexec.Run();
        }

        private RelayCommand _defaultKillsCommand;
        public RelayCommand DefaultKillsCommand { get { return _defaultKillsCommand ?? (_defaultKillsCommand = new RelayCommand(async () => await Machine.Kill_Defaultss())); } }
             

        #endregion

        private RelayCommand _uninstallJavaCommand;
        public RelayCommand UninstallJavaCommand { get { return _uninstallJavaCommand ?? (_uninstallJavaCommand = new RelayCommand(async () => await Uninstall_Java())); } }
        public async Task Uninstall_Java()
        {
            Minion.RemoteCommandImport item;
            if (Machine.Java == "NOT INSTALLED" || Machine.Java == "ERROR")
            {
                item = MinionCommands.Java.First(j => (j.Name == "Java") && (j.Version == "All")) as Minion.RemoteCommandImport;
            }
            else
            {
                item = MinionCommands.Java.First(j => (j.Name == "Java") && (j.Version == Machine.Java)) as Minion.RemoteCommandImport;
            }
            await Uninstall(item);
        }

        private RelayCommand _installJavaCommand;
        public RelayCommand InstallJavaCommand { get { return _installJavaCommand ?? (_installJavaCommand = new RelayCommand(async () => await Install_Java())); } }
        public async Task Install_Java()
        {
            var sort = MinionCommands.Java.OrderByDescending(x => x.Version).ToList();
            var item = sort[1];
            await Install(item);
        }

        private RelayCommand _uninstallFlashCommand;
        public RelayCommand UninstallFlashCommand { get { return _uninstallFlashCommand ?? (_uninstallFlashCommand = new RelayCommand(async () => await Uninstall_Flash())); } }
        public async Task Uninstall_Flash()
        {
            Minion.RemoteCommandImport item;
            if (Machine.Flash == "NOT INSTALLED" || Machine.Flash == "ERROR")
            {
                item = MinionCommands.Flash.First(f => (f.Name == "Flash") && (f.Version == "All")) as Minion.RemoteCommandImport;
            }
            else
            {
                item = MinionCommands.Flash.First(f => (f.Name == "Flash") && (f.Version == Machine.Flash)) as Minion.RemoteCommandImport;
            }
            
            await Uninstall(item);
        }

        private RelayCommand _installFlashCommand;
        public RelayCommand InstallFlashCommand { get { return _installFlashCommand ?? (_installFlashCommand = new RelayCommand(async () => await Install_Flash())); } }
        public async Task Install_Flash()
        {
            Minion.RemoteCommandImport item;
            item = MinionCommands.Flash.First(f => (f.Name == "Flash") && (f.Version != "All")) as Minion.RemoteCommandImport;
            await Install(item);
        }

        private RelayCommand _uninstallShockwaveCommand;
        public RelayCommand UninstallShockwaveCommand { get { return _uninstallShockwaveCommand ?? (_uninstallShockwaveCommand = new RelayCommand(async () => await Uninstall_Shockwave())); } }
        public async Task Uninstall_Shockwave()
        {
            Minion.RemoteCommandImport item;
            try
            { 
                item = MinionCommands.Shockwave.First(f => (f.Name == "Shockwave") && (f.Version == Machine.Shockwave)) as Minion.RemoteCommandImport; 
            }
            catch (Exception e)
            {
                log.Error(e);
                item = MinionCommands.Shockwave.First(f => (f.Name == "Shockwave") && (f.Version == "All")) as Minion.RemoteCommandImport;
            }
           

            await Uninstall(item);
        }

        private RelayCommand _installShockwaveCommand;
        public RelayCommand InstallShockwaveCommand { get { return _installShockwaveCommand ?? (_installShockwaveCommand = new RelayCommand(async () => await Install_Shockwave())); } }
        public async Task Install_Shockwave()
        {
            Minion.RemoteCommandImport item;
            item = MinionCommands.Shockwave.First(f => (f.Name == "Flash") && (f.Version != "All")) as Minion.RemoteCommandImport;
            await Install(item);
        }

        private async Task Install(Minion.RemoteCommandImport item)
        {
            Minion.RemoteCommand command = new Minion.RemoteCommand() { Name = item.Name, Version = item.Version, CopyFrom = item.Install_Copy, CopyTo = item.CopyTo, Command = item.Install_Command };
            //await Machine.Kill_Defaultss();
            var result = await Machine.Command(command, "Install");
            string vresult = await UpdateItemVersion(item);
                        
            if (vresult == "NOT INSTALLED")
                RaiseNoteWrite(string.Format("Ran Minion {0} install of version {1} but the install failed.", item.Name, item.Version));
            else if (vresult == "ERROR")
                RaiseNoteWrite(string.Format("Ran Minion {0} install but version lookup returned an error.", item.Name));
            else if (vresult == item.Version)
                RaiseNoteWrite(string.Format("Ran Minion {0} install and {0} version {1} is now installed.", item.Name, vresult));
            else
                RaiseNoteWrite(string.Format("Ran Minion {0} install but Minion was unable to verify install.", item.Name));
        }

        private async Task Uninstall(Minion.RemoteCommandImport item)
        {
            Minion.RemoteCommand command = new Minion.RemoteCommand() { Name = item.Name, Version = item.Version, CopyFrom = item.Uninstall_Copy, Command = item.Uninstall_Command };
            //await Machine.Kill_Defaultss();
            var result = await Machine.Command(command, "Uninstall");
            string vresult = await UpdateItemVersion(item);
                        
            if (vresult == "NOT INSTALLED")
                RaiseNoteWrite(string.Format("Ran Minion {0} uninstall and {0} is now no longer reported as installed.", item.Name));
            else if (vresult == "ERROR")
                RaiseNoteWrite(string.Format("Ran Minion {0} uninstall but lookup of current {0} verson returned an error.", item.Name));
            else
                RaiseNoteWrite(string.Format("Ran Minion {0} uninstall but was unable to verify uninstall.", item.Name));
        }

        private async Task<string> UpdateItemVersion(Minion.RemoteCommandImport item)
        {
            string result = string.Empty;
            if (item.Name.ToLower().Contains("java"))
                result = await Machine.Get_Java();
            else if (item.Name.ToLower().Contains("flash"))
                result = await Machine.Get_Flash();
            else if (item.Name.ToLower().Contains("shockwave"))
                result = await Machine.Get_Shockwave();
            else if (item.Name.ToLower().Contains("reader"))
                result = await Machine.Get_Reader();
            else if (item.Name.ToLower().Contains("quicktime"))
                result = await Machine.Get_Quicktime();
            return result;
        }
        
    }
}
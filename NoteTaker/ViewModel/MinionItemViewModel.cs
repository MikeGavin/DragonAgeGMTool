using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System;

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
        private void RaiseNoteWrite(string message)
        {
            if (NoteWrite != null) { NoteWrite(this, new Model.MinionArgs(message)); }
        }
        public event EventHandler<Model.MinionArgs> NoteWrite;
        public event EventHandler RequestClose;
        void RaiseRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new RelayCommand(RaiseRequestClose)); } }
        private Minion.MinionCommands _minionCommands;
        public Minion.MinionCommands MinionCommands { get { return _minionCommands; } set { _minionCommands = value; } }

        public Minion.EcotPC Machine { get; protected set; }
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>ef
        /// Initializes a new instance of the MvvmViewModel1 class.
        /// </summary>
        public MinionItemViewModel(IPAddress IP, Minion.MinionCommands commands)
        {
            Machine = new Minion.EcotPC(IP);
            MinionCommands = commands;
        }
        public string Title { get { return Machine.IPAddress.ToString(); } }
     
        #region SoftwareCommands

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
            Minion.RemoteCommand command = new Minion.RemoteCommand() { Name = item.Name, Version = item.Version, CopyFrom = item.Uninstall_Copy, Command = item.Uninstall_Command };

            await Machine.Kill_Defaultss();
            var result = await Machine.Command(command, "Uninstall");
            await Machine.Get_Java();
            if (Machine.Java == "NOT INSTALLED")
                RaiseNoteWrite("Ran automated Java uninstall and Java is no longer reported as installed.");
            else if (Machine.Java != "Error")
                RaiseNoteWrite("Ran automated Java uninstall but attempt to lookup current Java verson returned an error.");
            else
               RaiseNoteWrite("Ran automated Java uninstall but was unable to verify uninstall.");
        }


        private RelayCommand _installJavaCommand;
        public RelayCommand InstallJavaCommand { get { return _installJavaCommand ?? (_installJavaCommand = new RelayCommand(async () => await Install_Java())); } }

        public async Task Install_Java()
        {
            var sort = MinionCommands.Java.OrderByDescending(x => x.Version).ToList();
            var item = sort[1];
            //var item = _minionCommands.Java.First(j => (j.Name == "Java") && (j.Version.Contains("55"))) as Minion.RemoteCommandImport;
            Minion.RemoteCommand command = new Minion.RemoteCommand() { Name = item.Name, Version = item.Version, CopyFrom = item.Install_Copy, CopyTo = item.CopyTo, Command = item.Install_Command };

            await Machine.Kill_Defaultss();
            var result = await Machine.Command(command, "Install");
            await Machine.Get_Java();
            if (Machine.Java == "Error")
                RaiseNoteWrite("Ran automated Java install but attempt to lookup current Java verson returned an error.");
            else if (Machine.Java == "NOT INSTALLED")
                RaiseNoteWrite("Ran automated Java install but the install failed.");
            else
                RaiseNoteWrite(string.Format("Ran automated Java install and successfully installed Java {0}.", Machine.Java));

        }


        private RelayCommand<string> _remoteStartCommand;
        public RelayCommand<string> RemoteStartCommand { get { return _remoteStartCommand ?? (_remoteStartCommand = new RelayCommand<string>(async (param) => await RemoteStart(param))); } }

        public async Task RemoteStart(string command)
        {
            Minion.Tool.PSExec psexec = new Minion.Tool.PSExec(Machine.IPAddress, string.Format("-accepteula -i -s -h -d {0}", command));
            await psexec.Run();
        }
        
    }
}
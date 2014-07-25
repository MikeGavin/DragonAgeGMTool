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

        public event EventHandler RequestClose;
        void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new RelayCommand(OnRequestClose)); } }

        private Minion.MinionCommands _minionCommands;

        public Minion.EcotPC Machine { get; protected set; }
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>ef
        /// Initializes a new instance of the MvvmViewModel1 class.
        /// </summary>
        public MinionItemViewModel(IPAddress IP, Minion.MinionCommands commands)
        {
            Machine = new Minion.EcotPC(IP);
            _minionCommands = commands;
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

            var item = _minionCommands.Java.First(j => (j.Name == "Java") && (j.Version == Machine.Java)) as Minion.RemoteCommandImport;
            if (item == null)
            {
                item = _minionCommands.Java.First(j => (j.Name == "Java") && (j.Version == "All")) as Minion.RemoteCommandImport;
            }
            Minion.RemoteCommand command = new Minion.RemoteCommand() { Name = item.Name, Version = item.Version, Copy = item.Uninstall_Copy, Command = item.Uninstall_Command };

            await Machine.Kill_Defaultss();
            var result = await Machine.Command(command, "Uninstall");
            if (result == true)
            {
                throw new NotImplementedException();
            }
            else
                throw new NotImplementedException();

        }
        
    }
}
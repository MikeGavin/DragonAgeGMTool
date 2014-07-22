using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Minion;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace NoteTaker.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MinionItemViewModel : ViewModelBase
    {
        private MinionCommands _commands;
        public EcotPC RemoteMachine { get; protected set; }

        /// <summary>ef
        /// Initializes a new instance of the MvvmViewModel1 class.
        /// </summary>
        public MinionItemViewModel(IPAddress IP, MinionCommands commands)
        {
            RemoteMachine = new Minion.EcotPC(IP);
            _commands = commands;
            
        }

        #region SoftwareCommands

        private RelayCommand _getIECommand;
        public RelayCommand GetIECommand { get { return _getIECommand ?? (_getIECommand = new RelayCommand(async () => await RemoteMachine.Get_IE())); } }

        private RelayCommand _getJavaCommand;
        public RelayCommand GetJavaCommand { get { return _getJavaCommand ?? (_getJavaCommand = new RelayCommand(async () => await RemoteMachine.Get_Java())); } }

        private RelayCommand _getFlashCommand;
        public RelayCommand GetFlashCommand { get { return _getFlashCommand ?? (_getFlashCommand = new RelayCommand(async () => await RemoteMachine.Get_Flash())); } }

        private RelayCommand _getShockwaveCommand;
        public RelayCommand GetShockwaveCommand { get { return _getShockwaveCommand ?? (_getShockwaveCommand = new RelayCommand(async () => await RemoteMachine.Get_Shockwave())); } }

        private RelayCommand _getReaderCommand;
        public RelayCommand GetReaderCommand { get { return _getReaderCommand ?? (_getReaderCommand = new RelayCommand(async () => await RemoteMachine.Get_Reader())); } }

        private RelayCommand _getQuicktimeCommand;
        public RelayCommand GetQuicktimeCommand { get { return _getQuicktimeCommand ?? (_getQuicktimeCommand = new RelayCommand(async () => await RemoteMachine.Get_Quicktime())); } }

        #endregion

        #region ToolCommands

        private RelayCommand _openDamewareCommand;
        public RelayCommand OpenDamewareCommand { get { return _openDamewareCommand ?? (_openDamewareCommand = new RelayCommand(async () => await RemoteMachine.OpenDameware())); } }

        private RelayCommand _openCShareCommand;
        public RelayCommand OpenCShareCommand { get { return _openCShareCommand ?? (_openCShareCommand = new RelayCommand(async () => await RemoteMachine.OpenCShare())); } }
        #endregion

        private RelayCommand _uninstallJavaCommand;
        public RelayCommand UninstallJavaCommand { get { return _uninstallJavaCommand ?? (_uninstallJavaCommand = new RelayCommand(async () => await Uninstall_Java())); } }

        public async Task Uninstall_Java()
        {
            var item = _commands.Java.Where(j => (j.Name == "Java") && (j.Version == RemoteMachine.Java)) as Minion.RemoteCommandImport;
            if (item == null)
            {
                item = _commands.Java.Where(j => (j.Name == "Java") && (j.Version == "All")) as RemoteCommandImport;
            }
            Minion.RemoteCommand command = new Minion.RemoteCommand() { Name = item.Name, Version = item.Version, Copy = item.Uninstall_Copy, Command = item.Uninstall_Command };

            var temp = await RemoteMachine.Command(command, "Uninstall");
        }
        
    }
}
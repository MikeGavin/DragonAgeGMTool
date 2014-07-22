using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Net;

namespace NoteTaker.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ECOTPCViewModel : ViewModelBase
    {

        public Minion.EcotPC RemoteMachine { get; protected set; }


        /// <summary>
        /// Initializes a new instance of the MvvmViewModel1 class.
        /// </summary>
        public ECOTPCViewModel(IPAddress IP)
        {
            RemoteMachine = new Minion.EcotPC(IP);

        }

        #region SoftwareCommands

        private RelayCommand _getIECommand;
        public RelayCommand GetIECommand { get { return _getIECommand ?? (_getIECommand = new RelayCommand(GetIE)); } }

        private RelayCommand _getJavaCommand;
        public RelayCommand GetJavaCommand { get { return _getJavaCommand ?? (_getJavaCommand = new RelayCommand(GetJava)); } }

        private RelayCommand _getFlashCommand;
        public RelayCommand GetFlashCommand { get { return _getFlashCommand ?? (_getFlashCommand = new RelayCommand(GetFlash)); } }

        private RelayCommand _getShockwaveCommand;
        public RelayCommand GetShockwaveCommand { get { return _getShockwaveCommand ?? (_getShockwaveCommand = new RelayCommand(GetShockwave)); } }

        private RelayCommand _getReaderCommand;
        public RelayCommand GetReaderCommand { get { return _getReaderCommand ?? (_getReaderCommand = new RelayCommand(GetReader)); } }

        private RelayCommand _getQuicktimeCommand;
        public RelayCommand GetQuicktimeCommand { get { return _getQuicktimeCommand ?? (_getQuicktimeCommand = new RelayCommand(GetQuicktime)); } }

        public async void GetJava()
        {
            await RemoteMachine.Get_Java();
        }

        public async void GetIE()
        {
            await RemoteMachine.Get_IE();
        }

        public async void GetFlash()
        {
            await RemoteMachine.Get_Flash();
        }

        public async void GetShockwave()
        {
            await RemoteMachine.Get_Shockwave();
        }

        public async void GetReader()
        {
            await RemoteMachine.Get_Reader();
        }

        public async void GetQuicktime()
        {
            await RemoteMachine.Get_Quicktime();
        }

        #endregion

        #region ToolCommands

        private RelayCommand _openDamewareCommand;
        public RelayCommand OpenDamewareCommand { get { return _openDamewareCommand ?? (_openDamewareCommand = new RelayCommand(async () => await RemoteMachine.OpenDameware())); } }

        private RelayCommand _openCShareCommand;
        public RelayCommand OpenCShareCommand { get { return _openCShareCommand ?? (_openCShareCommand = new RelayCommand(async () => await RemoteMachine.OpenCShare())); } }

        #endregion
    }
}
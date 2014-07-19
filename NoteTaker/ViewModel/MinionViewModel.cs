using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Minion;
using System.Collections.ObjectModel;
using System.Net;
using System.Linq;

namespace NoteTaker.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MinionViewModel : ViewModelBase
    {

        public RelayCommand CloseCommand { get; set; }
        public RelayCommand ConnectCommand { get; set; }
        public RelayCommand GetIECommand { get; set; }
        public RelayCommand GetJavaCommand { get; set; }
        public RelayCommand GetFlashCommand { get; set; }
        public RelayCommand GetShockwaveCommand { get; set; }
        public RelayCommand GetReaderCommand { get; set; }
        public RelayCommand GetQuicktimeCommand { get; set; }
        private ObservableCollection<EcotPC> _MinionCollection = new ObservableCollection<EcotPC>();
        public ObservableCollection<EcotPC> MinionCollection { get { return _MinionCollection; } set { _MinionCollection = value; RaisePropertyChanged(); } }
        private EcotPC _selectedMinion;
        public EcotPC SelectedMinion { get { return _selectedMinion; } set { _selectedMinion = value; RaisePropertyChanged(); } }
        private RelayCommand _minion_CloseCommand;
        public RelayCommand Minion_CloseCommand { get { return _minion_CloseCommand ?? (_minion_CloseCommand = new RelayCommand(CloseMinion)); } }
        private RelayCommand _minion_ConnectCommand;
        public RelayCommand Minion_ConnectCommand { get { return _minion_ConnectCommand ?? (_minion_ConnectCommand = new RelayCommand(ConnectMinion)); } }
        private string _NewMinionIPAddress = "10.39.";
        public string NewMinionIPAddress { get { return _NewMinionIPAddress; } set { _NewMinionIPAddress = value; RaisePropertyChanged(); } }
        private bool _minionIPInputEnabeled = true;
        public bool MinionIPInputEnabeled { get { return _minionIPInputEnabeled; } set { _minionIPInputEnabeled = value; RaisePropertyChanged(); } }
        private bool _minionConnecting = false;
        public bool MinionConnecting { get { return _minionConnecting; } set { _minionConnecting = value; RaisePropertyChanged(); } }
        private bool isExpanded;
        public bool IsExpanded { get { return isExpanded; } set { isExpanded = value; RaisePropertyChanged(); } }
        private EcotPC _remotePC;
        public EcotPC RemotePC { get { return _remotePC; } set { _remotePC = value; RaisePropertyChanged(); } }
        //private IPAddress _iPAddress;
        //public IPAddress IPAddress { get { return _iPAddress; } set { _iPAddress = value; RaisePropertyChanged(); } }
        
        /// <summary>
        /// Initializes a new instance of the MvvmViewModel1 class.
        /// </summary>
        public MinionViewModel()
        {

        }

        public void CloseMinion()
        {
            MinionCollection.Remove(SelectedMinion);
            if (MinionCollection.Count <= 0)
                IsExpanded = false;
        }

        public async void ConnectMinion()
        {
            MinionConnecting = true;
            if (NewMinionIPAddress == null) { return; }
            MinionIPInputEnabeled = false;
            if (Minion.Tool.IP.IPv4_Check(NewMinionIPAddress) == true)
            {

                if (await System.Threading.Tasks.Task.Run(() => Minion.Tool.IP.Ping(IPAddress.Parse(NewMinionIPAddress)) == true))
                {
                    MinionCollection.Add(new EcotPC(IPAddress.Parse(NewMinionIPAddress)));
                    SelectedMinion = MinionCollection.Last();
                    NewMinionIPAddress = "10.39.";
                    IsExpanded = true;

                    var test = MinionCollection.Last().GetType().GetProperties();
                }
                else
                {
                    Helpers.MetroMessageBox.Show("Error!", "IP is unreachable!");
                }
            }
            else
            {
                Helpers.MetroMessageBox.Show("Error!", "Invalid IP address format!");
            }
            MinionIPInputEnabeled = true;
            MinionConnecting = false;
        }

        public async void GetJava()
        {
            await RemotePC.Get_Java();
        }

        public async void GetIE()
        {
            await RemotePC.Get_IE();
        }

        public async void GetFlash()
        {
            await RemotePC.Get_Flash();
        }

        public async void GetShockwave()
        {
            await RemotePC.Get_Shockwave();
        }

        public async void GetReader()
        {
            await RemotePC.Get_Reader();
        }

        public async void GetQuicktime()
        {
            await RemotePC.Get_Quicktime();
        }
    }
}
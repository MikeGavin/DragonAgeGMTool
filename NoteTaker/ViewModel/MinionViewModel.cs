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
        /// <summary>
        /// Initializes a new instance of the MvvmViewModel1 class.
        /// </summary>
        public MinionViewModel()
        {

        }

        private ObservableCollection<EcotPC> _MinionCollection = new ObservableCollection<EcotPC>();
        public ObservableCollection<EcotPC> MinionCollection { get { return _MinionCollection; } set { _MinionCollection = value; RaisePropertyChanged(); } }
        private EcotPC _selectedMinion;
        public EcotPC SelectedMinion { get { return _selectedMinion; } set { _selectedMinion = value; RaisePropertyChanged(); } }

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new RelayCommand(CloseMinion)); } }        
        private RelayCommand _addCommand;
        public RelayCommand AddCommand { get { return _addCommand ?? (_addCommand = new RelayCommand(AddMinion)); } }

        public void CloseMinion()
        {
            MinionCollection.Remove(SelectedMinion);
            if (MinionCollection.Count <= 0)
                IsExpanded = false;
        }

        public async void AddMinion()
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
            await SelectedMinion.Get_Java();
        }

        public async void GetIE()
        {
            await SelectedMinion.Get_IE();
        }

        public async void GetFlash()
        {
            await SelectedMinion.Get_Flash();
        }

        public async void GetShockwave()
        {
            await SelectedMinion.Get_Shockwave();
        }

        public async void GetReader()
        {
            await SelectedMinion.Get_Reader();
        }

        public async void GetQuicktime()
        {
            await SelectedMinion.Get_Quicktime();
        }

        #endregion

        #region ToolCommands

        private RelayCommand _openDamewareCommand;
        public RelayCommand OpenDamewareCommand { get { return _openDamewareCommand ?? (_openDamewareCommand = new RelayCommand(async () => await SelectedMinion.OpenDameware())); } }
        
        private RelayCommand _openCShareCommand;
        public RelayCommand OpenCShareCommand { get { return _openCShareCommand ?? (_openCShareCommand = new RelayCommand(async () => await SelectedMinion.OpenCShare())); } }

        #endregion

        #region ViewProperties
        private string _NewMinionIPAddress = "10.39.";
        public string NewMinionIPAddress { get { return _NewMinionIPAddress; } set { _NewMinionIPAddress = value; RaisePropertyChanged(); } }
        private bool _minionIPInputEnabeled = true;
        public bool MinionIPInputEnabeled { get { return _minionIPInputEnabeled; } set { _minionIPInputEnabeled = value; RaisePropertyChanged(); } }
        private bool _minionConnecting = false;
        public bool MinionConnecting { get { return _minionConnecting; } set { _minionConnecting = value; RaisePropertyChanged(); } }
        private bool isExpanded;
        public bool IsExpanded { get { return isExpanded; } set { isExpanded = value; RaisePropertyChanged(); } } 
        #endregion

    }
}
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Minion;
using System.Collections.ObjectModel;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

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

        private ObservableCollection<MinionItemViewModel> _MinionCollection = new ObservableCollection<MinionItemViewModel>();
        public ObservableCollection<MinionItemViewModel> MinionCollection { get { return _MinionCollection; } set { _MinionCollection = value; RaisePropertyChanged(); } }
        private MinionItemViewModel _selectedMinion;
        public MinionItemViewModel SelectedMinion { get { return _selectedMinion; } set { _selectedMinion = value; RaisePropertyChanged(); } }

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new RelayCommand(CloseMinionItem)); } }        
        private RelayCommand _addCommand;
        public RelayCommand AddCommand { get { return _addCommand ?? (_addCommand = new RelayCommand(AddMinionItem)); } }



        public void CloseMinionItem()
        {
            MinionCollection.Remove(SelectedMinion);
            if (MinionCollection.Count <= 0)
                IsExpanded = false;
        }

        public async void AddMinionItem()
        {
            MinionConnecting = true;
            if (NewMinionIPAddress == null) { return; }
            MinionIPInputEnabeled = false;
            if (Minion.Tool.IP.IPv4_Check(NewMinionIPAddress) == true)
            {

                if (await System.Threading.Tasks.Task.Run(() => Minion.Tool.IP.Ping(IPAddress.Parse(NewMinionIPAddress)) == true))
                {
                    MinionCollection.Add(new MinionItemViewModel(IPAddress.Parse(NewMinionIPAddress)));
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
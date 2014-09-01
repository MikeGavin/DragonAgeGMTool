using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Minion;
using System.Collections.ObjectModel;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System;
using Minion.ListItems;
using System.Text.RegularExpressions;
using Scrivener.Model;

namespace Scrivener.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MinionViewModel : ViewModelBase
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public MinionViewModel(ObservableCollection<MinionCommandItem> commands)
        {
            SetMinionInputDefault();
            MinionCollection.CollectionChanged += OnMinionItemsChanged;
            _minionCommands = commands;
            //debuging

            //_NewMinionIPAddress = "192.168.1.114";
            //AddMinionItem();
        }

        private RoleItem _role;
        #region Public Properties
        private string _NewMinionIPAddress;
        public string NewMinionIPAddress { get { return _NewMinionIPAddress; } set { _NewMinionIPAddress = value; RaisePropertyChanged(); } }
        private bool _minionIPInputEnabeled = true;
        public bool MinionIPInputEnabeled { get { return _minionIPInputEnabeled; } set { _minionIPInputEnabeled = value; RaisePropertyChanged(); } }
        private bool _minionConnecting = false;
        public bool MinionConnecting { get { return _minionConnecting; } set { _minionConnecting = value; RaisePropertyChanged(); } }
        private bool isExpanded;
        public bool IsExpanded { get { return isExpanded; } set { isExpanded = value; RaisePropertyChanged(); } }
        #endregion

        //Commands from Constructor
        private ObservableCollection<MinionCommandItem> _minionCommands;

        //Collection of Minion Items
        private ObservableCollection<MinionItemViewModel> _MinionCollection = new ObservableCollection<MinionItemViewModel>();
        public ObservableCollection<MinionItemViewModel> MinionCollection { get { return _MinionCollection; } set { _MinionCollection = value; RaisePropertyChanged(); } }
        void OnMinionItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (MinionItemViewModel minionItem in e.NewItems)
                {
                    minionItem.RequestClose += this.OnItemRequestClose;
                }

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (MinionItemViewModel minionItem in e.OldItems)
                {
                    minionItem.RequestClose -= this.OnItemRequestClose;
                }
        }
        private MinionItemViewModel _selectedMinion;
        public MinionItemViewModel SelectedMinion { get { return _selectedMinion; } set { _selectedMinion = value; RaisePropertyChanged(); } }
   
        //Add minion instances
        private RelayCommand _addCommand;
        public RelayCommand AddCommand { get { return _addCommand ?? (_addCommand = new RelayCommand(AddMinionItem)); } }
        public async void AddMinionItem()
        {
            MinionConnecting = true;
            if (NewMinionIPAddress == null) { return; }
            NewMinionIPAddress = NewMinionIPAddress.Trim();
            MinionIPInputEnabeled = false;
            Regex reg = new Regex("[a-zA-Z]+");

            if (reg.IsMatch(NewMinionIPAddress))
            {                
                try
                {
                    var returnedIPs = await Dns.GetHostAddressesAsync(NewMinionIPAddress);
                    foreach (IPAddress ip in returnedIPs)
                    {
                        if (await System.Threading.Tasks.Task.Run(() => Minion.Tool.IP.Ping(ip) == true))
                        {
                            try
                            {
                                OpenMinion(ip);
                                break;
                            }
                            catch (Exception e)
                            {
                                log.Error(e);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    var temp = Helpers.MetroMessageBox.Show("DNS Error!", e.Message.ToString());
                }
            }
            else if (Minion.Tool.IP.IPv4_Check(NewMinionIPAddress) == true)
            {
                if (await System.Threading.Tasks.Task.Run(() => Minion.Tool.IP.Ping(IPAddress.Parse(NewMinionIPAddress)) == true))
                {
                    try
                    {
                        OpenMinion(IPAddress.Parse(NewMinionIPAddress));
                    }
                    catch (Exception e)
                    {
                        Model.ExceptionReporting.Email(e);
                        var temp = Scrivener.Helpers.MetroMessageBox.Show("ERMAHGERD ERER!", e.ToString());
                    }
                }
                else
                {
                    await Helpers.MetroMessageBox.Show("Error!", "IP is unreachable!");
                }
            }
            else
            {
                await Helpers.MetroMessageBox.Show("Error!", "Invalid IP address format!");
            }
            MinionIPInputEnabeled = true;
            MinionConnecting = false;
        }

        private void OpenMinion(IPAddress ip)
        {
            MinionCollection.Add(new MinionItemViewModel(ip, _minionCommands));
            SelectedMinion = MinionCollection.Last();
            SetMinionInputDefault();
            IsExpanded = true;
        }

        private void SetMinionInputDefault()
        {
            var loc = new ViewModelLocator();
            if (loc.Main.CurrentRole.Name == "EdTech")
            {
                NewMinionIPAddress = string.Empty;
            }
            else
            {
                NewMinionIPAddress = "10.39.";
            }
        }

        //remove minion instance on self request
        void OnItemRequestClose(object sender, EventArgs e)
        {
            MinionItemViewModel minionItem = sender as MinionItemViewModel;
            MinionCollection.Remove(minionItem);
            if (MinionCollection.Count <= 0)
                IsExpanded = false;
        }
    }
}
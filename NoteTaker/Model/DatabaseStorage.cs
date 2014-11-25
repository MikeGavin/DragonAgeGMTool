using Minion;
using Minion.ListItems;
using Scrivener.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{

    public sealed class DatabaseStorage : INotifyPropertyChanged
    {
        // Thread safe Singleton with fully lazy instantiation á la Jon Skeet:
        // http://csharpindepth.com/Articles/General/Singleton.aspx
        DatabaseStorage()
        {
            LoadRoles();
        }
        public static DatabaseStorage Instance
        {
            get
            {
                return Nested.instance;
            }
        }
        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
               
            }

            internal static readonly DatabaseStorage instance = new DatabaseStorage();
        }

        internal void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private DataBaseReader DataBaseReader = new DataBaseReader();

        private ObservableCollection<RoleItem> _roles;
        public ObservableCollection<RoleItem> Roles { get { return _roles; } private set { _roles = value; RaisePropertyChanged(); } }
        private bool StringToBool(string x)
        {
            bool result = false;
            if (x == "1")
            {
                result = true;
            }
            return result;
        }
        private async Task LoadRoles()
        {
            //clear if reloading.
            if (Roles != null)
            {
                Roles = null;
            }
            Roles = await DataBaseReader.ReturnRoles();
        }

        private RoleItem _role;
        public RoleItem Role { get { return _role; } set { _role = value; RaisePropertyChanged(); LoadAll(); } }

        private QuickItem _quickitems;
        public QuickItem QuickItems { get { return _quickitems; } private set { _quickitems = value; RaisePropertyChanged(); } }
        public async Task LoadQuickItems()
        {
            if (Sites != null)
            {
                Sites = null;
            }
            QuickItems = await DataBaseReader.ReturnQuickItems(Role);
        }

        private Siteitem _sites;
        public Siteitem Sites { get { return _sites; } private set { _sites = value; RaisePropertyChanged(); } }
        public async Task LoadSites()
        {
            if (Sites != null)
            {
                Sites = null;
            }
            Sites = await DataBaseReader.ReturnSiteItems(Role);
        }

        private ObservableCollection<MinionCommandItem> _minionCommands;
        public ObservableCollection<MinionCommandItem> MinionCommands { get { return _minionCommands; } private set { _minionCommands = value; RaisePropertyChanged(); } }
        public async Task LoadMinionCommands()
        {
            if (Sites != null)
            {
                Sites = null;
            }
            MinionCommands = await DataBaseReader.ReturnMinionCommands(Role);
        }

        private HistoryItem _HistoryItems;
        public HistoryItem HistoryItems { get { return _HistoryItems; } private set { _HistoryItems = value; RaisePropertyChanged(); } }
        public async Task LoadHistoryItems()
        {
            HistoryItems = await DataBaseReader.ReturnHistory();
        }

        public async Task LoadAll()
        {
            await LoadMinionCommands();
            await LoadHistoryItems();
            await LoadQuickItems();
            await LoadSites(); //For some reason loading this item first causes the binding to not work.        
        }
      
    }
}

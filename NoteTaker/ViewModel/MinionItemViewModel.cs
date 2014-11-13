using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Minion;
using System.Collections.Generic;
using NLog.Targets;
using NLog;
using Scrivener.Helpers;
using System.Windows.Data;
using Minion.ListItems;
using Scrivener.Model;
using System.IO;
using System.Text.RegularExpressions;

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
        #region Screen Logging
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        
        private ObservableCollection<string> _logCollection;
        public ObservableCollection<string> LogCollection { get { return _logCollection ?? (_logCollection = new ObservableCollection<string>()); } set { _logCollection = value; RaisePropertyChanged(); } }
        //used to allow other threads to write to collection
        private object _syncLock = new object();

        void Machine_EventLogged(object sender, Minion.LogEventArgs e)
        {
            if (LogCollection.Count >= 50)
            {
                LogCollection.RemoveAt(LogCollection.Count - 1);
            }
            if (e.Log >= Minion.log.Info)
            {
                LogCollection.Insert(0, string.Format("{0} | {1} | {2}", e.Time.ToLongTimeString(), e.Log.ToString().ToUpper(), e.Message.ToString()));
            }
        }
        #endregion

        public event EventHandler<PasteEventArgs> PasteRequest;
        private void OnPasteRequest(string p)
        {
            if (string.IsNullOrEmpty(p) || p == "Updating...") { return; }
            if (PasteRequest != null)
            {
                PasteRequest( this, new PasteEventArgs() { PasteData = p} );
            }
        }
        public RelayCommand<string> PasteCommand { get { return new RelayCommand<string>((param) => OnPasteRequest(param)); } }      

        //Constructor
        public MinionItemViewModel(IPAddress IP)
        {
            Machine = new Minion.EcotPC(IP);
            //DataB.MinionCommands = commands;
            BindingOperations.EnableCollectionSynchronization(LogCollection, _syncLock);
            Machine.EventLogged += Machine_EventLogged;
            Machine.PropertyChanged += Machine_PropertyChanged;
            DataB = DatabaseStorage.Instance;
            //DataBaseWatcher.DataBaseUpdated += DataBaseWatcher_DataBaseUpdated;
            //var _ieCommands = new ObservableCollection<MinionCommandItem>(DataB.MinionCommands.Where(i => (i.Name == "Update") && (i.Action == "Install")));           
        }
        private DatabaseStorage DataB { get; set; }
        //private async void DataBaseWatcher_DataBaseUpdated(object sender, FileSystemEventArgs e)
        //{
        //    if (e.Name.ToLower().Contains("scrivener.sqlite"))
        //    {
        //        log.Debug("Updating MinionCommands on Minion: {0}", Title);
        //        try
        //        {
        //            DataB.MinionCommands = await DataBaseReader.ReturnMinionCommands(Properties.Settings.Default.Role_Current);
        //            SetIECommands();
        //        }
        //        catch(Exception ex)
        //        {
        //            log.Error(ex.Message);
        //        }
        //    }
        //}

        //Item title pulling IP address
        public string Title { get { return Machine.IPAddress.ToString(); } }        
        //Instance of ECOTPC Item
        public Minion.EcotPC Machine { get; protected set; }
        //Commands from DB
        //private ObservableCollection<MinionCommandItem> DataB.MinionCommands;
        
        
        //Event to raise and pass notewrite event
        public event EventHandler<Model.MinionArgs> NoteWrite;
        private void RaiseNoteWrite(string message) //Used to send message for writting to the note.
        {
            if (NoteWrite != null) { NoteWrite(this, new Model.MinionArgs(message)); }
        }

        #region Close Item
        public event EventHandler RequestClose;
        void RaiseRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new RelayCommand(RaiseRequestClose)); } }
        #endregion
                      
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
        
        
        private ObservableCollection<MinionCommandItem> _minionStartCommands;
        public ObservableCollection<MinionCommandItem> MinionStartCommands { get { return _minionStartCommands ?? (_minionStartCommands = new ObservableCollection<MinionCommandItem>((from item in DataB.MinionCommands where item.Action == "Start" select item).ToList())); } protected set { _minionStartCommands = value; RaisePropertyChanged(); } }
        private RelayCommand<MinionCommandItem> _remoteStartCommand;
        public RelayCommand<MinionCommandItem> RemoteStartCommand { get { return _remoteStartCommand ?? (_remoteStartCommand = new RelayCommand<MinionCommandItem>(async (param) => await RemoteStart(param))); } protected set { _remoteStartCommand = value; RaisePropertyChanged(); } }
        public async Task RemoteStart(MinionCommandItem command)
        {
            await Machine.Command(command);
        }

        private RelayCommand _rebootCommand;
        public RelayCommand RebootCommand { get { return _rebootCommand ?? (_rebootCommand = new RelayCommand(async () => await Machine.Reboot())); } }

        private RelayCommand _fixJNLPCommand;
        public RelayCommand FixJNLPCommand { get { return _fixJNLPCommand ?? (_fixJNLPCommand = new RelayCommand(async () => await Machine.FixJNLPAssoication())); } }

        private RelayCommand _disableProfileWipeCommand;
        public RelayCommand DisableProfileWipeCommand { get { return _disableProfileWipeCommand ?? (_disableProfileWipeCommand = new RelayCommand(async () => await Machine.ProfileWipe_Disable())); } }

        private RelayCommand _enableProfileWipeCommand;
        public RelayCommand EnableProfileWipeCommand { get { return _enableProfileWipeCommand ?? (_enableProfileWipeCommand = new RelayCommand(async () => await Machine.ProfileWipe_Enable())); } }

        private RelayCommand _backupProfileCommand;
        public RelayCommand BackupProfileCommand { get { return _backupProfileCommand ?? (_backupProfileCommand = new RelayCommand(async () => await Machine.ProfileBackup())); } }

        private RelayCommand _fixBackgroundCommand;
        public RelayCommand FixBackgroundCommand { get { return _fixBackgroundCommand ?? (_fixBackgroundCommand = new RelayCommand(async () => await Machine.FixBackGround())); } }

        private RelayCommand _openHDriveCommand;
        public RelayCommand OpenHDriveCommand { get { return _openHDriveCommand ?? (_openHDriveCommand = new RelayCommand(async () => await Machine.OpenHDrive())); } }

        private RelayCommand _fileCleanupCommand;
        public RelayCommand FileCleanupCommand { get { return _fileCleanupCommand ?? (_fileCleanupCommand = new RelayCommand(async () => await Machine.FileCleanup())); } }

        private RelayCommand _openDamewareCommand;
        public RelayCommand OpenDamewareCommand { get { return _openDamewareCommand ?? (_openDamewareCommand = new RelayCommand(async () => await Machine.OpenDameware())); } }

        private RelayCommand _openCShareCommand;
        public RelayCommand OpenCShareCommand { get { return _openCShareCommand ?? (_openCShareCommand = new RelayCommand(async () => await Machine.OpenCShare())); } }       

        private RelayCommand _defaultKillsCommand;
        public RelayCommand DefaultKillsCommand { get { return _defaultKillsCommand ?? (_defaultKillsCommand = new RelayCommand(async () => await Machine.KillDefaults())); } }
             

        #endregion

        #region Software Command Actions

        //Handels dynamic population of IE context menu commands
        private ObservableCollection<MinionCommandItem> _ieCommands;
        public ObservableCollection<MinionCommandItem> IECommands { get { return _ieCommands; } set { _ieCommands = value; RaisePropertyChanged(); } }
        private RelayCommand<MinionCommandItem> _runItemCommand;
        public RelayCommand<MinionCommandItem> RunItemCommand { get { return _runItemCommand ?? (_runItemCommand = new RelayCommand<MinionCommandItem>(async (param) => await RunCommandItem(param))); } }
        void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Listens for Bitness setting to populate context menu.
            if (e.PropertyName == "OSBit" && Machine.OSBit != null)
            {
                SetIECommands();
                SetJavaCommands();
            }
        }

        private void SetJavaCommands()
        {
            JavaCommands = new ObservableCollection<MinionCommandItem>(DataB.MinionCommands.Where(i => ((i.Name == "Java") )).OrderBy(i => i.Version));
           
        }

        private ObservableCollection<MinionCommandItem> _javaCommands;
        public ObservableCollection<MinionCommandItem> JavaCommands { get { return _javaCommands; } set { _javaCommands = value; RaisePropertyChanged(); } }
        private void SetIECommands()
        {
            IECommands = new ObservableCollection<MinionCommandItem>(DataB.MinionCommands.Where(i => ((i.Name == "Update") || (i.Name == "IE")) && (i.Action == "Install") && (i.Bit == Machine.OSBit.Remove(2))));
        }

        protected string _selectedjava;
        public string SelectedJava { get { return _selectedjava; } set { _selectedjava = value; RaisePropertyChanged(); } }
        private RelayCommand<string> _uninstallJavaCommand;
        public RelayCommand<string> UninstallJavaCommand { get { return _uninstallJavaCommand ?? (_uninstallJavaCommand = new RelayCommand<string>(async (param) => await Uninstall_Java(param))); } }
        public async Task Uninstall_Java(string data)
        {
            MinionCommandItem item;
            try
            {
                data = data.Replace("-Bit", string.Empty);
                string[] x = System.Text.RegularExpressions.Regex.Split(data, ", ");
                string current = x[0];
                string bit = x[1];
                item = DataB.MinionCommands.First(j => (j.Name == "Java") && (j.Action == "Uninstall") && (j.Version == current) && (j.Bit == bit)) as MinionCommandItem;               
            }
            catch (Exception e)
            {
                log.Debug(e);
                log.Error(string.Format("Could not find version, uninstalling all."));
                item = DataB.MinionCommands.First(j => (j.Name == "Java") && (j.Action == "Uninstall") && (j.Version == "All")) as MinionCommandItem;              
            }
            await RunCommandItem(item);
        }

        private RelayCommand _installJavaCommand;
        public RelayCommand InstallJavaCommand { get { return _installJavaCommand ?? (_installJavaCommand = new RelayCommand(async () => await Install_Java())); } }
        public async Task Install_Java()
        {
            try
            {
                var items = DataB.MinionCommands.Where((j) => j.Name == "Java" && j.Action == "Install").ToList<MinionCommandItem>();
                var item = new MinionCommandItem();
                foreach (var i in items)
                {
                    if (item.Version == null) { item = i; }

                    if (Convert.ToInt32(Regex.Replace(i.Version, @"[^\d]", string.Empty)) > Convert.ToInt32(Regex.Replace(item.Version, @"[^\d]", string.Empty)))
                    {
                        item = i;
                    }
                }
                await RunCommandItem(item);
                
                //item = DataB.MinionCommands.First((j) => j.Action == "Fix" && j.Name == "Java");
                //await RunCommandItem(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                var temp = MetroMessageBox.Show("ERMAHGERD ERER!", e.ToString());
                return;
            }
        }

        private RelayCommand _uninstallFlashCommand;
        public RelayCommand UninstallFlashCommand { get { return _uninstallFlashCommand ?? (_uninstallFlashCommand = new RelayCommand(async () => await Uninstall_Flash())); } }
        public async Task Uninstall_Flash()
        {
            try
            {
                MinionCommandItem item;

                item = DataB.MinionCommands.First(f => (f.Name == "Flash") && (f.Action == "Uninstall") && (f.Version == "All")) as MinionCommandItem;

                await RunCommandItem(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                var temp = MetroMessageBox.Show("ERMAHGERD ERER!", e.ToString());
                return;
            }


        }

        private RelayCommand _installFlashCommand;
        public RelayCommand InstallFlashCommand { get { return _installFlashCommand ?? (_installFlashCommand = new RelayCommand(async () => await Install_Flash())); } }
        public async Task Install_Flash()
        {
            try
            {
                var items = DataB.MinionCommands.Where((j) => j.Name == "Flash" && j.Action == "Install").ToList<MinionCommandItem>();
                var item = new MinionCommandItem();
                foreach (var i in items)
                {
                    if (item.Version == null) { item = i; }

                    if (Convert.ToInt32(i.Version.Replace(".", string.Empty)) > Convert.ToInt32(item.Version.Replace(".", string.Empty)))
                    {
                        item = i;
                    }
                }
                await RunCommandItem(item);
                //MinionCommandItem item;
                //item = DataB.MinionCommands.First(f => (f.Name == "Flash") && (f.Action == "Install") && (f.Version != "All")) as MinionCommandItem;
                //await RunCommandItem(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                var temp = MetroMessageBox.Show("ERMAHGERD ERER!", e.ToString());
                return;
            }
        }

        private RelayCommand _uninstallShockwaveCommand;
        public RelayCommand UninstallShockwaveCommand { get { return _uninstallShockwaveCommand ?? (_uninstallShockwaveCommand = new RelayCommand(async () => await Uninstall_Shockwave())); } }
        public async Task Uninstall_Shockwave()
        {
            MinionCommandItem item;
            try
            {
                item = DataB.MinionCommands.First(f => (f.Name == "Shockwave") && (f.Action == "Uninstall") && (f.Version == "All")) as MinionCommandItem;
            }
            catch (Exception e)
            {
                log.Error(e);
                item = DataB.MinionCommands.First(f => (f.Name == "Shockwave") && (f.Action == "Uninstall") && (f.Version == "WMIC")) as MinionCommandItem;
            }

            await RunCommandItem(item);
        }

        private RelayCommand _installShockwaveCommand;
        public RelayCommand InstallShockwaveCommand { get { return _installShockwaveCommand ?? (_installShockwaveCommand = new RelayCommand(async () => await Install_Shockwave())); } }
        public async Task Install_Shockwave()
        {
            try
            {
                MinionCommandItem item;
                item = DataB.MinionCommands.First(f => (f.Name == "Shockwave") && (f.Action == "Install")) as MinionCommandItem;
                await RunCommandItem(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                var temp = MetroMessageBox.Show("ERMAHGERD ERER!", e.ToString());
            }
        }

        private RelayCommand _uninstallReaderCommand;
        public RelayCommand UninstallReaderCommand { get { return _uninstallReaderCommand ?? (_uninstallReaderCommand = new RelayCommand(async () => await Uninstall_Reader())); } }
        public async Task Uninstall_Reader()
        {
            MinionCommandItem item;
            try
            {
                item = DataB.MinionCommands.First(f => (f.Name == "Reader") && (f.Action == "Uninstall") && (f.Version == "All")) as MinionCommandItem;
            }
            catch (Exception e)
            {
                log.Error(e);
                item = DataB.MinionCommands.First(f => (f.Name == "Reader") && (f.Action == "Uninstall") && (f.Version == "WMIC")) as MinionCommandItem;
            }

            await RunCommandItem(item);
        }

        private RelayCommand _installReaderCommand;
        public RelayCommand InstallReaderCommand { get { return _installReaderCommand ?? (_installReaderCommand = new RelayCommand(async () => await Install_Reader())); } }
        public async Task Install_Reader()
        {
            try
            {
                MinionCommandItem item;
                item = DataB.MinionCommands.First(f => (f.Name == "Reader") && (f.Action == "Install")) as MinionCommandItem;
                await RunCommandItem(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                var temp = MetroMessageBox.Show("ERMAHGERD ERER!", e.ToString());
            }
        }

        #endregion

        private async Task RunCommandItem(MinionCommandItem command)
        {
            await Machine.KillDefaults();
            var result = await Machine.Command(command);
            string vresult = await UpdateItemVersion(command);

            if (command.Name != "Java")
            {
                if (command.Action == "Uninstall")
                {
                    if (vresult == "NOT INSTALLED")
                        RaiseNoteWrite(string.Format("Ran Minion {0} uninstall and {0} is now no longer reported as installed.", command.Name));
                    else if (vresult == "ERROR")
                        RaiseNoteWrite(string.Format("Ran Minion {0} uninstall but lookup of current {0} verson returned an error.", command.Name));
                    else
                        RaiseNoteWrite(string.Format("Ran Minion {0} uninstall but was unable to verify uninstall.", command.Name));
                }
                else if (command.Action == "Install")
                {
                    if (vresult == "NOT INSTALLED")
                        RaiseNoteWrite(string.Format("Ran Minion {0} install of version {1} but the install failed.", command.Name, command.Version));
                    else if (vresult == "ERROR")
                        RaiseNoteWrite(string.Format("Ran Minion {0} install but version lookup returned an error.", command.Name));
                    else if (vresult.Contains(command.Version))
                        RaiseNoteWrite(string.Format("Ran Minion {0} install and {0} version {1} is now installed.", command.Name, vresult));
                    else
                        RaiseNoteWrite(string.Format("Ran Minion {0} install but Minion was unable to verify install.", command.Name));
                }
            }
            else
            {
                if (command.Action == "Uninstall" && command.Version != "All")
                {
                    if (!Machine.Javas.Contains(command.Version + ", " + command.Bit + "-Bit"))
                    {
                        RaiseNoteWrite(string.Format("Ran Minion {0} uninstall, version {1} is no longer installed.", command.Name, command.Version));
                    }
                    else
                    {
                        RaiseNoteWrite(string.Format("Ran Minion {0} uninstall, unable to verify version {1} was uninstalled.", command.Name, command.Version));
                    }
                }
                else if (command.Action == "Uninstall" && command.Version == "All")
                {
                    if (!Machine.Javas.Contains(command.Version + ", " + command.Bit + "-Bit"))
                    {
                        RaiseNoteWrite(string.Format("Ran Minion {0} uninstall, {1} versions are no longer installed.", command.Name, command.Version));
                    }
                    else
                    {
                        RaiseNoteWrite(string.Format("Ran Minion {0} uninstall, unable to verify {1} version were uninstalled.", command.Name, command.Version));
                    }
                }
                else if (command.Action == "Install")
                {
                    if (Machine.Javas.Contains(command.Version + ", " + command.Bit + "-Bit"))
                    {
                        RaiseNoteWrite(string.Format("Ran Minion {0} install, version {1} is now installed.", command.Name, command.Version));
                    }
                    else
                    {
                        RaiseNoteWrite(string.Format("Ran Minion {0} install, unable to verify version {1} was installed.", command.Name, command.Version));
                    }                    
                }
            }

            //if (command.Action == "Uninstall")
            //{
            //    if (vresult == "NOT INSTALLED")
            //        RaiseNoteWrite(string.Format("Ran Minion {0} uninstall and {0} is now no longer reported as installed.", command.Name));
            //    else if (vresult == "ERROR")
            //        RaiseNoteWrite(string.Format("Ran Minion {0} uninstall but lookup of current {0} verson returned an error.", command.Name));
            //    else
            //        RaiseNoteWrite(string.Format("Ran Minion {0} uninstall but was unable to verify uninstall.", command.Name));
            //}
            //else if (command.Action == "Install")
            //{
            //    if (vresult == "NOT INSTALLED")
            //        RaiseNoteWrite(string.Format("Ran Minion {0} install of version {1} but the install failed.", command.Name, command.Version));
            //    else if (vresult == "ERROR")
            //        RaiseNoteWrite(string.Format("Ran Minion {0} install but version lookup returned an error.", command.Name));
            //    else if (vresult.Contains(command.Version))
            //        RaiseNoteWrite(string.Format("Ran Minion {0} install and {0} version {1} is now installed.", command.Name, vresult));
            //    else
            //        RaiseNoteWrite(string.Format("Ran Minion {0} install but Minion was unable to verify install.", command.Name));
            //}

        }

        private async Task<string> UpdateItemVersion(MinionCommandItem item)
        {
            //Machine.Javas

            string result = string.Empty;
            if (item.Name.ToLower().Contains("java"))
            {
                string ver;
                await Machine.Get_Java();


                //if (item.Bit=="64")
                //{
                //    //ver = Machine.Java64;
                //}
                //else
                //{
                //    //ver = Machine.Java32;
                //}
                ////result = ver;
            }
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
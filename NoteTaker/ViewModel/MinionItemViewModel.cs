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
        
        //Constructor
        public MinionItemViewModel(IPAddress IP, ObservableCollection<MinionCommandItem> commands)
        {
            Machine = new Minion.EcotPC(IP);
            _minionCommands = commands;
            BindingOperations.EnableCollectionSynchronization(LogCollection, _syncLock);
            Machine.EventLogged += Machine_EventLogged;
            Machine.PropertyChanged += Machine_PropertyChanged;

            //var _ieCommands = new ObservableCollection<MinionCommandItem>(_minionCommands.Where(i => (i.Name == "Update") && (i.Action == "Install")));           
        }

        //Item title pulling IP address
        public string Title { get { return Machine.IPAddress.ToString(); } }
        
        //Instance of ECOTPC Item
        public Minion.EcotPC Machine { get; protected set; }

        //Commands from DB
        private ObservableCollection<MinionCommandItem> _minionCommands;
        
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
        public ObservableCollection<MinionCommandItem> MinionStartCommands { get { return _minionStartCommands ?? (_minionStartCommands = new ObservableCollection<MinionCommandItem>((from item in _minionCommands where item.Action == "Start" select item).ToList())); } }
        private RelayCommand<MinionCommandItem> _remoteStartCommand;
        public RelayCommand<MinionCommandItem> RemoteStartCommand { get { return _remoteStartCommand ?? (_remoteStartCommand = new RelayCommand<MinionCommandItem>(async (param) => await RemoteStart(param))); } }
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
        public RelayCommand DefaultKillsCommand { get { return _defaultKillsCommand ?? (_defaultKillsCommand = new RelayCommand(async () => await Machine.Kill_Defaultss())); } }
             

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
                IECommands = new ObservableCollection<MinionCommandItem>(_minionCommands.Where(i => ((i.Name == "Update") || (i.Name == "IE")) && (i.Action == "Install") && (i.Bit == Machine.OSBit.Remove(2))));
            }
        }

        private RelayCommand _uninstallJavaCommand;
        public RelayCommand UninstallJavaCommand { get { return _uninstallJavaCommand ?? (_uninstallJavaCommand = new RelayCommand(async () => await Uninstall_Java())); } }
        public async Task Uninstall_Java()
        {
            try
            {
                MinionCommandItem item;
                if (Machine.Java == "NOT INSTALLED" || Machine.Java == "ERROR")
                {
                    item = _minionCommands.First(j => (j.Name == "Java") && (j.Action == "Uninstall") && (j.Version == "All")) as MinionCommandItem;
                }
                else
                {
                    item = _minionCommands.First(j => (j.Name == "Java") && (j.Action == "Uninstall") && (j.Version == Machine.Java)) as MinionCommandItem;
                }
                await RunCommandItem(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                MetroMessageBox.Show("ERROR!", e.ToString());
                return;
            }
        }

        private RelayCommand _installJavaCommand;
        public RelayCommand InstallJavaCommand { get { return _installJavaCommand ?? (_installJavaCommand = new RelayCommand(async () => await Install_Java())); } }
        public async Task Install_Java()
        {
            try
            {
                var item = _minionCommands.First((j) => j.Version == "1.7.0_55" && j.Action == "Install");
                await RunCommandItem(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                MetroMessageBox.Show("ERROR!", e.ToString());
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

                item = _minionCommands.First(f => (f.Name == "Flash") && (f.Action == "Uninstall") && (f.Version == "All")) as MinionCommandItem;

                await RunCommandItem(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                MetroMessageBox.Show("ERROR!", e.ToString());
                return;
            }


        }

        private RelayCommand _installFlashCommand;
        public RelayCommand InstallFlashCommand { get { return _installFlashCommand ?? (_installFlashCommand = new RelayCommand(async () => await Install_Flash())); } }
        public async Task Install_Flash()
        {
            try
            {
                MinionCommandItem item;
                item = _minionCommands.First(f => (f.Name == "Flash") && (f.Action == "Install") && (f.Version != "All")) as MinionCommandItem;
                await RunCommandItem(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                MetroMessageBox.Show("ERROR!", e.ToString());
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
                item = _minionCommands.First(f => (f.Name == "Shockwave") && (f.Action == "Uninstall") && (f.Version == "All")) as MinionCommandItem;
            }
            catch (Exception e)
            {
                log.Error(e);
                item = _minionCommands.First(f => (f.Name == "Shockwave") && (f.Action == "Uninstall") && (f.Version == "WMIC")) as MinionCommandItem;
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
                item = _minionCommands.First(f => (f.Name == "Shockwave") && (f.Action == "Install")) as MinionCommandItem;
                await RunCommandItem(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                MetroMessageBox.Show("ERROR!", e.ToString());
            }
        }

        private RelayCommand _uninstallReaderCommand;
        public RelayCommand UninstallReaderCommand { get { return _uninstallReaderCommand ?? (_uninstallReaderCommand = new RelayCommand(async () => await Uninstall_Reader())); } }
        public async Task Uninstall_Reader()
        {
            MinionCommandItem item;
            try
            {
                item = _minionCommands.First(f => (f.Name == "Reader") && (f.Action == "Uninstall") && (f.Version == "All")) as MinionCommandItem;
            }
            catch (Exception e)
            {
                log.Error(e);
                item = _minionCommands.First(f => (f.Name == "Reader") && (f.Action == "Uninstall") && (f.Version == "WMIC")) as MinionCommandItem;
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
                item = _minionCommands.First(f => (f.Name == "Reader") && (f.Action == "Install")) as MinionCommandItem;
                await RunCommandItem(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                MetroMessageBox.Show("ERROR!", e.ToString());
            }
        }

        private async Task RunCommandItem(MinionCommandItem command)
        {
            await Machine.Kill_Defaultss();
            var result = await Machine.Command(command);
            string vresult = await UpdateItemVersion(command);

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
                else if (vresult == command.Version)
                    RaiseNoteWrite(string.Format("Ran Minion {0} install and {0} version {1} is now installed.", command.Name, vresult));
                else
                    RaiseNoteWrite(string.Format("Ran Minion {0} install but Minion was unable to verify install.", command.Name));
            }

        }

        private async Task<string> UpdateItemVersion(MinionCommandItem item)
        {
            string result = string.Empty;
            if (item.Name.ToLower().Contains("java"))
                result = await Machine.Get_Java();
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
        #endregion
        
    }
}
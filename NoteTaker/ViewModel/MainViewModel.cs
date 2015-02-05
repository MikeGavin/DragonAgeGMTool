using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Minion;
using Scrivener.Model;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using Scrivener.Helpers;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Scrivener.UserControls;
using System.Data.SQLite;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NLog.Config;
using Minion.ListItems;
using System.ComponentModel;
using System.Windows.Data;
using System.Runtime.CompilerServices;
using System.Deployment.Application;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using NLog.Targets;
using NLog;
using NLog.Targets.Wrappers;


namespace Scrivener.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        #region Boilerplate
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        
        
        private readonly IDataService _dataService; // Used by MVVMLight 

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed4

        ////    base.Cleanup();
        ////}
        #endregion

        private RelayCommand _dbUpdated_Click;
        public RelayCommand DBUpdated_Click { get { return _dbUpdated_Click ?? (_dbUpdated_Click = new RelayCommand(() => DBUpdated = false)); } }


        //Constructor
        public MainViewModel(IDataService dataService)
        {
            //Retreave previous version settings.
            //http://stackoverflow.com/questions/622764/persisting-app-config-variables-in-updates-via-click-once-deployment/622813#622813
            if (Properties.Settings.Default.UpgradeSettings)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeSettings = false;
            }
            //Event Listener to auto save notes if application failes through unhandeled expection
            App.Fucked += (s,e) => SaveAllNotes();
            Application.Current.MainWindow.Closing += (s, e) => SaveAllNotes();

            //Checks deployment and enables update systems if not in debug
            DeploymentSetup();

            //create DB singleton
            DataB = DatabaseStorage.Instance;           
           
            //Listen for note collection change
            Notes.CollectionChanged += OnNotesChanged;

            //Auto save settings on any change.
            Properties.Settings.Default.PropertyChanged += Settings_PropertyChanged;

            BFReplace = "";
        }

        //WindowLoaded runs functions only availalbe after window has loaded and are unavailable in constructor.
        public async void WindowLoaded()
        {
            if (Properties.Settings.Default.Role_Current == null)
            {
                Properties.Settings.Default.Role_Current = await MetroMessageBox.GetRole();
                Properties.Settings.Default.Save();
                if (Properties.Settings.Default.Role_Current == null)
                {
                    await MetroMessageBox.Show(string.Empty, "Apathy is death.");
                    Environment.Exit(0);
                }
            }
            DataB.Role = Properties.Settings.Default.Role_Current;
            //Hack to set current role in combobox
            var role = DataB.Roles.First((i) => i.Name == Properties.Settings.Default.Role_Current.Name);
            RolesView.MoveCurrentTo(role);
            var openNotes = await noteManager.GetCurrentNotes();
            if (openNotes.Count > 0)
            {
                foreach (INote n in openNotes)
                {                 
                    Notes.Add( new NoteViewModel(n));
                }
                SelectedNote = Notes.FirstOrDefault();
            }
            if (Notes.Count == 0)
            {
                NewNote();
            }

        }

        //Singleton instance of the DB to sync data across view models
        //private Singleton _dataB;
        //public Singleton DataB { get { return _dataB ?? (_dataB = Singleton.Instance); RaisePropertyChanged(); } }
        public DatabaseStorage DataB { get; set; }

        #region Deployment and Update
        //Deployment Systems
        private bool _updated;
        public bool Updated { get { return _updated; } protected set { _updated = value; RaisePropertyChanged(); } }
        private bool _dbupdated;
        public bool DBUpdated { get { return _dbupdated; } protected set { _dbupdated = value; RaisePropertyChanged(); } }
        private string appMode;
        public string AppMode { get { return appMode; } protected set { appMode = value; RaisePropertyChanged(); } }
        
        private void DeploymentSetup()
        {
            //Creates instance to define settings folder in a location and create it based on name of App and if Dev deployment
            var deployment = new DeploymentData(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
            SetLogFilePath("LogFile", deployment.SettingsFolder);
            AppMode = deployment.Mode;
            if (deployment.NetworkDeployed == true)
            {
                try
                {
                    //Start auto update system and subscribe to event
                    var updateManager = new UpdateManager(deployment.UpdateLocation);
                    updateManager.UpdateComplete += UpdateComplete;
                }
                catch(Exception e)
                {
                    log.Fatal(e);
                }                
                try
                {
                    //listen for DB updates
                    var WatchDataBase = new DataBaseWatcher(deployment.UpdateLocation);
                    DataBaseWatcher.DataBaseUpdated += (o, e) => { this.ReloadData(o, e.FullPath); DBUpdated = true; };
                }
                catch(Exception e)
                {
                    log.Fatal(e);
                }                
            }
        }

        private void SetLogFilePath(string targetName, string pathName)
        {
            string fileName = null;
            var fileTarget = Helpers.LoggingHelper.ReturnTarget(targetName) as FileTarget;
            fileTarget.FileName = pathName + "/Logs/${shortdate}.log";
            log.Debug("logfile path set");
            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            fileName = fileTarget.FileName.Render(logEventInfo);
        }

        void UpdateComplete(object sender, AsyncCompletedEventArgs e)
        {
                Updated = true;
                Observable
                    .Timer(DateTimeOffset.Parse("23:59:00-04:00"))
                    .Subscribe(x =>
                    {
                        SaveAllNotes();                       
                        log.Debug("Quitting application due to installed update.");
                        Process.GetCurrentProcess().Kill();

                    });
        }
        
        #endregion

        #region NoteCommands
        //Note Collection
        private MTObservableCollection<NoteViewModel> _Notes = new MTObservableCollection<NoteViewModel>();
        public MTObservableCollection<NoteViewModel> Notes { get { return _Notes; } set { _Notes = value; RaisePropertyChanged(); } }
        private INote _SelectedNote;
        public INote SelectedNote { get { return _SelectedNote; } set { _SelectedNote = value; RaisePropertyChanged(); } }

        //Closing of notes
        void OnNotesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (NoteViewModel note in e.NewItems)
                {
                    note.RequestClose += this.OnNoteRequestClose;
                    note.SaveNoteRequest += note_SaveNoteRequest;
                }

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (NoteViewModel workspace in e.OldItems)
                {
                    workspace.RequestClose -= this.OnNoteRequestClose;
                    workspace.SaveNoteRequest -= note_SaveNoteRequest;
                }

        }

        private void note_SaveNoteRequest(object sender, EventArgs e)
        {
            // this uses a task to save every time a change is detected in order to prevent sluggy database issues. 
            Task t = Task.Factory.StartNew(async () =>
            {
                await noteManager.SaveCurrent(sender as NoteViewModel);
            });

        }
        void OnNoteRequestClose(object sender, EventArgs e)
        {
            NoteViewModel note = sender as NoteViewModel;
            CloseNote(note);
        }

        private async Task CloseNote(NoteViewModel note)
        {
            if (Scrivener.Properties.Settings.Default.Close_Warning == true)
            {
                var result = await Helpers.MetroMessageBox.ShowResult("WARNING!", string.Format("Are you sure you want to close '{0}'?", note.Title));
                if (result == true)
                {
                    await SetLastSaveClose(note);
                }
            }
            else if (Scrivener.Properties.Settings.Default.Close_Warning == false)
            {
                await SetLastSaveClose(note);
            }

            if (Notes.Count == 0)
            {
                NewNote();
            }
        }

        private async Task SetLastSaveClose(NoteViewModel note)
        {
            await Task.Factory.StartNew(async () =>
            {
                await noteManager.SaveCurrent(note);
                lastClosedNote = note;
                Notes.Remove(note);
                await noteManager.ArchiveCurrent(note);
            });
        }

        private RelayCommand _closeAllNotesCommand;
        public RelayCommand CloseAllNotesCommand { get { return _closeAllNotesCommand ?? (_closeAllNotesCommand = new RelayCommand(CloseAllNotes)); } }

        private async void CloseAllNotes()
        {
            foreach (NoteViewModel n in Notes.ToList())
            {
                await CloseNote(n);
            }
        }

        //New Notes
        private RelayCommand<INote> _newNoteCommand;
        public RelayCommand<INote> NewNoteCommand { get { return _newNoteCommand ?? (_newNoteCommand = new RelayCommand<INote>((parm) => NewNote("RelayCommand", parm))); } }
        private async void NewNote([CallerMemberName]string memberName = "", INote note = null)
        {
            await Task.Factory.StartNew(() =>
            {
                log.Debug("{0} ran NewNote", memberName);
                Notes.Add(new NoteViewModel(note));
                SelectedNote = Notes.Last();
            });
        }

        private INote lastClosedNote;
        //Recall Notes
        private RelayCommand<string> _RecallNoteCommand;
        public RelayCommand<string> RecallNoteCommand { get { return _RecallNoteCommand ?? (_RecallNoteCommand = new RelayCommand<string>(async (parm) => await RecallNote("RecallNote"))); } }
        private async Task RecallNote([CallerMemberName]string memberName = "")
        {
            await Task.Factory.StartNew(() =>
            {
                if (lastClosedNote != null)
                {
                    Notes.Add(lastClosedNote as NoteViewModel);
                    SelectedNote = Notes.Last();
                    lastClosedNote = null;
                }
            });
        } 
        #endregion

        #region ToolBar Items

        private RelayCommand _QuickNoteToggleCommand;
        public RelayCommand QuickNoteToggleCommand { get { return _QuickNoteToggleCommand ?? (_QuickNoteToggleCommand = new RelayCommand(QuickNoteToggle)); } }
        public Visibility QuicknoteVisibility { get { return Properties.Settings.Default.QuickNotes_Visible; } set { Properties.Settings.Default.QuickNotes_Visible = value; RaisePropertyChanged(); } }
        private void QuickNoteToggle()
        {
            if (QuicknoteVisibility == Visibility.Collapsed)
            {
                QuicknoteVisibility = Visibility.Visible;           
            }
            else
            {
                QuicknoteVisibility = Visibility.Collapsed;                
            }
        }

        private RelayCommand _settingsbuttonCommand;
        public RelayCommand SettingsButtonCommand { get { return _settingsbuttonCommand ?? (_settingsbuttonCommand = new RelayCommand(SettingsButton)); } }
        public void SettingsButton()
        {
            if (Properties.Settings.Default.SettingsExpanded == true)
            {
                Properties.Settings.Default.SettingsExpanded = false;
            }

            else if (Properties.Settings.Default.SettingsExpanded == false)
            {
                Properties.Settings.Default.SettingsExpanded = true;
            }
        }

        private RelayCommand _SettingsExpandCommand;
        public RelayCommand SettingsExpandCommand { get { return _SettingsExpandCommand ?? (_SettingsExpandCommand = new RelayCommand(SettingsExpand)); } }
        public void SettingsExpand()
        {
            if (Properties.Settings.Default.SettingsExpanded == true && Properties.Settings.Default.SettingsVisibility == true)
            {
                Properties.Settings.Default.SettingsExpanded = false;
            }

            else if (Properties.Settings.Default.SettingsExpanded == false)
            {
                Properties.Settings.Default.SettingsExpanded = true;
            }

            Properties.Settings.Default.HistoryVisibility = false;
            Properties.Settings.Default.QuickARVisibility = false;
            Properties.Settings.Default.SettingsVisibility = true;
        }

        private RelayCommand _openHistoryCommand;
        public RelayCommand OpenHistoryCommand { get { return _openHistoryCommand ?? (_openHistoryCommand = new RelayCommand(OpenHistory)); } }
        public void OpenHistory()
        {
            if (Properties.Settings.Default.SettingsExpanded == true && Properties.Settings.Default.HistoryVisibility == true)
            {
                Properties.Settings.Default.SettingsExpanded = false;
            }

            else if (Properties.Settings.Default.SettingsExpanded == false)
            {
                Properties.Settings.Default.SettingsExpanded = true;
            }

            Properties.Settings.Default.HistoryVisibility = true;
            Properties.Settings.Default.QuickARVisibility = false;
            Properties.Settings.Default.SettingsVisibility = false;        
        }

        private RelayCommand _openQuickARCommand;
        public RelayCommand OpenQuickARCommand { get { return _openQuickARCommand ?? (_openQuickARCommand = new RelayCommand(OpenQuickAR)); } }
        public void OpenQuickAR()
        {
            if (Properties.Settings.Default.SettingsExpanded == true && Properties.Settings.Default.QuickARVisibility == true)
            {
                Properties.Settings.Default.SettingsExpanded = false;
            }

            else if (Properties.Settings.Default.SettingsExpanded == false)
            {
                Properties.Settings.Default.SettingsExpanded = true;
            }

            Properties.Settings.Default.HistoryVisibility = false;
            Properties.Settings.Default.QuickARVisibility = true;
            Properties.Settings.Default.SettingsVisibility = false;
        }

        //Search EKB
        private string _searchData;
        public string SearchData { get { return _searchData; } set { _searchData = value; RaisePropertyChanged(); } }
        private RelayCommand _searchboxcommand;
        public RelayCommand SearchBoxCommand { get { return _searchboxcommand ?? (_searchboxcommand = new RelayCommand(SearchKB)); } }
        public void SearchKB()
        {
            //var KB = string.Format("https://ecotshare.ecotoh.net/ecotsearch/Results.aspx?k={0}&cs=This%20Site&u=https%3A%2F%2Fecotshare.ecotoh.net%2Foperations%2Fhelpdesk", SearchData);

            var KB = string.Format("https://ecotshare.ecotoh.net/ecotsearch/results.aspx?k={0}&cs=This%20Site&u=https%3A%2F%2Fecotshare%2Eecotoh%2Enet%2Foperations%2Fhelpdesk%2FEKBW&r=site%3D%22https%3A%2F%2Fecotshare%2Eecotoh%2Enet%2Foperations%2Fhelpdesk%22", SearchData);
            Process.Start(KB);
        }

        //Open Site Link
        private RelayCommand<string> _openLinkCommand;
        public RelayCommand<string> OpenLinkCommand { get { return _openLinkCommand ?? (_openLinkCommand = new RelayCommand<string>((pram) => OpenLink(pram))); } }
        public void OpenLink(string link)
        {
            Process.Start(link);
        }

        private RelayCommand<string> _openCallCommand;
        public RelayCommand<string> OpenCallCommand { get { return _openCallCommand ?? (_openCallCommand = new RelayCommand<string>((pram) => OpenCall(pram))); } }
        public void OpenCall(string call)
        {
            Process.Start(call);
        }

        private RelayCommand<string> _dialCommand;
        public RelayCommand<string> DialCommand { get { return _dialCommand ?? (_dialCommand = new RelayCommand<string>((pram) => Dial())); } }
        public void Dial()
        {
            Process.Start("tel:9-");
        }

        //Copy All
        private RelayCommand _copyallcommand;
        public RelayCommand CopyAllCommand { get { return _copyallcommand ?? (_copyallcommand = new RelayCommand(CopyAll)); } }
        public void CopyAll()
        {
            try
            {
                Clipboard.SetDataObject(SelectedNote.Text);
            }
            catch (Exception e)
            {
                Model.ExceptionReporting.Email(e);
                var temp = MetroMessageBox.Show("ERMAHGERD ERER!", e.ToString());
            }

        }
        
        #endregion

        #region Settings

        private void SettingsFolder(string settingsFolder)
        {
            //check for settings folder. Create if missing.            
            if (!Directory.Exists(settingsFolder))
            {
                Directory.CreateDirectory(settingsFolder);
            }
        }

        //Listener for settings changed properity in order to clear out imports
        void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "Role_Current" & Properties.Settings.Default.Role_Current != null))
            {
                DataB.Role = Properties.Settings.Default.Role_Current;
                ////Hack to set current role in combobox
                var role = DataB.Roles.First((i) => i.Name == Properties.Settings.Default.Role_Current.Name);
                RolesView.MoveCurrentTo(role);

                //reset roll properties to force updates.
                ReloadData(sender, "scrivener.sqlite");
                Properties.Settings.Default.Minion_Visibility = Properties.Settings.Default.Role_Current.Minion;
            }
            Properties.Settings.Default.Save();
            
        }

        private async void ReloadData(object o, string f)
        {
            if (f.ToLower().Contains("scrivener.sqlite"))
            {
                log.Debug("{0} requested db reload.", o.ToString());
                await DataB.LoadAll();
            }
        }

        public static CollectionView _rolesView;
        public CollectionView RolesView { get { return _rolesView ?? (_rolesView = new CollectionView(DataB.Roles)); } set { _rolesView = value; RaisePropertyChanged(); } }
        public RoleItem CurrentRole { get { return Properties.Settings.Default.Role_Current; } set { if (value != Properties.Settings.Default.Role_Current) { Properties.Settings.Default.Role_Current = value; } RaisePropertyChanged(); } }

        private RelayCommand _savetemplatecommand;
        public RelayCommand SaveTemplateCommand { get { return _savetemplatecommand ?? (_savetemplatecommand = new RelayCommand(SaveTemplate)); } }
        public void SaveTemplate()
        {

            Properties.Settings.Default.Default_Note_Template = SelectedNote.Text;
            Properties.Settings.Default.Save();
        }

        //Allows getting of current version
        public string AssemblyVersion { get { return System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString(); } }
        public string PublishVersion 
        { 
            get 
            { 
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed) 
                {
                    return System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                }
                else
                {
                    return "unplublished";
                }
            }
        }

        #endregion

        #region QuickAR

        #region Breakfixform
        //private RelayCommand _phonenumberregexCommand;
        //public RelayCommand PhoneNumberRegexCommand { get { return _phonenumberregexCommand ?? (_phonenumberregexCommand = new RelayCommand(PhoneNumberRegex)); } }
        //public void PhoneNumberRegex()
        //{
        //    Regex regex = new Regex("[^0-9]+");
        //    BFPhone.Handled = regex.IsMatch(BFPhone);
        //}

        private RelayCommand _createbreakfixCommand;
        public RelayCommand CreateBreakfixCommand { get { return _createbreakfixCommand ?? (_createbreakfixCommand = new RelayCommand(CreateBreakfix)); } }
        public void CreateBreakfix()
        {


        }        

        #region Publicproperties
        private string bfreplace;
        public string BFReplace { get { return bfreplace; } set { bfreplace = value; RaisePropertyChanged(); } }

        #endregion
      
        #region Replaceitemvisibility
        private RelayCommand _towerbreakfixCommand;
        public RelayCommand TowerBreakfixCommand { get { return _towerbreakfixCommand ?? (_towerbreakfixCommand = new RelayCommand(TowerBreakfix)); } }
        public void TowerBreakfix()
        {
            Properties.Settings.Default.Towervisible = Visibility.Visible;
            Properties.Settings.Default.Monitorvisible = Visibility.Visible;
            Properties.Settings.Default.Printervisible = Visibility.Visible;
            Properties.Settings.Default.Keyboardvisible = Visibility.Visible;
            Properties.Settings.Default.Mousevisible = Visibility.Visible;
            Properties.Settings.Default.Mousepadvisible = Visibility.Visible;
            Properties.Settings.Default.Headsetvisible = Visibility.Visible;
            Properties.Settings.Default.Speakersrvisible = Visibility.Collapsed;
            Properties.Settings.Default.Ethernetcablevisible = Visibility.Visible;
            Properties.Settings.Default.Usbcablevisible = Visibility.Visible;
            Properties.Settings.Default.Vgacablevisible = Visibility.Visible;
            Properties.Settings.Default.Powerstripvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletvisible = Visibility.Collapsed;
            Properties.Settings.Default.Tabletchargervisible = Visibility.Collapsed;
            Properties.Settings.Default.Routervisible = Visibility.Collapsed;        
        }

        private RelayCommand _canceltowerbreakfixCommand;
        public RelayCommand CancelTowerBreakfixCommand { get { return _canceltowerbreakfixCommand ?? (_canceltowerbreakfixCommand = new RelayCommand(CancelTowerBreakfix)); } }
        public void CancelTowerBreakfix()
        {
            Properties.Settings.Default.Towervisible = Visibility.Visible;
            Properties.Settings.Default.Monitorvisible = Visibility.Visible;
            Properties.Settings.Default.Printervisible = Visibility.Visible;
            Properties.Settings.Default.Keyboardvisible = Visibility.Visible;
            Properties.Settings.Default.Mousevisible = Visibility.Visible;
            Properties.Settings.Default.Mousepadvisible = Visibility.Visible;
            Properties.Settings.Default.Headsetvisible = Visibility.Visible;
            Properties.Settings.Default.Speakersrvisible = Visibility.Visible;
            Properties.Settings.Default.Ethernetcablevisible = Visibility.Visible;
            Properties.Settings.Default.Usbcablevisible = Visibility.Visible;
            Properties.Settings.Default.Vgacablevisible = Visibility.Visible;
            Properties.Settings.Default.Powerstripvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletchargervisible = Visibility.Visible;
            Properties.Settings.Default.Routervisible = Visibility.Visible;
        }

        private RelayCommand _monitorbreakfixCommand;
        public RelayCommand MonitorBreakfixCommand { get { return _monitorbreakfixCommand ?? (_monitorbreakfixCommand = new RelayCommand(MonitorBreakfix)); } }
        public void MonitorBreakfix()
        {
            Properties.Settings.Default.Towervisible = Visibility.Visible;
            Properties.Settings.Default.Monitorvisible = Visibility.Visible;
            Properties.Settings.Default.Printervisible = Visibility.Visible;
            Properties.Settings.Default.Keyboardvisible = Visibility.Visible;
            Properties.Settings.Default.Mousevisible = Visibility.Visible;
            Properties.Settings.Default.Mousepadvisible = Visibility.Visible;
            Properties.Settings.Default.Headsetvisible = Visibility.Visible;
            Properties.Settings.Default.Speakersrvisible = Visibility.Collapsed;
            Properties.Settings.Default.Ethernetcablevisible = Visibility.Visible;
            Properties.Settings.Default.Usbcablevisible = Visibility.Visible;
            Properties.Settings.Default.Vgacablevisible = Visibility.Visible;
            Properties.Settings.Default.Powerstripvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletvisible = Visibility.Collapsed;
            Properties.Settings.Default.Tabletchargervisible = Visibility.Collapsed;
            Properties.Settings.Default.Routervisible = Visibility.Collapsed;
        }

        private RelayCommand _cancelmonitorbreakfixCommand;
        public RelayCommand CancelMonitorBreakfixCommand { get { return _cancelmonitorbreakfixCommand ?? (_cancelmonitorbreakfixCommand = new RelayCommand(CancelMonitorBreakfix)); } }
        public void CancelMonitorBreakfix()
        {
            Properties.Settings.Default.Towervisible = Visibility.Visible;
            Properties.Settings.Default.Monitorvisible = Visibility.Visible;
            Properties.Settings.Default.Printervisible = Visibility.Visible;
            Properties.Settings.Default.Keyboardvisible = Visibility.Visible;
            Properties.Settings.Default.Mousevisible = Visibility.Visible;
            Properties.Settings.Default.Mousepadvisible = Visibility.Visible;
            Properties.Settings.Default.Headsetvisible = Visibility.Visible;
            Properties.Settings.Default.Speakersrvisible = Visibility.Visible;
            Properties.Settings.Default.Ethernetcablevisible = Visibility.Visible;
            Properties.Settings.Default.Usbcablevisible = Visibility.Visible;
            Properties.Settings.Default.Vgacablevisible = Visibility.Visible;
            Properties.Settings.Default.Powerstripvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletchargervisible = Visibility.Visible;
            Properties.Settings.Default.Routervisible = Visibility.Visible;
        }

        private RelayCommand _printerbreakfixCommand;
        public RelayCommand PrinterBreakfixCommand { get { return _printerbreakfixCommand ?? (_printerbreakfixCommand = new RelayCommand(PrinterBreakfix)); } }
        public void PrinterBreakfix()
        {
            Properties.Settings.Default.Towervisible = Visibility.Visible;
            Properties.Settings.Default.Monitorvisible = Visibility.Visible;
            Properties.Settings.Default.Printervisible = Visibility.Visible;
            Properties.Settings.Default.Keyboardvisible = Visibility.Visible;
            Properties.Settings.Default.Mousevisible = Visibility.Visible;
            Properties.Settings.Default.Mousepadvisible = Visibility.Visible;
            Properties.Settings.Default.Headsetvisible = Visibility.Visible;
            Properties.Settings.Default.Speakersrvisible = Visibility.Collapsed;
            Properties.Settings.Default.Ethernetcablevisible = Visibility.Visible;
            Properties.Settings.Default.Usbcablevisible = Visibility.Visible;
            Properties.Settings.Default.Vgacablevisible = Visibility.Visible;
            Properties.Settings.Default.Powerstripvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletvisible = Visibility.Collapsed;
            Properties.Settings.Default.Tabletchargervisible = Visibility.Collapsed;
            Properties.Settings.Default.Routervisible = Visibility.Collapsed;
        }

        private RelayCommand _cancelprinterbreakfixCommand;
        public RelayCommand CancelPrinterBreakfixCommand { get { return _cancelprinterbreakfixCommand ?? (_cancelprinterbreakfixCommand = new RelayCommand(CancelPrinterBreakfix)); } }
        public void CancelPrinterBreakfix()
        {
            Properties.Settings.Default.Towervisible = Visibility.Visible;
            Properties.Settings.Default.Monitorvisible = Visibility.Visible;
            Properties.Settings.Default.Printervisible = Visibility.Visible;
            Properties.Settings.Default.Keyboardvisible = Visibility.Visible;
            Properties.Settings.Default.Mousevisible = Visibility.Visible;
            Properties.Settings.Default.Mousepadvisible = Visibility.Visible;
            Properties.Settings.Default.Headsetvisible = Visibility.Visible;
            Properties.Settings.Default.Speakersrvisible = Visibility.Visible;
            Properties.Settings.Default.Ethernetcablevisible = Visibility.Visible;
            Properties.Settings.Default.Usbcablevisible = Visibility.Visible;
            Properties.Settings.Default.Vgacablevisible = Visibility.Visible;
            Properties.Settings.Default.Powerstripvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletchargervisible = Visibility.Visible;
            Properties.Settings.Default.Routervisible = Visibility.Visible;
        }

        private RelayCommand _keyboardbreakfixCommand;
        public RelayCommand KeyboardBreakfixCommand { get { return _keyboardbreakfixCommand ?? (_keyboardbreakfixCommand = new RelayCommand(KeyboardBreakfix)); } }
        public void KeyboardBreakfix()
        {
            Properties.Settings.Default.Towervisible = Visibility.Visible;
            Properties.Settings.Default.Monitorvisible = Visibility.Visible;
            Properties.Settings.Default.Printervisible = Visibility.Visible;
            Properties.Settings.Default.Keyboardvisible = Visibility.Visible;
            Properties.Settings.Default.Mousevisible = Visibility.Visible;
            Properties.Settings.Default.Mousepadvisible = Visibility.Visible;
            Properties.Settings.Default.Headsetvisible = Visibility.Visible;
            Properties.Settings.Default.Speakersrvisible = Visibility.Collapsed;
            Properties.Settings.Default.Ethernetcablevisible = Visibility.Visible;
            Properties.Settings.Default.Usbcablevisible = Visibility.Visible;
            Properties.Settings.Default.Vgacablevisible = Visibility.Visible;
            Properties.Settings.Default.Powerstripvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletvisible = Visibility.Collapsed;
            Properties.Settings.Default.Tabletchargervisible = Visibility.Collapsed;
            Properties.Settings.Default.Routervisible = Visibility.Collapsed;
        }

        private RelayCommand _cancelkeyboardbreakfixCommand;
        public RelayCommand CancelKeyboardBreakfixCommand { get { return _cancelkeyboardbreakfixCommand ?? (_cancelkeyboardbreakfixCommand = new RelayCommand(CancelKeyboardBreakfix)); } }
        public void CancelKeyboardBreakfix()
        {
            Properties.Settings.Default.Towervisible = Visibility.Visible;
            Properties.Settings.Default.Monitorvisible = Visibility.Visible;
            Properties.Settings.Default.Printervisible = Visibility.Visible;
            Properties.Settings.Default.Keyboardvisible = Visibility.Visible;
            Properties.Settings.Default.Mousevisible = Visibility.Visible;
            Properties.Settings.Default.Mousepadvisible = Visibility.Visible;
            Properties.Settings.Default.Headsetvisible = Visibility.Visible;
            Properties.Settings.Default.Speakersrvisible = Visibility.Visible;
            Properties.Settings.Default.Ethernetcablevisible = Visibility.Visible;
            Properties.Settings.Default.Usbcablevisible = Visibility.Visible;
            Properties.Settings.Default.Vgacablevisible = Visibility.Visible;
            Properties.Settings.Default.Powerstripvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletchargervisible = Visibility.Visible;
            Properties.Settings.Default.Routervisible = Visibility.Visible;
        }

        private RelayCommand _speakerbreakfixCommand;
        public RelayCommand SpeakerBreakfixCommand { get { return _speakerbreakfixCommand ?? (_speakerbreakfixCommand = new RelayCommand(SpeakerBreakfix)); } }
        public void SpeakerBreakfix()
        {
            Properties.Settings.Default.Towervisible = Visibility.Collapsed;
            Properties.Settings.Default.Monitorvisible = Visibility.Collapsed;
            Properties.Settings.Default.Printervisible = Visibility.Collapsed;
            Properties.Settings.Default.Keyboardvisible = Visibility.Collapsed;
            Properties.Settings.Default.Mousevisible = Visibility.Collapsed;
            Properties.Settings.Default.Mousepadvisible = Visibility.Collapsed;
            Properties.Settings.Default.Headsetvisible = Visibility.Collapsed;
            Properties.Settings.Default.Speakersrvisible = Visibility.Visible;
            Properties.Settings.Default.Ethernetcablevisible = Visibility.Collapsed;
            Properties.Settings.Default.Usbcablevisible = Visibility.Collapsed;
            Properties.Settings.Default.Vgacablevisible = Visibility.Collapsed;
            Properties.Settings.Default.Powerstripvisible = Visibility.Collapsed;
            Properties.Settings.Default.Tabletvisible = Visibility.Collapsed;
            Properties.Settings.Default.Tabletchargervisible = Visibility.Collapsed;
            Properties.Settings.Default.Routervisible = Visibility.Collapsed;
        }

        private RelayCommand _cancelspeakerbreakfixCommand;
        public RelayCommand CancelSpeakerBreakfixCommand { get { return _cancelspeakerbreakfixCommand ?? (_cancelspeakerbreakfixCommand = new RelayCommand(CancelSpeakerBreakfix)); } }
        public void CancelSpeakerBreakfix()
        {
            Properties.Settings.Default.Towervisible = Visibility.Visible;
            Properties.Settings.Default.Monitorvisible = Visibility.Visible;
            Properties.Settings.Default.Printervisible = Visibility.Visible;
            Properties.Settings.Default.Keyboardvisible = Visibility.Visible;
            Properties.Settings.Default.Mousevisible = Visibility.Visible;
            Properties.Settings.Default.Mousepadvisible = Visibility.Visible;
            Properties.Settings.Default.Headsetvisible = Visibility.Visible;
            Properties.Settings.Default.Speakersrvisible = Visibility.Visible;
            Properties.Settings.Default.Ethernetcablevisible = Visibility.Visible;
            Properties.Settings.Default.Usbcablevisible = Visibility.Visible;
            Properties.Settings.Default.Vgacablevisible = Visibility.Visible;
            Properties.Settings.Default.Powerstripvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletvisible = Visibility.Visible;
            Properties.Settings.Default.Tabletchargervisible = Visibility.Visible;
            Properties.Settings.Default.Routervisible = Visibility.Visible;
        }
        #endregion
        
        #region Addressandphoneformatting
        private string bfaddress;
        public string BFAddress { get { return bfaddress; } set { bfaddress = value; RaisePropertyChanged(); } }

        private RelayCommand _replacebgaddressCommand;
        public RelayCommand ReplaceBGAddressCommand { get { return _replacebgaddressCommand ?? (_replacebgaddressCommand = new RelayCommand(ReplaceBFAddress)); } }
        public void ReplaceBFAddress()
        {
            if (BFAddress == "v" || BFAddress == "V")
            {
                BFAddress = "Verified physical address.";
            }
        }

        private string bfphone;
        public string BFPhone { get { return bfphone; } set { bfphone = value; RaisePropertyChanged(); } }

        private RelayCommand _formatbfphoneCommand;
        public RelayCommand FormatBFPhoneCommand { get { return _formatbfphoneCommand ?? (_formatbfphoneCommand = new RelayCommand(FormatBFPhone)); } }
        public void FormatBFPhone()
        {
            if (BFPhone == null || BFPhone == "")
            {

            }

            else if (BFPhone != null || BFPhone != "")
            {
                string[] split = BFPhone.Split(new char[] { '-', '(', ')', ' ', '.' });
                // remove all old format,if your phone number is like(001)123-456-789
                StringBuilder bfpn = new StringBuilder();
                foreach (string s in split)
                {
                    if (s.Trim() != "")
                    {
                        bfpn.Append(s);
                    }
                }

                if (bfpn.Length == 10)
                {
                    BFPhone = String.Format("{0:(000) 000-0000}", double.Parse(bfpn.ToString()));
                }
                else if (bfpn.Length == 11)
                {
                    BFPhone = String.Format("{0:0(000) 000-0000}", double.Parse(bfpn.ToString()));
                }

                else if (bfpn.Length == 7)
                {
                    MessageBox.Show("Invalid phone number, please include area code");
                }

                else if (bfpn.Length < 10 | bfpn.Length >= 12)
                {
                    MessageBox.Show("Invalid phone number, please correct");
                }
            }
        }  
        #endregion

        #endregion

        #region Curriculumform
        //Controls visibility for Curriculum AR Bullets
        private RelayCommand _curriculumarcommand;
        public RelayCommand CurriculumARCommand { get { return _curriculumarcommand ?? (_curriculumarcommand = new RelayCommand(CurriculumAR)); } }
        public void CurriculumAR()
        {

            if (Properties.Settings.Default.ClassContentChecked == true)
            {
                Properties.Settings.Default.MCGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.CCGridVisibility = Visibility.Visible;
                Properties.Settings.Default.AuxGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AccountGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.DiscGridVisibility = Visibility.Collapsed;
            }

            else if (Properties.Settings.Default.MCCAChecked == true)
            {
                Properties.Settings.Default.CCGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.MCGridVisibility = Visibility.Visible;
                Properties.Settings.Default.AuxGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AccountGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.DiscGridVisibility = Visibility.Collapsed;
            }
            else if (Properties.Settings.Default.AuxSiteChecked == true)
            {
                Properties.Settings.Default.CCGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.MCGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AuxGridVisibility = Visibility.Visible;
                Properties.Settings.Default.AccountGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.DiscGridVisibility = Visibility.Collapsed;
            }
            else if (Properties.Settings.Default.AuxAccountChecked == true)
            {
                Properties.Settings.Default.CCGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.MCGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AuxGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AccountGridVisibility = Visibility.Visible;
                Properties.Settings.Default.DiscGridVisibility = Visibility.Collapsed;
            }
            else if (Properties.Settings.Default.DiscBoardChecked == true)
            {
                Properties.Settings.Default.CCGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.MCGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AuxGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AccountGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.DiscGridVisibility = Visibility.Visible;
            }

            else
            {
                Properties.Settings.Default.CCGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.MCGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AuxGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AccountGridVisibility = Visibility.Collapsed;
                Properties.Settings.Default.DiscGridVisibility = Visibility.Collapsed;
            }
        }

        private string ccurl;
        public string CCURL { get { return ccurl; } set { ccurl = value; RaisePropertyChanged(); } }
        private string ccname;
        public string CCName { get { return ccname; } set { ccname = value; RaisePropertyChanged(); } }
        private string ccbrowser;
        public string CCBrowser { get { return ccbrowser; } set { ccbrowser = value; RaisePropertyChanged(); } }
        private string ccpath;
        public string CCPath { get { return ccpath; } set { ccpath = value; RaisePropertyChanged(); } }
        private string ccfinal;
        public string CCFinal { get { return ccfinal; } set { ccfinal = value; RaisePropertyChanged(); } }

        //Create class content notes
        private RelayCommand _createcccommand;
        public RelayCommand CreateCCCommand { get { return _createcccommand ?? (_createcccommand = new RelayCommand(CreateCC)); } }
        public void CreateCC()
        {
            Properties.Settings.Default.CCFinalEnabled = true;
            CCFinal = "Class URL: " + CCURL + Environment.NewLine + Environment.NewLine + "Teachers Name: " + CCName + Environment.NewLine + Environment.NewLine + "Browser: " + CCBrowser + Environment.NewLine + Environment.NewLine + "Path: " + CCPath + Environment.NewLine + Environment.NewLine + "_______________________________________________________________";
            Properties.Settings.Default.CCAddEnabled = true;
        }

        //Resets class content form
        private RelayCommand _resetcccommand;
        public RelayCommand ResetCCCommand { get { return _resetcccommand ?? (_resetcccommand = new RelayCommand(ResetCC)); } }
        public void ResetCC()
        {
            CCFinal = "";
            CCURL = "";
            CCName = "";
            CCBrowser = "";
            CCPath = "";
            Properties.Settings.Default.CCFinalEnabled = false;
            Properties.Settings.Default.CCAddEnabled = false;

            Properties.Settings.Default.MCCAChecked = false;
            Properties.Settings.Default.ClassContentChecked = false;
            Properties.Settings.Default.AuxSiteChecked = false;
            Properties.Settings.Default.AuxAccountChecked = false;
            Properties.Settings.Default.DiscBoardChecked = false;
            CurriculumAR();
        }

        //Appends class contents notes to main note field
        private RelayCommand _addcccommand;
        public RelayCommand AddCCCommand { get { return _addcccommand ?? (_addcccommand = new RelayCommand(AddCC)); } }
        public void AddCC()
        {
            SelectedNote.Text = CCFinal + Environment.NewLine + Environment.NewLine + SelectedNote.Text;
        }

        private string mcurl;
        public string MCURL { get { return mcurl; } set { mcurl = value; RaisePropertyChanged(); } }
        private string mcname;
        public string MCName { get { return mcname; } set { mcname = value; RaisePropertyChanged(); } }
        private string mcsubject;
        public string MCSubject { get { return mcsubject; } set { mcsubject = value; RaisePropertyChanged(); } }
        private string mcdate;
        public string MCDate { get { return mcdate; } set { mcdate = value; RaisePropertyChanged(); } }
        private string mcfinal;
        public string MCFinal { get { return mcfinal; } set { mcfinal = value; RaisePropertyChanged(); } }

        //Create class content notes
        private RelayCommand _createmccommand;
        public RelayCommand CreateMCCommand { get { return _createmccommand ?? (_createmccommand = new RelayCommand(CreateMC)); } }
        public void CreateMC()
        {
            Properties.Settings.Default.MCFinalEnabled = true;
            MCFinal = "Class URL: " + MCURL + Environment.NewLine + Environment.NewLine + "Teachers Name: " + MCName + Environment.NewLine + Environment.NewLine + "Subject: " + MCSubject + Environment.NewLine + Environment.NewLine + "Date: " + MCDate + Environment.NewLine + Environment.NewLine + "_______________________________________________________________";
            Properties.Settings.Default.MCAddEnabled = true;
        }

        //Resets class content form
        private RelayCommand _resetmccommand;
        public RelayCommand ResetMCCommand { get { return _resetmccommand ?? (_resetmccommand = new RelayCommand(ResetMC)); } }
        public void ResetMC()
        {
            MCFinal = "";
            MCURL = "";
            MCName = "";
            MCSubject = "";
            MCDate = "";
            Properties.Settings.Default.MCFinalEnabled = false;
            Properties.Settings.Default.MCAddEnabled = false;

            Properties.Settings.Default.MCCAChecked = false;
            Properties.Settings.Default.ClassContentChecked = false;
            Properties.Settings.Default.AuxSiteChecked = false;
            Properties.Settings.Default.AuxAccountChecked = false;
            Properties.Settings.Default.DiscBoardChecked = false;
            CurriculumAR();
        }

        //Appends class contents notes to main note field
        private RelayCommand _addmccommand;
        public RelayCommand AddMCCommand { get { return _addmccommand ?? (_addmccommand = new RelayCommand(AddMC)); } }
        public void AddMC()
        {
            SelectedNote.Text = MCFinal + Environment.NewLine + Environment.NewLine + SelectedNote.Text;
        }


        private string auxurl;
        public string AuxURL { get { return auxurl; } set { auxurl = value; RaisePropertyChanged(); } }
        private string auxname;
        public string AuxName { get { return auxname; } set { auxname = value; RaisePropertyChanged(); } }
        private string auxbrowser;
        public string AuxBrowser { get { return auxbrowser; } set { auxbrowser = value; RaisePropertyChanged(); } }
        private string auxsite;
        public string AuxSite { get { return auxsite; } set { auxsite = value; RaisePropertyChanged(); } }
        private string auxpath;
        public string AuxPath { get { return auxpath; } set { auxpath = value; RaisePropertyChanged(); } }
        private string auxcbr;
        public string AuxCBR { get { return auxcbr; } set { auxcbr = value; RaisePropertyChanged(); } }
        private string auxfinal;
        public string AuxFinal { get { return auxfinal; } set { auxfinal = value; RaisePropertyChanged(); } }

        //Create aux site notes
        private RelayCommand _createauxcommand;
        public RelayCommand CreateAuxCommand { get { return _createauxcommand ?? (_createauxcommand = new RelayCommand(CreateAux)); } }
        public void CreateAux()
        {
            Properties.Settings.Default.AuxFinalEnabled = true;
            AuxFinal = "Class URL: " + AuxURL + Environment.NewLine + Environment.NewLine + "Teachers Name: " + AuxName + Environment.NewLine + Environment.NewLine + "Browser: " + AuxBrowser + Environment.NewLine + Environment.NewLine + "Site URL: " + AuxSite + Environment.NewLine + Environment.NewLine + "Path: " + AuxPath + Environment.NewLine + Environment.NewLine + "Best contact number: " + AuxCBR + Environment.NewLine + Environment.NewLine + "_______________________________________________________________";
            Properties.Settings.Default.AuxAddEnabled = true;
        }

        //Resets class content form
        private RelayCommand _resetauxcommand;
        public RelayCommand ResetAuxCommand { get { return _resetauxcommand ?? (_resetauxcommand = new RelayCommand(ResetAux)); } }
        public void ResetAux()
        {
            AuxFinal = "";
            AuxURL = "";
            AuxName = "";
            AuxBrowser = "";
            AuxPath = "";
            AuxSite = "";
            AuxCBR = "";
            Properties.Settings.Default.AuxFinalEnabled = false;
            Properties.Settings.Default.AuxAddEnabled = false;

            Properties.Settings.Default.MCCAChecked = false;
            Properties.Settings.Default.ClassContentChecked = false;
            Properties.Settings.Default.AuxSiteChecked = false;
            Properties.Settings.Default.AuxAccountChecked = false;
            Properties.Settings.Default.DiscBoardChecked = false;
            CurriculumAR();
        }

        //Appends class contents notes to main note field
        private RelayCommand _addauxcommand;
        public RelayCommand AddAuxCommand { get { return _addauxcommand ?? (_addauxcommand = new RelayCommand(AddAux)); } }
        public void AddAux()
        {
            SelectedNote.Text = AuxFinal + Environment.NewLine + Environment.NewLine + SelectedNote.Text;
        }


        private string accounturl;
        public string AccountURL { get { return accounturl; } set { accounturl = value; RaisePropertyChanged(); } }
        private string accountname;
        public string AccountName { get { return accountname; } set { accountname = value; RaisePropertyChanged(); } }
        private string accountcbr;
        public string AccountCBR { get { return accountcbr; } set { accountcbr = value; RaisePropertyChanged(); } }
        private string accountfinal;
        public string AccountFinal { get { return accountfinal; } set { accountfinal = value; RaisePropertyChanged(); } }

        //Create class content notes
        private RelayCommand _createaccountcommand;
        public RelayCommand CreateAccountCommand { get { return _createaccountcommand ?? (_createaccountcommand = new RelayCommand(CreateAccount)); } }
        public void CreateAccount()
        {
            Properties.Settings.Default.AccountFinalEnabled = true;
            AccountFinal = "Site URL/Program Name: " + AccountURL + Environment.NewLine + Environment.NewLine + "Username/Password tested: " + AccountName + Environment.NewLine + Environment.NewLine + "Best Contact Phone Number: " + AccountCBR + Environment.NewLine + Environment.NewLine + "_______________________________________________________________";
            Properties.Settings.Default.AccountAddEnabled = true;
        }

        //Resets class content form
        private RelayCommand _resetaccountcommand;
        public RelayCommand ResetAccountCommand { get { return _resetaccountcommand ?? (_resetaccountcommand = new RelayCommand(ResetAccount)); } }
        public void ResetAccount()
        {
            AccountFinal = "";
            AccountURL = "";
            AccountName = "";
            AccountCBR = "";
            Properties.Settings.Default.AccountFinalEnabled = false;
            Properties.Settings.Default.AccountAddEnabled = false;

            Properties.Settings.Default.MCCAChecked = false;
            Properties.Settings.Default.ClassContentChecked = false;
            Properties.Settings.Default.AuxSiteChecked = false;
            Properties.Settings.Default.AuxAccountChecked = false;
            Properties.Settings.Default.DiscBoardChecked = false;
            CurriculumAR();
        }

        //Appends class contents notes to main note field
        private RelayCommand _addaccountcommand;
        public RelayCommand AddAccountCommand { get { return _addaccountcommand ?? (_addaccountcommand = new RelayCommand(AddAccount)); } }
        public void AddAccount()
        {
            SelectedNote.Text = AccountFinal + Environment.NewLine + Environment.NewLine + SelectedNote.Text;
        }

        private string discurl;
        public string DiscURL { get { return discurl; } set { discurl = value; RaisePropertyChanged(); } }
        private string discname;
        public string DiscName { get { return discname; } set { discname = value; RaisePropertyChanged(); } }
        private string discbrowser;
        public string DiscBrowser { get { return discbrowser; } set { discbrowser = value; RaisePropertyChanged(); } }
        private string discpath;
        public string DiscPath { get { return discpath; } set { discpath = value; RaisePropertyChanged(); } }
        private string discfinal;
        public string DiscFinal { get { return discfinal; } set { discfinal = value; RaisePropertyChanged(); } }

        //Create class content notes
        private RelayCommand _createdisccommand;
        public RelayCommand CreateDiscCommand { get { return _createdisccommand ?? (_createdisccommand = new RelayCommand(CreateDisc)); } }
        public void CreateDisc()
        {
            Properties.Settings.Default.DiscFinalEnabled = true;
            DiscFinal = "Class URL: " + DiscURL + Environment.NewLine + Environment.NewLine + "Teachers Name: " + DiscName + Environment.NewLine + Environment.NewLine + "Browser: " + DiscBrowser + Environment.NewLine + Environment.NewLine + "Path: " + DiscPath + Environment.NewLine + Environment.NewLine + "_______________________________________________________________";
            Properties.Settings.Default.DiscAddEnabled = true;
        }

        //Resets class content form
        private RelayCommand _resetdisccommand;
        public RelayCommand ResetDiscCommand { get { return _resetdisccommand ?? (_resetdisccommand = new RelayCommand(ResetDisc)); } }
        public void ResetDisc()
        {
            DiscFinal = "";
            DiscURL = "";
            DiscName = "";
            DiscBrowser = "";
            DiscPath = "";
            Properties.Settings.Default.DiscFinalEnabled = false;
            Properties.Settings.Default.DiscAddEnabled = false;

            Properties.Settings.Default.MCCAChecked = false;
            Properties.Settings.Default.ClassContentChecked = false;
            Properties.Settings.Default.AuxSiteChecked = false;
            Properties.Settings.Default.AuxAccountChecked = false;
            Properties.Settings.Default.DiscBoardChecked = false;
            CurriculumAR();
        }

        //Appends class contents notes to main note field
        private RelayCommand _adddisccommand;
        public RelayCommand AddDiscCommand { get { return _adddisccommand ?? (_adddisccommand = new RelayCommand(AddDisc)); } }
        public void AddDisc()
        {
            SelectedNote.Text = DiscFinal + Environment.NewLine + Environment.NewLine + SelectedNote.Text;
        }  
        #endregion

        #endregion

        #region Call history

        private NoteManager noteManager = new NoteManager();

        public void Saveallnotesonclose(object sender, CancelEventArgs e)
        {
            SaveAllNotes();
        }

        private async void SaveAllNotes()
        {
            foreach (NoteViewModel n in Notes)
            {
                await noteManager.SaveCurrent(n);
            }

        }

        private ObservableCollection<NoteType> _HistoricNotes;
        public ObservableCollection<NoteType> HistoricNotes { get { return _HistoricNotes; } set { _HistoricNotes = value; RaisePropertyChanged();  } }
        private NoteType _SelectedHistoryNote;
        public NoteType SelectedHistoryNote { get { return _SelectedHistoryNote; } set { _SelectedHistoryNote = value; RaisePropertyChanged(); } }
        public Nullable<DateTime> HistoryDate { get; set; }
        private RelayCommand _HistoryLookupCommand;
        public RelayCommand HistoryLookupCommand { get { return _HistoryLookupCommand ?? (_HistoryLookupCommand = new RelayCommand(GetHistory)); } }
        private async void GetHistory()
        {
            HistoricNotes = await noteManager.GetArchivedNotes(HistoryDate ?? DateTime.Now);
        }
        
        //private void DumpNotes(object sender, EventArgs e)
        //{
        //    var crashTime = DateTime.Now;
        //    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), crashTime.ToString().Replace(@"/", ".").Replace(":", ".")).Replace(" ", "_");
        //    System.IO.Directory.CreateDirectory(path);
        //    foreach (NoteViewModel note in Notes)
        //    {
        //        if (note.Text != string.Empty)
        //        {
        //            var p = Path.Combine(path, string.Format(@"{0}.txt", note.Title));
        //            try
        //            {
        //                System.IO.File.WriteAllText(p, note.Text);
        //            }
        //            catch(Exception ex)
        //            {
        //                log.Fatal(ex);
        //            }
        //        }
        //    }
        //}


        #endregion

        #region Drag & Drop Tests
        private RelayCommand<DragEventArgs> _dropCommand;
        public RelayCommand<DragEventArgs> DropCommand { get { return _dropCommand ?? (_dropCommand = new RelayCommand<DragEventArgs>(Drop)); } }

        private static void Drop(DragEventArgs e)
        {
            // do something here
        } 
        #endregion

    }
}
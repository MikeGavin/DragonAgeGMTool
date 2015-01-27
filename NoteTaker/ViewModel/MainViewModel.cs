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

        //Singleton instance of the DB to sync data across view models
        //private Singleton _dataB;
        //public Singleton DataB { get { return _dataB ?? (_dataB = Singleton.Instance); RaisePropertyChanged(); } }
        public DatabaseStorage DataB { get; set; }
          
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
        public RelayCommand<INote> NewNoteCommand { get { return _newNoteCommand ?? (_newNoteCommand = new RelayCommand<INote>((parm) => NewNote("RelayCommand", parm) )); } }
        private async void NewNote([CallerMemberName]string memberName = "", INote note = null )
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

        private RelayCommand _SettingsExpandCommand;
        public RelayCommand SettingsExpandCommand { get { return _SettingsExpandCommand ?? (_SettingsExpandCommand = new RelayCommand(SettingsExpand)); } }
        public void SettingsExpand()
        {
            if (Properties.Settings.Default.SettingsExpanded == false && Properties.Settings.Default.SettingsVisibility == false)
            {
                Properties.Settings.Default.SettingsExpanded = true;
                Properties.Settings.Default.HistoryVisibility = false;
                Properties.Settings.Default.SettingsVisibility = true;
            }
            else if (Properties.Settings.Default.SettingsExpanded == true && Properties.Settings.Default.SettingsVisibility == false)
            {
                Properties.Settings.Default.SettingsExpanded = true;
                Properties.Settings.Default.HistoryVisibility = false;
                Properties.Settings.Default.SettingsVisibility = true;
            }
            else if (Properties.Settings.Default.SettingsExpanded == false && Properties.Settings.Default.SettingsVisibility == true)
            {
                Properties.Settings.Default.SettingsExpanded = true;
            }
            else if (Properties.Settings.Default.SettingsExpanded == true && Properties.Settings.Default.SettingsVisibility == true)
            {
                Properties.Settings.Default.SettingsExpanded = false;                
            }

        }

        private RelayCommand _openHistoryCommand;
        public RelayCommand OpenHistoryCommand { get { return _openHistoryCommand ?? (_openHistoryCommand = new RelayCommand(OpenHistory)); } }
        public void OpenHistory()
        {
            if (Properties.Settings.Default.SettingsExpanded == false && Properties.Settings.Default.HistoryVisibility == false)
            {
                Properties.Settings.Default.SettingsExpanded = true;
                Properties.Settings.Default.HistoryVisibility = true;
                Properties.Settings.Default.SettingsVisibility = false;
            }
            else if (Properties.Settings.Default.SettingsExpanded == false && Properties.Settings.Default.HistoryVisibility == true)
            {
                Properties.Settings.Default.SettingsExpanded = true;
                Properties.Settings.Default.HistoryVisibility = true;
                Properties.Settings.Default.SettingsVisibility = false;
            }
            else if (Properties.Settings.Default.SettingsExpanded == true && Properties.Settings.Default.HistoryVisibility == false)
            {
                Properties.Settings.Default.SettingsExpanded = true;
                Properties.Settings.Default.HistoryVisibility = true;
                Properties.Settings.Default.SettingsVisibility = false;
            }            
            else if (Properties.Settings.Default.SettingsExpanded == true && Properties.Settings.Default.HistoryVisibility == true)
            {
                Properties.Settings.Default.SettingsExpanded = false;
                Properties.Settings.Default.HistoryVisibility = false;
                Properties.Settings.Default.SettingsVisibility = true;
            }

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

        private RelayCommand _curriculumarcommand;
        public RelayCommand CurriculumARCommand { get { return _curriculumarcommand ?? (_curriculumarcommand = new RelayCommand(CurriculumAR)); } }
        public void CurriculumAR()
        {
            if (Properties.Settings.Default.ClassContentVisibility == Visibility.Collapsed || Properties.Settings.Default.MCCAVisibility == Visibility.Collapsed || Properties.Settings.Default.AuxSiteVisibility == Visibility.Collapsed || Properties.Settings.Default.AuxAccountVisibility == Visibility.Collapsed || Properties.Settings.Default.DiscBoardVisibility == Visibility.Collapsed)
            {
                Properties.Settings.Default.ClassContentChecked = false;
                Properties.Settings.Default.MCCAChecked = false;
                Properties.Settings.Default.AuxSiteChecked = false;
                Properties.Settings.Default.AuxAccountChecked = false;
                Properties.Settings.Default.DiscBoardChecked = false;

                Properties.Settings.Default.ClassContentVisibility = Visibility.Visible;
                Properties.Settings.Default.MCCAVisibility = Visibility.Visible;
                Properties.Settings.Default.AuxSiteVisibility = Visibility.Visible;
                Properties.Settings.Default.AuxAccountVisibility = Visibility.Visible;
                Properties.Settings.Default.DiscBoardVisibility = Visibility.Visible;

                Properties.Settings.Default.CCGridVisibility = Visibility.Collapsed;
            }
            else if (Properties.Settings.Default.ClassContentChecked == true)
            {
                Properties.Settings.Default.MCCAVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AuxSiteVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AuxAccountVisibility = Visibility.Collapsed;
                Properties.Settings.Default.DiscBoardVisibility = Visibility.Collapsed;
            }
            else if (Properties.Settings.Default.MCCAChecked == true)
            {
                Properties.Settings.Default.ClassContentVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AuxSiteVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AuxAccountVisibility = Visibility.Collapsed;
                Properties.Settings.Default.DiscBoardVisibility = Visibility.Collapsed;
            }
            else if (Properties.Settings.Default.AuxSiteChecked == true)
            {
                Properties.Settings.Default.ClassContentVisibility = Visibility.Collapsed;
                Properties.Settings.Default.MCCAVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AuxAccountVisibility = Visibility.Collapsed;
                Properties.Settings.Default.DiscBoardVisibility = Visibility.Collapsed;
            }
            else if (Properties.Settings.Default.AuxAccountChecked == true)
            {
                Properties.Settings.Default.ClassContentVisibility = Visibility.Collapsed;
                Properties.Settings.Default.MCCAVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AuxSiteVisibility = Visibility.Collapsed;
                Properties.Settings.Default.DiscBoardVisibility = Visibility.Collapsed;
            }
            else if (Properties.Settings.Default.DiscBoardChecked == true)
            {
                Properties.Settings.Default.ClassContentVisibility = Visibility.Collapsed;
                Properties.Settings.Default.MCCAVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AuxSiteVisibility = Visibility.Collapsed;
                Properties.Settings.Default.AuxAccountVisibility = Visibility.Collapsed;
            }
            else
            {
                Properties.Settings.Default.ClassContentVisibility = Visibility.Visible;
                Properties.Settings.Default.MCCAVisibility = Visibility.Visible;
                Properties.Settings.Default.AuxSiteVisibility = Visibility.Visible;
                Properties.Settings.Default.AuxAccountVisibility = Visibility.Visible;
                Properties.Settings.Default.DiscBoardVisibility = Visibility.Visible;
            }
            if (Properties.Settings.Default.ClassContentVisibility == Visibility.Visible && Properties.Settings.Default.MCCAVisibility == Visibility.Collapsed && Properties.Settings.Default.AuxSiteVisibility == Visibility.Collapsed && Properties.Settings.Default.AuxAccountVisibility == Visibility.Collapsed && Properties.Settings.Default.DiscBoardVisibility == Visibility.Collapsed)
            {
                Properties.Settings.Default.CCGridVisibility = Visibility.Visible;
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

        private RelayCommand _createcccommand;
        public RelayCommand CreateCCCommand { get { return _createcccommand ?? (_createcccommand = new RelayCommand(CreateCC)); } }
        public void CreateCC()
        {
            Properties.Settings.Default.CCFinalEnabled = true;
            CCFinal = "Class URL: " + CCURL + Environment.NewLine + Environment.NewLine + "Teachers Name: " + CCName + Environment.NewLine + Environment.NewLine + "Browser: " + CCBrowser + Environment.NewLine + Environment.NewLine + "Path: " + CCPath + Environment.NewLine + Environment.NewLine + "_______________________________________________________________";
            Properties.Settings.Default.CCAddEnabled = true;
        }

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
        }

        private RelayCommand _addcccommand;
        public RelayCommand AddCCCommand { get { return _addcccommand ?? (_addcccommand = new RelayCommand(AddCC)); } }
        public void AddCC()
        {
            SelectedNote.Text = CCFinal + Environment.NewLine + Environment.NewLine + SelectedNote.Text;
        } 
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
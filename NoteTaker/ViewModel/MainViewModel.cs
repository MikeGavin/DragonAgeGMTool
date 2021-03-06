﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Scrivener.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using Scrivener.Helpers;
using System.Collections.Specialized;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using System.Runtime.CompilerServices;
using System.IO;
using System.Reactive.Linq;
using NLog.Targets;
using NLog;
using System.Text;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Interactivity;


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
            //Application.Current.MainWindow.Closing += (s, e) => SaveAllNotes();

            //Checks deployment and enables update systems if not in debug
            DeploymentSetup();

            //create DB singleton
            DataB = DatabaseStorage.Instance;            
           
            //Listen for note collection change
            Notes.CollectionChanged += OnNotesChanged;           
            
            //Auto save settings on any change.
            Properties.Settings.Default.PropertyChanged += Settings_PropertyChanged;

            NewName = "";

        }

        //WindowLoaded runs functions only availalbe after window has loaded and are unavailable in constructor.
        public async void WindowLoaded()
        {
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

        private RelayCommand _dbUpdated_Click;
        public RelayCommand DBUpdated_Click { get { return _dbUpdated_Click ?? (_dbUpdated_Click = new RelayCommand(() => DBUpdated = false)); } }
        
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

        //https://code.msdn.microsoft.com/windowsdesktop/Grouping-Expanders-just-b1bbba57
        private string _CurrentExpanded;
        public string CurrentExpanded { get { return _CurrentExpanded; } set { if (_CurrentExpanded != value) { _CurrentExpanded = value; RaisePropertyChanged(); } } }

        private bool _formsVisibility;
        public bool FormsVisibility { get { return _formsVisibility; } set { _formsVisibility = value; RaisePropertyChanged(); } }

        #region Note System

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
        private void OnNoteRequestClose(object sender, EventArgs e)
        {
            NoteViewModel note = sender as NoteViewModel;
            CloseNote(note);
        }        

        private async Task CloseNote(NoteViewModel note)
        {
            if (Properties.Settings.Default.CharacterNameBoxVis == false)
            {
                Notes.Remove(note);

                if (Notes.Count == 0)
                {
                    NewNote();
                }
            }

            //if (Properties.Settings.Default.Saveonclose == false)
            //{
            //    if (Scrivener.Properties.Settings.Default.Close_Warning == true)
            //    {
            //        var result = await Helpers.MetroMessageBox.ShowResult("WARNING!", string.Format("Are you sure you want to close '{0}'?", note.Title));
            //        if (result == true)
            //        {
            //            await SetLastSaveClose(note);
            //        }
            //    }
            //    else if (Scrivener.Properties.Settings.Default.Close_Warning == false)
            //    {
            //        await SetLastSaveClose(note);
            //    }

            //    if (Notes.Count == 0)
            //    {
            //        NewNote();
            //    }
            //}
            //else if (Properties.Settings.Default.Saveonclose == true && SelectedNote.Text != "")
            //{
            //    if (Scrivener.Properties.Settings.Default.Close_Warning == true)
            //    {
            //        var result = await Helpers.MetroMessageBox.ShowResult("WARNING!", string.Format("Are you sure you want to close '{0}'?", note.Title));
            //        if (result == true)
            //        {
            //            await SetLastSaveClose(note);
            //        }
            //    }
            //    else if (Scrivener.Properties.Settings.Default.Close_Warning == false)
            //    {
            //        await SetLastSaveClose(note);
            //    }

            //    if (Notes.Count == 0)
            //    {
            //        NewNote();
            //    }

            //}
            //else
            //{
            //    if (Scrivener.Properties.Settings.Default.Close_Warning == true)
            //    {
            //        var result = await Helpers.MetroMessageBox.ShowResult("WARNING!", string.Format("Are you sure you want to close '{0}'?", note.Title));
            //        if (result == true)
            //        {
            //            await SetLastSaveClose(note);
            //        }
            //    }
            //    else if (Scrivener.Properties.Settings.Default.Close_Warning == false)
            //    {
            //        await SetLastSaveClose(note);
            //    }

            //    if (Notes.Count == 0)
            //    {
            //        NewNote();
            //    }
            //}
        }

        private async Task SetLastSaveClose(NoteViewModel note)
        {
            await Task.Factory.StartNew(async () =>
            {
                if (note.Text != "" && note.Text != Properties.Settings.Default.Default_Note_Template)
                {
                    await noteManager.SaveCurrent(note);
                    lastClosedNote = note;
                    await noteManager.ArchiveCurrent(note);
                }
                Notes.Remove(note);                
            });
        }

        public string NewName { get { return Properties.Settings.Default.NewName; } set { if (value != Properties.Settings.Default.NewName) { Properties.Settings.Default.NewName = value; } RaisePropertyChanged(); } }
        
        private RelayCommand _newNameCommand;
        public RelayCommand NewNameCommand { get { return _newNameCommand ?? (_newNameCommand = new RelayCommand(NameChange)); } }
        public void NameChange()
        {
            if (NewName != "")
            {
                SelectedNote.Title = NewName;
                Properties.Settings.Default.CharacterNameBoxVis = false;
                NewName = "";                
            }
        }

        private RelayCommand _closeNameCommand;
        public RelayCommand CloseNameCommand { get { return _closeNameCommand ?? (_closeNameCommand = new RelayCommand(CloseName)); } }
        public void CloseName()
        {
                Properties.Settings.Default.CharacterNameBoxVis = false;
                NewName = "";
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

        #endregion

        #region ToolBar
 
        private RelayCommand _Settingsbuttoncommand;
        public RelayCommand Settingsbuttoncommand { get { return _Settingsbuttoncommand ?? (_Settingsbuttoncommand = new RelayCommand(Settingsbutton)); } }
        public void Settingsbutton()
        {
            if (Properties.Settings.Default.SettingsExpanded == false)
            {
                Properties.Settings.Default.QARExpanded = false;
                Properties.Settings.Default.SettingsExpanded = true;
            }
            else
            {
                Properties.Settings.Default.SettingsExpanded = false;
            }
        }

        private RelayCommand _SettingsExpandCommand;
        public RelayCommand SettingsExpandCommand { get { return _SettingsExpandCommand ?? (_SettingsExpandCommand = new RelayCommand(SettingsExpand)); } }
        public void SettingsExpand()
        {            
            if (Properties.Settings.Default.SettingsExpanded == false)
            {
                Properties.Settings.Default.QARExpanded = false;
                Properties.Settings.Default.SettingsExpanded = true;
                Properties.Settings.Default.SettingsSelected = true;
            }
            else if (Properties.Settings.Default.SettingsExpanded == true && Properties.Settings.Default.SettingsSelected == false)
            {
                Properties.Settings.Default.SettingsSelected = true;
            }
            else
            {
                Properties.Settings.Default.SettingsExpanded = false;
            }
        }
        
        #endregion

        #region Settings

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

                ARHelperVisibility(Properties.Settings.Default.Role_Current);
            }
            Properties.Settings.Default.Save();
            
        }

        private void ARHelperVisibility(RoleItem role)
        {
            if (role.Name.ToLower() == "Help Desk".ToLower())
            {
                FormsVisibility = true;
            }
            else
            {
                FormsVisibility = false;
            }
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
                if (n.Text != "" && n.Text != Properties.Settings.Default.Default_Note_Template)
                {
                    await noteManager.SaveCurrent(n);
                }
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

        
    }
}
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
        private bool _updated;
        public bool Updated { get { return _updated; } protected set { _updated = value; RaisePropertyChanged(); } }
        private bool _dbupdated;
        public bool DBUpdated { get { return _dbupdated; } protected set { _dbupdated = value; RaisePropertyChanged(); } }
        private RelayCommand _dbUpdated_Click;
        public RelayCommand DBUpdated_Click { get { return _dbUpdated_Click ?? (_dbUpdated_Click = new RelayCommand(() => DBUpdated = false)); } }

        private string appMode;
        public string AppMode { get { return appMode; } protected set { appMode = value; RaisePropertyChanged(); } }
        
        //Constructor
        public MainViewModel(IDataService dataService)
        {
            //Event Listener to auto save notes if application failes through unhandeled expection
            App.Fucked += SaveNotes;
            DataB = Singleton.Instance;
            
            //Checks deployment and enables update systems if necessary
            DeploymentCheck();
           
            //Listen for note collection change
            Notes.CollectionChanged += OnNotesChanged;           
            
            //Auto save settings on any change.
            Properties.Settings.Default.PropertyChanged += Settings_PropertyChanged;

            //Self Explained
            //CleanDatabase();           
            //StartNoteSaveTask();            
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
            await DataB.LoadSites(Properties.Settings.Default.Role_Current);
            //Hack to set current role in combobox
            var role = Roles.First((i) => i.Name == Properties.Settings.Default.Role_Current.Name);
            RolesView.MoveCurrentTo(role);
            if (Notes.Count == 0)
            {
                NewNote();
            }

        }

        //Deployment Systems
        private void DeploymentCheck()
        {
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                Uri uri = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.UpdateLocation;
                if (uri.LocalPath.ToLower().Contains("dev"))
                {
                    //Start auto update system and subscribe to event
                    var updateManager = new UpdateManager(uri);
                    updateManager.UpdateComplete += UpdateComplete;
                    //listen for DB updates
                    var WatchDataBase = new DataBaseWatcher(uri);
                    DataBaseWatcher.DataBaseUpdated += (o, e) => { this.ReloadData(o, e.FullPath); DBUpdated = true; };
                    AppMode = "development";
                }
                else if (uri.LocalPath.ToLower().Contains(@"\\fs1\edTech\scrivener"))
                {
                    AppMode = "Production";
                }
                else
                {
                    AppMode = "unknown";
                }
            }
            else
            {
                AppMode = "debug";
            }
        }
        void UpdateComplete(object sender, AsyncCompletedEventArgs e)
        {

                Updated = true;
                Observable
                    .Timer(DateTimeOffset.Parse("23:59:00-04:00"))
                    .Subscribe(x =>
                    {
                        SaveNotes(this, new EventArgs());
                        log.Debug("Quitting application due to installed update.");
                        Process.GetCurrentProcess().Kill();

                    });
        }

        //Singleton instance of the DB to sync data across view models
        //private Singleton _dataB;
        //public Singleton DataB { get { return _dataB ?? (_dataB = Singleton.Instance); RaisePropertyChanged(); } }
        public Singleton DataB { get; set; }

        //builds or gets QuickItems
        private QuickItem _root;
        private QuickItem QuickItemTree { get { return _root ?? (_root = LocalDatabase.ReturnQuickItems(Properties.Settings.Default.Role_Current).Result); } set { _root = value; RaisePropertyChanged(); } }
        //Builds or gets collection of commands used by minion
        private ObservableCollection<MinionCommandItem> _minionCommands;
        private ObservableCollection<MinionCommandItem> MinionCommands { get { return _minionCommands ?? (_minionCommands = Model.LocalDatabase.ReturnMinionCommands(Properties.Settings.Default.Role_Current).Result); } set { _minionCommands = value; RaisePropertyChanged(); } }
          
        //Note Collection
        private ObservableCollection<NoteViewModel> _Notes = new ObservableCollection<NoteViewModel>();
        public ObservableCollection<NoteViewModel> Notes { get { return _Notes; } set { _Notes = value; RaisePropertyChanged(); } }
        private NoteViewModel _SelectedNote;
        public NoteViewModel SelectedNote { get { return _SelectedNote; } set { _SelectedNote = value; RaisePropertyChanged(); } }

        //Closing of notes
        void OnNotesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (NoteViewModel note in e.NewItems)
                    note.RequestClose += this.OnNoteRequestClose;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (NoteViewModel workspace in e.OldItems)
                    workspace.RequestClose -= this.OnNoteRequestClose;
        }
        void OnNoteRequestClose(object sender, EventArgs e)
        {
            NoteViewModel note = sender as NoteViewModel;
            CloseNote(note);
        }
        private async void CloseNote(NoteViewModel note)
        {            
            if (Scrivener.Properties.Settings.Default.Close_Warning == true)
            {
                var result = await Helpers.MetroMessageBox.ShowResult("WARNING!", string.Format("Are you sure you want to close '{0}'?", note.Title));
                if (result == true)
                {
                    //Closereplacenotes();
                    Notes.Remove(note);
                }
            }
            else if (Scrivener.Properties.Settings.Default.Close_Warning == false)
            {
                //Closereplacenotes();
                Notes.Remove(note);
            }
            if (Notes.Count == 0)
                NewNote();
        }

        //New Notes
        private RelayCommand<string> _newNoteCommand;
        public RelayCommand<string> NewNoteCommand { get { return _newNoteCommand ?? (_newNoteCommand = new RelayCommand<string>((parm) => NewNote("RelayCommand") )); } }
        private async void NewNote([CallerMemberName]string memberName = "")
        {
            //async versions to attempt to keep interface from locking
            if (QuickItemTree == null)
            {
                QuickItemTree = await LocalDatabase.ReturnQuickItems(Properties.Settings.Default.Role_Current);
            }
            if (MinionCommands == null)
            {
                MinionCommands = await LocalDatabase.ReturnMinionCommands(Properties.Settings.Default.Role_Current);
            }

            //CreateCallHistory();
            log.Debug("{0} ran NewNote", memberName);
            Notes.Add(new NoteViewModel(QuickItemTree, MinionCommands));
            SelectedNote = Notes.Last();
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

        //Listener for settings changed properity in order to clear out imports
        void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "Role_Current" & Properties.Settings.Default.Role_Current != null))
            {
                ////Hack to set current role in combobox
                var role = Roles.First((i) => i.Name == Properties.Settings.Default.Role_Current.Name);
                RolesView.MoveCurrentTo(role);

                //reset roll properties to force updates.
                ReloadData(sender, "scrivener.sqlite");
                Properties.Settings.Default.Minion_Visibility = Properties.Settings.Default.Role_Current.Minion;
                if (Notes.Count == 0)
                {
                    NewNote();
                }
            }
            Properties.Settings.Default.Save();
            
        }

        private void ReloadData(object o, string f)
        {
            if (f.ToLower().Contains("scrivener.sqlite"))
            {
                log.Debug("{0} requested nulling of tree, sites, & Minion commands.", o.ToString());
                QuickItemTree = null;
                DataB.LoadSites(Properties.Settings.Default.Role_Current);
                MinionCommands = null;
                
            }
        }

        
        //private static ObservableCollection<RoleItem> _roles;
        public ObservableCollection<RoleItem> Roles { get { return DataB.Roles; } }

        public static CollectionView _rolesView;
        public CollectionView RolesView { get { return _rolesView ?? (_rolesView = new CollectionView(Roles)); } set { _rolesView = value; RaisePropertyChanged(); } }
        public RoleItem CurrentRole { get { return Properties.Settings.Default.Role_Current; } set { if (value != Properties.Settings.Default.Role_Current) { Properties.Settings.Default.Role_Current = value; } RaisePropertyChanged(); } }

        //public bool QuicknotesVisible { get { return Properties.Settings.Default.QuickNotes_Visible; } set { Properties.Settings.Default.QuickNotes_Visible = value; RaisePropertyChanged(); } }

        //Save Template
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

        #region Call histor

        public void SaveNotes(object sender, EventArgs e)
        {
            var crashTime = DateTime.Now;
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), crashTime.ToString().Replace(@"/", ".").Replace(":", ".")).Replace(" ", "_");
            System.IO.Directory.CreateDirectory(path);
            foreach (NoteViewModel note in Notes)
            {
                if (note.Text != string.Empty)
                {
                    var p = Path.Combine(path, string.Format(@"{0}.txt", note.Title));
                    try
                    {
                        System.IO.File.WriteAllText(p, note.Text);
                    }
                    catch(Exception ex)
                    {
                        log.Fatal(ex);
                    }
                }
            }
        }

        ////builds or gets History
        //private ObservableCollection<HistoryItem> _history;
        //public ObservableCollection<HistoryItem> QuickHistory { get { return _history ?? (_history = LocalDatabase.ReturnHistory().Result); } set { _history = value; RaisePropertyChanged(); } }

        //private static void CreateCallHistory()
        //{
        //    //String for naming the table
        //    string Date = DateTime.Now.ToString("D").Replace(" ", "").Replace(",", "");
        //    //DB connection
        //    SQLiteConnection Call_history = new SQLiteConnection("Data Source=Call_History.db;Version=3;New=True;Compress=True;");
        //    //creates Call History Database and populates table with todays date if none exist
        //    string query = string.Format("CREATE TABLE IF NOT EXISTS [{0}]([ID],[Caller],[Notes]);", Date);
        //    SQLiteCommand command = new SQLiteCommand(query, Call_history);

        //    Call_history.Open();
        //    //creates DB and table for todays saving of notes 
        //    command.ExecuteNonQuery();
        //    Call_history.Close();
        //}
        //private int SaveNotes()
        //{
        //    CreateCallHistory();
        //    string Date = DateTime.Now.ToString("D").Replace(" ", "").Replace(",", "");
        //    string Title = "Title";
        //    string Text = "Text";
        //    int index = 0;
        //    SQLiteConnection Call_history = new SQLiteConnection("Data Source=Call_History.db;Version=3;New=True;Compress=True;");

        //    string count = string.Format("SELECT COUNT (ID) from {0}", Date);

        //    Call_history.Open();
        //    try
        //    {

        //        using (SQLiteCommand docount = Call_history.CreateCommand())
        //        {
        //            docount.CommandText = count;
        //            docount.ExecuteNonQuery();
        //            index = Convert.ToInt32(docount.ExecuteScalar());
        //            index++;
                    
        //            string insert = string.Format("INSERT INTO {0} (ID,Caller,Notes) values ('{1}','{2}','{3}');", Date, index, Title, Text);
                    
        //            SQLiteCommand insertcommand = new SQLiteCommand(insert, Call_history);
                    
        //            insertcommand.ExecuteNonQuery();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error(e);
        //        Model.ExceptionReporting.Email(e);
        //    }

        //    Call_history.Close();

        //    return index;

        //}

        //CancellationTokenSource tokenSource = new CancellationTokenSource();
        //public async Task ReplaceNotes(CancellationToken token)
        //{
        //    while (token.IsCancellationRequested == false)
        //    {
        //        Thread.Sleep(30000);
        //        string Date = DateTime.Now.ToString("D").Replace(" ", "").Replace(",", "");
        //        string Title = "Title";
        //        string Text = "Text";
        //        SQLiteConnection Call_history = new SQLiteConnection("Data Source=Call_History.db;Version=3;New=True;Compress=True;");
        //        await Call_history.OpenAsync();
        //        foreach (NoteViewModel n in Notes)
        //        {
        //            string replacetitle = string.Format("UPDATE {0} SET Caller = '{1}' WHERE ID = '{2}';", Date, n.Title, n.SaveIndex);
        //            string replacenote = string.Format("UPDATE {0} SET Notes = '{1}' WHERE ID = '{2}';", Date, n.Text, n.SaveIndex);
                    
        //            SQLiteCommand replacetitlecommand = new SQLiteCommand(replacetitle, Call_history);
        //            SQLiteCommand replacenotecommand = new SQLiteCommand(replacenote, Call_history);
                    
        //            try
        //            {
        //                await replacetitlecommand.ExecuteNonQueryAsync();
        //                await replacenotecommand.ExecuteNonQueryAsync();
                        
        //            }
        //            catch (Exception e)
        //            {
        //                log.Error(e);
        //                //Model.ExceptionReporting.Email(e);
        //            }
        //        }

        //        string cleantable = string.Format("DELETE FROM {0} WHERE Caller = '{1}' AND Notes = '{2}'", Date, Title, Text);
        //        SQLiteCommand cleantablecommand = new SQLiteCommand(cleantable, Call_history);
        //        try
        //            {
        //                await cleantablecommand.ExecuteNonQueryAsync();
        //            }
        //        catch (Exception e)
        //            {
        //                log.Error(e);
        //            }
        //        Call_history.Close();
        //    }
        //}
        //private void StartNoteSaveTaskX()
        //{
        //    var token = tokenSource.Token;
        //    Task noteSaving = new Task(() => ReplaceNotes(token), token, TaskCreationOptions.LongRunning);
        //    noteSaving.Start();
        //}
        
        //public async void CleanDatabase()
        //{
        //    string Mondaycheck = DateTime.Now.ToString("dddd");
        //    string Today = DateTime.Now.ToString("D").Replace(" ", "").Replace(",", "");
        //    string Yesterday = DateTime.Now.AddDays(-1).ToString("D").Replace(" ", "").Replace(",", "");
        //    string Saturday = DateTime.Now.AddDays(-2).ToString("D").Replace(" ", "").Replace(",", "");
        //    string Friday = DateTime.Now.AddDays(-3).ToString("D").Replace(" ", "").Replace(",", "");

        //    if (Mondaycheck == "Monday")

        //    {
        //        SQLiteConnection Call_history = new SQLiteConnection("Data Source=Call_History.db;Version=3;New=True;Compress=True;");
        //        await Call_history.OpenAsync();

        //        {
        //            string cleanup = String.Format("PRAGMA writable_schema = 1;delete from sqlite_master where type = 'table' AND name NOT LIKE '{0}' AND name NOT LIKE '{1}' AND name NOT LIKE '{2}';PRAGMA writable_schema = 0;", Today, Friday, Saturday);
        //            SQLiteCommand docleanup = new SQLiteCommand(cleanup, Call_history);

        //            try
        //            {
        //                await docleanup.ExecuteNonQueryAsync();
        //            }
        //            catch (Exception e)
        //            {
        //                log.Error(e);
        //                //Model.ExceptionReporting.Email(e);
        //            }
        //        }
        //        Call_history.Close();
        //    }
        //    else if (Mondaycheck != "Monday")
        //    {
        //        SQLiteConnection Call_history = new SQLiteConnection("Data Source=Call_History.db;Version=3;New=True;Compress=True;");
        //        await Call_history.OpenAsync();

        //        {
        //            string cleanup = String.Format("PRAGMA writable_schema = 1;delete from sqlite_master where type = 'table' AND name NOT LIKE '{0}' AND name NOT LIKE '{1}';PRAGMA writable_schema = 0;", Today, Yesterday);
        //            SQLiteCommand docleanup = new SQLiteCommand(cleanup, Call_history);

        //            try
        //            {
        //                await docleanup.ExecuteNonQueryAsync();
        //            }
        //            catch (Exception e)
        //            {
        //                log.Error(e);
        //                //Model.ExceptionReporting.Email(e);
        //            }
        //        }
        //        Call_history.Close();
        //    }
        //}

        //public async void Closereplacenotes()
        //{
        //    string Date = DateTime.Now.ToString("D").Replace(" ", "").Replace(",", "");
        //    string Title = "Title";
        //    string Text = "Text";
        //    SQLiteConnection Call_history = new SQLiteConnection("Data Source=Call_History.db;Version=3;New=True;Compress=True;");
        //    Call_history.Open();
        //    foreach (NoteViewModel n in Notes)
        //    {
        //        string replacetitle = string.Format("UPDATE {0} SET Caller = '{1}' WHERE ID = '{2}';", Date, n.Title, n.SaveIndex);
        //        string replacenote = string.Format("UPDATE {0} SET Notes = '{1}' WHERE ID = '{2}';", Date, n.Text, n.SaveIndex);
                
        //        SQLiteCommand replacetitlecommand = new SQLiteCommand(replacetitle, Call_history);
        //        SQLiteCommand replacenotecommand = new SQLiteCommand(replacenote, Call_history);
        //        try
        //        {
        //           await  replacetitlecommand.ExecuteNonQueryAsync();
        //           await  replacenotecommand.ExecuteNonQueryAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            log.Error(e);
        //            //Model.ExceptionReporting.Email(e);
        //        }
        //    }

        //    string cleantable = string.Format("DELETE FROM {0} WHERE Caller = '{1}' AND Notes = '{2}'", Date, Title, Text);
        //    SQLiteCommand cleantablecommand = new SQLiteCommand(cleantable, Call_history);
        //    try
        //    { 
        //        await cleantablecommand.ExecuteNonQueryAsync(); 
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error(e);
        //        //Model.ExceptionReporting.Email(e);
        //    }
        //    Call_history.Close();

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
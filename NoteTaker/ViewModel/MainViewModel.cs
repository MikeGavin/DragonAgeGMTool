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
            App.Fucked += (s,e) => SaveAllNotes();

            DataB = DatabaseStorage.Instance;
            
            //Checks deployment and enables update systems if necessary
            DeploymentCheck();
           
            //Listen for note collection change
            Notes.CollectionChanged += OnNotesChanged;           
            
            //Auto save settings on any change.
            Properties.Settings.Default.PropertyChanged += Settings_PropertyChanged;

            //Self Explained
            SettingsFolder();
            StartNoteSaveTask();
            setmidnight();
            CleanDatabase();
            //HistoryCleanuponlaunch();

            Application.Current.MainWindow.Closing += new CancelEventHandler(Saveallnotesonclose);

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
                else if (uri.LocalPath.ToLower().Contains(@"/edTech/scrivener"))
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
        public DatabaseStorage DataB { get; set; }
          
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

        public int lastnoteindex = 0;

        private async void CloseNote(NoteViewModel note)
        {            
            if (Scrivener.Properties.Settings.Default.Close_Warning == true)
            {
                var result = await Helpers.MetroMessageBox.ShowResult("WARNING!", string.Format("Are you sure you want to close '{0}'?", note.Title));
                if (result == true)
                {
                    SaveCurrentTabOnClose(note);
                    if (note.Text != Properties.Settings.Default.Default_Note_Template.ToString() && note.Text != "")
                    {
                        lastnoteindex = SelectedNote.SaveIndex;
                        
                    }
                    else { }
                    Notes.Remove(note);
                }
            }
            else if (Scrivener.Properties.Settings.Default.Close_Warning == false)
            {
                SaveCurrentTabOnClose(note);
                if (note.Text != Properties.Settings.Default.Default_Note_Template.ToString() && note.Text != "")
                {
                    lastnoteindex = SelectedNote.SaveIndex;

                }
                else { }
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
            log.Debug("{0} ran NewNote", memberName);
            Notes.Add(new NoteViewModel(CreatesHistory()));
            SelectedNote = Notes.Last();
        }

        
        //Recall Notes
        private RelayCommand<string> _RecallNoteCommand;
        public RelayCommand<string> RecallNoteCommand { get { return _RecallNoteCommand ?? (_RecallNoteCommand = new RelayCommand<string>((parm) => RecallNote("RelayCommand"))); } }
        private async void RecallNote([CallerMemberName]string memberName = "")
        {
            string lasttext = String.Format("SELECT Notes FROM CurrentHistory WHERE ID = '{0}'", lastnoteindex);
            string lasttitle = String.Format("SELECT Caller FROM CurrentHistory WHERE ID = '{0}'", lastnoteindex);
            string lasttextvalue = "";
            string lasttitlevalue = "";

            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            //SQLiteConnection Call_history = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");

            using (SQLiteConnection Call_history = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;"))
            {
                Call_history.Open();

                using (SQLiteCommand lastcalltext = Call_history.CreateCommand())
                {
                    lastcalltext.CommandText = lasttext;
                    lastcalltext.ExecuteNonQuery();
                    lasttextvalue = Convert.ToString(lastcalltext.ExecuteScalar());
                }
                using (SQLiteCommand lastcalltitle = Call_history.CreateCommand())
                {
                    lastcalltitle.CommandText = lasttitle;
                    lastcalltitle.ExecuteNonQuery();
                    lasttitlevalue = Convert.ToString(lastcalltitle.ExecuteScalar());
                }  

                Call_history.Close();

                if (lastnoteindex != 0)
                {
                    lasttextvalue = lasttextvalue.Replace("`", "'");
                    NewNote();
                    SelectedNote.Text = lasttextvalue;
                    SelectedNote.Title = lasttitlevalue;
                    lastnoteindex = 0;
                }
                else if (lastnoteindex == 0)
                { }

            }
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

        private void SettingsFolder()
        {
            //check for settings folder. Create if missing.
            var settingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Scrivener");
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
                if (Notes.Count == 0)
                {
                    NewNote();
                }
            }
            Properties.Settings.Default.Save();
            
        }

        private async void ReloadData(object o, string f)
        {
            if (f.ToLower().Contains("scrivener.sqlite"))
            {
                log.Debug("{0} requested db reload.", o.ToString());
                await DataB.LoadAll();
                //QuickItemTree = null;
                //DataB.LoadSites(Properties.Settings.Default.Role_Current);
                //MinionCommands = null;
                
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

        //builds or gets History
        //private ObservableCollection<HistoryItem> _history;
        //public ObservableCollection<HistoryItem> QuickHistory { get { return _history ?? (_history = LocalDatabase.ReturnHistory().Result); } set { _history = value; RaisePropertyChanged(); } }

        private int CreatesHistory()
        {
            //String for naming the table
            string name = "CurrentHistory";
            string name2 = "ArchiveHistory";
            //DB connection
            string Title = "Title";
            string Text = "Text";
            int initialcountvalue = 0;
            int countvalue = 0;
            int index = 0;
            
            
            string initialcount = string.Format("SELECT COUNT (ID) from {0}", name);
            string initialinsert = string.Format("INSERT INTO {0} (Date,Time,ID,Caller,Notes) values ('Date','Time','1','{1}','{2}');", name, Title, Text);
                        
            String count = "SELECT ID from CurrentHistory ORDER BY ID desc limit 1";

            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            SQLiteConnection Call_history = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");
            //creates Call History Database and populates table with todays date if none exist
            string query = string.Format("CREATE TABLE IF NOT EXISTS [{0}](Date Text,Time Text,ID Integer,Caller Text,Notes Text)", name);
            string query2 = string.Format("CREATE TABLE IF NOT EXISTS [{0}](Date Text,Time Text,ID Integer,Caller Text,Notes Text)", name2);
            
            SQLiteCommand command = new SQLiteCommand(query, Call_history);
            SQLiteCommand command2 = new SQLiteCommand(query2, Call_history);
            
            Call_history.Open();
            //creates DB and table for todays saving of notes 
            command.ExecuteNonQuery();
            command2.ExecuteNonQuery();            

            try
            {
                using (SQLiteCommand doinitialcount = Call_history.CreateCommand())
                {
                    doinitialcount.CommandText = initialcount;
                    doinitialcount.ExecuteNonQuery();
                    initialcountvalue = Convert.ToInt32(doinitialcount.ExecuteScalar());
                    if (initialcountvalue == 0)
                    {
                        SQLiteCommand initialinsertcommand = new SQLiteCommand(initialinsert, Call_history);
                        initialinsertcommand.ExecuteNonQuery();
                        index = 1;
                    }
                    else if (initialcountvalue > 0)
                    {
                        using (SQLiteCommand docount = Call_history.CreateCommand())
                        {
                            docount.CommandText = count;
                            docount.ExecuteNonQuery();
                            countvalue = Convert.ToInt32(docount.ExecuteScalar());
                        }                        

                        index = countvalue;
                        index++;

                        string insert = string.Format("INSERT INTO {0} (Date,Time,ID,Caller,Notes) values ('Date','Time','{1}','{2}','{3}');", name, index, Title, Text);

                        SQLiteCommand insertcommand = new SQLiteCommand(insert, Call_history);

                        insertcommand.ExecuteNonQuery();
                    }

                    
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                Model.ExceptionReporting.Email(e);
            }

            Call_history.Close();

            return index;
        }

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        public async Task ReplaceHistory(CancellationToken token)
        {
            
            while (token.IsCancellationRequested == false)
            {
                Thread.Sleep(30000);
                string name = "CurrentHistory";
                string Date = DateTime.Now.ToString("ddd MMM d yyyy");
                string Time = DateTime.Now.ToString("HH:mm:ss");
                string Title = "Title";
                string Text = "Text";
                
                AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                SQLiteConnection Call_history = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");
                await Call_history.OpenAsync();
                foreach (NoteViewModel n in Notes)
                {
                    string correcttext = n.Text.Replace("'", "`");
                    string replacetitle = string.Format("UPDATE {0} SET Caller = '{1}' WHERE ID = '{2}';", name, n.Title, n.SaveIndex);
                    string replacenote = string.Format("UPDATE {0} SET Notes = '{1}' WHERE ID = '{2}';", name, correcttext, n.SaveIndex);
                    string replacedate = string.Format("UPDATE {0} SET Date = '{1}' WHERE ID = '{2}';", name, Date, n.SaveIndex);
                    string replacetime = string.Format("UPDATE {0} SET Time = '{1}' WHERE ID = '{2}';", name, Time, n.SaveIndex);

                    SQLiteCommand replacetitlecommand = new SQLiteCommand(replacetitle, Call_history);
                    SQLiteCommand replacenotecommand = new SQLiteCommand(replacenote, Call_history);
                    SQLiteCommand replacedatecommand = new SQLiteCommand(replacedate, Call_history);
                    SQLiteCommand replacetimecommand = new SQLiteCommand(replacetime, Call_history);

                    try
                    {
                        await replacetitlecommand.ExecuteNonQueryAsync();
                        await replacedatecommand.ExecuteNonQueryAsync();
                        await replacenotecommand.ExecuteNonQueryAsync();
                        await replacetimecommand.ExecuteNonQueryAsync();
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                        //Model.ExceptionReporting.Email(e);
                    }
               }               
                Call_history.Close();
                midnighttimer();    
            }
        }
        private void StartNoteSaveTask()
        {
            var token = tokenSource.Token;
            Task noteSaving = new Task(() => ReplaceHistory(token), token, TaskCreationOptions.LongRunning);
            noteSaving.Start();
        }

        public void CleanDatabase()
        {
            //string Mondaycheck = DateTime.Now.ToString("dddd");
            string Today = DateTime.Now.ToString("ddd MMM d yyyy");
            //string Yesterday = DateTime.Now.AddDays(-1).ToString("ddd MMM d yyyy");
            //string Saturday = DateTime.Now.AddDays(-2).ToString("ddd MMM d yyyy");
            //string Friday = DateTime.Now.AddDays(-3).ToString("ddd MMM d yyyy");

            //if (Mondaycheck == "Monday")
            //{
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            SQLiteConnection Call_history = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");
                Call_history.Open();

                {
                    string copy = String.Format("INSERT INTO ArchiveHistory (Date,Time,ID,Caller,Notes) SELECT Date,Time,ID,Caller,Notes FROM CurrentHistory WHERE Date NOT LIKE '{0}'", Today);
                    string cleanup = String.Format("DELETE FROM CurrentHistory WHERE Date NOT LIKE '{0}'", Today);
                    string cleanupdefaultinarch = "DELETE FROM ArchiveHistory WHERE Date LIKE 'Date'";
                    SQLiteCommand docopy = new SQLiteCommand(copy, Call_history);
                    SQLiteCommand docleanup = new SQLiteCommand(cleanup, Call_history);
                    SQLiteCommand docleanupdefaultinarch = new SQLiteCommand(cleanupdefaultinarch, Call_history);

                    try
                    {
                        docopy.ExecuteNonQuery();
                        docleanup.ExecuteNonQuery();
                        docleanupdefaultinarch.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                        //Model.ExceptionReporting.Email(e);
                    }
                }
                Call_history.Close();
            ////}
            ////else if (Mondaycheck != "Monday")
            ////{
                //AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                //SQLiteConnection Call_history = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");
                //Call_history.Open();

                //{
                //    string copy = String.Format("INSERT INTO ArchiveHistory (Date,Time,ID,Caller,Notes) SELECT Date,Time,ID,Caller,Notes FROM CurrentHistory WHERE Date NOT LIKE '{0}' AND Date NOT LIKE '{1}'", Today, Yesterday);
                //    string cleanup = String.Format("DELETE FROM CurrentHistory WHERE Date NOT LIKE '{0}' AND Date NOT LIKE '{1}'", Today, Yesterday);
                //    string cleanupdefaultinarch = "DELETE FROM ArchiveHistory WHERE Date LIKE 'Date'";
                //    SQLiteCommand docopy = new SQLiteCommand(copy, Call_history);
                //    SQLiteCommand docleanup = new SQLiteCommand(cleanup, Call_history);
                //    SQLiteCommand docleanupdefaultinarch = new SQLiteCommand(cleanupdefaultinarch, Call_history);

                //    try
                //    {
                //        docopy.ExecuteNonQuery();
                //        docleanup.ExecuteNonQuery();
                //        docleanupdefaultinarch.ExecuteNonQuery();
                //    }
                //    catch (Exception e)
                //    {
                //        log.Error(e);
                //        //Model.ExceptionReporting.Email(e);
                //    }
                //}
                //Call_history.Close();
            //}
        }

        string timertoday = null;
        
        public void setmidnight()
        {
            string today = DateTime.Now.Date.ToString();
            
            timertoday = today;
        }

        public void midnighttimer()
        {
            string today = DateTime.Now.Date.ToString();
            
            if (today != timertoday)
            {
                //MessageBox.Show("It worked");
                CleanDatabase();
                setmidnight();
            }
            else { }            
        }

        public void Saveallnotesonclose(object sender, CancelEventArgs e)
        {
            SaveAllNotes();
        }

        private void SaveAllNotes()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            SQLiteConnection Call_history = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");
            Call_history.Open();
            foreach (NoteViewModel n in Notes)
            {
                SaveCurrentTabOnClose(n);
            }
            Call_history.Close();
        }

        private static void SaveCurrentTabOnClose(NoteViewModel n)

        {
            //String for naming the table
            string name = "CurrentHistory";
            //String for creating time stamp
            string Date = DateTime.Now.ToString("ddd MMM d yyyy");
            string Time = DateTime.Now.ToString("HH:mm:ss");
            //Strings for replacing "Caller" and "Notes" value
            string correcttext = n.Text.Replace("'", "`");
            string replacetitle = string.Format("UPDATE {0} SET Caller = '{1}' WHERE ID = '{2}';", name, n.Title, n.SaveIndex);
            string replacenote = string.Format("UPDATE {0} SET Notes = '{1}' WHERE ID = '{2}';", name, correcttext, n.SaveIndex);
            string replacedate = string.Format("UPDATE {0} SET Date = '{1}' WHERE ID = '{2}';", name, Date, n.SaveIndex);
            string replacetime = string.Format("UPDATE {0} SET Time = '{1}' WHERE ID = '{2}';", name, Time, n.SaveIndex);
            //Database Path
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            SQLiteConnection Call_history = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");
            //Updates notes in Database
            Call_history.Open();
            SQLiteCommand replacetitlecommand = new SQLiteCommand(replacetitle, Call_history);
            SQLiteCommand replacenotecommand = new SQLiteCommand(replacenote, Call_history);
            SQLiteCommand replacedatecommand = new SQLiteCommand(replacedate, Call_history);
            SQLiteCommand replacetimecommand = new SQLiteCommand(replacetime, Call_history);
            try
            {
                replacetitlecommand.ExecuteNonQuery();
                replacenotecommand.ExecuteNonQuery();
                replacedatecommand.ExecuteNonQuery();
                replacetimecommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                log.Error(e);
                //Model.ExceptionReporting.Email(e);
            }
            
            HistoryCleanup(n, name, Call_history);
            Call_history.Close();
            
        }

        private static void HistoryCleanup(NoteViewModel n, string name, SQLiteConnection Call_history)
        {
            //Deletes call from history if notes match default template
            if (n.Text == Properties.Settings.Default.Default_Note_Template.ToString())
            {
                string cleantable = string.Format("DELETE FROM {0} WHERE ID = '{1}'", name, n.SaveIndex);

                SQLiteCommand cleantablecommand = new SQLiteCommand(cleantable, Call_history);

                try
                {
                    cleantablecommand.ExecuteNonQueryAsync();                    
                }
                catch (Exception e)
                {
                    log.Error(e);
                    //Model.ExceptionReporting.Email(e);
                }
            }
            //Deletes call from history if notes are empty
            if (n.Text == "")
            {
                string cleantable = string.Format("DELETE FROM {0} WHERE ID = '{1}'", name, n.SaveIndex);

                SQLiteCommand cleantablecommand = new SQLiteCommand(cleantable, Call_history);

                try
                {
                    cleantablecommand.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    log.Error(e);
                    //Model.ExceptionReporting.Email(e);
                }
            }
        }

        //private static void HistoryCleanuponlaunch()
        //{
        //        string arch = "ArchiveHistory";
        //        string current = "CurrentHistory";

        //        string blank = "";
        //        string template = Properties.Settings.Default.Default_Note_Template.ToString();

        //        string cleanarch = string.Format("DELETE FROM {0} WHERE Notes = '{1}'", arch, template, blank);
        //        string cleancurrent = string.Format("DELETE FROM {0} WHERE Notes = '{1}'", current, template, blank);

        //AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        //    SQLiteConnection Call_history = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");
                
        //        SQLiteCommand cleanarchcommand = new SQLiteCommand(cleanarch, Call_history);
        //        SQLiteCommand cleancurrentcommand = new SQLiteCommand(cleancurrent, Call_history);

        //        try
        //        {
        //            MessageBox.Show("FUZZY WUZZY GOPHER NUTS");
        //            cleanarchcommand.ExecuteNonQuery();
        //            cleancurrentcommand.ExecuteNonQuery();
        //        }
        //        catch (Exception e)
        //        {
        //            log.Error(e);
        //            //Model.ExceptionReporting.Email(e);
        //        }
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
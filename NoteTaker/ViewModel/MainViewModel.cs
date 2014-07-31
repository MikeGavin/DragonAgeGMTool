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
            //Listen for note collection change
            Notes.CollectionChanged += OnNotesChanged;
            
            //Self Explained
            LoadUserSettings();
            CreateCallHistory();
            NewNote();
            StartNoteSaveTask();

        }        

        //builds or gets QuickItems
        private QuickItem _root;
        private QuickItem QuickItemTree { get { return _root ?? ( _root = new Treefiller().filltree() ); } }
        //builds or gets QuickSites
        private Siteitem _sites;
        public Siteitem QuickSites { get { return _sites ?? (_sites = new Sitefiller("Sites").fillsite()); } }
        //Builds or gets collection of commands used by minion
        private ObservableCollection<MinionCommandItem> _minionCommands;
        private ObservableCollection<MinionCommandItem> MinionCommands { get { return _minionCommands ?? (_minionCommands = Model.LocalDatabase.MinionCommands()); } }
       
     
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
                    Closereplacenotes();
                    Notes.Remove(note);
                }
            }
            else if (Scrivener.Properties.Settings.Default.Close_Warning == false)
            {
                Closereplacenotes();
                Notes.Remove(note);
            }
            if (Notes.Count == 0)
                NewNote();
        }

        //New Notes
        private RelayCommand _newNoteCommand;
        public RelayCommand NewNoteCommand { get { return _newNoteCommand ?? (_newNoteCommand = new RelayCommand(NewNote)); } }
        private async void NewNote()
        {

            Notes.Add(new NoteViewModel(QuickItemTree, MinionCommands, SaveNotes()));
            SelectedNote = Notes.Last();
        }

        #region ToolBar Items

        private RelayCommand _QuickNoteToggleCommand;
        public RelayCommand QuickNoteToggleCommand { get { return _QuickNoteToggleCommand ?? (_QuickNoteToggleCommand = new RelayCommand(QuickNoteToggle)); } }
        private string _quicknoteVisibility;
        public string QuicknoteVisibility { get { return _quicknoteVisibility; } set { _quicknoteVisibility = value; RaisePropertyChanged(); } }
        private void QuickNoteToggle()
        {
            if (QuicknoteVisibility == Visibility.Collapsed.ToString())
            {
                QuicknoteVisibility = Visibility.Visible.ToString();
            }
            else
            {
                QuicknoteVisibility = Visibility.Collapsed.ToString();
            }
        }

        //Search EKB
        private string _searchData;
        public string SearchData { get { return _searchData; } set { _searchData = value; RaisePropertyChanged(); } }
        private RelayCommand _searchboxcommand;
        public RelayCommand SearchBoxCommand { get { return _searchboxcommand ?? (_searchboxcommand = new RelayCommand(SearchKB)); } }
        public void SearchKB()
        {
            var KB = string.Format("https://ecotshare.ecotoh.net/ecotsearch/Results.aspx?k={0}&cs=This%20Site&u=https%3A%2F%2Fecotshare.ecotoh.net%2Foperations%2Fhelpdesk", SearchData);

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
                MetroMessageBox.Show("Error!", e.ToString());
            }

        }
        
        #endregion

        #region Settings
        private void LoadUserSettings()
        {
            //Load user settings. Changed to switch to allow for null settings value crashing.
            switch (Scrivener.Properties.Settings.Default.QuickNotes_Visibility)
            {
                case true:
                    QuicknoteVisibility = Visibility.Visible.ToString();
                    break;
                case false:
                    QuicknoteVisibility = Visibility.Collapsed.ToString();
                    break;
                default:
                    QuicknoteVisibility = Visibility.Visible.ToString();
                    break;
            }
        }

        //Save Template
        private RelayCommand _savetemplatecommand;
        public RelayCommand SaveTemplateCommand { get { return _savetemplatecommand ?? (_savetemplatecommand = new RelayCommand(SaveTemplate)); } }
        public void SaveTemplate()
        {

            Properties.Settings.Default.Default_Note_Template = SelectedNote.Text;
            Properties.Settings.Default.Save();
        }

        //Allows getting of current version
        private string _version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        public string Version { get { return _version; } protected set { _version = value; RaisePropertyChanged(); } }

        #endregion

        #region Call history

        private string Tempnotes = "";

        private static void CreateCallHistory()
        {
            //creates Call History Database and populates table with todays date if none exist
            string Date = DateTime.Now.ToString("D");
            Date = Date.Replace(" ", "");
            Date = Date.Replace(",", "");
            //creates DB and table for todays saving of notes 
            SQLiteConnection Call_history = new SQLiteConnection("Data Source=Call_History.db;Version=3;New=True;Compress=True;");
            string query = string.Format("CREATE TABLE IF NOT EXISTS [{0}]([ID],[Caller],[Notes]);", Date);
            SQLiteCommand command = new SQLiteCommand(query, Call_history);
            Call_history.Open();
            command.ExecuteNonQuery();
            Call_history.Close();
        }
        private int SaveNotes()
        {
            string Date = DateTime.Now.ToString("D");
            Date = Date.Replace(" ", "");
            Date = Date.Replace(",", "");
            string Title = "Title";
            string Text = "Text";
            int index =0;
            SQLiteConnection Call_history = new SQLiteConnection("Data Source=Call_History.db;Version=3;New=True;Compress=True;");

            string count = string.Format("SELECT COUNT (ID) from {0}", Date);

            Call_history.Open();
            try
            {

                using (SQLiteCommand docount = Call_history.CreateCommand())
                {
                    docount.CommandText = count;
                    docount.ExecuteNonQuery();
                    index = Convert.ToInt32(docount.ExecuteScalar());
                    index++;

                    string insert = string.Format("INSERT INTO {0} (ID,Caller,Notes) values ('{1}','{2}','{3}');", Date, index, Title, Text);
                    SQLiteCommand command = new SQLiteCommand(insert, Call_history);
                    command.ExecuteNonQuery();
                    }
            }
            catch (Exception e)
                    {
                log.Error(e);
                    }

            Call_history.Close();

            Tempnotes = Text;

            return index;
                        
        }

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        private async Task ReplaceNotes(CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                Thread.Sleep(30000);
                string Date = DateTime.Now.ToString("D");
                Date = Date.Replace(" ", "");
                Date = Date.Replace(",", "");
                SQLiteConnection Call_history = new SQLiteConnection("Data Source=Call_History.db;Version=3;New=True;Compress=True;");
                await Call_history.OpenAsync();
                foreach (NoteViewModel n in Notes)
                {
                    string replacetitle = string.Format("UPDATE {0} SET Caller = '{1}' WHERE ID = '{2}';", Date, n.Title, n.SaveIndex);
                    string replacenote = string.Format("UPDATE {0} SET Notes = '{1}' WHERE ID = '{2}';", Date, n.Text, n.SaveIndex);
                    SQLiteCommand replacetitlecommand = new SQLiteCommand(replacetitle, Call_history);
                    SQLiteCommand replacenotecommand = new SQLiteCommand(replacenote, Call_history);

                    try
                    {
                        await replacetitlecommand.ExecuteNonQueryAsync();
                        await replacenotecommand.ExecuteNonQueryAsync();
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }
                }
                Call_history.Close();
            }
        }
        private void StartNoteSaveTask()
        {
            var token = tokenSource.Token;
            Task noteSaving = new Task(() => ReplaceNotes(token), token, TaskCreationOptions.LongRunning);
            noteSaving.Start();
        }
     
        private async void Closereplacenotes()
        {
            string Date = DateTime.Now.ToString("D");
            Date = Date.Replace(" ", "");
            Date = Date.Replace(",", "");
            SQLiteConnection Call_history = new SQLiteConnection("Data Source=Call_History.db;Version=3;New=True;Compress=True;");
            await Call_history.OpenAsync();
            foreach (NoteViewModel n in Notes)
            {
                string replacetitle = string.Format("UPDATE {0} SET Caller = '{1}' WHERE ID = '{2}';", Date, n.Title, n.SaveIndex);
                string replacenote = string.Format("UPDATE {0} SET Notes = '{1}' WHERE ID = '{2}';", Date, n.Text, n.SaveIndex);
                SQLiteCommand replacetitlecommand = new SQLiteCommand(replacetitle, Call_history);
                SQLiteCommand replacenotecommand = new SQLiteCommand(replacenote, Call_history);

                try
                {
                    await replacetitlecommand.ExecuteNonQueryAsync();
                    await replacenotecommand.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
            }
            Call_history.Close();
        }

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
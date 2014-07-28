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


namespace Scrivener.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private QuickItem _root;
        private QuickItem QuickItemTree { get { return _root ?? ( _root = new Treefiller().filltree() ); } }
        private MinionCommands _minionCommands;
        private MinionCommands MinionCommands { get { return _minionCommands ?? (_minionCommands = new MinionCommands()); } }



        private string _quicknoteVisibility;
        public string QuicknoteVisibility { get { return _quicknoteVisibility; } set { _quicknoteVisibility = value; RaisePropertyChanged(); } }
        
        

        private string _version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        public string Version { get { return _version; } protected set { _version = value; RaisePropertyChanged(); }}
        
        private ObservableCollection<NoteViewModel> _Notes = new ObservableCollection<NoteViewModel>();
        public ObservableCollection<NoteViewModel> Notes { get { return _Notes; } set { _Notes = value; RaisePropertyChanged(); } }

        private NoteViewModel _SelectedNote;
        public NoteViewModel SelectedNote { get { return _SelectedNote; } set { _SelectedNote = value; RaisePropertyChanged(); } }

        

        private RelayCommand _newNoteCommand;
        public RelayCommand NewNoteCommand { get { return _newNoteCommand ?? (_newNoteCommand = new RelayCommand(NewNote)); } }        
        private RelayCommand<DragEventArgs> _dropCommand; 
        public RelayCommand<DragEventArgs> DropCommand { get { return _dropCommand ?? (_dropCommand = new RelayCommand<DragEventArgs>(Drop)); } }
        private RelayCommand _QuickNoteToggleCommand;
        public RelayCommand QuickNoteToggleCommand { get { return _QuickNoteToggleCommand ?? (_QuickNoteToggleCommand = new RelayCommand(QuickNoteToggle)); } }


        #region searchbar

        private string _searchData;
        public string SearchData { get { return _searchData; } set { _searchData = value; RaisePropertyChanged(); } }

        private RelayCommand _searchboxcommand;
        public RelayCommand SearchBoxCommand { get { return _searchboxcommand ?? (_searchboxcommand = new RelayCommand(SearchKB)); } }

        public void SearchKB()
        {
            var KB = string.Format("https://ecotshare.ecotoh.net/ecotsearch/Results.aspx?k={0}&cs=This%20Site&u=https%3A%2F%2Fecotshare.ecotoh.net%2Foperations%2Fhelpdesk", SearchData);

            Process.Start(KB);
        }

        #endregion

        #region savetemplate

        private RelayCommand _savetemplatecommand;
        public RelayCommand SaveTemplateCommand { get { return _savetemplatecommand ?? (_savetemplatecommand = new RelayCommand(SaveTemplate)); } }

        public void SaveTemplate()
        {

            Properties.Settings.Default.Default_Note_Template = SelectedNote.Text;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region site

        private Siteitem _sites;
        public Siteitem QuickSites { get { return _sites ?? (_sites = new Sitefiller("Sites").fillsite()); } }
        private RelayCommand<string> _openLinkCommand;
        public RelayCommand<string> OpenLinkCommand { get { return _openLinkCommand ?? (_openLinkCommand = new RelayCommand<string>((pram) => OpenLink(pram))); } }

        public async void OpenLink(string link)
        {
            Process.Start(link);
        }

        #endregion

        void OnTabsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (NoteViewModel note in e.NewItems)
                    note.RequestClose += this.OnTabRequestClose;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (NoteViewModel workspace in e.OldItems)
                    workspace.RequestClose -= this.OnTabRequestClose;
        }

        void OnTabRequestClose(object sender, EventArgs e)
        {
            NoteViewModel note = sender as NoteViewModel;
            CloseNote(note);
          

        }

  
        private readonly IDataService _dataService;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            //Necessary for closing correct notes.
            Notes.CollectionChanged += OnTabsChanged;
            
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

            NewNote();            
        }

        public async void CloseNote(NoteViewModel note)
        {
            if (Scrivener.Properties.Settings.Default.Close_Warning == true)
            {
            var result = await Helpers.MetroMessageBox.ShowResult("WARNING!", string.Format("Are you sure you want to close '{0}'?", note.Title));
            if (result == true)
                Notes.Remove(note);  
            }
            else if (Scrivener.Properties.Settings.Default.Close_Warning == false)
            {
                Notes.Remove(note);
            }
            if (Notes.Count == 0)
                NewNote();
        }


        public async void NewNote()
        {
            Notes.Add(new NoteViewModel(QuickItemTree, MinionCommands));
            SelectedNote = Notes.Last();
        }

        #region CopyAll

        private RelayCommand _copyallcommand;
        public RelayCommand CopyAllCommand { get { return _copyallcommand ?? (_copyallcommand = new RelayCommand(CopyAll)); } }

        public async void CopyAll()
        {
            try
                    {
                        Clipboard.SetText(SelectedNote.Text);
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show("Unable to copy");
                    }

        } 
        #endregion

        public async void QuickNoteToggle()
        {
            if (QuicknoteVisibility == "Visible")
            {
                QuicknoteVisibility = "Collapsed";
            }
            else
            {
                QuicknoteVisibility = "Visible";
            }
        }

        private static void Drop(DragEventArgs e)
        {
            // do something here
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed4

        ////    base.Cleanup();
        ////}


    }
}
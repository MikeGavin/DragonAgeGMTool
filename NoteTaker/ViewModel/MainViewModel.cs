using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Minion;
using NoteTaker.Model;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using NoteTaker.Helpers;
using System.Collections.Specialized;
using System;

namespace NoteTaker.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        private QuickItem _root;
        public QuickItem Root { get { return _root; } set { _root = value; RaisePropertyChanged(); } }

        private string _quicknoteVisibility;
        public string QuicknoteVisibility { get { return _quicknoteVisibility; } set { _quicknoteVisibility = value; RaisePropertyChanged(); } }
        
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private string _version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        public string Version { get { return _version; } protected set { _version = value; RaisePropertyChanged(); }}
        
        private ObservableCollection<NoteViewModel> _Notes = new ObservableCollection<NoteViewModel>();
        public ObservableCollection<NoteViewModel> Notes { get { return _Notes; } set { _Notes = value; RaisePropertyChanged(); } }
        private MinionCommands _minionCommands;
        public MinionCommands MinionCommands { get { return _minionCommands ?? (_minionCommands = new MinionCommands()); } }

        private NoteViewModel _SelectedNote;
        public NoteViewModel SelectedNote { get { return _SelectedNote; } set { _SelectedNote = value; RaisePropertyChanged(); } }
        
        private RelayCommand _newNoteCommand;
        public RelayCommand NewNoteCommand { get { return _newNoteCommand ?? (_newNoteCommand = new RelayCommand(NewNote)); } }        
        private RelayCommand<DragEventArgs> _dropCommand; 
        public RelayCommand<DragEventArgs> DropCommand { get { return _dropCommand ?? (_dropCommand = new RelayCommand<DragEventArgs>(Drop)); } }
        private RelayCommand _QuickNoteToggleCommand;
        public RelayCommand QuickNoteToggleCommand { get { return _QuickNoteToggleCommand ?? (_QuickNoteToggleCommand = new RelayCommand(QuickNoteToggle)); } }

        private NoteViewModel _closeItem;
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
            Notes.CollectionChanged += OnTabsChanged;
            //_minionCommands = new MinionCommands();
            //create single note
            NewNote();
            //Load user settings.
            QuicknoteVisibility = NoteTaker.Properties.Settings.Default.QuickNotes;
            var temp = new Treefiller();
            _root = temp.filltree(); 
            
            
        }

        public void CloseNote(NoteViewModel note)
        {
            
            _closeItem = note;
            var message = new DialogMessage("Close Note '" + note.Title.ToString() + "'?", DialogMessageCallback)
            {
                Button = MessageBoxButton.OKCancel,
                Caption = "Continue?"
            };

            Messenger.Default.Send(message);           
        }

        private void DialogMessageCallback(MessageBoxResult result)
        {
            if (result == MessageBoxResult.OK)
            {
                Notes.Remove(_closeItem);
            }
        }

        public async void NewNote()
        {
            Notes.Add(new NoteViewModel(_minionCommands));
            SelectedNote = Notes.Last();
            
        }

        public async void QuickNoteToggle()
        {
            await Helpers.MetroMessageBox.test();
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
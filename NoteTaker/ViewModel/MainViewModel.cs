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
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private string windowTitle = string.Format("NoteTaker {0}", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
        public string WindowTitle
        {
            get { return windowTitle; }
            set { windowTitle = value; RaisePropertyChanged(); }
        }
        

        private string _Log2Interface;
        public string Log2Interface { get { return _Log2Interface; } set { Log2Interface = value.Substring(value.IndexOf("]") + 1); ; _Log2Interface += value; RaisePropertyChanged(); } }

        private ObservableCollection<Note> _Notes = new ObservableCollection<Note>();
        public ObservableCollection<Note> Notes { get { return _Notes; } set { _Notes = value; RaisePropertyChanged(); } }

        private Note _SelectedNote;
        public Note SelectedNote { get { return _SelectedNote; } set { _SelectedNote = value; RaisePropertyChanged(); } }

        public RelayCommand CloseNoteCommand { get; set; }
        public RelayCommand NewNoteCommand { get; set; }
        
        private RelayCommand<DragEventArgs> _dropCommand; 
        public RelayCommand<DragEventArgs> DropCommand { get { return _dropCommand ?? (_dropCommand = new RelayCommand<DragEventArgs>(Drop)); } }


  
        private readonly IDataService _dataService;

        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }

            set
            {
                if (_welcomeTitle == value)
                {
                    return;
                }

                _welcomeTitle = value;
                RaisePropertyChanged(WelcomeTitlePropertyName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            CloseNoteCommand = new RelayCommand(CloseNote);
            NewNoteCommand = new RelayCommand(NewNote);
            NewNote();     

            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    WelcomeTitle = item.Title;
                });          
        }

        public void CloseNote()
        {
            Notes.Remove(SelectedNote);
        }

        public async void NewNote()
        {
            Notes.Add(new Note());
            SelectedNote = Notes.Last();
        }

        private static void Drop(DragEventArgs e)
        {
            // do something here
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}


    }
}
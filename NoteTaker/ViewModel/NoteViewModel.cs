using GalaSoft.MvvmLight.Command;
using Minion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;
using GalaSoft.MvvmLight;
using Scrivener.Model;
using Minion.ListItems;
using Scrivener.Helpers;
using System.IO;

namespace Scrivener.ViewModel
{
    public class NoteViewModel : ViewModelBase
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public NoteViewModel(QuickItem _tree, ObservableCollection<MinionCommandItem> commands)
        {
            
            Text = Properties.Settings.Default.Default_Note_Template;
            _minionCommands = commands;
            Title = string.Format("Note {0}", ++_number);
            _titlechanged = false;
            _root = _tree;
            //SaveIndex = new_index;

            DataBaseWatcher.DataBaseUpdated += DataBaseWatcher_DataBaseUpdated;
            this.TextChanged += Note_TextChanged;
            NoteMinion.MinionCollection.CollectionChanged += MinionCollection_CollectionChanged;
        }

        private async void DataBaseWatcher_DataBaseUpdated(object sender, FileSystemEventArgs e)
        {
            if (e.Name.ToLower().Contains("scrivener.sqlite"))
            {
                log.Debug("Updating MinionCommands on note {0}", Title);
                _minionCommands = await LocalDatabase.ReturnMinionCommands(Properties.Settings.Default.Role_Current);
                log.Debug("Updating QuickItems on note {0}", Title);
                Root = await LocalDatabase.ReturnQuickItems(Properties.Settings.Default.Role_Current);
            }
        }

        private string _minionVisibility;
        public string MinionVisibility { get { return _minionVisibility; } set { _minionVisibility = value; RaisePropertyChanged(); } }

        #region Public Properties
        private int _saveIndex; // used for ID for note saving
        public int SaveIndex { get { return _saveIndex; } protected set { _saveIndex = value; RaisePropertyChanged(); } }

        private static int _number = 0; // used to nuber default notes
        private bool _titlechanged = false; // defines if note title has already been changed
        private string title;
        public string Title { get { return title; } set { title = value; RaisePropertyChanged(); _titlechanged = true; } }

        private string text;
        public string Text { get { return text; } set { text = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        #endregion        

        #region EventBased Actions
        //Text change events for note
        internal void RaiseTextChanged()
        {
            if (TextChanged != null) { TextChanged(this, new EventArgs()); }
        }
        public event EventHandler TextChanged;
        //listener runs regex checks on text change
        void Note_TextChanged(object sender, EventArgs e)
        {
            Regex ip = new Regex(@"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b\s+");
            Regex sepid = new Regex(@"[a-z]{2,3}[0-9]{5,6} ", RegexOptions.IgnoreCase);


            MatchCollection mc = ip.Matches(Text);

            foreach (Match m in mc)
            {

            }

            if (_titlechanged == false) //Changes Title to first SEP entered then stops.
            {
                MatchCollection sepmatches = sepid.Matches(Text);
                if (sepmatches.Count > 0)
                {
                    Title = sepmatches[0].ToString().Trim();
                    _titlechanged = true;
                }

            }
        }

        /// <summary>
        /// Raised when this workspace should be removed from the UI.
        /// </summary>
        public event EventHandler RequestClose;
        void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        private RelayCommand _closeNoteCommand;
        public RelayCommand CloseNoteCommand { get { return _closeNoteCommand ?? (_closeNoteCommand = new RelayCommand(OnRequestClose)); } }

        #endregion

        #region Minion
        //Commands from constructor
        private ObservableCollection<MinionCommandItem> _minionCommands;
        // local minion instance for this note.
        private MinionViewModel _noteMinion;
        public MinionViewModel NoteMinion { get { return _noteMinion ?? (_noteMinion = new MinionViewModel(_minionCommands)); } set { _noteMinion = value; RaisePropertyChanged(); } }
        //Register and Unregister Minion instances to allow writing to note
        void MinionCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (MinionItemViewModel minionItem in e.NewItems)
                {
                    minionItem.NoteWrite += minionItem_NoteWrite;
                }

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (MinionItemViewModel minionItem in e.OldItems)
                {
                    minionItem.NoteWrite -= minionItem_NoteWrite;
                }
        }
        //fires on event to write note from minion instance with message
        void minionItem_NoteWrite(object sender, MinionArgs e)
        {
            Text += Environment.NewLine + e.Message;
        } 
        #endregion
      
        #region QuickItems
        //Tree
        private QuickItem _root;
        public QuickItem Root { get { return _root; } set { _root = value; RaisePropertyChanged(); } }
        private QuickItem _selectedQuickItem;
        public QuickItem SelectedQuickItem { get { return _selectedQuickItem; } set { _selectedQuickItem = value; RaisePropertyChanged(); } }
        
        //Append
        private RelayCommand<QuickItem> _appendQuickItem;
        public RelayCommand<QuickItem> AppendQuickItemCommand { get { return _appendQuickItem ?? (_appendQuickItem = new RelayCommand<QuickItem>((pram) => AppendQuickItem(pram))); } }
        public void AppendQuickItem(QuickItem note)
        {
            
            if (note != null)
            {
                try
                {
                    if (note.SubItems.Count == 0) // causes crash if null
                    {
                        if (Properties.Settings.Default.DashinNotes == true)
                        {
                            Text += "- " + note.Content + System.Environment.NewLine;
                            //SaveNotes();
                        }
                        else if (Properties.Settings.Default.DashinNotes == false)
                        {
                            Text += note.Content + System.Environment.NewLine;
                            //SaveNotes();
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error(e);
                    var temp = MetroMessageBox.Show("NOPE!", e.ToString());
                }
            }
        }
        
        //private RelayCommand _copyQuickItemCommand;
        //public RelayCommand CopyQuickItemCommand { get { return _copyQuickItemCommand ?? (_copyQuickItemCommand = new RelayCommand(CopyQuickItem)); } }
        //public void CopyQuickItem()
        //{
        //    //if (_selectedQuickItem != null)
        //    //{
        //    //    if (SelectedQuickItem.SubItems.Count > 0)
        //    //        return;
        //    //    else
        //    //    {
        //    //        try
        //    //        {
        //    //            Clipboard.SetText(SelectedQuickItem.Content);
        //    //        }
        //    //        catch (Exception e)
        //    //        {
        //    //            log.Error(e);
        //    //        }

        //    //    }
        //    //}
        //} 
        #endregion
    }
}

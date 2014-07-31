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

namespace Scrivener.ViewModel
{
    public class NoteViewModel : ViewModelBase
    {
        #region Events
        internal void RaiseTextChanged()
        {
            if (TextChanged != null) { TextChanged(this, new EventArgs()); }
        }
        public event EventHandler TextChanged;

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
        #endregion

        #region Public Properties
        private int _saveIndex;
        public int SaveIndex { get { return _saveIndex; } protected set { _saveIndex = value; RaisePropertyChanged(); } }
        private string title;
        public string Title { get { return title; } set { title = value; RaisePropertyChanged(); _titlechanged = true; } }
        private string text;
        public string Text { get { return text; } set { text = value; RaisePropertyChanged(); RaiseTextChanged(); } }        
        #endregion
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private static int _number = 0;
        private bool _titlechanged = false;

        private QuickItem _root;
        public QuickItem Root { get { return _root; } set { _root = value; RaisePropertyChanged(); } }
        private QuickItem _selectedQuickItem;
        public QuickItem SelectedQuickItem { get { return _selectedQuickItem; } set { _selectedQuickItem = value; RaisePropertyChanged(); } }
        


        private RelayCommand _closeNoteCommand;
        public RelayCommand CloseNoteCommand { get { return _closeNoteCommand ?? (_closeNoteCommand = new RelayCommand(OnRequestClose)); } }

        // local minion instance for this note.
        private ObservableCollection<MinionCommandItem> _minionCommands;
        private MinionViewModel _noteMinion;
        public MinionViewModel NoteMinion { get { return _noteMinion ?? (_noteMinion = new MinionViewModel(_minionCommands)); } set { _noteMinion = value; RaisePropertyChanged(); } }


        public NoteViewModel(QuickItem _tree, ObservableCollection<MinionCommandItem> commands, int new_index)
        {
            Text = Properties.Settings.Default.Default_Note_Template;
            _minionCommands = commands;
            Title = string.Format("Note {0}", ++_number);
            _titlechanged = false;
            _root = _tree;
            SaveIndex = new_index;


            this.TextChanged += Note_TextChanged;
            NoteMinion.MinionCollection.CollectionChanged += MinionCollection_CollectionChanged;
        }

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

        void minionItem_NoteWrite(object sender, MinionArgs e)
        {
            Text += Environment.NewLine + e.Message;
        }
        

        //Test Notify Event. Must be changed to only process a textbox changed event (which needs created).
        void Note_TextChanged(object sender, EventArgs e)
        {
           Regex ip = new Regex(@"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b\s+");
           Regex sepid = new Regex(@"\b[a-z]{2,3}[0-9]{5,6}\b\s+", RegexOptions.IgnoreCase);

           
           MatchCollection mc = ip.Matches(Text);

           foreach (Match m in mc)
           {

           }

           if (_titlechanged == false) //Changes Title to first SEP entered then stops.
           {
               MatchCollection sepmatches = sepid.Matches(Text);
               if (sepmatches.Count > 0)
               {
                   Title = sepmatches[0].ToString();
                   _titlechanged = true;
               }

           }
        }

        #region Items
        private RelayCommand _appendQuickItem;
        public RelayCommand AppendQuickItemCommand { get { return _appendQuickItem ?? (_appendQuickItem = new RelayCommand(AppendQuickItem)); } }
        private RelayCommand _copyQuickItemCommand;
        public RelayCommand CopyQuickItemCommand { get { return _copyQuickItemCommand ?? (_copyQuickItemCommand = new RelayCommand(CopyQuickItem)); } }

        public void AppendQuickItem()
        {
            if (Properties.Settings.Default.DashinNotes == true)
            {
                if (SelectedQuickItem.SubItems.Count > 0)
                    return;
                else
                    Text += "- " + SelectedQuickItem.Content + System.Environment.NewLine;
                    //SaveNotes();
            }
            else if (Properties.Settings.Default.DashinNotes == false)
            {
                try
                {
                if (SelectedQuickItem.SubItems.Count > 0)
                    return;
                else
                    Text += SelectedQuickItem.Content + System.Environment.NewLine;
                        //SaveNotes();
                }
                catch (Exception e)
                {
                    //log.Error(e);
                    MetroMessageBox.Show("NOPE!", e.ToString());

            }
        }
        }



        public void CopyQuickItem()
        {
            if (_selectedQuickItem != null)
            {
                if (SelectedQuickItem.SubItems.Count > 0)
                    return;
                else
                {
                    try
                    {
                        Clipboard.SetText(SelectedQuickItem.Content);
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                        CopyQuickItem();
                    }

                }
                
            
            }
        } 
        #endregion
    }
}

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
using System.Windows.Input;

namespace Scrivener.ViewModel
{
    public class NoteViewModel : ViewModelBase, INote
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
      
        public NoteViewModel(INote incomingNote = null)
        {
            //Creates a shared version of the menus
            DataB = DatabaseStorage.Instance;
            //This creates a per instance version of the menus. 
            Root = DataB.QuickItems;
            //Used for saving unique notes
            
            if (incomingNote == null)
            {
                Guid = Guid.NewGuid();
                Text = Properties.Settings.Default.Default_Note_Template;
                Title = string.Format("Note {0}", ++_number);
                _titlechanged = false;
                LastUpdated = DateTime.Now;
            }
            else
            {
                Guid = incomingNote.Guid;
                Text = incomingNote.Text;
                Title = incomingNote.Title;
                _titlechanged = true;
                LastUpdated = incomingNote.LastUpdated;
            }
                        
            this.TextChanged += Note_TextChanged;
            NoteMinion.MinionCollection.CollectionChanged += MinionCollection_CollectionChanged;

        }

        private string _minionVisibility;
        public string MinionVisibility { get { return _minionVisibility; } set { _minionVisibility = value; RaisePropertyChanged(); } }

        #region Public Properties
        public DatabaseStorage DataB { get; set; }

        private Guid _guid; // used for ID for note saving
        public Guid Guid { get { return _guid; } protected set { _guid = value; RaisePropertyChanged(); } }
        

        private static int _number = 0; // used to nuber default notes
        private bool _titlechanged = false; // defines if note title has already been changed
        private string title;
        public string Title { get { return title; } set { title = value; RaisePropertyChanged(); _titlechanged = true; } }

        //private ICSharpCode.AvalonEdit.Document.TextDocument document;
        //public ICSharpCode.AvalonEdit.Document.TextDocument Document { get { return document; } set { document = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private string text;
        public string Text { get { return text; } set { text = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int caretPosition;
        public int CaretPoisition { get { return caretPosition; } set { caretPosition = value; RaisePropertyChanged(); } }
        private DateTime _lastUpdated;
        public DateTime LastUpdated { get { return _lastUpdated; } protected set { _lastUpdated = value; RaisePropertyChanged(); } }

        #endregion        
        
        #region EventBased Actions
        //Text change events for note
        public void RaiseNoteSave()
        {
            if (SaveNoteRequest != null) { SaveNoteRequest(this, new EventArgs()); }
        }
        public event EventHandler SaveNoteRequest;

        internal void RaiseTextChanged()
        {
            if (TextChanged != null) { TextChanged(this, new EventArgs()); }
        }
        public event EventHandler TextChanged;
        //listener runs regex checks on text change
        private DateTime lastsaved = DateTime.Now;
        void Note_TextChanged(object sender, EventArgs e)
        {
            LastUpdated = DateTime.Now;
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
            
            //Saves note on change if one secend has passed since the last save to prevent DB locks.
            var oneSec = new TimeSpan(0,0,0,1);
            if (DateTime.Now > lastsaved + oneSec)
            {
                lastsaved = DateTime.Now;
                RaiseNoteSave();
                log.Trace("Saved note");
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
        //private ObservableCollection<MinionCommandItem> _minionCommands;
        // local minion instance for this note.
        private MinionViewModel _noteMinion;
        public MinionViewModel NoteMinion { get { return _noteMinion ?? (_noteMinion = new MinionViewModel()); } set { _noteMinion = value; RaisePropertyChanged(); } }
        //Register and Unregister Minion instances to allow writing to note
        void MinionCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (MinionItemViewModel minionItem in e.NewItems)
                {
                    minionItem.NoteWrite += minionItem_NoteWrite;
                    minionItem.PasteRequest += (s, p) => Text += p.PasteData.PadLeft(p.PasteData.Length + 1).PadRight(p.PasteData.Length + 1);
                }

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (MinionItemViewModel minionItem in e.OldItems)
                {
                    minionItem.NoteWrite -= minionItem_NoteWrite;
                    minionItem.PasteRequest -= (s, p) => Text += string.Format(" {0} ", p.PasteData);
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
        public void AppendQuickItem(QuickItem qi)
        {
            if (qi != null)
            {
                try
                {
                    if (qi.SubItems.Count == 0) // causes crash if null
                    {
                        //Due to Issues where the updating of a textbox or richtextbox via a binding would cause
                        //the cursor position to reset we were forced to rely on the messager service here to 
                        //access the append and inset methods
                        //GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<string>(qi.Content, "ProcessQI");

                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                        {
                            var temp = CaretPoisition;
                            if(Text[CaretPoisition - 1] == ' ')
                            {
                                Text = Text.Insert(CaretPoisition, qi.Content);
                                
                            }
                            else
                            {
                                Text = Text.Insert(CaretPoisition, " " + qi.Content);
                                temp++;
                            }
                            CaretPoisition = temp + qi.Content.Length;
                        }
                        else
                        {
                            if (Properties.Settings.Default.DashinNotes)
                            {
                                Text += Environment.NewLine + "- " + qi.Content;
                            }
                            else
                            {
                                Text += Environment.NewLine + qi.Content;
                            }
                            CaretPoisition = Text.Length;                           
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

        private RelayCommand _copyQuickItemCommand;
        public RelayCommand CopyQuickItemCommand { get { return _copyQuickItemCommand ?? (_copyQuickItemCommand = new RelayCommand(CopyQuickItem)); } }
        public void CopyQuickItem()
        {
            //if (_selectedQuickItem != null)
            //{
            //    if (SelectedQuickItem.SubItems.Count > 0)
            //        return;
            //    else
            //    {
            //        try
            //        {
            //            Clipboard.SetText(SelectedQuickItem.Content);
            //        }
            //        catch (Exception e)
            //        {
            //            log.Error(e);
            //        }

            //    }
            //}
        } 
        #endregion
    }
}

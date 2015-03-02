using GalaSoft.MvvmLight.Command;
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
                Communication = 0;
                Speed = 0;
                Constitution = 0;
                Cunning = 0;
                Dexterity = 0;
                Defense = 0;
                Magic = 0;
                Perception = 0;
                Armor = 0;
                Strength = 0;
                Willpower = 0;
                Title = string.Format("New Character");
                
                LastUpdated = DateTime.Now;
            }
            else
            {
                //Guid = incomingNote.Guid;
                //Text = incomingNote.Text;
                //Title = incomingNote.Title;
                ////_titlechanged = true;
                //LastUpdated = incomingNote.LastUpdated;
            }
                        
            this.TextChanged += Note_TextChanged;
            

        }

        private string _minionVisibility;
        public string MinionVisibility { get { return _minionVisibility; } set { _minionVisibility = value; RaisePropertyChanged(); } }

        #region Public Properties
        public DatabaseStorage DataB { get; set; }

        private Guid _guid; // used for ID for note saving
        public Guid Guid { get { return _guid; } protected set { _guid = value; RaisePropertyChanged(); } }
        

        //private static int _number = 0; // used to nuber default notes
        //private bool _titlechanged = false; // defines if note title has already been changed
        private string title;
        public string Title { get { return title; } set { title = value; RaisePropertyChanged(); } }

        private string text;
        public string Text { get { return text; } set { text = value; RaisePropertyChanged(); } }
        //private ICSharpCode.AvalonEdit.Document.TextDocument document;
        //public ICSharpCode.AvalonEdit.Document.TextDocument Document { get { return document; } set { document = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int communication;
        public int Communication { get { return communication; } set { communication = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int speed;
        public int Speed { get { return speed; } set { speed = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int constitution;
        public int Constitution { get { return constitution; } set { constitution = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int cunning;
        public int Cunning { get { return cunning; } set { cunning = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private int dexterity;
        public int Dexterity { get { return dexterity; } set { dexterity = value; RaisePropertyChanged(); } }
        private int defense;
        public int Defense { get { return defense; } set { defense = value; RaisePropertyChanged(); } }
        private int magic;
        public int Magic { get { return magic; } set { magic = value; RaisePropertyChanged(); } }
        private int perception;
        public int Perception { get { return perception; } set { perception = value; RaisePropertyChanged(); } }
        private int armor;
        public int Armor { get { return armor; } set { armor = value; RaisePropertyChanged(); } }
        private int strength;
        public int Strength { get { return strength; } set { strength = value; RaisePropertyChanged(); } }
        private int willpower;
        public int Willpower { get { return willpower; } set { willpower = value; RaisePropertyChanged(); } }
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


            //MatchCollection mc = ip.Matches(Text);

            //foreach (Match m in mc)
            //{

            //}

            if (Title.ToLower() == "New Note".ToLower()) //Changes Title to first SEP entered then stops.
            {
                MatchCollection sepmatches = sepid.Matches(Text);
                if (sepmatches.Count > 0)
                {
                    Title = sepmatches[0].ToString().Trim();
                    //_titlechanged = true;
                }

            }
            
            //Saves note on change if one secend has passed since the last save to prevent DB locks.
            var oneSec = new TimeSpan(0,0,0,1);
            if (DateTime.Now > lastsaved + oneSec)
            {
                if (Text != "" && Text != Properties.Settings.Default.Default_Note_Template)
                {
                    lastsaved = DateTime.Now;
                    RaiseNoteSave();
                    log.Trace("Saved note");
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
                        GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<string>(qi.Content, "ProcessQI");
                        //if (text.Length < 2)
                        //{
                        //    if (Properties.Settings.Default.DashinNotes)
                        //    {
                        //        Text += "- " + qi.Content;
                        //    }
                        //    else
                        //    {
                        //        Text += qi.Content;
                        //    }
                        //    return;
                        //}

                        //if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                        //{
                        //    var temp = CaretPoisition;
                        //    if(Text[CaretPoisition - 1] == ' ')
                        //    {
                        //        Text = Text.Insert(CaretPoisition, qi.Content);
                                
                        //    }
                        //    else
                        //    {
                        //        Text = Text.Insert(CaretPoisition, " " + qi.Content);
                        //        temp++;
                        //    }
                        //    CaretPoisition = temp + qi.Content.Length;
                        //}
                        //else
                        //{
                        //    var substring = Text.Substring(Text.Length - 2, 2);
                        //    if (substring == "\r\n" || substring == "- ")
                        //    {
                        //        Text += qi.Content;
                        //    }
                        //    else
                        //    {
                        //        if (Properties.Settings.Default.DashinNotes)
                        //        {

                        //            Text += Environment.NewLine + "- " + qi.Content;
                        //        }
                        //        else
                        //        {

                        //            Text += Environment.NewLine + qi.Content;
                        //        }
                        //    }
                        //    CaretPoisition = Text.Length;                           
                        //}
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

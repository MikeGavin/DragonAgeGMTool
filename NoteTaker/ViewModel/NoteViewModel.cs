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
        private MinionCommands _minionCommands;
        private MinionViewModel _noteMinion;
        public MinionViewModel NoteMinion { get { return _noteMinion ?? (_noteMinion = new MinionViewModel(_minionCommands)); } set { _noteMinion = value; RaisePropertyChanged(); } }


        public NoteViewModel(QuickItem _tree, MinionCommands commands)
        {
            Text = Properties.Settings.Default.Default_Note_Template;
            _minionCommands = commands;
            Title = string.Format("Note {0}", ++_number);
            _titlechanged = false;
            _root = _tree;

            this.TextChanged += Note_TextChanged;
            
        }

        //Test Notify Event. Must be changed to only process a textbox changed event (which needs created).
        void Note_TextChanged(object sender, EventArgs e)
        {
           Regex ip = new Regex(@"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b");
           Regex sepid = new Regex(@"\b[a-z]{2,3}[0-9]{6}\b", RegexOptions.IgnoreCase);

           MatchCollection mc = ip.Matches(Text);

           foreach (Match m in mc)
           {
               MessageBox.Show(m.ToString());
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
            if (SelectedQuickItem.SubItems.Count > 0)
                return;
            else
                Text += System.Environment.NewLine + SelectedQuickItem.Content;
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
                    catch(Exception e)
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

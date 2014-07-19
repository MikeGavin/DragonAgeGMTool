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
using NoteTaker.Model;

namespace NoteTaker.ViewModel
{
    public class NoteViewModel : ViewModelBase
    {
        #region Events
        internal void RaiseTextChanged()
        {
            if (TextChanged != null) { TextChanged(this, new EventArgs()); }
        }
        public event EventHandler TextChanged;
        #endregion

        #region Public Properties
        private string title;
        public string Title { get { return title; } set { title = value; RaisePropertyChanged(); _titlechanged = true; } }
        private string text;
        public string Text { get { return text; } set { text = value; RaisePropertyChanged(); RaiseTextChanged(); } }        
        #endregion

        private static int _number = 0;
        private bool _titlechanged = false;

        private QuickItem _root;
        public QuickItem Root { get { return _root; } set { _root = value; RaisePropertyChanged(); } }
        private QuickItem _selectedQuickItem;
        public QuickItem SelectedQuickItem { get { return _selectedQuickItem; } set { _selectedQuickItem = value; RaisePropertyChanged(); } }
        private RelayCommand _appendQuickItem;
        public RelayCommand AppendQuickItemCommand { get { return _appendQuickItem ?? (_appendQuickItem = new RelayCommand(CopyQuickItem)); } }
        private RelayCommand _copyQuickItemCommand;
        public RelayCommand CopyQuickItemCommand { get { return _copyQuickItemCommand ?? (_copyQuickItemCommand = new RelayCommand(CopyQuickItem)); } }

        private MinionViewModel _minion;
        public MinionViewModel Minion { get { return _minion; } set { _minion = value; RaisePropertyChanged(); } }

        public NoteViewModel()
        {
            TextChanged += Note_TextChanged;
            Text = "Test";

            //Populate Tree!
            Title = string.Format("Note {0}", ++_number);
            _titlechanged = false;
            var temp = new Treefiller();
            _root = temp.filltree();
            
        }

        //Test Notify Event. Must be changed to only process a textbox changed event (which needs created).
        void Note_TextChanged(object sender, EventArgs e)
        {
           Regex ip = new Regex(@"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b");
           Regex sepid = new Regex(@"\b[a-z]{3}[0-9]{6}\b", RegexOptions.IgnoreCase);

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

        public void AppendQuickItem()
        {
            //if (SelectedQuickItem.Content != string.Empty && SelectedQuickItem.Content != null)
            if (SelectedQuickItem.SubItems.Count > 0)
                return;
            else
                Text += System.Environment.NewLine + SelectedQuickItem.Content;
        }

        public void CopyQuickItem()
        {
            //if (SelectedQuickItem.Content != string.Empty && SelectedQuickItem.Content != null)
            if (_selectedQuickItem != null)
            {
                if (SelectedQuickItem.SubItems.Count > 0)
                    return;
                else
                    Clipboard.SetText(SelectedQuickItem.Content);
            }
        }
    }
}

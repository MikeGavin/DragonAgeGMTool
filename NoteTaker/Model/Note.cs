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

namespace NoteTaker.Model
{
    public class Note : INotifyPropertyChanged
    {
        #region Events
        internal void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }

        }
        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaiseTextChanged()
        {
            if (TextChanged != null) { TextChanged(this, new EventArgs()); }
        }
        public event EventHandler TextChanged;
        #endregion

        #region Public Properties
        private string title;
        public string Title { get { return title; } set { title = value; RaisePropertyChanged(); titlechanged = true; } }
        private string text;
        public string Text { get { return text; } set { text = value; RaisePropertyChanged(); RaiseTextChanged(); } }
        private bool isExpanded;
        public bool IsExpanded { get { return isExpanded; } set { isExpanded = value; RaisePropertyChanged(); } }
        
        #endregion

        private static int number = 0;

        private bool titlechanged = false;

        private QuickItem root;
        public QuickItem Root { get { return root; } set { root = value; RaisePropertyChanged(); } }
        private QuickItem selectedQuickItem;
        public QuickItem SelectedQuickItem { get { return selectedQuickItem; } set { selectedQuickItem = value; RaisePropertyChanged(); } }
        public RelayCommand AppendQuickItemCommand { get; set; }
        private RelayCommand _CopyQuickItemCommand;
        public RelayCommand CopyQuickItemCommand { get { return _CopyQuickItemCommand ?? (_CopyQuickItemCommand = new RelayCommand(CopyQuickItem)); } }

        private ObservableCollection<EcotPC> _MinionCollection = new ObservableCollection<EcotPC>();
        public ObservableCollection<EcotPC> MinionCollection { get { return _MinionCollection; } set { _MinionCollection = value; RaisePropertyChanged(); } }

        private EcotPC _SelectedMinion;
        public EcotPC SelectedMinion { get { return _SelectedMinion; } set { _SelectedMinion = value; RaisePropertyChanged(); } }

        private string _NewMinionIPAddress = "10.39.";
        public string NewMinionIPAddress { get { return _NewMinionIPAddress; } set { _NewMinionIPAddress = value; RaisePropertyChanged(); } }
        private bool minionIPInputEnabeled = false;
        public bool MinionIPInputEnabeled { get { return minionIPInputEnabeled; } set { minionIPInputEnabeled = value; RaisePropertyChanged(); } }
        private bool minionConnecting = false;
        public bool MinionConnecting { get { return minionConnecting; } set { minionConnecting = value; RaisePropertyChanged(); } }

        public RelayCommand Minion_CloseCommand { get; set; }
        public RelayCommand Minion_ConnectCommand { get; set; }
        public RelayCommand Minion_GetIECommand { get; set; }
        public RelayCommand Minion_GetJavaCommand { get; set; }
        public RelayCommand Minion_GetFlashCommand { get; set; }
        public RelayCommand Minion_GetShockwaveCommand { get; set; }
        public RelayCommand Minion_GetReaderCommand { get; set; }
        public RelayCommand Minion_GetQuicktimeCommand { get; set; }

        public Note()
        {
            TextChanged += Note_TextChanged;
            AppendQuickItemCommand = new RelayCommand(AppendQuickItem);
            Text = "Test";
            MinionIPInputEnabeled = true;
            Minion_CloseCommand = new RelayCommand(CloseMinion);
            Minion_ConnectCommand = new RelayCommand(ConnectMinion);
            Minion_GetIECommand = new RelayCommand(GetIE);
            Minion_GetJavaCommand = new RelayCommand(GetJava);
            Minion_GetFlashCommand = new RelayCommand(GetFlash);
            Minion_GetShockwaveCommand = new RelayCommand(GetShockwave);
            Minion_GetReaderCommand = new RelayCommand(GetReader);
            Minion_GetQuicktimeCommand = new RelayCommand(GetQuicktime);
            //Populate Tree!

            Title = string.Format("Note {0}", ++number);
            titlechanged = false;
            var temp = new Treefiller();
            root = temp.filltree();           
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

           if (titlechanged == false) //Changes Title to first SEP entered then stops.
           {
               MatchCollection sepmatches = sepid.Matches(Text);
               if (sepmatches.Count > 0)
               {
                   Title = sepmatches[0].ToString();
                   titlechanged = true;
               }

           }


        }

        public void CloseMinion()
        {
            MinionCollection.Remove(SelectedMinion);
            if (MinionCollection.Count <= 0)
                IsExpanded = false;
        }

        public async void ConnectMinion()
        {
            MinionConnecting = true;
            if (NewMinionIPAddress == null) { return; }
            MinionIPInputEnabeled = false;
            if (Minion.Tool.IP.IPv4_Check(NewMinionIPAddress) == true)
            {

                if (await System.Threading.Tasks.Task.Run(() => Minion.Tool.IP.Ping(IPAddress.Parse(NewMinionIPAddress)) == true))
                {
                    MinionCollection.Add(new EcotPC(IPAddress.Parse(NewMinionIPAddress)));
                    SelectedMinion = MinionCollection.Last();
                    NewMinionIPAddress = "10.39.";
                }
                else
                {
                    Helpers.MetroMessageBox.Show("Error!", "IP is unreachable!");
                }
            }
            else
            {
                Helpers.MetroMessageBox.Show("Error!", "Invalid IP address format!");
            }
            MinionIPInputEnabeled = true;
            MinionConnecting = false;
        }

        public async void GetJava()
        {
            await SelectedMinion.Get_Java();
        }

        public async void GetIE()
        {
            await SelectedMinion.Get_IE();
        }

        public async void GetFlash()
        {
            await SelectedMinion.Get_Flash();
        }

        public async void GetShockwave()
        {
            await SelectedMinion.Get_Shockwave();
        }

        public async void GetReader()
        {
            await SelectedMinion.Get_Reader();
        }

        public async void GetQuicktime()
        {
            await SelectedMinion.Get_Quicktime();
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
            if (selectedQuickItem != null)
            {
                if (SelectedQuickItem.SubItems.Count > 0)
                    return;
                else
                    Clipboard.SetText(SelectedQuickItem.Content);
            }
        }
    }
}

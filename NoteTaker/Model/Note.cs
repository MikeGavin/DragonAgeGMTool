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
        #endregion

        #region Public Properties
        private string title;
        public string Title { get { return title; } set { title = value; RaisePropertyChanged(); } }
        private string text;
        public string Text { get { return text; } set { text = value; RaisePropertyChanged(); } }
        #endregion

        private static int number = 0;

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

        public bool MinionIPInputEnabeled { get; set; }

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
            var temp = new Treefiller();
            root = temp.filltree();     

        }

        public void CloseMinion()
        {
            MinionCollection.Remove(SelectedMinion);
        }

        public async void ConnectMinion()
        {
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

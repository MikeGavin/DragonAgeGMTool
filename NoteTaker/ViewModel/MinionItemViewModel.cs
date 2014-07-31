﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Minion;
using System.Collections.Generic;
using NLog.Targets;
using NLog;
using Scrivener.Helpers;
using System.Windows.Data;
using Minion.ListItems;

namespace Scrivener.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MinionItemViewModel : ViewModelBase
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private object _syncLock = new object();
        private ObservableCollection<string> _logCollection;
        public ObservableCollection<string> LogCollection { get { return _logCollection ?? (_logCollection = new ObservableCollection<string>()); } set { _logCollection = value; RaisePropertyChanged(); } }       

        #region Events
        public event EventHandler<Model.MinionArgs> NoteWrite;
        private void RaiseNoteWrite(string message)
        {
            if (NoteWrite != null) { NoteWrite(this, new Model.MinionArgs(message)); }
        }
        public event EventHandler RequestClose;
        void RaiseRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        } 
        #endregion
        
        //Constructor
        public MinionItemViewModel(IPAddress IP, ObservableCollection<MinionCommandItem> commands)
        {

            Machine = new Minion.EcotPC(IP);
            _minionCommands = commands;
            BindingOperations.EnableCollectionSynchronization(LogCollection, _syncLock);
            Machine.EventLogged += Machine_EventLogged;
            // init memory queue
            //_logTarget = new MemoryEventTarget();
            //NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(_logTarget, LogLevel.Debug);

            //foreach (Target target in NLog.LogManager.Configuration.AllTargets)
            //{
            //    if (target is MemoryEventTarget)
            //    {
            //        ((MemoryEventTarget)target).EventReceived += EventReceived;
            //    }
            //}
        }

        void Machine_EventLogged(object sender, Minion.LogEventArgs e)
        {
            int l;
            if (LogCollection.Count >= 50)
                LogCollection.RemoveAt(LogCollection.Count - 1);
            if (e.Log >= Minion.log.Info)
                LogCollection.Add(string.Format("{0} | {1} | {2}", e.Time.ToLongTimeString(), e.Log.ToString().ToUpper(), e.Message.ToString()));
        }


   
        public string Title { get { return Machine.IPAddress.ToString(); } }
        
        public Minion.EcotPC Machine { get; protected set; }

        private ObservableCollection<MinionCommandItem> _minionCommands;
        private ObservableCollection<MinionCommandItem> _minionStartCommands;
        public ObservableCollection<MinionCommandItem> MinionStartCommands { get { return _minionStartCommands ?? (_minionStartCommands = new ObservableCollection<MinionCommandItem>( (from item in _minionCommands where item.Action == "Start" select item).ToList())); } }

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand { get { return _closeCommand ?? (_closeCommand = new RelayCommand(RaiseRequestClose)); } }
        
        #region GetVersionCommands

        private RelayCommand _getIECommand;
        public RelayCommand GetIECommand { get { return _getIECommand ?? (_getIECommand = new RelayCommand(async () => await Machine.Get_IE())); } }

        private RelayCommand _getJavaCommand;
        public RelayCommand GetJavaCommand { get { return _getJavaCommand ?? (_getJavaCommand = new RelayCommand(async () => await Machine.Get_Java())); } }

        private RelayCommand _getFlashCommand;
        public RelayCommand GetFlashCommand { get { return _getFlashCommand ?? (_getFlashCommand = new RelayCommand(async () => await Machine.Get_Flash())); } }

        private RelayCommand _getShockwaveCommand;
        public RelayCommand GetShockwaveCommand { get { return _getShockwaveCommand ?? (_getShockwaveCommand = new RelayCommand(async () => await Machine.Get_Shockwave())); } }

        private RelayCommand _getReaderCommand;
        public RelayCommand GetReaderCommand { get { return _getReaderCommand ?? (_getReaderCommand = new RelayCommand(async () => await Machine.Get_Reader())); } }

        private RelayCommand _getQuicktimeCommand;
        public RelayCommand GetQuicktimeCommand { get { return _getQuicktimeCommand ?? (_getQuicktimeCommand = new RelayCommand(async () => await Machine.Get_Quicktime())); } }

        #endregion

        #region ToolCommands

        private RelayCommand _openDamewareCommand;
        public RelayCommand OpenDamewareCommand { get { return _openDamewareCommand ?? (_openDamewareCommand = new RelayCommand(async () => await Machine.OpenDameware())); } }

        private RelayCommand _openCShareCommand;
        public RelayCommand OpenCShareCommand { get { return _openCShareCommand ?? (_openCShareCommand = new RelayCommand(async () => await Machine.OpenCShare())); } }

        private RelayCommand<MinionCommandItem> _remoteStartCommand;
        public RelayCommand<MinionCommandItem> RemoteStartCommand { get { return _remoteStartCommand ?? (_remoteStartCommand = new RelayCommand<MinionCommandItem>(async (param) => await RemoteStart(param))); } }
        public async Task RemoteStart(MinionCommandItem command)
        {
            await Machine.Command(command);
        }

        private RelayCommand _defaultKillsCommand;
        public RelayCommand DefaultKillsCommand { get { return _defaultKillsCommand ?? (_defaultKillsCommand = new RelayCommand(async () => await Machine.Kill_Defaultss())); } }
             

        #endregion

        private RelayCommand _uninstallJavaCommand;
        public RelayCommand UninstallJavaCommand { get { return _uninstallJavaCommand ?? (_uninstallJavaCommand = new RelayCommand(async () => await Uninstall_Java())); } }
        public async Task Uninstall_Java()
        {
            try
            {
                MinionCommandItem item;
                if (Machine.Java == "NOT INSTALLED" || Machine.Java == "ERROR")
                {
                    item = _minionCommands.First(j => (j.Name == "Java") && (j.Action == "Uninstall") && (j.Version == "All")) as MinionCommandItem;
                }
                else
                {
                    item = _minionCommands.First(j => (j.Name == "Java") && (j.Action == "Uninstall") && (j.Version == Machine.Java)) as MinionCommandItem;
                }
                await RunSoftwareCommand(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                MetroMessageBox.Show("ERROR!", e.ToString());
                return;
            }
        }

        private RelayCommand _installJavaCommand;
        public RelayCommand InstallJavaCommand { get { return _installJavaCommand ?? (_installJavaCommand = new RelayCommand(async () => await Install_Java())); } }
        public async Task Install_Java()
        {
            try 
            { 
                var sort = _minionCommands.OrderByDescending(x => x.Version).ToList();
                var item = sort[9];
                await RunSoftwareCommand(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                MetroMessageBox.Show("ERROR!", e.ToString());
                return;
            }
        }

        private RelayCommand _uninstallFlashCommand;
        public RelayCommand UninstallFlashCommand { get { return _uninstallFlashCommand ?? (_uninstallFlashCommand = new RelayCommand(async () => await Uninstall_Flash())); } }
        public async Task Uninstall_Flash()
        {
            try
            {
                MinionCommandItem item;

                item = _minionCommands.First(f => (f.Name == "Flash") && (f.Action == "Uninstall") && (f.Version == "All")) as MinionCommandItem;
                
                await RunSoftwareCommand(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                MetroMessageBox.Show("ERROR!", e.ToString());
                return;
            }

            
        }

        private RelayCommand _installFlashCommand;
        public RelayCommand InstallFlashCommand { get { return _installFlashCommand ?? (_installFlashCommand = new RelayCommand(async () => await Install_Flash())); } }
        public async Task Install_Flash()
        {
            try
            {
                MinionCommandItem item;
                item = _minionCommands.First(f => (f.Name == "Flash") && (f.Action == "Install") && (f.Version != "All")) as MinionCommandItem;
                await RunSoftwareCommand(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                MetroMessageBox.Show("ERROR!", e.ToString());
                return;
            }
        }

        private RelayCommand _uninstallShockwaveCommand;
        public RelayCommand UninstallShockwaveCommand { get { return _uninstallShockwaveCommand ?? (_uninstallShockwaveCommand = new RelayCommand(async () => await Uninstall_Shockwave())); } }
        public async Task Uninstall_Shockwave()
        {
            MinionCommandItem item;
            try
            {
                item = _minionCommands.First(f => (f.Name == "Shockwave") && (f.Action == "Uninstall") && (f.Version == "All")) as MinionCommandItem;
            }
            catch (Exception e)
            {
                log.Error(e);
                item = _minionCommands.First(f => (f.Name == "Shockwave") && (f.Action == "Uninstall") && (f.Version == "WMIC")) as MinionCommandItem;
            }

            await RunSoftwareCommand(item);
        }

        private RelayCommand _installShockwaveCommand;
        public RelayCommand InstallShockwaveCommand { get { return _installShockwaveCommand ?? (_installShockwaveCommand = new RelayCommand(async () => await Install_Shockwave())); } }
        public async Task Install_Shockwave()
        {
            try
            {
                MinionCommandItem item;
                item = _minionCommands.First(f => (f.Name == "Shockwave") && (f.Action == "Install")) as MinionCommandItem;
                await RunSoftwareCommand(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                MetroMessageBox.Show("ERROR!", e.ToString());
            }
        }

        private RelayCommand _uninstallReaderCommand;
        public RelayCommand UninstallReaderCommand { get { return _uninstallReaderCommand ?? (_uninstallReaderCommand = new RelayCommand(async () => await Uninstall_Reader())); } }
        public async Task Uninstall_Reader()
        {
            MinionCommandItem item;
            try
            {
                item = _minionCommands.First(f => (f.Name == "Reader") && (f.Action == "Uninstall") && (f.Version == "All")) as MinionCommandItem;
            }
            catch (Exception e)
            {
                log.Error(e);
                item = _minionCommands.First(f => (f.Name == "Reader") && (f.Action == "Uninstall") && (f.Version == "WMIC")) as MinionCommandItem;
            }

            await RunSoftwareCommand(item);
        }

        private RelayCommand _installReaderCommand;
        public RelayCommand InstallReaderCommand { get { return _installReaderCommand ?? (_installReaderCommand = new RelayCommand(async () => await Install_Reader())); } }
        public async Task Install_Reader()
        {
            try
            {
                MinionCommandItem item;
                item = _minionCommands.First(f => (f.Name == "Reader") && (f.Action == "Install")) as MinionCommandItem;
                await RunSoftwareCommand(item);
            }
            catch (Exception e)
            {
                log.Error(e);
                MetroMessageBox.Show("ERROR!", e.ToString());
            }
        }

        private async Task RunSoftwareCommand(MinionCommandItem command)
        {
            
            await Machine.Kill_Defaultss();
            var result = await Machine.Command(command);
            string vresult = await UpdateItemVersion(command);

            if (command.Action == "Uninstall")
            {
                if (vresult == "NOT INSTALLED")
                    RaiseNoteWrite(string.Format("Ran Minion {0} uninstall and {0} is now no longer reported as installed.", command.Name));
                else if (vresult == "ERROR")
                    RaiseNoteWrite(string.Format("Ran Minion {0} uninstall but lookup of current {0} verson returned an error.", command.Name));
                else
                    RaiseNoteWrite(string.Format("Ran Minion {0} uninstall but was unable to verify uninstall.", command.Name));
            }
            else if (command.Action == "Install")
            {
                if (vresult == "NOT INSTALLED")
                    RaiseNoteWrite(string.Format("Ran Minion {0} install of version {1} but the install failed.", command.Name, command.Version));
                else if (vresult == "ERROR")
                    RaiseNoteWrite(string.Format("Ran Minion {0} install but version lookup returned an error.", command.Name));
                else if (vresult == command.Version)
                    RaiseNoteWrite(string.Format("Ran Minion {0} install and {0} version {1} is now installed.", command.Name, vresult));
                else
                    RaiseNoteWrite(string.Format("Ran Minion {0} install but Minion was unable to verify install.", command.Name));
            }

        }

        private async Task<string> UpdateItemVersion(MinionCommandItem item)
        {
            string result = string.Empty;
            if (item.Name.ToLower().Contains("java"))
                result = await Machine.Get_Java();
            else if (item.Name.ToLower().Contains("flash"))
                result = await Machine.Get_Flash();
            else if (item.Name.ToLower().Contains("shockwave"))
                result = await Machine.Get_Shockwave();
            else if (item.Name.ToLower().Contains("reader"))
                result = await Machine.Get_Reader();
            else if (item.Name.ToLower().Contains("quicktime"))
                result = await Machine.Get_Quicktime();
            return result;
        }
        
    }
}
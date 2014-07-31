using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Minion.Process
{
    public class SilentProcess : INotifyPropertyChanged  //Class is used to send necessary settings to SilentProcess method and create obserable collections
    {
        #region Property Changed

        protected void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }

        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private string _Version;
        public string Version { get { return _Version; } set { _Version = value; RaisePropertyChanged("Version"); } }

        //private bool _RunKills = true;
        protected bool Copied { get; set; }
        //public bool IsSelected { get; set; }
        //public bool RunKills { get { return _RunKills; } set { _RunKills = value; } }
        public string ProcessLog { get; set; }
        protected internal Tools _tools;

        protected List<string> Kills = new List<string>();

        protected internal void DefaultKills()
        {
            Kills.Add("msiexec.exe");
            Kills.Add("iexplore.exe");
        } 

        public SilentProcess(string ip)
        {
            _tools = new Tools(ip);
            DefaultKills();
        }


        protected internal bool SilentPA(string command, bool kills) // This is the main process used for silent installs
        {
            if (kills == true)
            foreach (string process in Kills)
                _tools.PSKill(process);
            log.Info("Sending commands to remote using PAExec...");
            bool test = _tools.PAExec(command);
            return test;
        }

        protected internal bool SilentPA(string command, string copyfrom, string copyto) // This is the main process used for silent installs
        {
            foreach (string process in Kills)
                _tools.PSKill(process);
            log.Info("Copying files if necessary");
            _tools.Copy(copyfrom, copyto);
            log.Info("Sending commands to remote using PAExec...");
            return _tools.PAExec(command);
        }

        protected internal bool SilentPS(string command, string copyfrom, string copyto) // This is the main process used for silent installs
        {
            foreach (string process in Kills)
                _tools.PSKill(process);
            log.Info("Copying files if necessary");
            _tools.Copy(copyfrom, copyto);
            log.Info("Sending commands to remote using PSExec...");
            return _tools.PSExec(command);
        }

        protected internal bool SilentPS(string command, bool kills) // This is the main process used for silent installs
        {
            if (kills == true)
                foreach (string process in Kills)
                    _tools.PSKill(process);
            log.Info("Sending commands to remote using PSExec...");
            bool test = _tools.PSExec(command);
            return test;
        }
    }

    public class SilentPList 
    {
        public string command { get; set; }
        public string copyfrom { get; set; }
        public string copyto { get; set; }
    }
}

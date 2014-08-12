using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

namespace Minion.Tool
{
    /// <summary>
    /// Default process for running programs with no command box while returning redirected standard output and standard error data to properties. 
    /// </summary>
    public class StandardProcess : INotifyPropertyChanged, IStandardProcess //Base class used to create other actions. Presets some always used settings and redirects which are necessary for error and return processing.
    {
        #region Custom Logger
        /// <summary>
        /// This logger addition is used to allow for raising an event which passes the logged message 
        /// so that it can be captured by listeners for screen logging
        /// </summary>
        protected static NLog.Logger nlog = NLog.LogManager.GetCurrentClassLogger();
        protected void Log(Minion.log type, string message, params object[] args)
        {
            int count = 0;
            foreach (object item in args)
            {
                message = message.Replace(@"{" + count.ToString() + "}", item.ToString());
                count++;
            }

            if (type == log.Trace)
            {
                nlog.Trace(message);
            }
            else if (type == log.Debug)
            {
                nlog.Debug(message);
            }
            else if (type == log.Info)
            {
                nlog.Info(message);
            }
            else if (type == log.Warn)
            {
                nlog.Warn(message);
            }
            else if (type == log.Error)
            {
                nlog.Error(message);
            }
            else if (type == log.Fatal)
            {
                nlog.Fatal(message);
            }

            if (EventLogged != null) { EventLogged(this, new Minion.LogEventArgs(DateTime.Now, type, message)); }
        }
        protected void PassEventLogged(object sender, Minion.LogEventArgs e)
        {
            if (EventLogged != null) { EventLogged(this, e); }
        }
        public event EventHandler<Minion.LogEventArgs> EventLogged;
        #endregion
        #region Events

        internal void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }

        }
        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaiseExecuted()
        {
            HasExecuted = true;
            if (Executed != null) { Executed(this, new EventArgs()); }

        }
        public event EventHandler Executed;

        #endregion
       
        protected string _filename;
        protected string _path = Environment.CurrentDirectory + @"\Resources\";
        protected string _arguments;
        protected string _copyto;
        protected string _copyfrom;
        protected bool _forceoverwrite;
        
        #region Public Properties               
        //Public Properties
        protected ProcessStartInfo _startInfo = new ProcessStartInfo //sets defaults for all processes
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            //RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        /// <summary>
        /// Defaults can only be over written by sending a ProcessStartInfo via the constructor
        /// </summary>
        public ProcessStartInfo StartInfo { get { return _startInfo; } }
        protected string _StandardOutput;
        public string StandardOutput { get { return _StandardOutput; } protected set { _StandardOutput = value; RaisePropertyChanged(); }}
        protected string _StandardError;
        public string StandardError { get { return _StandardError; } protected set { _StandardError = value; RaisePropertyChanged(); } }
        protected int _ExitCode;
        public int ExitCode { get { return _ExitCode; } protected set { _ExitCode = value; RaisePropertyChanged(); } }
        protected bool _HasExecuted = false;
        public bool HasExecuted { get { return _HasExecuted; } protected set { _HasExecuted = value; RaisePropertyChanged(); } }
        #endregion

        #region Constructors

        /// <summary>
        /// Run tools installed in resources with optional startprocessinfo to override default
        /// </summary>
        /// <param name="filename">Filename in default resources to execute</param>
        /// <param name="arguments">Arguments for program</param>
        /// <param name="startinfo">Optional to override default ProcessStartInfo</param>
        public StandardProcess(string filename, string arguments, ProcessStartInfo startinfo = null) 
        {
            CheckStartInfo(startinfo);

            _filename = filename;
            _arguments = arguments;
        }

        /// <summary>
        /// Run program from specified path with optional startprocessinfo
        /// </summary>
        /// <param name="path">Path to filename</param>
        /// <param name="filename">Filename to execute</param>
        /// <param name="arguments">Arguments for program</param>
        /// <param name="startinfo">Optional to override default ProcessStartInfo</param>
        public StandardProcess(string path, string filename, string arguments, ProcessStartInfo startinfo = null)
        {
            CheckStartInfo(startinfo);
            _path = path;
            _filename = filename;
            _arguments = arguments;
        }

        /// <summary>
        /// Copy files or folders with optional full overwrite then run a program from default resources. Default will overwrite if copied file is newer. Optional startprocessinfo override.
        /// </summary>
        /// <param name="filename">Filename to execute</param>
        /// <param name="arguments">Arguments for program</param>
        /// <param name="copyfrom">Location to copy from</param>
        /// <param name="copyto">Location to copy to</param>
        /// <param name="forceoverwrite">Option to force overwrite of existing files</param>
        /// <param name="startinfo"Optional to override default ProcessStartInfo></param>
        public StandardProcess(string filename, string arguments, string copyfrom, string copyto, bool forceoverwrite = false, ProcessStartInfo startinfo = null) //constructor
        {
            CheckStartInfo(startinfo);
            _filename = filename;
            _arguments = arguments;
            _copyfrom = copyfrom;
            _copyto = copyto;
            _forceoverwrite = forceoverwrite;
        }
        /// <summary>
        /// Copy files or folders with optional full overwrite then run a program from a specified path. Default will overwrite if copied file is newer
        /// </summary>
        /// <param name="path">path to filename</param>
        /// <param name="filename">Filename to execute</param>
        /// <param name="arguments">Arguments for program</param>
        /// <param name="copyfrom">Location to copy from</param>
        /// <param name="copyto">Location to copy to</param>
        /// <param name="forceoverwrite">Option to force overwrite of existing files</param>
        /// <param name="startinfo"Optional to override default ProcessStartInfo></param>
        public StandardProcess(string path, string filename, string arguments, string copyfrom, string copyto, bool forceoverwrite = false, ProcessStartInfo startinfo = null)
        {
            CheckStartInfo(startinfo);
            _path = path;
            _filename = filename;
            _arguments = arguments;
            _copyfrom = copyfrom;
            _copyto = copyto;
            _forceoverwrite = forceoverwrite;
        }
        /// <summary>
        /// Used by constructor to alter options for how the process starts if a user submits it to the constructor. 
        /// </summary>
        /// <param name="startinfo"></param>
        private void CheckStartInfo(ProcessStartInfo startinfo)
        {
            if (startinfo != null)
                _startInfo = startinfo;
        }
#endregion

        /// <summary>
        /// Runs program and returns output to properties
        /// </summary>
        public async System.Threading.Tasks.Task Run()
        {
            try
            {
                if (_copyfrom != null & _copyto != null) { await Copy(); }

                StartInfo.FileName = (_path + _filename);
                StartInfo.Arguments = _arguments;
                Log(log.Debug, "Filename:{0}", StartInfo.FileName);
                Log(log.Debug, "Arguments:{0}", StartInfo.Arguments);
                var pProcess = System.Diagnostics.Process.Start(StartInfo);
                StandardOutput = await pProcess.StandardOutput.ReadToEndAsync();
                StandardError = await pProcess.StandardError.ReadToEndAsync();
                //pProcess.WaitForExit();

                ExitCode = pProcess.ExitCode;
                Log(log.Debug, "ExitCode: " + ExitCode.ToString());

                ManageOutput();
                Log(log.Debug, "StandardOutput: " + StandardOutput.Replace("\r", " ").Replace("\n", " "));
                Log(log.Debug, "StandardError: " + StandardError.Replace("\r", " ").Replace("\n", " "));

                RaiseExecuted();
            }
            catch (Exception e)
            {
                Log(log.Error, e.ToString());
            }
        }

        protected virtual async System.Threading.Tasks.Task Copy()
        {
            var file = new Tool.Files();
            file.EventLogged += PassEventLogged;
            await System.Threading.Tasks.Task.Run(() => file.Copy(_copyfrom, _copyto, _forceoverwrite));
            file.EventLogged -= PassEventLogged;
        }

        /// <summary>
        /// Manage and log output to properties
        /// </summary>
        protected virtual void ManageOutput()
            {
                StandardOutput.Trim();
                StandardError.Trim();
            }
    }
}

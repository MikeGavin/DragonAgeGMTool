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
        protected static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
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
       
        protected enum Log
        {
            Trace,
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }
        protected void RaiseLogUpdated(Log type, string message)
        {
            if (type == Log.Trace)
            {
                log.Trace(message);
            }
            else if (type == Log.Debug)
            {
                log.Debug(message);
            }
            else if (type == Log.Info)
            {
                log.Info(message);
            }
            else if (type == Log.Warn)
            {
                log.Warn(message);
            }
            else if (type == Log.Error)
            {
                log.Error(message);
            }
            else if (type == Log.Fatal)
            {
                log.Fatal(message);
            }

            if (EventLogged != null) { EventLogged(this, string.Format("{0} |{1}| {2}", DateTime.Now.ToShortTimeString(), type.ToString("F"), message)); }
        }
        public event EventHandler<string> EventLogged;

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
        //public void Run()
        //{
        //    if (_copyfrom != null & _copyto != null) { Copy(); }

        //    StartInfo.FileName = (_path + _filename);
        //    StartInfo.Arguments = _arguments;
        //    log.Debug(string.Format("Filename:{0}", StartInfo.FileName));
        //    log.Debug(string.Format("Arguments:{0}", StartInfo.Arguments));
        //    var pProcess = System.Diagnostics.Process.Start(StartInfo);
        //    StandardOutput = pProcess.StandardOutput.ReadToEnd();
        //    StandardError = pProcess.StandardError.ReadToEnd();
        //    pProcess.WaitForExit();
            

        //    ExitCode = pProcess.ExitCode;
        //    log.Debug("ExitCode: " + ExitCode.ToString());

        //    ManageOutput();

        //    RaiseExecuted();
        //}

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
                log.Debug(string.Format("Filename:{0}", StartInfo.FileName));
                log.Debug(string.Format("Arguments:{0}", StartInfo.Arguments));
                var pProcess = System.Diagnostics.Process.Start(StartInfo);
                StandardOutput = await pProcess.StandardOutput.ReadToEndAsync();
                StandardError = await pProcess.StandardError.ReadToEndAsync();
                //pProcess.WaitForExit();


                ExitCode = pProcess.ExitCode;
                log.Debug("ExitCode: " + ExitCode.ToString());

                ManageOutput();
                log.Debug("StandardOutput: " + StandardOutput);
                log.Debug("StandardError: " + StandardError);

                RaiseExecuted();
            }
            catch (Exception e)
            {
                log.Debug(e);
            }
        }

        protected virtual async System.Threading.Tasks.Task Copy()
        {
            await System.Threading.Tasks.Task.Run(() => Tool.Files.Copy(_copyfrom, _copyto, _forceoverwrite));
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

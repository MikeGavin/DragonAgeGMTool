using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minion.Process
{
    public class ToolBase //Base class used to create other actions. Presets some always used settings and redirects which are necessary for error and return processing.
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        protected string Program { get; set; }
        public string StandardError { get; set; }
        public string StandardOutput { get; set; }
        public int ExitCode { get; set; }

        protected internal void execute(string arguments)
        {
            //System.Windows.Forms.MessageBox.Show(path + exe + ip + arguments, "Sending String");
            //System.Diagnostics.Process pProcess = new System.Diagnostics.Process();

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = (Environment.CurrentDirectory + @"\Resources\" + Program),
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                //RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            log.Debug(string.Format("Executing [ {0} {1} ]", Program, arguments));
            

            var pProcess = System.Diagnostics.Process.Start(startInfo);
            StandardOutput = pProcess.StandardOutput.ReadToEnd();
            StandardError = pProcess.StandardError.ReadToEnd();
            pProcess.WaitForExit();
            ExitCode = pProcess.ExitCode;

            log.Debug("StandardOutput: " + StandardOutput);
            log.Debug("StandardError: " + StandardError);
            log.Debug("ExitCode: " + ExitCode.ToString());

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;



namespace Minion.Tool
{
    /// <summary>
    /// PAExec process with special standard error managed output
    /// </summary>
    public class PAExec : PXBase, IStandardProcess
    {
        public PAExec(System.Net.IPAddress ip, string arguments)
            : base(ip, "paexec.exe", arguments)
        {

        }

        public PAExec(System.Net.IPAddress ip, string arguments, string copyfrom, string copytoremote = @"\Temp\", bool forceoverwrite = false)
            : base(ip, "paexec.exe", arguments, copyfrom, copytoremote, forceoverwrite)
        {

        }

        protected override void ManageOutput()
        {
            //PAExec does not return standard error only Exit Codes. This interpuates the exit code and adds it to the standard error out.
            if (ExitCode < 0)
            {
                var errorData = string.Format("PAEXEC Error {0}: ", ExitCode);
                if (ExitCode == -1) { errorData += "internal error"; }
                if (ExitCode == -2) { errorData += "command line error"; }
                if (ExitCode == -3) { errorData += "failed to launch app (locally)"; }
                if (ExitCode == -4) { errorData += "failed to copy PAExec to remote (connection to ADMIN$ might have failed)"; }
                if (ExitCode == -5) { errorData += "connection to server taking too long (timeout)"; }
                if (ExitCode == -6) { errorData += "PAExec service could not be installed/started on remote server"; }
                if (ExitCode == -7) { errorData += "could not communicate with remote PAExec service"; }
                if (ExitCode == -8) { errorData += "failed to copy app to remote server"; }
                if (ExitCode == -9) { errorData += "failed to launch app (remotely)"; }
                if (ExitCode == -10) { errorData += "app was terminated after timeout expired"; }
                if (ExitCode == -11) { errorData += "forcibly stopped with Ctrl-C / Ctrl-Break"; }

                nlog.Error("[PAExec Arguments] {1}{0}[StandardError]{0}{2}{0}[StandardOutput]{0}{3}{0}[Exit Code] {4}", System.Environment.NewLine, StartInfo.Arguments, StandardError, StandardOutput, errorData);
                Log(log.Error, errorData);
            }
            else if (ExitCode > 0)
            {
                nlog.Error("[PAExec Arguments] {1}{0}[StandardError]{0}{2}{0}[StandardOutput]{0}{3}{0}[Exit Code] {4}", System.Environment.NewLine, StartInfo.Arguments, StandardError, StandardOutput, ExitCode);
                Log(log.Error, StandardError = string.Format("PAExec ran but command returned error {0} -- {1} --", ExitCode, StandardError.Trim()));
            }
            
        }
    }
}
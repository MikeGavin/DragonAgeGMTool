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
                StandardError += string.Format("| PAEXEC Error {0}: ", ExitCode);
                if (ExitCode == -1) { StandardError += "internal error"; }
                if (ExitCode == -2) { StandardError += "command line error"; }
                if (ExitCode == -3) { StandardError += "failed to launch app (locally)"; }
                if (ExitCode == -4) { StandardError += "failed to copy PAExec to remote (connection to ADMIN$ might have failed)"; }
                if (ExitCode == -5) { StandardError += "connection to server taking too long (timeout)"; }
                if (ExitCode == -6) { StandardError += "PAExec service could not be installed/started on remote server"; }
                if (ExitCode == -7) { StandardError += "could not communicate with remote PAExec service"; }
                if (ExitCode == -8) { StandardError += "failed to copy app to remote server"; }
                if (ExitCode == -9) { StandardError += "failed to launch app (remotely)"; }
                if (ExitCode == -10) { StandardError += "app was terminated after timeout expired"; }
                if (ExitCode == -11) { StandardError += "forcibly stopped with Ctrl-C / Ctrl-Break"; }
                log.Error(StandardError);          
            }
            else if (ExitCode > 0)
            {
                log.Error(StandardError = string.Format("Program ran but returned error {0}: {1}", ExitCode, StandardError.Trim()));
            }
            log.Fatal(StandardError += " <--is some crazy shit that happend and I failed to process it.");
        }
    }
}
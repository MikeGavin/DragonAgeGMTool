using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;



namespace Minion.Tool
{
    /// <summary>
    /// PSExec process with special standard error managed output
    /// </summary>
    public class PSExec : PXBase, IStandardProcess
    {
        public PSExec(System.Net.IPAddress ip, string arguments)
            : base(ip, "psexec.exe", arguments)
        {

        }

        public PSExec(System.Net.IPAddress ip, string arguments, string copyfrom, string copytoremote, bool forceoverwrite = false)
            : base(ip, "psexec.exe", arguments, copyfrom, copytoremote, forceoverwrite)
        {

        }

        protected override void ManageOutput()
        {
            StandardError = StandardError.Replace("\r\r", string.Empty).Replace("\r\nPsExec v1.98 - Execute processes remotely\r\nCopyright (C) 2001-2010 Mark Russinovich\r\nSysinternals - www.sysinternals.com\r\n", string.Empty).Trim(null);
        }
    }
}
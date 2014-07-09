using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;



namespace Minion.Tool
{
    public class PSShutdown : PXBase, IStandardProcess
    {
        public PSShutdown(System.Net.IPAddress ip, string arguments)
            : base(ip, "psshutdown.exe", arguments)
        {

        }
    }
}
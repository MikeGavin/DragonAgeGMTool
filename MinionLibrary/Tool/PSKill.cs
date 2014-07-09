using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;



namespace Minion.Tool
{
    public class PSKill : PXBase, IStandardProcess
    {
        public PSKill(System.Net.IPAddress ip, string arguments)
            : base(ip, "pskill.exe", arguments)
        {

        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;



namespace Minion.Tool
{
    /// <summary>
    /// Abstract base for P(X) command line tools. Allows setting defaults for specific programs along with formatting incomming IP address to arguments
    /// </summary>
    public abstract class PXBase : StandardProcess, IStandardProcess
    {

        public PXBase(IPAddress ip, string filename, string arguments)
            : base( filename, FormatArguments(ip, arguments))
        {
            
        }

       public PXBase(IPAddress ip, string filename, string arguments, string copyfrom, string copytoremote, bool forceoverwrite = false)
            : base( filename, FormatArguments(ip, arguments), copyfrom, FormatTo(ip, copytoremote), forceoverwrite)
        {

        }

        protected static string FormatArguments(IPAddress ip, string arguments)
        {
            return string.Format(@"\\{0} {1}", ip.ToString(), arguments);
        }

        protected static string FormatTo(IPAddress ip, string copyto)
        {
            return string.Format(@"\\{0}\c$\{1)", ip.ToString(), copyto);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minion
{
    /// <summary>
    /// Defins the accepted type for command instances of remote machines
    /// </summary>
    public class RemoteCommandImport
    {
        public string Name { get; internal set; }
        public string Version { get; internal set; }
        public string Install_Copy { get; internal set; }
        public string Install_Command { get; internal set; }
        public string Uninstall_Copy { get; internal set; }
        public string Uninstall_Command { get; internal set; }
        public string CopyTo { get; internal set; }
    }

    public class RemoteCommand
    {
        public string Name { get;  set; }
        public string Version { get;  set; }
        public string CopyFrom { get;  set; }
        public string CopyTo { get; set; }
        public string Command { get;  set; }

    }
}

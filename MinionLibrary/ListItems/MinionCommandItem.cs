using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minion.ListItems
{
    public class MinionCommandItem
    {
        public string Name { get; set; }
        public string Action { get; set; }
        public string Version { get; set; }
        public string CopyFrom { get; set; }
        public string CopyTo { get; set; }
        public string Command { get; set; }
        public string Bit { get; set; }
        public string FullName { get { return string.Format("{0} {1} {2} {3}", Action, Name, Version, Bit); } }
    }
}

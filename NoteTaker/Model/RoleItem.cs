using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Scrivener.Model
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class RoleItem
    {       
        public string Name { get; set; }
        public bool Minion { get; set; }
        public string QuickItem_Table { get; set; }
        public string SiteItem_Table { get; set; }
    }
}

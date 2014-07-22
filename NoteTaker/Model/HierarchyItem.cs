using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteTaker.Model
{
    public class HierarchyItem
    {
        public HierarchyItem()
        {
            this.Children = new List<HierarchyItem>();
        }


        public string Title { get; set; }
        public string Verbage { get; set; }
        public bool IsSelected { get; set; }

        public List<HierarchyItem> Children { get; set; }
    }
}

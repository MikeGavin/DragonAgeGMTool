using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public class HistoryItem
    {
        public HistoryItem()
        {
            this.SubItems = new ObservableCollection<HistoryItem>();
        }


        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsSelected { get; set; }

        public ObservableCollection<HistoryItem> SubItems { get; set; }
    }
}

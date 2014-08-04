using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public class Historyitem
    {
        public Historyitem()
        {
            this.SubItems = new ObservableCollection<Historyitem>();
        }


        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsSelected { get; set; }

        public ObservableCollection<Historyitem> SubItems { get; set; }
    }
}
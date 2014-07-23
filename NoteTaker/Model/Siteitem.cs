using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteTaker.Model
{
    public class Siteitem
    {
        public Siteitem()
        {
            this.SubItems = new ObservableCollection<Siteitem>();
        }


        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsSelected { get; set; }

        public ObservableCollection<Siteitem> SubItems { get; set; }
    }
}
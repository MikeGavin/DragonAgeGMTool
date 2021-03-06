﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public class QuickItem
    {
        public QuickItem()
        {
            this.SubItems = new ObservableCollection<QuickItem>();
        }
     
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsSelected { get; set; }

        public ObservableCollection<QuickItem> SubItems { get; set; }
    }
}

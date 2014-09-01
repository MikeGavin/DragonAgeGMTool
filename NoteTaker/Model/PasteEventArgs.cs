using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrivener.Model
{
    public class PasteEventArgs : EventArgs
    {
        public string PasteData { get; set; }
    }
}

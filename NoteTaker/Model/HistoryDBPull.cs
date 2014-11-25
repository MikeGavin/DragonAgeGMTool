using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    class HistoryDBPull
    {        
        public int ID { get; set; }        
        public string Date { get; set; }
        public string Time { get; set; }
        public string Caller { get; set; }
        public string Notes { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minion
{
    public class LogEventArgs : EventArgs
    {   
        public DateTime Time { get; protected set; }
        public log Log { get; protected set; }
        public string Message { get; protected set; }
 
        public LogEventArgs(DateTime time, log logtype, string message)
        {
            Time = time;
            Log = logtype;
            Message = message;
        }
    }
}

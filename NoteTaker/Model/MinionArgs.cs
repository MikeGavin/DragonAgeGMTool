using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{   
    public class MinionArgs : EventArgs
    {
        public string Message { get; protected set; }

        public MinionArgs(string message)
        {
            Message = message;
        }
    }
}

using NLog;
using NLog.Targets;

using NLog.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener
{
    [Target("MemoryEventTarget")] 
    public class MemoryEventTarget : Target
    {
        public event Action<LogEventInfo> EventReceived;

        /// <summary>
        /// Notifies listeners about new event
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (EventReceived != null)
            {
                EventReceived(logEvent);
            }
        }
    }
}

using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Helpers
{
    public class LoggingHelper
    {
        public static Target ReturnTarget(string targetName)
        {
            Target test = null;
            if (LogManager.Configuration != null && LogManager.Configuration.ConfiguredNamedTargets.Count != 0)
            {
                Target target = LogManager.Configuration.FindTargetByName(targetName);
                if (target == null)
                {
                    throw new Exception("Could not find target named: " + targetName);
                }

                WrapperTargetBase wrapperTarget = target as WrapperTargetBase;

                // Unwrap the target if necessary.
                if (wrapperTarget == null)
                {
                    test = target;
                }
                else
                {
                    test = wrapperTarget.WrappedTarget;
                }

                if (target == null)
                {
                    throw new Exception("Could not get a Target from " + target.GetType());
                }

            }
            else
            {
                throw new Exception("LogManager contains no Configuration or there are no named targets");
            }
            return test;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minion.Tool
{
    /// <summary>
    /// Contains common tests for IP addresses
    /// </summary>
    public class IP
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Tests if a string is a 4 part IPV4 address
        /// </summary>
        /// <param name="ip"></param>
        /// <returns>True or False</returns>
        public static bool IPv4_Check(string ip)
        {
            var quads = ip.Split('.');

            // if we do not have 4 quads, return false
            if (!(quads.Length == 4))
            {
                log.Error("Invalid IPV4 Address");
                return false;
            }

            // for each quad
            foreach (var quad in quads)
            {
                int q;
                // if parse fails 
                // or length of parsed int != length of quad string (i.e.; '1' vs '001')
                // or parsed int < 0
                // or parsed int > 255
                // return false
                if (!System.Int32.TryParse(quad, out q)
                    || !q.ToString().Length.Equals(quad.Length)
                    || q < 0
                    || q > 255) { log.Error("Invalid IPV4 Address"); return false; }
            }
            log.Info("Valid IPV4");
            return true;
        }

        /// <summary>
        /// Tests if a IP address is reachable via ping
        /// </summary>
        /// <param name="ip"></param>
        /// <returns>True or False</returns>
        public static bool Ping(System.Net.IPAddress ip)
        {

            System.Net.NetworkInformation.Ping pingClass = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingReply pingReply = pingClass.Send(ip);

            //if ip is valid run checks else
            if (pingReply.Status == System.Net.NetworkInformation.IPStatus.Success)
            { log.Info("Ping Success (" + ip.ToString() +")"); return true; }
            else
            { log.Error("Ping Failed (" + ip.ToString() + ")"); return false; }

        }
    }
}

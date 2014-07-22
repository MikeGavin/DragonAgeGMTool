using System;
namespace Minion
{
    public interface IEcotPC
    {
        string ChipStyle { get; set; }
        System.Threading.Tasks.Task<bool> Command(RemoteCommandImport command);
        string CurrentUser { get; }
        string Flash { get; set; }
        string IEVersion { get; set; }
        System.Net.IPAddress IPAddress { get; }
        string Java { get; set; }
        string PCName { get; }
        string OSBit { get; set; }
        string Quicktime { get; set; }
        string RAM { get; set; }
        string Reader { get; set; }
        string Shockwave { get; set; }
    }
}

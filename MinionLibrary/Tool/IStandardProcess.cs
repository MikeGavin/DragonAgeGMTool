using System;
using System.Threading.Tasks;
namespace Minion.Tool
{
    /// <summary>
    /// Layout for standard Process classes
    /// </summary>
    interface IStandardProcess
    {
        event EventHandler Executed;
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        int ExitCode { get; }
        bool HasExecuted { get; }
        string StandardError { get; }
        string StandardOutput { get; }
        System.Diagnostics.ProcessStartInfo StartInfo { get; }

        Task Run();     
    }
}

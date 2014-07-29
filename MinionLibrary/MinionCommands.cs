using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minion
{
    public class RemoteStartItem //used to create list of remote start items
    {
        public string Name { get; set; }
        public string Command { get; set; }
    }

    public class MinionCommands
    {
        public List<RemoteCommandImport> Java { get; private set; }
        public List<RemoteCommandImport> Flash { get; private set; }
        public List<RemoteCommandImport> Shockwave { get; private set; }
        public List<RemoteCommandImport> Reader { get; private set; }
        public ObservableCollection<RemoteStartItem> RemoteStartCommands { get; private set; }

        private List<RemoteCommandImport> CommandList;
//        public List<RemoteCommandImport> CommandList { get; set; }
        public MinionCommands()
        {

            SQLiteConnection minion = new SQLiteConnection(string.Format(@"Data Source={0}\Resources\Minion.sqlite", Environment.CurrentDirectory));
            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = "SELECT * FROM Software";
            pullall.Connection = minion;
            CommandList = new List<RemoteCommandImport>();
            try
            {
                
                minion.Open();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (reader.Read())
                    CommandList.Add(new RemoteCommandImport() 
                    { 
                        Name = reader["Name"].ToString().Trim(),
                        Version = reader["Version"].ToString().Trim(),
                        Install_Copy = reader["Install_Copy"].ToString().Trim(),
                        Install_Command = reader["Install_Command"].ToString().Trim(),
                        Uninstall_Copy = reader["Uninstall_Copy"].ToString().Trim(),
                        Uninstall_Command = reader["Uninstall_Command"].ToString().Trim(),
                        CopyTo = reader["CopyTo"].ToString().Trim()
                    });
                minion.Close();


                RemoteStartCommands = new ObservableCollection<RemoteStartItem>() 
            { 
                new RemoteStartItem { Name = "Task Manager", Command = @"C:\Windows\system32\taskmgr.exe" },
                new RemoteStartItem { Name = "Command Prompt", Command = @"C:\Windows\system32\cmd.exe" },
                new RemoteStartItem { Name = "Default Programs", Command = @"C:\Windows\System32\control.exe /name Microsoft.DefaultPrograms" },
                new RemoteStartItem { Name = "Devices & Printers", Command = @"C:\Windows\System32\control.exe /name Microsoft.DevicesAndPrinters" },
                new RemoteStartItem { Name = "Sound", Command = @"C:\Windows\System32\control.exe /name Microsoft.Sound" },
                new RemoteStartItem { Name = "Mouse", Command = @"C:\Windows\System32\control.exe /name Microsoft.Mouse" },
                new RemoteStartItem { Name = "Keyboard", Command = @"C:\Windows\System32\control.exe /name Microsoft.Keyboard" },
                new RemoteStartItem { Name = "Power Options", Command = @"C:\Windows\System32\control.exe powercfg.cpl,,1" },
                new RemoteStartItem { Name = "Services", Command = @"C:\Windows\System32\mmc.exe services.msc" },
                new RemoteStartItem { Name = "Device Manager", Command = @"C:\Windows\System32\mmc.exe devmgmt.msc" },
                new RemoteStartItem { Name = "Task Scheduler", Command = @"C:\Windows\System32\mmc.exe taskschd.msc" },
                new RemoteStartItem { Name = "Programs and Features", Command = @"C:\Windows\System32\rundll32.exe shell32.dll,Control_RunDLL appwiz.cpl" },
                new RemoteStartItem { Name = "Add Printer", Command = @"C:\Windows\System32\rundll32.exe SHELL32.DLL,SHHelpShortcuts_RunDLL AddPrinter" },
                new RemoteStartItem { Name = "Personalization", Command = @"C:\Windows\System32\rundll32.exe shell32.dll,Control_RunDLL desk.cpl,,2" },
            };
      
            }
            catch (Exception e)
            {
                
            }
            
            Java = CommandList.Where(n => n.Name.Contains("Java")).ToList();
            Flash = CommandList.Where(n => n.Name.Contains("Flash")).ToList();
            Shockwave = CommandList.Where(n => n.Name.Contains("Shockwave")).ToList();
            Reader = CommandList.Where(n => n.Name.Contains("Reader")).ToList();
        }

    }
}

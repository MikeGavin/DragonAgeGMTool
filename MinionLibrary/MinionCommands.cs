using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minion
{
    public class MinionCommands
    {
        public List<RemoteCommandImport> Java { get; private set; }
        public List<RemoteCommandImport> Flash { get; private set; }
        public List<RemoteCommandImport> Shockwave { get; private set; }
        public List<RemoteCommandImport> Reader { get; private set; }


        private static List<RemoteCommandImport> CommandList { get; set; }
        public MinionCommands()
        {

            SQLiteConnection minion = new SQLiteConnection(string.Format(@"Data Source={0}\Resources\Minion.sqlite", Environment.CurrentDirectory));
            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = "SELECT * FROM Version";
            pullall.Connection = minion;

            try
            {

                minion.Open();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (reader.Read())
                    CommandList.Add(new RemoteCommandImport() 
                    { 
                        Name = reader["Name"].ToString(),
                        Version = reader["Version"].ToString(), 
                        Install_Copy = reader["Install_Copy"].ToString(),
                        Install_Command = reader["Install_Command"].ToString(),
                        Uninstall_Copy = reader["Uninstall_Copy"].ToString(),
                        Uninstall_Command = reader["Uninstall_Command"].ToString()
                    });
                minion.Close();
      
            }
            catch (Exception e)
            {
                
            }
            
            Java = CommandList.Where(n => n.Name == "Java").ToList();
            Flash = CommandList.Where(n => n.Name == "Flash").ToList();
            Shockwave = CommandList.Where(n => n.Name == "Shockwave").ToList();
            Reader = CommandList.Where(n => n.Name == "Reader").ToList();
        }

    }
}

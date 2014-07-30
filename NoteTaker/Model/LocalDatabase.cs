using Minion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public static class LocalDatabase
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public static ObservableCollection<MinionCommandItem> MinionCommands()
        {
            SQLiteConnection minion = new SQLiteConnection(string.Format(@"Data Source={0}\Resources\Scrivener.sqlite", Environment.CurrentDirectory));
            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = "SELECT * FROM Minion_Commands";
            pullall.Connection = minion;
            var commandList = new ObservableCollection<MinionCommandItem>();
            try
            {

                log.Info("Opening {0}", minion.DataSource);
                minion.Open();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (reader.Read())
                    commandList.Add(new MinionCommandItem() 
                    { 
                        Name = reader["Name"].ToString().Trim(),
                        Action = reader["Action"].ToString().Trim(),
                        Version = reader["Version"].ToString().Trim(),
                        CopyFrom = reader["CopyFrom"].ToString().Trim(),
                        CopyTo = reader["CopyTo"].ToString().Replace("c:", string.Empty).Trim(),
                        Command = reader["Command"].ToString().Trim(),
                    });
                log.Info("Closing {0}", minion.DataSource);
                minion.Close();  
     
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            return commandList;

        }
    }
}

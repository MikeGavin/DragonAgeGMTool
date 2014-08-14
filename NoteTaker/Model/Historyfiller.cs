using Scrivener.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public class Historyfiller
    {
        private Historyitem Historyroot;
        public Historyitem Root { get { return Historyroot; } set { Historyroot = value; } }
        private string _table;


        private List<HistoryPull> HistoryCommandList = new List<HistoryPull>()
        {

        };

        public Historyfiller(string table)
        {
            _table = table;
        }

        public Historyitem fillhistory()
        {
            string Date = DateTime.Now.ToString("D").Replace(" ", "").Replace(",", "");
            
            SQLiteConnection CallHistory = new SQLiteConnection(string.Format("Data Source=Call_History.db;Version=3;New=True;Compress=True;"));

            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = string.Format("SELECT * FROM {0}", Date);
            pullall.Connection = CallHistory;

            try
            {
                CallHistory.Open();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (reader.Read())
                    HistoryCommandList.Add(new HistoryPull() { ID = reader["ID"].ToString(), Caller = reader["Caller"].ToString(), Notes = reader["Notes"].ToString() });
                CallHistory.Close();
            }
            catch
            {

            }


            List<HistoryPull> Root_uniqueitems = HistoryCommandList.GroupBy(s => s.ID).Select(p => p.First()).ToList();
            
            Historyroot = new Historyitem() { Title = "Menu" };

            foreach (HistoryPull item in Root_uniqueitems)
            {
                Historyitem Root_Item = new Historyitem() { Title = item.Caller, Content = item.Notes };

                Historyroot.SubItems.Add(Root_Item);

            }
            
            return Historyroot;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteTaker.Model
{
    public class HierarchyBuilder
    {

        public HierarchyBuilder()
        {
            
        }

        private HierarchyItem root;
        public HierarchyItem Root { get { return root; } set { root = value ?? (root = new HierarchyItem()); } }


        public HierarchyItem filltree()
        {
            SQLiteConnection QuickNotesDB = new SQLiteConnection(string.Format(@"Data Source={0}\Resources\QuickNotes.db;Version=3;New=True;Compress=True;", Environment.CurrentDirectory));

            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = "SELECT * FROM Test";
            pullall.Connection = QuickNotesDB;
            List<QuickItemEntry> rawlist = new List<QuickItemEntry>();

            try
            {
                QuickNotesDB.Open();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (reader.Read())
                    rawlist.Add(new QuickItemEntry() { Verbage = reader["Verbage"].ToString(), Title = reader["Item_Title"].ToString(), Parent = reader["Parent"].ToString() });
                QuickNotesDB.Close();
            }
            catch
            {

            }
            var Root = new HierarchyItem();
            while (rawlist.Count > 0)
            {
                var removelist = new List<QuickItemEntry>();
                //Add First Iter items
                foreach (QuickItemEntry item in rawlist)
                {
                    if ((item.Verbage == string.Empty || item.Verbage == null) && (item.Parent == string.Empty || item.Parent == null))
                    {
                        Root.Children.Add(new HierarchyItem() { Title = item.Title });
                        removelist.Add(item);
                    }
                }

                RemoveItems(rawlist, removelist);
                // add 2nd tier
                foreach (QuickItemEntry item in rawlist)
                {
                    var test = Root.Children.Find(p => p.Title == item.Parent);
                    if (test != null)
                    {
                        test.Children.Add(new HierarchyItem { Title = item.Title, Verbage = item.Verbage });
                        removelist.Add(item);
                    }
                    
                }
                RemoveItems(rawlist, removelist);

            }

            return Root;
        }

        private static List<QuickItemEntry> RemoveItems(List<QuickItemEntry> rawlist, List<QuickItemEntry> removelist)
        {
            foreach (QuickItemEntry item in removelist)
            {
                rawlist.Remove(item);
            }
            return rawlist;
        }
    }
}

using Notemaker.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteTaker.Model
{
    public class Treefiller
    {
        private QuickItem root;
        public QuickItem Root { get { return root; } set { root = value; } }

        private List<DBPull> CommandList = new List<DBPull>()
        {

        };

        public Treefiller()
        {

       
        }

        public QuickItem filltree()
        {

            SQLiteConnection QuickNotesDB = new SQLiteConnection(string.Format(@"Data Source={0}\Resources\QuickNotes.db;Version=3;New=True;Compress=True;", Environment.CurrentDirectory));

            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = "SELECT * FROM HD_Calls";
            pullall.Connection = QuickNotesDB;

            try
            {
                QuickNotesDB.Open();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (reader.Read())
                    CommandList.Add(new DBPull() { Verbage = reader["QuickNotes_Verbage"].ToString(), Root_Folder = reader["QuickNotes_Root_Folder"].ToString(), Sub_Folder_1 = reader["QuickNotes_1"].ToString(), Sub_Folder_2 = reader["QuickNotes_2"].ToString(), Sub_Folder_3 = reader["QuickNotes_3"].ToString(), Sub_Folder_4 = reader["QuickNotes_4"].ToString() });
                QuickNotesDB.Close();
            }
            catch
            {

            }


            List<DBPull> Root_uniqueitems = CommandList.GroupBy(s => s.Root_Folder).Select(p => p.First()).ToList();
            List<DBPull> Sub1uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_1).Select(p => p.First()).ToList();
            List<DBPull> Sub2uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_2).Select(p => p.First()).ToList();
            List<DBPull> Sub3uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_3).Select(p => p.First()).ToList();
            List<DBPull> Sub4uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_4).Select(p => p.First()).ToList();

            root = new QuickItem() { Title = "Menu" };

            foreach (DBPull item in Root_uniqueitems)
            {
                QuickItem childItem1 = new QuickItem() { Title = item.Root_Folder };


                foreach (DBPull item1 in CommandList)
                {
                    if (item1.Root_Folder == childItem1.Title && item1.Sub_Folder_2 != string.Empty | item1.Sub_Folder_2 == "NULL")
                    {
                        childItem1.SubItems.Add(new QuickItem() { Title = item1.Sub_Folder_2, Content = item1.Verbage });
                    }
                }

                root.SubItems.Add(childItem1);
            }

            //foreach (DBPull item in Sub1uniqueitems)
            //{
            //    QuickItem childItem2 = new QuickItem() { Title = item.Sub_Folder_1 };

            //    foreach (DBPull item2 in CommandList)
            //    {
            //        if (item2.Sub_Folder_1 == childItem2.Title)
            //        {
            //            childItem2.SubItems.Add(new QuickItem() { Title = item2.Sub_Folder_2, Content = item2.Verbage });
            //        }
            //    }
            //    root.SubItems.Add(childItem2);
            //}
            
            //QuickItem childItem1 = new QuickItem() { Title = "Child item #1" };
            //childItem1.SubItems.Add(new QuickItem() { Title = "Child item #1.1", Content = "Blah blah blah blah"});
            //childItem1.SubItems.Add(new QuickItem() { Title = "Child item #1.2", Content = "Blah blah blah blah" });
            //root.SubItems.Add(childItem1);
            //root.SubItems.Add(new QuickItem() { Title = "Child item #2", Content = "Blah blah blah blah"});
            return root;
        }
    }
}

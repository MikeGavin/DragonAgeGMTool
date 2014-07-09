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

            SQLiteConnection QuickNotesDB = new SQLiteConnection("Data Source=" + @"C:\Users\michael.gavin\Documents\Visual Studio 2013\Projects\SQL Lite Test\SQL Lite Test\bin\Debug\database.db ;Version=3;New=True;Compress=True;");

            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = "SELECT * FROM QuickNotes";
            pullall.Connection = QuickNotesDB;

            try
            {
                QuickNotesDB.Open();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (reader.Read())
                    CommandList.Add(new DBPull() { Step = reader["Step"].ToString(), SubStep = reader["SubStep"].ToString(), Verbage = reader["Verbage"].ToString() });
                QuickNotesDB.Close();
            }
            catch
            {

            }


            List<DBPull> uniqueitems = CommandList.GroupBy(s => s.Step).Select(p => p.First()).ToList();

            //List<DBPull> uniqueitems2 = CommandList.GroupBy(s => s.SubStep).Select(p => p.First()).ToList() ;

            root = new QuickItem() { Title = "Menu" };

            foreach (DBPull item in uniqueitems)
            {
                QuickItem childItem1 = new QuickItem() { Title = item.Step, Content = item.SubStep};

                foreach (DBPull item2 in CommandList)
                 {
                    if (item2.Step == childItem1.Title && item2.SubStep != string.Empty)
                     childItem1.SubItems.Add(new QuickItem() { Title = item2.SubStep, Content = item2.Verbage});
                 }
                root.SubItems.Add(childItem1);
            }
            
            //QuickItem childItem1 = new QuickItem() { Title = "Child item #1" };
            //childItem1.SubItems.Add(new QuickItem() { Title = "Child item #1.1", Content = "Blah blah blah blah"});
            //childItem1.SubItems.Add(new QuickItem() { Title = "Child item #1.2", Content = "Blah blah blah blah" });
            //root.SubItems.Add(childItem1);
            //root.SubItems.Add(new QuickItem() { Title = "Child item #2", Content = "Blah blah blah blah"});
            return root;
        }
    }
}

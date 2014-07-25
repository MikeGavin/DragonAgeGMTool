using Scrivener.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public class Sitefiller
    {
        private Siteitem siteroot;
        public Siteitem Root { get { return siteroot; } set { siteroot = value; } }

        private List<SiteDBPull> SiteCommandList = new List<SiteDBPull>()
        {

        };

        public Sitefiller()
        {


        }

        public Siteitem fillsite()
        {

            SQLiteConnection QuickNotesDB = new SQLiteConnection(string.Format(@"Data Source={0}\Resources\database.db;Version=3;New=True;Compress=True;", Environment.CurrentDirectory));

            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = "SELECT * FROM Sites";
            pullall.Connection = QuickNotesDB;

            try
            {
                QuickNotesDB.Open();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (reader.Read())
                    SiteCommandList.Add(new SiteDBPull() { URL = reader["URL"].ToString(), Parent = reader["Parent"].ToString(), Child_1 = reader["Child_1"].ToString() });
                QuickNotesDB.Close();
            }
            catch
            {

            }


            List<SiteDBPull> Root_uniqueitems = SiteCommandList.GroupBy(s => s.Parent).Select(p => p.First()).ToList();
            List<SiteDBPull> Site1uniqueitems = SiteCommandList.GroupBy(s => s.Child_1).Select(p => p.First()).ToList();

            siteroot = new Siteitem() { Title = "Menu" };

            foreach (SiteDBPull item in Root_uniqueitems)
            {
                Siteitem Root_Item = new Siteitem() { Title = item.Parent, Content = item.URL };

                foreach (SiteDBPull item1 in Site1uniqueitems)
                {
                    Siteitem Sub_Item_1 = new Siteitem() { Title = item1.Child_1, Content = item1.URL };

                    if (item1.Parent == Root_Item.Title && item1.Child_1 != string.Empty)
                    {
                        Root_Item.SubItems.Add(Sub_Item_1);
                    }
                }
                siteroot.SubItems.Add(Root_Item);
            }
            return siteroot;
        }
    }
}
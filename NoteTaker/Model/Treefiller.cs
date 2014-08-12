using Scrivener.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public class Treefiller
    {
        private QuickItem root;
        public QuickItem Root { get { return root; } set { root = value; } }

        private List<QuickItemDBPull> CommandList = new List<QuickItemDBPull>()
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
                    CommandList.Add(new QuickItemDBPull() { Verbage = reader["QuickNotes_Verbage"].ToString(), Root_Folder = reader["QuickNotes_Root_Folder"].ToString(), Sub_Folder_1 = reader["QuickNotes_1"].ToString(), Sub_Folder_2 = reader["QuickNotes_2"].ToString(), Sub_Folder_3 = reader["QuickNotes_3"].ToString(), Sub_Folder_4 = reader["QuickNotes_4"].ToString(), Sub_Folder_5 = reader["QuickNotes_5"].ToString(), Sub_Folder_6 = reader["QuickNotes_6"].ToString(), Sub_Folder_7 = reader["QuickNotes_7"].ToString(), Sub_Folder_8 = reader["QuickNotes_8"].ToString(), Sub_Folder_9 = reader["QuickNotes_9"].ToString(), Sub_Folder_10 = reader["QuickNotes_10"].ToString() });
                QuickNotesDB.Close();
            }
            catch
            {

            }


            List<QuickItemDBPull> Root_uniqueitems = CommandList.GroupBy(s => s.Root_Folder).Select(p => p.First()).ToList();
            List<QuickItemDBPull> Sub1uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_1).Select(p => p.First()).ToList();
            List<QuickItemDBPull> Sub2uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_2).Select(p => p.First()).ToList();
            List<QuickItemDBPull> Sub3uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_3).Select(p => p.First()).ToList();
            List<QuickItemDBPull> Sub4uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_4).Select(p => p.First()).ToList();
            List<QuickItemDBPull> Sub5uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_5).Select(p => p.First()).ToList();
            List<QuickItemDBPull> Sub6uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_6).Select(p => p.First()).ToList();
            List<QuickItemDBPull> Sub7uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_7).Select(p => p.First()).ToList();
            List<QuickItemDBPull> Sub8uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_8).Select(p => p.First()).ToList();
            List<QuickItemDBPull> Sub9uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_9).Select(p => p.First()).ToList();
            List<QuickItemDBPull> Sub10uniqueitems = CommandList.GroupBy(s => s.Sub_Folder_10).Select(p => p.First()).ToList();


            root = new QuickItem() { Title = "Menu" };

            if (Root_uniqueitems.Count > 1)
            {
                foreach (QuickItemDBPull item in Root_uniqueitems)
                {
                    QuickItem Root_Item = new QuickItem() { Title = item.Root_Folder };
                    if (Sub1uniqueitems.Count > 1)
                    {
                        foreach (QuickItemDBPull item1 in Sub1uniqueitems)
                        {
                            QuickItem Sub_Item_1 = new QuickItem() { Title = item1.Sub_Folder_1, Content = item1.Verbage };

                            if (item1.Root_Folder == Root_Item.Title && item1.Sub_Folder_1 != string.Empty)
                            {
                                Root_Item.SubItems.Add(Sub_Item_1);
                            }
                            if (Sub2uniqueitems.Count > 1)
                            {
                                foreach (QuickItemDBPull item2 in Sub2uniqueitems)
                                {
                                    QuickItem Sub_Item_2 = new QuickItem() { Title = item2.Sub_Folder_2, Content = item2.Verbage };

                                    if (item2.Sub_Folder_1 == Sub_Item_1.Title && item2.Sub_Folder_2 != string.Empty)
                                    {
                                        Sub_Item_1.SubItems.Add(Sub_Item_2);
                                    }
                                    if (Sub3uniqueitems.Count > 1)
                                    {
                                        foreach (QuickItemDBPull item3 in Sub3uniqueitems)
                                        {
                                            QuickItem Sub_Item_3 = new QuickItem() { Title = item3.Sub_Folder_3, Content = item3.Verbage };

                                            if (item3.Sub_Folder_2 == Sub_Item_2.Title && item3.Sub_Folder_3 != string.Empty)
                                            {
                                                Sub_Item_2.SubItems.Add(Sub_Item_3);
                                            }
                                            if (Sub4uniqueitems.Count > 1)
                                            {
                                                foreach (QuickItemDBPull item4 in Sub4uniqueitems)
                                                {
                                                    QuickItem Sub_Item_4 = new QuickItem() { Title = item4.Sub_Folder_4, Content = item4.Verbage };

                                                    if (item4.Sub_Folder_3 == Sub_Item_3.Title && item4.Sub_Folder_4 != string.Empty)
                                                    {
                                                        Sub_Item_3.SubItems.Add(Sub_Item_4);
                                                    }
                                                    if (Sub5uniqueitems.Count > 1)
                                                    {
                                                        foreach (QuickItemDBPull item5 in Sub5uniqueitems)
                                                        {
                                                            QuickItem Sub_Item_5 = new QuickItem() { Title = item5.Sub_Folder_5, Content = item5.Verbage };

                                                            if (item5.Sub_Folder_4 == Sub_Item_4.Title && item5.Sub_Folder_5 != string.Empty)
                                                            {
                                                                Sub_Item_4.SubItems.Add(Sub_Item_5);
                                                            }
                                                            if (Sub6uniqueitems.Count > 1)
                                                            {
                                                                foreach (QuickItemDBPull item6 in Sub6uniqueitems)
                                                                {
                                                                    QuickItem Sub_Item_6 = new QuickItem() { Title = item6.Sub_Folder_6, Content = item6.Verbage };

                                                                    if (item6.Sub_Folder_5 == Sub_Item_5.Title && item6.Sub_Folder_6 != string.Empty)
                                                                    {
                                                                        Sub_Item_5.SubItems.Add(Sub_Item_6);
                                                                    }
                                                                    if (Sub7uniqueitems.Count > 1)
                                                                    {
                                                                        foreach (QuickItemDBPull item7 in Sub7uniqueitems)
                                                                        {
                                                                            QuickItem Sub_Item_7 = new QuickItem() { Title = item7.Sub_Folder_7, Content = item7.Verbage };

                                                                            if (item7.Sub_Folder_6 == Sub_Item_6.Title && item7.Sub_Folder_7 != string.Empty)
                                                                            {
                                                                                Sub_Item_6.SubItems.Add(Sub_Item_7);
                                                                            }
                                                                            if (Sub8uniqueitems.Count > 1)
                                                                            {
                                                                                foreach (QuickItemDBPull item8 in Sub8uniqueitems)
                                                                                {
                                                                                    QuickItem Sub_Item_8 = new QuickItem() { Title = item8.Sub_Folder_8, Content = item8.Verbage };

                                                                                    if (item8.Sub_Folder_7 == Sub_Item_7.Title && item8.Sub_Folder_8 != string.Empty)
                                                                                    {
                                                                                        Sub_Item_7.SubItems.Add(Sub_Item_8);
                                                                                    }
                                                                                    if (Sub9uniqueitems.Count > 1)
                                                                                    {
                                                                                        foreach (QuickItemDBPull item9 in Sub9uniqueitems)
                                                                                        {
                                                                                            QuickItem Sub_Item_9 = new QuickItem() { Title = item9.Sub_Folder_9, Content = item9.Verbage };

                                                                                            if (item9.Sub_Folder_8 == Sub_Item_8.Title && item9.Sub_Folder_9 != string.Empty)
                                                                                            {
                                                                                                Sub_Item_8.SubItems.Add(Sub_Item_9);
                                                                                            }                                                                                            
                                                                                            if (Sub10uniqueitems.Count > 1)
                                                                                            {
                                                                                                foreach (QuickItemDBPull item10 in Sub10uniqueitems)
                                                                                                {
                                                                                                    QuickItem Sub_Item_10 = new QuickItem() { Title = item10.Sub_Folder_10, Content = item10.Verbage };

                                                                                                    if (item10.Sub_Folder_9 == Sub_Item_9.Title && item10.Sub_Folder_10 != string.Empty)
                                                                                                    {
                                                                                                        Sub_Item_9.SubItems.Add(Sub_Item_10);
                                                                                                    }

                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                root.SubItems.Add(Root_Item);
                }                
            }
            return root;
        }
    }
}
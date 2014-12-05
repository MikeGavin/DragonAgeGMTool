using Minion;
using Minion.ListItems;
using Scrivener.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public class DataBaseReader
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        //Temp until single DB instance
        //private static SQLiteConnection QuickNotesDB = new SQLiteConnection(string.Format(@"Data Source={0}\Resources\QuickNotes.db;Version=3;New=True;Compress=True;", Environment.CurrentDirectory));
        private string mainDB = string.Format(@"Data Source={0}\Resources\Scrivener.sqlite", Environment.CurrentDirectory);
        private SQLiteConnection CallHistory = new SQLiteConnection(string.Format("Data Source=Call_History.db;Version=3;New=True;Compress=True;"));

        public async Task<ObservableCollection<RoleItem>> ReturnRoles()
        {
            log.Debug("Getting Roles");
            var db = new SQLiteConnection(mainDB);
            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = "SELECT * FROM Roles";
            
            pullall.Connection = db;
            log.Debug(pullall.CommandText);
            var importedRoles = new ObservableCollection<RoleItem>();
            try
            {              
                log.Info("CommandText: {0}", pullall.CommandText.ToString());
                db.Open();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (reader.Read())
                    importedRoles.Add(new RoleItem()
                    {
                        Name = reader["Name"].ToString().Trim(),
                        Minion = StringToBool(reader["Minion"].ToString().Trim()),
                        QuickItem_Table = reader["QuickItem_Table"].ToString().Trim(),
                        SiteItem_Table = reader["SiteItem_Table"].ToString().Trim()
                    });
               
                db.Close();

            }
            catch (Exception e)
            {
                log.Error(e);
                log.Info("Closing {0}", db.DataSource);
                db.Close();
            }
            return importedRoles;

        }    
        private bool StringToBool(string x)
        {
            bool result = false;
            if (x == "1")
            {
                result = true;
            }
            return result;       
        }       

        public async Task<ObservableCollection<MinionCommandItem>> ReturnMinionCommands(RoleItem role)
        {
            log.Debug("Getting Minion Commands for {0}", role.Name);
            var db = new SQLiteConnection(mainDB);
            //Return empty command list if role does not allow minion access.
            if (role == null || role.Minion == false || Properties.Settings.Default.Role_Current == null)
            {
                return new ObservableCollection<MinionCommandItem>();
            }

            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = "SELECT * FROM Minion_Commands";
            pullall.Connection = db;
            log.Debug(pullall.CommandText);
            var commandList = new ObservableCollection<MinionCommandItem>();
            try
            {
                db.Open();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (await reader.ReadAsync())
                    commandList.Add(new MinionCommandItem() 
                    { 
                        Name = reader["Name"].ToString().Trim(),
                        Action = reader["Action"].ToString().Trim(),
                        Version = reader["Version"].ToString().Trim(),
                        CopyFrom = reader["CopyFrom"].ToString().Trim(),
                        CopyTo = reader["CopyTo"].ToString().Replace("c:", string.Empty).Trim(),
                        Command = reader["Command"].ToString().Trim(),
                        Bit = reader["Bit"].ToString(),
                    });
                db.Close();  
     
            }
            catch (Exception e)
            {
                log.Error(e);
                db.Close(); 
                Model.ExceptionReporting.Email(e);
            }
            
            return commandList;

        }

        public async Task<QuickItem> ReturnQuickItems(RoleItem role)
        {
            log.Debug("Returning QuickItems for {0}", role.Name);
            var db = new SQLiteConnection(mainDB);
            if (role ==null) { return new QuickItem(); }
            List<QuickItemDBPull> CommandList = new List<QuickItemDBPull>();            
            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = string.Format("SELECT * FROM {0}", role.QuickItem_Table);
            pullall.Connection = db;
            log.Debug(pullall.CommandText);

            try
            {
                db.Open();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (await reader.ReadAsync())
                {
                    CommandList.Add(new QuickItemDBPull() { Verbage = reader["QuickNotes_Verbage"].ToString(), Root_Folder = reader["QuickNotes_Root_Folder"].ToString(), Sub_Folder_1 = reader["QuickNotes_1"].ToString(), Sub_Folder_2 = reader["QuickNotes_2"].ToString(), Sub_Folder_3 = reader["QuickNotes_3"].ToString(), Sub_Folder_4 = reader["QuickNotes_4"].ToString(), Sub_Folder_5 = reader["QuickNotes_5"].ToString(), Sub_Folder_6 = reader["QuickNotes_6"].ToString(), Sub_Folder_7 = reader["QuickNotes_7"].ToString(), Sub_Folder_8 = reader["QuickNotes_8"].ToString(), Sub_Folder_9 = reader["QuickNotes_9"].ToString(), Sub_Folder_10 = reader["QuickNotes_10"].ToString() });
                }
                db.Close();
            }
            catch (Exception e)
            {
                log.Error(e);
                Model.ExceptionReporting.Email(e);
                db.Close();
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

            #region Fill Tree
            var root = new QuickItem() { Title = "Menu" };

            if (Root_uniqueitems.Count > 1)
            {
                foreach (QuickItemDBPull item in Root_uniqueitems)
                {
                    QuickItem Root_Item = new QuickItem() { Title = item.Root_Folder, Content = item.Verbage };
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

#endregion
        }

        public async Task<Siteitem> ReturnSiteItems(RoleItem role)
        {
            log.Debug("Getting SiteItems for {0}", role.Name);
            var db = new SQLiteConnection(mainDB);
            if (role == null) { return new Siteitem(); }

            List<SiteDBPull> SiteCommandList = new List<SiteDBPull>();
            
            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = string.Format("SELECT * FROM {0}", role.SiteItem_Table);
            pullall.Connection = db;
            log.Debug(pullall.CommandText);

            try
            {
                db.Open();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (await reader.ReadAsync())
                    SiteCommandList.Add(new SiteDBPull() { URL = reader["URL"].ToString(), Parent = reader["Parent"].ToString(), Child_1 = reader["Child_1"].ToString(), Child_2 = reader["Child_2"].ToString() });
                db.Close();
            }
            catch (Exception e)
            {
                log.Error(e);
                var temp = Helpers.MetroMessageBox.Show("ERMAHGERD ERER!", e.ToString());
                Model.ExceptionReporting.Email(e);
                db.Close();
            }


            List<SiteDBPull> Root_uniqueitems = SiteCommandList.GroupBy(s => s.Parent).Select(p => p.First()).ToList();
            List<SiteDBPull> Site1uniqueitems = SiteCommandList.GroupBy(s => s.Child_1).Select(p => p.First()).ToList();
            List<SiteDBPull> Site2uniqueitems = SiteCommandList.GroupBy(s => s.Child_2).Select(p => p.First()).ToList();

            #region Fill Site
            var root = new Siteitem() { Title = "Menu" };

            if (Root_uniqueitems.Count > 1)
            {
                foreach (SiteDBPull item in Root_uniqueitems)
                {
                    Siteitem Root_Item = new Siteitem() { Title = item.Parent, Content = item.URL };
                    if (Site1uniqueitems.Count > 1)
                    {
                        foreach (SiteDBPull item1 in Site1uniqueitems)
                        {
                            Siteitem Sub_Item_1 = new Siteitem() { Title = item1.Child_1, Content = item1.URL };

                            if (item1.Parent == Root_Item.Title && item1.Child_1 != string.Empty)
                            {
                                Root_Item.SubItems.Add(Sub_Item_1);
                            }

                            if (Site2uniqueitems.Count > 1)
                            {
                                foreach (SiteDBPull item2 in Site2uniqueitems)
                                {
                                    Siteitem Sub_Item_2 = new Siteitem() { Title = item2.Child_2, Content = item2.URL };

                                    if (item2.Child_1 == Sub_Item_1.Title && item2.Child_2 != string.Empty)
                                    {
                                        Sub_Item_1.SubItems.Add(Sub_Item_2);
                                    }
                                }
                            }
                        }
                    }
                    root.SubItems.Add(Root_Item);
                }
            }
            Sorting(root);  
            root.SubItems = new ObservableCollection<Siteitem>(root.SubItems.OrderBy(n=> n.Title));
            return root;

            #endregion
        }

        private void Sorting(Siteitem root)
        {
            foreach (Siteitem item in root.SubItems)
            {
                if (item.SubItems.Count > 0)
                {
                    item.SubItems = new ObservableCollection<Siteitem>(item.SubItems.OrderBy(n => n.Title));
                    Sorting(item);
                }
            }
        }

        public async Task<HistoryItem> ReturnHistory()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            SQLiteConnection Call_History = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");

            log.Debug("Getting History");
            System.Data.StateChangeEventHandler handel = (s, e) => log.Debug("CallHistory: {0}", e.CurrentState);
            Call_History.StateChange += handel;

            List<HistoryDBPull> HistoryList = new List<HistoryDBPull>();
            string Today = "CurrentHistory";

            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = string.Format("SELECT * FROM {0}", Today);
            pullall.Connection = Call_History;
            log.Debug(pullall.CommandText);
            try
            {
                await Call_History.OpenAsync();
                SQLiteDataReader reader = await pullall.ExecuteReaderAsync() as SQLiteDataReader;
                while (await reader.ReadAsync())
                    HistoryList.Add(new HistoryDBPull() { Date = reader["Date"].ToString(), Time = reader["Time"].ToString(), Caller = reader["Caller"].ToString(), Notes = reader["Notes"].ToString() });
                Call_History.Close();
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            var root = new HistoryItem() { Title = "Menu" };


            List<HistoryDBPull> Root_uniquetime = HistoryList.GroupBy(s => s.Time).Select(p => p.First()).ToList();

            foreach (HistoryDBPull Item in Root_uniquetime)
            {
                HistoryItem Root_Item = new HistoryItem() { Title = Item.Date + ", " + Item.Time, Content = Item.Notes };
                root.SubItems.Add(Root_Item);
            }            

            Call_History.StateChange -= handel;
            return root;

        }
    }
}

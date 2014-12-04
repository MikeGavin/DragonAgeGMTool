using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    public class SaveNotes
    {
        //Logger Boilerplate
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        //Constructor
        public SaveNotes()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            CreatesHistory();
        }

        //Shared method data
        private string MainTableName { get { return "CurrentHistory"; } }
        private string ArchiveTableName { get { return "ArchiveHistory"; } }
        private SQLiteConnection Call_history = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");

        ///<summary>
        ///<para>creates Call History Database and populates table with todays date if none exist</para>
        ///</summary>
        private void CreatesHistory()
        {
            //creates Call History Database and populates table with todays date if none exist
            string query = string.Format("CREATE TABLE IF NOT EXISTS [{0}](Date Text,Time Text,ID Integer,Caller Text,Notes Text)", MainTableName);
            string query2 = string.Format("CREATE TABLE IF NOT EXISTS [{0}](Date Text,Time Text,ID Integer,Caller Text,Notes Text)", ArchiveTableName);

            SQLiteCommand command = new SQLiteCommand(query, Call_history);
            SQLiteCommand command2 = new SQLiteCommand(query2, Call_history);

            Call_history.Open();
            //creates DB and table for todays saving of notes 
            command.ExecuteNonQuery();
            command2.ExecuteNonQuery();

            Call_history.Close();
        }

        public int GetNewIndex()
        {
            int initialcountvalue = 0;
            int countvalue = 0;
            int index = 0;
            string Title = "Title";
            string Text = "Text";

            string initialcount = string.Format("SELECT COUNT (ID) from {0}", MainTableName);
            string initialinsert = string.Format("INSERT INTO {0} (Date,Time,ID,Caller,Notes) values ('Date','Time','1','{1}','{2}');", MainTableName, Title, Text);
            String count = "SELECT ID from CurrentHistory ORDER BY ID desc limit 1";

            try
            {
                using (SQLiteCommand doinitialcount = Call_history.CreateCommand())
                {
                    doinitialcount.CommandText = initialcount;
                    doinitialcount.ExecuteNonQuery();
                    initialcountvalue = Convert.ToInt32(doinitialcount.ExecuteScalar());
                    if (initialcountvalue == 0)
                    {
                        SQLiteCommand initialinsertcommand = new SQLiteCommand(initialinsert, Call_history);
                        initialinsertcommand.ExecuteNonQuery();
                        index = 1;
                    }
                    else if (initialcountvalue > 0)
                    {
                        using (SQLiteCommand docount = Call_history.CreateCommand())
                        {
                            docount.CommandText = count;
                            docount.ExecuteNonQuery();
                            countvalue = Convert.ToInt32(docount.ExecuteScalar());
                        }

                        index = countvalue;
                        index++;

                        string insert = string.Format("INSERT INTO {0} (Date,Time,ID,Caller,Notes) values ('Date','Time','{1}','{2}','{3}');", MainTableName, index, Title, Text);

                        SQLiteCommand insertcommand = new SQLiteCommand(insert, Call_history);

                        insertcommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                Model.ExceptionReporting.Email(e);
            }

            Call_history.Close();

            return index;
        }

        //test of .net guid generation for indexing. Need more unique item as count of items in tables will not work with 2 tables.
        public void guid()
        {
            Guid guid = Guid.NewGuid();
            string str = guid.ToString();
            Guid test = Guid.Parse(str);
        }

        //Move all closed items to archive. Only read archive.

    }


}

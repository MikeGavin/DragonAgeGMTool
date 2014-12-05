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
    public class NoteManager
    {
        //Logger Boilerplate
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        //Constructor
        public NoteManager()
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
        private async Task CreatesHistory()
        {
            //creates Call History Database and populates table with todays date if none exist
            string query = string.Format("CREATE TABLE IF NOT EXISTS [{0}](Date Text,Time Text,ID Integer PRIMARY KEY,Caller Text,Notes Text,guid Text)", MainTableName);
            string query2 = string.Format("CREATE TABLE IF NOT EXISTS [{0}](Date Text,Time Text,ID Integer PRIMARY KEY,Caller Text,Notes Text,guid Text )", ArchiveTableName);

            SQLiteCommand command = new SQLiteCommand(query, Call_history);
            SQLiteCommand command2 = new SQLiteCommand(query2, Call_history);

            await Call_history.OpenAsync();
            
            //creates DB and table for saving of notes 
            command.ExecuteNonQuery();
            command2.ExecuteNonQuery();

            Call_history.Close();
        }

        //test of .net guid generation for indexing. Need more unique item as count of items in tables will not work with 2 tables.
        public void guid()
        {
            Guid guid = Guid.NewGuid();
            string str = guid.ToString();
            Guid test = Guid.Parse(str);
        }

        //Move all closed items to archive. Only read archive.


        private async Task SaveNote(INote n, string tablename)
        {
            //String for creating time stamp
            string Date = DateTime.Now.ToString("ddd MMM d yyyy");
            string Time = DateTime.Now.ToString("HH:mm:ss");
            //Strings for replacing "Caller" and "Notes" value
            string correcttext = n.Text.Replace("'", "`");
            string replacetitle = string.Format("UPDATE {0} SET Caller = '{1}' WHERE ID = '{2}';", tablename, n.Title, n.Guid.ToString());
            string replacenote = string.Format("UPDATE {0} SET Notes = '{1}' WHERE ID = '{2}';", tablename, correcttext, n.Guid.ToString());
            string replacedate = string.Format("UPDATE {0} SET Date = '{1}' WHERE ID = '{2}';", tablename, Date, n.Guid.ToString());
            string replacetime = string.Format("UPDATE {0} SET Time = '{1}' WHERE ID = '{2}';", tablename, Time, n.Guid.ToString());
            //Database Path
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            SQLiteConnection Call_history = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");
            //Updates notes in Database
            await Call_history.OpenAsync();
            SQLiteCommand replacetitlecommand = new SQLiteCommand(replacetitle, Call_history);
            SQLiteCommand replacenotecommand = new SQLiteCommand(replacenote, Call_history);
            SQLiteCommand replacedatecommand = new SQLiteCommand(replacedate, Call_history);
            SQLiteCommand replacetimecommand = new SQLiteCommand(replacetime, Call_history);
            try
            {
                replacetitlecommand.ExecuteNonQuery();
                replacenotecommand.ExecuteNonQuery();
                replacedatecommand.ExecuteNonQuery();
                replacetimecommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                log.Error(e);
                //Model.ExceptionReporting.Email(e);
            }

            HistoryCleanup(n, tablename, Call_history);
            Call_history.Close();
        }

        public async Task SaveCurrent(INote n)
        {
            await SaveNote(n, MainTableName);
        }

        public async Task ArchiveCurrent(INote n)
        {
            await SaveNote(n, ArchiveTableName);
            await DeleteNote(n, ArchiveTableName);
        }

        private async Task DeleteNote(INote n, string tableName)
        {
            throw new NotImplementedException();
        }
    }


}

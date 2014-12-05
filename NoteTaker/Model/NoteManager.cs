using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private string MainTableName { get { return "OpenNotes"; } }
        private string ArchiveTableName { get { return "NoteArchive"; } }
        //private SQLiteConnection noteDatabase = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");
        private string noteDatabase = @"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;";

        ///<summary>
        ///<para>creates Call History Database and populates table with todays date if none exist</para>
        ///</summary>
        private async Task CreatesHistory()
        {
            //creates Call History Database and populates table with todays date if none exist
            string query = string.Format(@"CREATE TABLE IF NOT EXISTS [{0}](
                                            `Date` Text,
                                            `Time` Text,
                                            `ID` INTEGER PRIMARY KEY,
                                            `Title` Text,
                                            `Notes` Text,
                                            `guid` Text NOT NULL UNIQUE 
                                        )", MainTableName);
            string query2 = string.Format(@"CREATE TABLE IF NOT EXISTS [{0}](
                                            `Date` Text,
                                            `Time` Text,
                                            `ID` INTEGER PRIMARY KEY,
                                            `Title` Text,
                                            `Notes` Text,
                                            `guid` Text NOT NULL UNIQUE 
                                        )", ArchiveTableName);

            await WriteDatabase(query);
            await WriteDatabase(query2);

            //SQLiteCommand command = new SQLiteCommand(query, noteDatabase);
            //SQLiteCommand command2 = new SQLiteCommand(query2, noteDatabase);

            //await noteDatabase.OpenAsync();

            ////creates DB and table for saving of notes 
            //await command.ExecuteNonQueryAsync();
            //await command2.ExecuteNonQueryAsync();

            //noteDatabase.Close();
        }

        private async Task WriteDatabase(string command)
        {
            try
            {
                //Open the database
                using (SQLiteConnection conn = new SQLiteConnection((noteDatabase)))
                {
                    await conn.OpenAsync();
                    using (SQLiteCommand cmd = new SQLiteCommand(command, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch(SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Busy)
                    log.Error("Database is locked by another process!");
            }

        }

        //test of .net guid generation for indexing. Need more unique item as count of items in tables will not work with 2 tables.
        private void guid()
        {
            Guid guid = Guid.NewGuid();
            string str = guid.ToString();
            Guid test = Guid.Parse(str);
        }

        private async Task SaveNote(INote n, string tablename)
        {
            //String for creating time stamp
            string Date = DateTime.Now.ToString("ddd MMM d yyyy");
            string Time = DateTime.Now.ToString("HH:mm:ss");

            var update = string.Format(@"UPDATE OR IGNORE {0}
                                            SET Title = '{1}', Notes = '{2}', Date = '{3}', Time = '{4}',
                                            WHERE guid = '{5}';", tablename, n.Title, n.Text, Date, Time, n.Guid.ToString());
            var command = string.Format(@"INSERT OR IGNORE INTO {0} (guid, Title, Notes, Date, Time) 
                                            VALUES ( '{1}', '{2}', '{3}', '{4}', '{5}' );", tablename, n.Guid.ToString(), n.Title, n.Text, Date, Time);


            //await WriteDatabase(update);
            await WriteDatabase(command);


            //Updates notes in Database
            //await noteDatabase.OpenAsync();
            //SQLiteCommand replacetitlecommand = new SQLiteCommand(replacetitle, noteDatabase);
            //SQLiteCommand replacenotecommand = new SQLiteCommand(replacenote, noteDatabase);
            //SQLiteCommand replacedatecommand = new SQLiteCommand(replacedate, noteDatabase);
            //SQLiteCommand replacetimecommand = new SQLiteCommand(replacetime, noteDatabase);
            //try
            //{
            //    replacetitlecommand.ExecuteNonQuery();
            //    replacenotecommand.ExecuteNonQuery();
            //    replacedatecommand.ExecuteNonQuery();
            //    replacetimecommand.ExecuteNonQuery();
            //}
            //catch (Exception e)
            //{
            //    log.Error(e);
            //    //Model.ExceptionReporting.Email(e);
            //}

            ////HistoryCleanup(n, tablename, Call_history);
            //noteDatabase.Close();
        }

        private async Task DeleteNote(INote n, string tableName)
        {
            var deleteNote = string.Format("DELETE FROM {0} WHERE guid = '{1}';", tableName, n.Guid.ToString());

            await WriteDatabase(deleteNote);
        }

        public async Task SaveCurrent(INote n)
        {
            await SaveNote(n, MainTableName);
        }

        public async Task ArchiveCurrent(INote n)
        {
            await SaveNote(n, ArchiveTableName);
            await DeleteNote(n, MainTableName);
        }

        public async Task<ObservableCollection<NoteType>> GetOpenNotes()
        {
            log.Debug("Getting Open Notes");
            var db = new SQLiteConnection(noteDatabase);
            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = string.Format("SELECT * FROM {0}", MainTableName);

            pullall.Connection = db;
            log.Debug(pullall.CommandText);
            var openNotes = new ObservableCollection<NoteType>();
            try
            {
                log.Info("CommandText: {0}", pullall.CommandText.ToString());
                await db.OpenAsync();
                SQLiteDataReader reader = pullall.ExecuteReader();
                while (await reader.ReadAsync())
                {
                    var guid = reader["guid"].ToString().Trim();
                    var title = reader["Title"].ToString().Trim();
                    var text = reader["Notes"].ToString().Trim();
                    try
                    {
                        openNotes.Add(new NoteType(Guid.Parse(guid), title, text));                    
                    }
                    catch(Exception ex)
                    {
                        log.Error(ex);
                    }
                }

                db.Close();

            }
            catch (Exception e)
            {
                log.Error(e);
                log.Info("Closing {0}", db.DataSource);
                db.Close();
            }
    
            return openNotes;
        }

    }
}

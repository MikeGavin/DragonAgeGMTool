﻿using System;
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
            var deployment = new DeploymentData(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            AppDomain.CurrentDomain.SetData("DataDirectory", deployment.SettingsFolder);
            Task.Run(async () => await CreatesHistory());            
        }



        //Shared method data
        private string MainTableName { get { return "OpenNotes"; } }
        private string ArchiveTableName { get { return "NoteArchive"; } }
        //private SQLiteConnection noteDatabase = new SQLiteConnection(@"Data Source=|DataDirectory|\Scrivener\userdata.db;Version=3;New=True;Compress=True;");
        private string noteDatabase = @"Data Source=|DataDirectory|\notedata.db;Version=3;New=True;Compress=True;";

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
                    conn.Close();
                }
            }
            catch(SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Busy)
                    log.Error("Database is locked by another process!");
                else
                    log.Error(ex);
                CreatesHistory();
                
                string writeerror = "SQL logic error or missing database\r\nno such table: OpenNotes";
                string exstring = ex.ToString();

                if(exstring.Contains(writeerror))
                {
                    Model.ExceptionReporting.Email(ex); 
                }                
            }
            finally
            {
                using (SQLiteConnection conn = new SQLiteConnection((noteDatabase)))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(command, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
        }

        private async Task SaveNote(INote n, string tablename)
        {
            
            var Date = n.LastUpdated.ToString("d");
            var Time = n.LastUpdated.ToString("T");

            var update = string.Format(@"UPDATE OR IGNORE {0}
                                            SET Title = '{1}', Notes = '{2}', Date = '{3}', Time = '{4}'
                                            WHERE guid = '{5}';", tablename, n.Title.Replace("'", "&#39;"), n.Text.Replace("'", "&#39;"), Date, Time, n.Guid.ToString());
            var command = string.Format(@"INSERT OR IGNORE INTO {0} (guid, Title, Notes, Date, Time) 
                                            VALUES ( '{1}', '{2}', '{3}', '{4}', '{5}' );", tablename, n.Guid.ToString(), n.Title.Replace("'", "&#39;"), n.Text.Replace("'", "&#39;"), Date, Time);
            
            await WriteDatabase(update);
            await WriteDatabase(command);

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

        public async Task<ObservableCollection<NoteType>> GetCurrentNotes()
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
                    var datetime = DateTime.Parse(string.Format("{0} {1}", reader["Date"].ToString().Trim(), reader["Time"].ToString().Trim()));
                    try
                    {
                        openNotes.Add(new NoteType(Guid.Parse(guid), title.Replace("&#39;", "'"), text.Replace("&#39;", "'"), datetime));                    
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

        public async Task<ObservableCollection<NoteType>> GetArchivedNotes(DateTime searchDate)
        {
            log.Debug("Searching Archive Notes");
            var db = new SQLiteConnection(noteDatabase);
            SQLiteCommand pullall = new SQLiteCommand();
            pullall.CommandText = string.Format("SELECT * FROM {0} WHERE Date = '{1}'", ArchiveTableName, searchDate.ToString("d"));

            pullall.Connection = db;
            log.Debug(pullall.CommandText);
            var archiveSearch = new ObservableCollection<NoteType>();
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
                    var datetime = DateTime.Parse(string.Format("{0} {1}", reader["Date"].ToString().Trim(), reader["Time"].ToString().Trim()));
                    try
                    {
                        archiveSearch.Add(new NoteType(title.Replace("&#39;", "'"), text.Replace("&#39;", "'"), datetime)); //Forces generation of a new GUID to prevent updates to history while keeping datetime
                    }
                    catch (Exception ex)
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

            return archiveSearch;
        }

        public int count = 0;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void getcallcount()
        {
            var db = new SQLiteConnection(noteDatabase);
            db.OpenAsync();

            using (SQLiteCommand cmd = db.CreateCommand())
            {
                
                cmd.CommandText = string.Format("SELECT COUNT(Date) FROM {0} WHERE Date is '{1}'", ArchiveTableName, DateTime.Now.ToString("M/d/yyyy"));
                cmd.ExecuteNonQuery();
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }
            db.Close();

            Properties.Settings.Default.Callcount = count;
        }
    }
}

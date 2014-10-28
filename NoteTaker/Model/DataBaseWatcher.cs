using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Permissions;
using System.Data.SQLite;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Scrivener.Model
{
    public class DataBaseWatcher
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private string sourceDBpath { get; set; }
        private string appDBpath = string.Format(@"{0}\Resources\", Environment.CurrentDirectory);
        //private Uri uri;
        public static event EventHandler<FileSystemEventArgs> DataBaseUpdated;
        private void OnDataBaseUpdate(FileSystemEventArgs e)
        {
            if (DataBaseUpdated != null)
            {
                DataBaseUpdated(this, e);
            }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]

        private string GetDBLocation(Uri uri)
        {
            var uriString = uri.LocalPath.ToLower().Replace("scrivener.application", string.Empty);
            // Also:
            //deploy.DataDirectory
            //deploy.UpdateLocation
            if (uriString.ToLower().Contains("dev"))
            {
                log.Debug(@"Setting Database folder as {0}devbase", uriString);
                return string.Format(@"{0}devbase", uriString);
            }
            else
            {
                log.Debug(@"Setting Database folder as {0}database", uriString);
                return string.Format(@"{0}database", uriString);
            }          
        }

        public DataBaseWatcher(Uri incomingUri)
        {
            
            sourceDBpath = GetDBLocation(incomingUri);
            if (Directory.Exists(sourceDBpath))
            {
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    //check for main DB and update. Needs changed to scan for all DB's
                    log.Debug(sourceDBpath);
                    var t = SyncDBs();

                    // Create a new FileSystemWatcher and set its properties.
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = sourceDBpath;
                    /* Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories. */
                    watcher.NotifyFilter = NotifyFilters.LastWrite;
                    // Only watch text files.
                    watcher.Filter = "*.sqlite";
                    // Add event handlers.
                    watcher.Changed += watcher_Changed;
                    watcher.Created += watcher_Changed;
                    //watcher.Deleted += new FileSystemEventHandler(CopyDBs);
                    //watcher.Renamed += new RenamedEventHandler(OnRenamed);

                    // Begin watching.
                    watcher.EnableRaisingEvents = true;
                }
                else
                {
                    throw new System.Deployment.Application.InvalidDeploymentException();
                }
            }
            else
            {
                log.Debug("{0} is unreachable. Not listening.", sourceDBpath);
            }
        }
        
        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var localDB = appDBpath + e.Name;
            var sourceDB = e.FullPath;          
            
            if (File.GetLastWriteTime(sourceDB) > File.GetLastWriteTime(localDB))
            {
                System.Threading.Thread.Sleep(30000); //Pause 30 seconds.
                CheckCopy(localDB, sourceDB, e);                
            }
            // else discard the (duplicated) OnChanged event

            
        }

        private void CheckCopy(string destination, string source, FileSystemEventArgs e = null)
        {
            if (File.Exists(destination)) //Checks is file exists
            {
                System.Security.Cryptography.HashAlgorithm ha = System.Security.Cryptography.HashAlgorithm.Create();
                using (FileStream f1 = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                                  f2 = new FileStream(destination, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    /* Calculate Hash */
                    byte[] hash1 = ha.ComputeHash(f1);
                    byte[] hash2 = ha.ComputeHash(f2);
                    //Compare files and copy if destination does not match server
                    if (BitConverter.ToString(hash1) != BitConverter.ToString(hash2))
                    {
                        log.Info("New Database version detected in ", sourceDBpath);
                        log.Debug("Copy [{0}] To [{1}]", source, destination);
                        File.Copy(source, destination, true);
                        OnDataBaseUpdate(e);
                    }
                    else
                    {
                        log.Debug("Remote DB matches current copy");
                    }
                }             

            }
            else //copies if no destination file/
            {
                log.Warn("Database missing: {0} ", destination);
                log.Debug("Copy [{0}] To [{1}]", source, destination);
                File.Copy(source, destination, true);

            }
        }
        
        private async Task SyncDBs()
        {
            //Get a list of DB files and copy them
            var extensions = new[] { ".sqlite" };

            var files = (from file in Directory.EnumerateFiles(sourceDBpath)
                         where extensions.Contains(Path.GetExtension(file), StringComparer.InvariantCultureIgnoreCase)
                         select new
                         {
                             Source = file,
                             Destination = Path.Combine(appDBpath, Path.GetFileName(file))
                         });

            foreach (var file in files)
            {
                try
                {
                    CheckCopy(file.Destination, file.Source);
                }
                catch (Exception f)
                {
                    log.Error(f);
                    Model.ExceptionReporting.Email(f);
                }
            }
        }

        //private void OnRenamed(object source, RenamedEventArgs e)
        //{
        //    // Specify what is done when a file is renamed.

        //    var extensions = new[] { ".sqlite" };

        //    var files = (from file in Directory.EnumerateFiles(newDBpath)
        //                 where extensions.Contains(Path.GetExtension(file), StringComparer.InvariantCultureIgnoreCase)
        //                 select new
        //                 {
        //                     Source = file,
        //                     Destination = Path.Combine(oldDBpath, Path.GetFileName(file))
        //                 });

        //    foreach (var file in files)
        //    {
        //        File.Copy(file.Source, file.Destination, true);
        //    }
        //}
    }

}

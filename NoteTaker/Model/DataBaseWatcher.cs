using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Permissions;

namespace Scrivener.Model
{
    public class DataBaseWatcher
    {
        public string newDBpath = "\\\\fs1\\EdTech\\Scrivener\\Database";
        public string oldDBpath = string.Format("{0}\\Resources", Environment.CurrentDirectory);

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    
        public void Run()
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = newDBpath;
            /* Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter = "*.db";
            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
        }

        // Define the event handlers. 
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.

            var extensions = new[] { ".db" };

            var files = (from file in Directory.EnumerateFiles(newDBpath)
                         where extensions.Contains(Path.GetExtension(file), StringComparer.InvariantCultureIgnoreCase)
                         select new
                         {
                             Source = file,
                             Destination = Path.Combine(oldDBpath, Path.GetFileName(file))
                         });

            foreach (var file in files)
            {
                File.Copy(file.Source, file.Destination);
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.

            var extensions = new[] { ".db" };

            var files = (from file in Directory.EnumerateFiles(newDBpath)
                         where extensions.Contains(Path.GetExtension(file), StringComparer.InvariantCultureIgnoreCase)
                         select new
                         {
                             Source = file,
                             Destination = Path.Combine(oldDBpath, Path.GetFileName(file))
                         });

            foreach (var file in files)
            {
                File.Copy(file.Source, file.Destination, true);
            }
        }
    }

}

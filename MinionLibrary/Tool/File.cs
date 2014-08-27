using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Minion.Tool
{
    public class Files
    {
        #region Custom Logger
        /// <summary>
        /// This logger addition is used to allow for raising an event which passes the logged message 
        /// so that it can be captured by listeners for screen logging
        /// </summary>
        protected static NLog.Logger nlog = NLog.LogManager.GetCurrentClassLogger();
        protected void Log(Minion.log type, string message, params object[] args)
        {
            int count = 0;
            foreach (object item in args)
            {
                message = message.Replace(@"{" + count.ToString() + "}", item.ToString());
                count++;
            }

            if (type == log.Trace)
            {
                nlog.Trace(message);
            }
            else if (type == log.Debug)
            {
                nlog.Debug(message);
            }
            else if (type == log.Info)
            {
                nlog.Info(message);
            }
            else if (type == log.Warn)
            {
                nlog.Warn(message);
            }
            else if (type == log.Error)
            {
                nlog.Error(message);
            }
            else if (type == log.Fatal)
            {
                nlog.Fatal(message);
            }

            if (EventLogged != null) { EventLogged(this, new Minion.LogEventArgs(DateTime.Now, type, message)); }
        }
        protected void PassEventLogged(object sender, Minion.LogEventArgs e)
        {
            if (EventLogged != null) { EventLogged(this, e); }
        }
        public event EventHandler<Minion.LogEventArgs> EventLogged;
        #endregion

        #region FileCopying
        //file copy functions
        public void Copy(string From, string To, bool forceoverwrite = false)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(From);
            //detect whether its a directory or file
            Log(log.Debug,"Copy command was [ {0} ] To [ {1} ]", From, To);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DirectoryCopy(From, To, true, forceoverwrite);
            }
            else
                //MessageBox.Show("Its a file");
                FileCopy(From, To, forceoverwrite);
        }

        private void FileCopy(string sourcePath, string targetPath, bool overwrite)
        {
            // Use Path class to manipulate file and directory paths. 
            string fileName = System.IO.Path.GetFileName(sourcePath);
            string sourceFile = System.IO.Path.Combine(sourcePath);
            string destFile = System.IO.Path.Combine(targetPath, fileName);

            // To copy a folder's contents to a new location: 
            // Create a new target folder, if necessary. 
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }
            // To copy a file to another location and  
            // overwrite the destination file if it already exists.
            FileInfo dest = new FileInfo(destFile);
            FileInfo source = new FileInfo(sourceFile);
            if (!overwrite)
            {
                if (source.LastWriteTime > dest.LastWriteTime)
                {
                    CopyingFile(sourceFile, destFile);
                }
                else
                    Log(log.Info, @"Skipping: '{0}' remote file matches current version", dest.FullName);
            }
            else
            {
                CopyingFile(sourceFile, destFile);
            }
        }

        private void CopyingFile(string sourceFile, string destFile)
        {
            Log(log.Info, "Copying [ {0} ] To [ {1} ]", sourceFile, destFile);
            // now you can safely overwrite it
            try
            {
                System.IO.File.Copy(sourceFile, destFile, true);
            }
            catch (Exception e)
            {
                Log(log.Error, e.ToString());
            }
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                //throw new DirectoryNotFoundException(
                Log(log.Error, "Source directory does not exist or could not be found: "  + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                FileInfo destFile = new FileInfo(temppath);

                if (System.IO.File.Exists(temppath))
                {
                    if (!overwrite)
                    {
                        if (file.LastWriteTime > destFile.LastWriteTime)
                        {
                            try
                            {
                                file.CopyTo(temppath, true);
                            }
                            catch (Exception e)
                            {
                                Log(log.Error, e.ToString());
                                return;
                            }
                        }
                        else
                            Log(log.Info, @"Skipping: '{0}' File matches current version", destFile.FullName);
                    }
                    else
                    {
                        try
                        {
                            file.CopyTo(temppath, true);
                        }
                        catch (Exception e)
                        {
                            Log(log.Error, e.ToString());
                            return;
                        }
                    }
                }
                else
                {
                    try { file.CopyTo(temppath, false); }
                    catch (Exception e) { Log(log.Error, e.ToString()); return; }
                }
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, overwrite);
                }
            }
        }
        #endregion

    }
}

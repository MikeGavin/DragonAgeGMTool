using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Minion.Tool
{
    internal class Files
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        #region FileCopying
        //file copy functions
        public static void Copy(string From, string To, bool forceoverwrite = false)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(From);
            //detect whether its a directory or file
            log.Debug(string.Format("Copy command was [ {0} ] To [ {1} ]", From, To));
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DirectoryCopy(From, To, true, forceoverwrite);
            }
            else
                //MessageBox.Show("Its a file");
                FileCopy(From, To, forceoverwrite);
        }

        private static void FileCopy(string sourcePath, string targetPath, bool overwrite)
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
                    log.Debug(string.Format(@"Skipping: '{0}' File matches current version", dest.FullName));
            }
            else
            {
                CopyingFile(sourceFile, destFile);
            }
        }

        private static void CopyingFile(string sourceFile, string destFile)
        {
            log.Debug(string.Format("Copying [ {0} ] To [ {1} ]", sourceFile, destFile));
            // now you can safely overwrite it
            try
            {
                System.IO.File.Copy(sourceFile, destFile, true);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                //throw new DirectoryNotFoundException(
                log.Error("Source directory does not exist or could not be found: "
                + sourceDirName);
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
                            catch (Exception ex)
                            {
                                log.Error(ex);
                                return;
                            }
                        }
                        else
                            log.Info(string.Format(@"Skipping: '{0}' File matches current version", destFile.FullName));
                    }
                    else
                    {
                        try
                        {
                            file.CopyTo(temppath, true);
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex);
                            return;
                        }
                    }
                }
                else
                {
                    try { file.CopyTo(temppath, false); }
                    catch (Exception ex) { log.Error(ex); return; }
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

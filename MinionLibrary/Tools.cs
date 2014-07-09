using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;



namespace Minion
{
    public class Tools : Process.ToolBase
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private string IP { get; set; }

        public Tools(string ip)
        {
            IP = ip;
        }

        public void PSKill(string victim)
        {
            Program = "pskill.exe";
            log.Info(string.Format("Killing process: {0}", victim));
            execute(Px_Arguments("-accepteula -t " + victim));
        }

        public void PSShutdown(string arguments)
        {

            Program = "psshutdown.exe";
            execute(Px_Arguments("-accepteula" + arguments));
        }

        public bool PSExec(string arguments)
        {
            Program = "psexec.exe";
               
            execute(Px_Arguments(arguments));
            StandardError = StandardError.Replace("\r\r", string.Empty).Replace("\r\nPsExec v1.98 - Execute processes remotely\r\nCopyright (C) 2001-2010 Mark Russinovich\r\nSysinternals - www.sysinternals.com\r\n", string.Empty).TrimEnd(null);
            if (StandardError.Contains("error code 0") == true)
            {
                return true;
            }
            else
                return false;
        }

        public bool PAExec(string arguments)
        {
            Program = "paexec.exe";
            execute(Px_Arguments(arguments));
            if (ExitCode == 0)
            {
                return true;
            }
            else if (ExitCode < 0)
            {
                StandardError += string.Format("| PAEXEC Error {0}: ", ExitCode);
                if (ExitCode == -1)  {StandardError += "internal error";}
                if (ExitCode == -2)  {StandardError += "command line error";}
                if (ExitCode == -3)  {StandardError += "failed to launch app (locally)";}
                if (ExitCode == -4)  {StandardError += "failed to copy PAExec to remote (connection to ADMIN$ might have failed)";}
                if (ExitCode == -5)  {StandardError += "connection to server taking too long (timeout)";}
                if (ExitCode == -6)  {StandardError += "PAExec service could not be installed/started on remote server";}
                if (ExitCode == -7)  {StandardError += "could not communicate with remote PAExec service";}
                if (ExitCode == -8)  {StandardError += "failed to copy app to remote server";}
                if (ExitCode == -9)  {StandardError += "failed to launch app (remotely)";}
                if (ExitCode == -10) {StandardError += "app was terminated after timeout expired";}
                if (ExitCode == -11) {StandardError += "forcibly stopped with Ctrl-C / Ctrl-Break";}
                log.Error(StandardError);
                return false;
            }
            else if (ExitCode > 0)
            {
                log.Error(StandardError = string.Format("Program ran but returned error {0}: {1}", ExitCode, StandardError.Trim()));
                return false;
            }
            log.Fatal(StandardError += " <--is some crazy shit that happend and I failed to process it.");
            return false;
            
                
        }

        public bool PSLoggedon(string arguments)
        {
            Program = "psloggedon.exe";

            //Remotely enable required serice
            RemoteRegistry("start", IP, "remoteRegistry");
            execute(Px_Arguments(arguments));
            //Remotely disable  required serice
            RemoteRegistry("stop", IP, "remoteRegistry");
            if (StandardError.Contains("error code 0") == true)
            {
                return true;
            }
            else
                return false;
        }

        public void RemoteRegistry(string status, string ip, string service)
        {
            //remote regestry service
            //System.Windows.MessageBox.Show("service change");
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = (@"c:\windows\sysnative\sc.exe"),
                Arguments = (@"\\" + ip + " " + status + @" " + service),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                //RedirectStandardError = true
            };

            var pProcess = System.Diagnostics.Process.Start(startInfo);
            string outPut = pProcess.StandardOutput.ReadToEnd();
            //string standardError = RR.StandardError.ReadToEnd();
            pProcess.WaitForExit();
            //System.Windows.MessageBox.Show(outPut);
            //System.Windows.MessageBox.Show(outPut);
            //System.Diagnostics.Process.Start(@"c:\windows\sysnative\sc.exe", @"\\" + ip + @" start RemoteRegistry");
        }

        public void Copy(string From, string To)
        {
            To = string.Format(@"\\{0}\c${1}", IP, To);
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(From);
            //detect whether its a directory or file
            log.Debug(string.Format("Copy command was [ {0} ] To [ {1} ]", From, To));
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DirectoryCopy(From, To, true);
                
            }
            else
                //MessageBox.Show("Its a file");
                FileCopy(From, To);
        }

        private void FileCopy(string sourcePath, string targetPath)
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
            if (source.LastWriteTime > dest.LastWriteTime)
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
                    return;
                }
            }
            //delete
            else
                log.Debug(string.Format(@"Skipping: '{0}' File matches current version", dest.FullName));
            
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
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
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private string Px_Arguments(string argument)
        {
            var result = string.Format(@"\\{0} {1}", IP, argument);
            log.Debug(string.Format("Built P(x) argument [ {0} ]", result));
            return result;
        }

        public void GenericProcess(string program, string arguments)
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = program,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                //RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            log.Debug(string.Format("Executing [ {0} {1} ]", Program, arguments));

            var pProcess = System.Diagnostics.Process.Start(startInfo);
            StandardOutput = pProcess.StandardOutput.ReadToEnd();
            StandardError = pProcess.StandardError.ReadToEnd();
            pProcess.WaitForExit();
            log.Debug("StandardOutput:");
            log.Debug(StandardOutput);
            log.Debug("StandardError:");
            log.Debug(StandardError);
        }

        public bool IPv4_Check()
        {
            var quads = IP.Split('.');

            // if we do not have 4 quads, return false
            if (!(quads.Length == 4))
            {
                log.Error("Invalid IPV4 Address");
                return false;
            }

            // for each quad
            foreach (var quad in quads)
            {
                int q;
                // if parse fails 
                // or length of parsed int != length of quad string (i.e.; '1' vs '001')
                // or parsed int < 0
                // or parsed int > 255
                // return false
                if (!System.Int32.TryParse(quad, out q)
                    || !q.ToString().Length.Equals(quad.Length)
                    || q < 0
                    || q > 255) { log.Error("Invalid IPV4 Address"); return false; }
            }
            log.Info("Valid IPV4");
            return true;
        }

        public bool PingTest()
        {

            System.Net.NetworkInformation.Ping pingClass = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingReply pingReply = pingClass.Send(IP);

            //if ip is valid run checks else
            if (pingReply.Status == System.Net.NetworkInformation.IPStatus.Success)
            { log.Info("Successfully pinged IP"); return true; }
            else
            { log.Error("Ping Failed"); return false; }

        }

    }
}
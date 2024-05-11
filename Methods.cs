using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;

namespace MCenters
{
       class Methods
    {
        public class ProgressEventArgs : EventArgs
        {
            public ProgressEventArgs(string status, double progress)
            {
                Status = status;
                Progress = progress;
            }
            public string Status { get; set; }
            public double Progress { get; set; }
        }
        public class Method

        {
            public static event EventHandler<ProgressEventArgs> ProgressChanged;
            static private protected void ReportProgress(string status, double progress)
            {
                ProgressChanged?.Invoke(null, new ProgressEventArgs(status, progress));
            }


            public void Close()
            {
                logWriter.Close();
            }

            public MCentersFileStream logWriter;
            public static string CommonLog = "C:\\ProgramData\\MCenters\\Logs\\";

            
            public static string LogFileName="log.txt";
            public static MCentersFileStream CommonLogWriter;
            public string LogPath { get; set; }
            public static string Dllx64URL = "https://raw.githubusercontent.com/tinedpakgamer/mcenterdlls/main/{identity}/x64/Windows.ApplicationModel.Store.dll";

            public static string Dllx86URL = "https://raw.githubusercontent.com/tinedpakgamer/mcenterdlls/main/{identity}/x86/Windows.ApplicationModel.Store.dll";

            public static WebClient client = new WebClient();
            public static string LogDirectory = "C:\\ProgramData\\MCenters\\Logs\\";
            public static string ClipboardFolder = "C:\\ProgramData\\MCenters\\Clipboard\\";
            public static string baseDllPath = "C:\\ProgramData\\MCenters\\Methods\\Dll";
            public static string baseExePath = "C:\\ProgramData\\MCenters\\Methods\\Exe";
            public static string Dllx64 = "C:\\Windows\\System32\\Windows.ApplicationModel.Store.dll";
            public static string Dllx86 = "C:\\Windows\\SysWOW64\\Windows.ApplicationModel.Store.dll";
            public string Version { get; set; }


            public static bool IsUsable()
            {

                return false;
            }
            public static string GetVersion()
            {
                return "";
            }
            public static bool IsAvailable(string version)
            {
                throw new NotImplementedException("Method IsAvailable needs to be implemented in derived classes.");
            }
            public bool IsDownloaded { get; set; }
            public virtual bool Download()
            {
                return true;
            }
            public virtual bool Install()
            {
                return true;
            }





        }
        public class ExeMethod : Method
        {
            public ExeMethod(string version)
            {
                throw new NotImplementedException("Exe Method not implemented");
            }


        }
        public class DllMethod : Method
        {
            public static void Uninstall()
            {
                ReportProgress("Fixing ClipSVC", 0);
                UninstallClipSVC();
                if (Environment.Is64BitProcess)
                    ReportProgress("Fixing x64 Dll", 25);
                else
                    ReportProgress("Fixing Dll", 50);
                Uninstall("C:\\Windows\\System32\\sfc.exe", "C:\\Windows\\System32\\Windows.ApplicationModel.Store.dll");
                if (Environment.Is64BitProcess)
                {
                    ReportProgress("Fixing x86 Dll", 50);
                    Uninstall("C:\\Windows\\System32\\sfc.exe", "C:\\Windows\\SysWOW64\\Windows.ApplicationModel.Store.dll");
                }
                ReportProgress("Uninstall Successful", 100);


            }


            static void UninstallClipSVC()
            {
                try
                {
                    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\ClipSVC\\Parameters", true);
                    registryKey.SetValue("ServiceDll", "%SystemRoot%\\System32\\ClipSVC.dll", RegistryValueKind.ExpandString);
                    registryKey.Close();
                }
                catch (Exception)
                {

                }
            }
            static void Uninstall(string path, string cmd)
            {
                CommonLogWriter = new MCentersFileStream(CommonLog+LogFileName, FileMode.Append);

                CommonLogWriter.Write(Encoding.ASCII.GetBytes("Started Operation: \tRestoring " + cmd));
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo(path, " /scanfile=" + cmd) { UseShellExecute = false, RedirectStandardOutput = true },
                    EnableRaisingEvents = true
                };
                ; p.OutputDataReceived += P_OutputDataReceived1;

                p.Start();
                p.BeginOutputReadLine();
                p.WaitForExit();
                CommonLogWriter.Write(Encoding.ASCII.GetBytes("Ended Operation: \tRestoring " + cmd));
                CommonLogWriter.Close();

            }

            private static void P_OutputDataReceived1(object sender, DataReceivedEventArgs e)
            {
                CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nOutput Recieved:\t" + e.Data));
            }




            public DllMethod(string DllVersion)
            {

                ReportProgress("Starting Mod Installation " + DllVersion, 16.66);
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);
                logRetry:;
                LogPath = LogDirectory + LogFileName;
                try
                {
                    logWriter = new MCentersFileStream(LogPath, FileMode.Append);
                }
                catch(IOException) {
                    LogFileName = DateTime.Now.ToString("dddd_d_MMMM_yyyy hh_mm_ss_tt").Replace(':','_') + ".txt";
                    goto logRetry;
                }
                Version = DllVersion;
                if (Directory.Exists(baseDllPath))
                {
                    IsDownloaded = false;
                    var baseDirInfo = new DirectoryInfo(baseDllPath);
                    foreach (var dirInfo in baseDirInfo.EnumerateDirectories())
                    {
                        if (dirInfo.Name == DllVersion)
                        {

                            IsDownloaded = true;
                            break;
                        }
                    }

                }
                else
                {

                    Directory.CreateDirectory(baseDllPath);
                    IsDownloaded = false;

                }
            }

            public static new bool IsAvailable(string version)
            {
                ReportProgress("Checking  Mod Support for " + version, 8.33);
                CommonLogWriter = new MCentersFileStream(CommonLog+LogFileName, FileMode.Append);

                CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nStarting Operation:  Is Dll mode available for " + version));
                if (Directory.Exists(baseDllPath))
                {

                    var baseDirInfo = new DirectoryInfo(baseDllPath);
                    foreach (var dirInfo in baseDirInfo.EnumerateDirectories())
                    {

                        if (dirInfo.Name == version)
                        {
                            CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nA downloaded version was found " + dirInfo.FullName));
                            CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nEnded Operation:  Is Dll mode available for " + version));
                            CommonLogWriter.Close();
                            return true;

                        }
                    }

                }
                else
                    Directory.CreateDirectory(baseDllPath);

                CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nChecking Online"));


                if (File.Exists(baseDllPath + "\\Records.txt"))

                    File.Delete(baseDllPath + "\\Records.txt");

                try
                {

                    client.DownloadFile("https://raw.githubusercontent.com/tinedpakgamer/mcenterdlls/main/main", baseDllPath + "\\Records.txt");

                    CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nFile Downloaded"));
                }
                catch (WebException)
                {
                    CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nError Occured while downloading dll records"));
                }


                var rawContent = File.ReadAllText(baseDllPath + "\\Records.txt");
                var rawContentCopy = rawContent;
                for (int index = 0; index < 10; ++index)
                    rawContentCopy = rawContentCopy.Replace(index.ToString(), "");
                rawContentCopy = rawContentCopy.Replace(".", "");
                rawContentCopy = rawContentCopy.Replace(",", "");
                var uselessCharArray = rawContentCopy.ToCharArray();
                foreach (var uselessChar in uselessCharArray)
                {

                    rawContent = rawContent.Replace(uselessChar.ToString(), "");
                }
                var Records = rawContent.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var entry in Records)
                {

                    if (entry == version)
                    {
                        CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nThe requested version is available online"));
                        CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nEnded Operation:  Is Dll mode available for " + version));
                        CommonLogWriter.Close();
                        return true;
                    }
                }
                CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nUnsupported Version"));
                CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nEnded Operation:  Is Dll mode available for " + version));
                CommonLogWriter.Close();
                return false;


            }
            public static new string GetVersion()
            {

                ReportProgress("Fetching System Dll version", 0);
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);
                CommonLogWriter = new MCentersFileStream(CommonLog + LogFileName, FileMode.Append);

                CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nStarting Operation:  Checking Version of System Dlls "));
                FileVersionInfo versionInfo;
                if (File.Exists(Dllx64))
                    versionInfo = FileVersionInfo.GetVersionInfo(Dllx64);
                else if (File.Exists(Dllx86))
                    versionInfo = FileVersionInfo.GetVersionInfo(Dllx86);
                else return "22000.348";
                var k = versionInfo.FileBuildPart.ToString() + "." + versionInfo.FilePrivatePart.ToString();
                CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nDetected " + k));
                CommonLogWriter.Write(Encoding.ASCII.GetBytes("\nEnded Operation:  Checking Version of System Dlls "));
                CommonLogWriter.Close();
                return k;

            }
            public static new bool IsUsable()
            {

                return true;
            }

            public override bool Download()
            {
                ReportProgress("Downloading " + Version, 25);
                logWriter.Write(Encoding.ASCII.GetBytes("\nStarting Operation: \tDownload of version " + Version));
                logWriter.Flush();
                if (Directory.Exists(baseDllPath + "\\" + Version))
                    Directory.Delete(baseDllPath + "\\" + Version);

                Directory.CreateDirectory(baseDllPath + "\\" + Version + "\\x64");
                Directory.CreateDirectory(baseDllPath + "\\" + Version + "\\x86");
                var x64DllDownload = Dllx64URL.Replace("{identity}", Version);

                var x86DllDownload = Dllx86URL.Replace("{identity}", Version);
                try
                {
                    client.Proxy = WebRequest.DefaultWebProxy;

                    client.DownloadFile(x64DllDownload, baseDllPath + "\\" + Version + "\\x64\\Windows.ApplicationModel.Store.dll");
                    client.DownloadFile(x86DllDownload, baseDllPath + "\\" + Version + "\\x86\\Windows.ApplicationModel.Store.dll");
                    logWriter.Write(Encoding.ASCII.GetBytes("\nFiles Downloaded"));
                    logWriter.Flush();
                }
                catch (WebException)
                {
                    logWriter.Write(Encoding.ASCII.GetBytes("\nFiles failed to download"));
                    logWriter.Flush();
                    return false;
                }

                return true;
            }


            private void TakePermissions(string path)
            {
                logWriter.Write(Encoding.ASCII.GetBytes("\nStarting Operation: \tTaking Permissions of " + path));
                logWriter.Flush();
                if (File.Exists(path))
                {
                    var Info = new ProcessStartInfo("cmd.exe", @"/k takeown /f " + path + @" && icacls " + path + " /grant " + "\"" + Environment.UserName + "\"" + ":F")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        Verb = "runas"
                    };
                    Process p = new Process
                    {
                        StartInfo = Info,
                        EnableRaisingEvents = true
                    };

                    p.OutputDataReceived += PermissionTakingOutput;
                    p.ErrorDataReceived += PermissionTakingError;

                    p.Start();
                    logWriter.Write(Encoding.ASCII.GetBytes("\nProcess Started"));
                    logWriter.Flush();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();

                    logWriter.Write(Encoding.ASCII.GetBytes("\nEnded Operation: \tTaking Permissions of" + path));
                    logWriter.Flush();



                }
            }

            private void PermissionTakingError(object sender, DataReceivedEventArgs e)
            {
                logWriter.Write(Encoding.ASCII.GetBytes("\nError Recieved:\t" + e.Data));
                logWriter.Flush();
            }

            private void PermissionTakingOutput(object sender, DataReceivedEventArgs e)
            {
                logWriter.Write(Encoding.ASCII.GetBytes("\nOutput Recieved:\t" + e.Data));
                logWriter.Flush();
                if (String.IsNullOrWhiteSpace(e.Data))
                    return;
                if (e.Data == Environment.CurrentDirectory)
                {

                    (sender as Process).Kill();
                    return;

                }

                if (e.Data.Contains("Failed processing 0 files"))
                {
                    (sender as Process).Kill();

                }
                if (e.Data == Environment.CurrentDirectory)
                    return;

            }
            private bool VerifyPermission(string path)
            {
                logWriter.Write(Encoding.ASCII.GetBytes("\nStarting Operation: \tVerify Permissions of " + path));
                logWriter.Flush();
                if (!File.Exists(path))
                    return true;

                var accessControl = new FileInfo(path).GetAccessControl();
                var fileOwner = accessControl.GetOwner(typeof(NTAccount)).Value;

                var userDomain = Environment.UserDomainName;
                var userName = Environment.UserName;
                if (fileOwner == userDomain + "\\" + userName)
                {


                    logWriter.Write(Encoding.ASCII.GetBytes("\nFile is Owned\nEnded Operation: \tVerify Permissions of " + path));
                    logWriter.Flush();
                    return true;
                }

                logWriter.Write(Encoding.ASCII.GetBytes("\nFile is not Owned\nEnded Operation: \tVerify Permissions of " + path));
                logWriter.Flush();
                return false;
            }
            bool Delete(string path)
            {

                logWriter.Write(Encoding.ASCII.GetBytes("\nStarting Operation: \tDelete " + path));
                logWriter.Flush();
                try
                {

                    Process[] p = Process.GetProcesses();
                    foreach (var item in p)
                    {

                        try
                        {
                            if (!item.HasExited)
                            {
                                foreach (var i in item.Modules)
                                {
                                    ProcessModule o = (ProcessModule)i;
                                    if (o.FileName == path)
                                    {
                                        logWriter.Write(Encoding.ASCII.GetBytes("\nEnded Process " + item.ProcessName));
                                        logWriter.Flush();
                                        item.Kill();
                                    }
                                }
                            }
                        }
                        catch (System.ComponentModel.Win32Exception)
                        {
                            logWriter.Write(Encoding.ASCII.GetBytes("\nUnable to end Process " + item.ProcessName));
                            logWriter.Flush();
                        }
                    }
                    logWriter.Write(Encoding.ASCII.GetBytes("\nProcess Ending Completed"));
                    logWriter.Flush();
                    ;
                    File.Delete(path);

                    logWriter.Write(Encoding.ASCII.GetBytes("\nFile Deleted\nEnded Operation: \tDelete " + path));
                    logWriter.Flush();
                    return true;
                }
                catch (UnauthorizedAccessException)
                {

                }
                catch (IOException)
                {
                }
                catch (ArgumentException)
                {
                }
                return false;

            }
            void Replace(string Path, bool Is64)
            {


                logWriter.Write(Encoding.ASCII.GetBytes("\nStarting Operation: \tReplace file " + Path + "   with x64 Mode: " + Is64));
                logWriter.Flush();
                if (Is64)
                {
                    File.Copy(baseDllPath + "\\" + Version + "/x64/Windows.ApplicationModel.Store.dll", Path);

                    logWriter.Write(Encoding.ASCII.GetBytes("\nFile Replaced\nEnded Operation: \tReplace file " + Path + "   with x64 Mode: " + Is64));
                    logWriter.Flush();
                    return;
                }
                File.Copy(baseDllPath + "\\" + Version + "/x86/Windows.ApplicationModel.Store.dll", Path);

                logWriter.Write(Encoding.ASCII.GetBytes("\nFile Replaced\nEnded Operation: \tReplace file " + Path + "   with x64 Mode: " + Is64));
                logWriter.Flush();
            }

            public override bool Install()
            {
                int i = 0;
                bool Is64 = Environment.Is64BitProcess;
                if (IsDownloaded)
                    i = -1;
                ReportProgress("Taking permission from System32", Is64 ? ((4.0 + i) / 12.0) * 100 : ((4 + i) / 8) * 100);
                TakePermissions(Dllx64);
                ReportProgress("Verifying permission from System32", Is64 ? ((5.0 + i) / 12.0) * 100 : ((5 + i) / 8) * 100);
                VerifyPermission(Dllx64);
                ReportProgress("Deleting a file from System32", Is64 ? ((6.0 + i) / 12.0) * 100 : ((6 + i) / 8) * 100);
                Delete(Dllx64);
                ReportProgress("Replacing a file in System32", Is64 ? ((7.0 + i) / 12.0) * 100 : ((7 + i) / 8) * 100);
                Replace(Dllx64, Environment.Is64BitProcess);

                if (Environment.Is64BitProcess)
                {
                    ReportProgress("Taking permission from SysWoW64", ((8.0 + i) / 12.0) * 100);
                    TakePermissions(Dllx86);
                    ReportProgress("Verifying permission from SysWoW64", ((9.0 + i) / 12.0) * 100);
                    VerifyPermission(Dllx86);
                    ReportProgress("Deleting a file from SysWoW64", ((10.0 + i) / 12.0) * 100);
                    Delete(Dllx86);
                    ReportProgress("Replacing a file in SysWoW64", ((11.0 + i) / 12.0) * 100);
                    Replace(Dllx86, false);

                }
                ReportProgress("Mod Installed", 100);
                return true;
            }
        }
    }
}

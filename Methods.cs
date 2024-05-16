using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
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


            public static string ClipboardFolder = "C:\\ProgramData\\MCenters\\Clipboard\\";

            

            public static WebClient client = new WebClient();
            
            
            
            
            


            






        }
        public class ExeMethod : Method
        {
            public static string baseExePath = "C:\\ProgramData\\MCenters\\Methods\\Exe";
            public ExeMethod(string version)
            {
                throw new NotImplementedException("Exe Method not implemented");
            }


        }
        public class DllMethod : Method
        {
            public static string Dllx64URL = "https://raw.githubusercontent.com/tinedpakgamer/mcenterdlls/main/{identity}/x64/Windows.ApplicationModel.Store.dll";

            public static string Dllx86URL = "https://raw.githubusercontent.com/tinedpakgamer/mcenterdlls/main/{identity}/x86/Windows.ApplicationModel.Store.dll";

            public static string baseDllPath = "C:\\ProgramData\\MCenters\\Methods\\Dll";
            public static string Dllx64 = "C:\\Windows\\System32\\Windows.ApplicationModel.Store.dll";
            public static string Dllx86 = "C:\\Windows\\SysWOW64\\Windows.ApplicationModel.Store.dll";
            public string Version { get; set; }
            public bool IsDownloaded { get; private set; }
            public static void Uninstall()
            {
                ReportProgress("Fixing ClipSVC", 0);
                UninstallClipSVC();
                if (Environment.Is64BitProcess)
                    ReportProgress("Fixing x64 Dll", 25);
                else
                    ReportProgress("Fixing Dll", 50);
                SfcFileScan("C:\\Windows\\System32\\Windows.ApplicationModel.Store.dll");
                if (Environment.Is64BitProcess)
                {
                    ReportProgress("Fixing x86 Dll", 50);
                    SfcFileScan("C:\\Windows\\SysWOW64\\Windows.ApplicationModel.Store.dll");
                }
                ReportProgress("Uninstall Successful", 100);


            }


            static void UninstallClipSVC()
            {
                
                    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\ClipSVC\\Parameters", true);
                    registryKey.SetValue("ServiceDll", "%SystemRoot%\\System32\\ClipSVC.dll", RegistryValueKind.ExpandString);
                    registryKey.Close();
                
            }
            static void SfcFileScan(string fileName)
            {
              var  sfcPath = "C:\\Windows\\System32\\sfc.exe";

                Logger.StartOperation("Restoring " + fileName);
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo(sfcPath, " /scanfile=" + fileName) { UseShellExecute = false, RedirectStandardOutput = true, StandardOutputEncoding=Encoding.Unicode },
                    EnableRaisingEvents = true,
                    
                };
                p.OutputDataReceived += (sender, e)=>
                {
                    if (string.IsNullOrWhiteSpace(e.Data)) return;
                    var asciiData = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(e.Data));
                    Logger.Write("Output Recieved:\t" + asciiData);
                };
               
                p.Start();
                p.BeginOutputReadLine();
                p.WaitForExit();
                Logger.CompleteOperation("Restoring " + fileName);
                

            }

            




            public DllMethod(string DllVersion)
            {

                ReportProgress("Starting Mod Installation " + DllVersion, 16.66);
                
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

            public static bool IsAvailable(string version)
            {
                ReportProgress("Checking  Mod Support for " + version, 8.33);
                

                Logger.StartOperation("Is Dll mode available for " + version);
                if (Directory.Exists(baseDllPath))
                {

                    var baseDirInfo = new DirectoryInfo(baseDllPath);
                    foreach (var dirInfo in baseDirInfo.EnumerateDirectories())
                    {

                        if (dirInfo.Name == version)
                        {
                            Logger.Write("A downloaded version was found " + dirInfo.FullName);
                            Logger.CompleteOperation("Is Dll mode available for " + version);
                            
                            return true;

                        }
                    }

                }
                else
                    Directory.CreateDirectory(baseDllPath);

                Logger.Write("Checking Online");


                if (File.Exists(baseDllPath + "\\Records.txt"))

                    File.Delete(baseDllPath + "\\Records.txt");

                try
                {

                    client.DownloadFile("https://raw.githubusercontent.com/tinedpakgamer/mcenterdlls/main/main", baseDllPath + "\\Records.txt");

                    Logger.Write("File Downloaded");
                }
                catch (WebException)
                {
                    Logger.Write("Error Occured while downloading dll records");
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
                        Logger.Write("The requested version is available online");
                        Logger.CompleteOperation("Is Dll mode available for " + version);
                        
                        return true;
                    }
                }
                Logger.Write("Unsupported Version");
                Logger.CompleteOperation("Is Dll mode available for " + version);
                
                return false;


            }
            public static  string GetVersion()
            {

                ReportProgress("Fetching System Dll version", 0);
                
                

                Logger.StartOperation("Checking Version of System Dlls ");
                FileVersionInfo versionInfo;
                if (File.Exists(Dllx64))
                    versionInfo = FileVersionInfo.GetVersionInfo(Dllx64);
                else if (File.Exists(Dllx86))
                    versionInfo = FileVersionInfo.GetVersionInfo(Dllx86);
                else return "22000.348";
                var k = versionInfo.FileBuildPart.ToString() + "." + versionInfo.FilePrivatePart.ToString();
                Logger.Write("Detected " + k);
                Logger.CompleteOperation("Checking Version of System Dlls ");
                
                return k;

            }
          

            public  bool Download()
            {
                ReportProgress("Downloading " + Version, 25);
                Logger.StartOperation("Download of version " + Version);
                
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
                    Logger.Write("Files Downloaded");
                    Logger.CompleteOperation("Download of version " + Version);

                }
                catch (WebException)
                {
                    Logger.Write("Files failed to download");
                    Logger.CompleteOperation("Download of version " + Version);

                    return false;
                }

                return true;
            }


            private void TakePermissions(string path)
            {
                Logger.StartOperation("Taking Permissions of " + path);
                                    
                if (File.Exists(path))
                {
                    var executor = "takeown.exe";
                    var arguments= $"/f {path}";
                takeperms:;
                    var icaclsCmd = $"icacls {path} /grant \"{Environment.UserName}\":F";
                    var Info = new ProcessStartInfo(executor, arguments)
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

                    p.OutputDataReceived += (sender, e)=>
                    {


                        if (String.IsNullOrWhiteSpace(e.Data))
                            return;

                        Logger.Write("Output Recieved:\t" + e.Data);
                    };
                    p.ErrorDataReceived += (sender,e)=>
                    {
                        if (String.IsNullOrWhiteSpace(e.Data))
                            return;

                        Logger.Write("Error Recieved:\t" + e.Data);

                    };

                    p.Start();
                    Logger.Write($"Process Started:\t{executor} {arguments}");
                    
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                    {
                        throw new MCenterException($"Failed executing {executor} {arguments}\nProcess Exit Code: {p.ExitCode}");
                    }
                    if (executor == "takeown.exe")
                    {
                        executor = $"icacls.exe";
                        arguments = $"{path} /grant \"{Environment.UserName}\":F";
                        goto takeperms;
                    }
                    Logger.CompleteOperation("Taking Permissions of " + path);
                    



                }
            }

            private bool VerifyPermission(string path)
            {
                Logger.StartOperation("Verify Permissions of " + path);
                
                if (!File.Exists(path))
                    return true;

                var accessControl = new FileInfo(path).GetAccessControl();
                var fileOwner = accessControl.GetOwner(typeof(NTAccount)).Value;

                var userDomain = Environment.UserDomainName;
                var userName = Environment.UserName;
                if (fileOwner == userDomain + "\\" + userName)
                {


                    Logger.Write("File is Owned");
                    Logger.CompleteOperation("Verify Permissions of " + path);
                    
                    return true;
                }

                Logger.Write("File is not Owned");
                Logger.CompleteOperation("Verify Permissions of " + path);

                return false;
            }
            bool Delete(string path)
            {

                Logger.StartOperation("Delete " + path);
                
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
                                        Logger.Write("Ended Process " + item.ProcessName);
                                        
                                        item.Kill();
                                    }
                                }
                            }
                        }
                        catch (System.ComponentModel.Win32Exception)
                        {
                            Logger.Write("Unable to end Process " + item.ProcessName);
                            
                        }
                    }
                    Logger.Write("Process Ending Completed");
                    
                    ;
                    File.Delete(path);

                    Logger.Write("File Deleted");
                    Logger.CompleteOperation("Delete " + path);
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
                Logger.CompleteOperation("Delete " + path);
                return false;

            }
            void Replace(string Path, bool Is64)
            {


                Logger.StartOperation("Replace file " + Path + "   with x64 Mode: " + Is64);
                
                if (Is64)
                {
                    File.Copy(baseDllPath + "\\" + Version + "/x64/Windows.ApplicationModel.Store.dll", Path);

                    Logger.Write("File Replaced");
                    Logger.CompleteOperation("Replace file " + Path + "   with x64 Mode: " + Is64);
                    
                    return;
                }
                File.Copy(baseDllPath + "\\" + Version + "/x86/Windows.ApplicationModel.Store.dll", Path);

                Logger.Write("File Replaced");
                Logger.CompleteOperation("Replace file " + Path + "   with x64 Mode: " + Is64);
            }

            public  bool Install()
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

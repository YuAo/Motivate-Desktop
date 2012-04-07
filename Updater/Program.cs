using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace Updater
{
    static class Program
    {
        static string UpdateTargetFolder;
        static string UpdateAppPath;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 2) return;

            UpdateTargetFolder = args[0];
            UpdateAppPath = args[1];

            Update();

            Application.Run();
        }


        static void Update()
        {
            System.Timers.Timer timer = new System.Timers.Timer(300);
            timer.AutoReset = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(checkIfUpdateAppExited);
            timer.Start();
            checkIfUpdateAppExited(timer, null);
        }

        static void checkIfUpdateAppExited(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (killAppThatNeedsUpdate(UpdateAppPath))
            {
                ((System.Timers.Timer)sender).Stop();
                InstallUpdate();
            }
        }

        static void InstallUpdate()
        {
            UnzipUpdateTo(UpdateTargetFolder);
            try
            {
                System.IO.File.Delete(System.IO.Path.Combine(Application.StartupPath, "Update.zip"));
            }
            catch { }
            System.Diagnostics.Process.Start(UpdateAppPath);
            Application.Exit();
        }

        static bool killAppThatNeedsUpdate(string path)
        {
            bool killed = true;

            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                try
                {
                    if (process.MainModule.FileName != null)
                    {
                        if (process.MainModule.FileName.ToLower() == path.ToLower())
                        {
                            killed = false;
                            if (!process.CloseMainWindow())
                            {
                                process.Kill();
                            }
                        }
                    }
                }
                catch { }
            }
            return killed;
        }

        static void UnzipUpdateTo(string targerFolder)
        {
            try
            {
                Shell32.ShellClass sc = new Shell32.ShellClass();
                Shell32.Folder SrcFolder = sc.NameSpace(System.IO.Path.Combine(Application.StartupPath,"Update.zip"));
                Shell32.Folder DestFolder = sc.NameSpace(targerFolder);
                Shell32.FolderItems items = SrcFolder.Items();
                DestFolder.CopyHere(items, 20);
            }
            catch
            {
                return;
            }
        }
    }
}

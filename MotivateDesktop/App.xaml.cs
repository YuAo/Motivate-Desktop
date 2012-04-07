using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;

namespace MotivateDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Process OtherRunningInstance()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            foreach (Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    return process;
                }
            }
            return null;
        }

        private static void OnlyAllowOneRunningInstance()
        {
            if (OtherRunningInstance() != null)
            {
                App.Current.Shutdown();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            OnlyAllowOneRunningInstance();
            Updater.Instance().CheckAndInstallCachedUpdate();
            Updater.Instance().CheckUpdateAsync();
        }
    }
}

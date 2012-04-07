using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace MotivateDesktop
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private bool isClosed = false;
        private static SettingsWindow instance = null;
        public static SettingsWindow Instance()
        {
            if (instance == null || instance.isClosed)
            {
                instance = new SettingsWindow();
            }
            return instance;
        }

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            isClosed = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left) this.DragMove();
            }
            catch { };
        }

        private void image_close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void image_close_MouseEnter(object sender, MouseEventArgs e)
        {
            image_close.Opacity = 0.5;
            label_esc.Visibility = Visibility.Visible;
        }

        private void image_close_MouseLeave(object sender, MouseEventArgs e)
        {
            image_close.Opacity = 0.3;
            label_esc.Visibility = Visibility.Hidden;
        }

        private bool isAppAutorun
        {
            get
            {
                RegistryKey HKLM = Registry.CurrentUser;
                RegistryKey Run = HKLM.CreateSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\");
                Object path = Run.GetValue(System.Windows.Forms.Application.ProductName);
                HKLM.Close();
                if (path != null)
                {
                    if (path.ToString() != "\"" + System.Windows.Forms.Application.ExecutablePath + "\"")
                    {
                        HKLM = Registry.CurrentUser;
                        Run = HKLM.CreateSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\");
                        Run.SetValue(System.Windows.Forms.Application.ProductName, "\"" + System.Windows.Forms.Application.ExecutablePath + "\"");
                        HKLM.Close();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value != isAppAutorun)
                {
                    RegistryKey HKLM = Registry.CurrentUser;
                    RegistryKey Run = HKLM.CreateSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\");
                    if (value == true)
                    {
                        try
                        {
                            Run.SetValue(System.Windows.Forms.Application.ProductName, "\"" + System.Windows.Forms.Application.ExecutablePath + "\"");
                            HKLM.Close();
                        }
                        catch (Exception Err)
                        {
                            MessageBox.Show(Err.Message.ToString(), "设置为开机启动时项出错", MessageBoxButton.OK, MessageBoxImage.Warning);
                            checkbox_autorun.IsChecked = isAppAutorun;
                        }
                    }
                    else if (value == false)
                    {
                        try
                        {
                            Run.DeleteValue(System.Windows.Forms.Application.ProductName);
                            HKLM.Close();
                        }
                        catch (Exception Err)
                        {
                            MessageBox.Show(Err.Message.ToString(), "删除开机启动时项出错", MessageBoxButton.OK, MessageBoxImage.Warning);
                            checkbox_autorun.IsChecked = isAppAutorun;
                        }
                    }
                }
            }
        }

        private void loadSettings()
        {
            checkbox_autorun.IsChecked = isAppAutorun;
            checkbox_autoexit.IsChecked = MotivateDesktop.Properties.Settings.Default.AutoExit;
        }

        private void saveSettings()
        {
            isAppAutorun = checkbox_autorun.IsChecked.Value;
            MotivateDesktop.Properties.Settings.Default.AutoExit = checkbox_autoexit.IsChecked.Value;
            MotivateDesktop.Properties.Settings.Default.Save();
        }

        private void checkbox_autorun_Click(object sender, RoutedEventArgs e)
        {
            saveSettings();
        }

        private void checkbox_autoexit_Click(object sender, RoutedEventArgs e)
        {
            saveSettings();
            BackgroundWindow.setAutoexitTimerEnabled(MotivateDesktop.Properties.Settings.Default.AutoExit);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadSettings();
            textBlock_copyrightInfo.Text = "Motivate Desktop "+ App.ResourceAssembly.GetName().Version.ToString(3) +" © YuAo 2012";
            Updater.Instance().StatusUpdated += new Updater.StatusUpdatedEventHandler(delegate { checkUpdaterStatus(); });
            if (Updater.Instance().CurrentStatus == Updater.UpdaterStatus.Failed)
            {
                Updater.Instance().CheckUpdateAsync();
            }
            checkUpdaterStatus();
        }

        private void button_downloadWallpaperPackage_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(MotivateDesktopUtility.WallpaperPackageDownloadUrl);
        }

        private void button_openSource_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(MotivateDesktopUtility.GitHubProjectPageUrl);
        }

        private void checkUpdaterStatus()
        {
            switch (Updater.Instance().CurrentStatus)
            {
                case Updater.UpdaterStatus.Checking:
                    textBlock_update.Text = "正在检查更新...";
                    break;
                case Updater.UpdaterStatus.Downloading:
                    textBlock_update.Text = "正在下载更新 " + Updater.Instance().NewVersion + " >> " + Updater.Instance().CurrentDownloadProgress +"%...";
                    break;
                case Updater.UpdaterStatus.Failed:
                    textBlock_update.Text = "获取更新信息失败.";
                    break;
                case Updater.UpdaterStatus.Idle:
                    Updater.Instance().CheckUpdateAsync();
                    break;
                case Updater.UpdaterStatus.IsUpToDate:
                    textBlock_update.Text = "Motivate Desktop 已是最新版.";
                    break;
                case Updater.UpdaterStatus.ReadyToUpdate:
                    textBlock_update.Text = "已经准备好更新，将在软件下次启动时安装.";
                    break;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}

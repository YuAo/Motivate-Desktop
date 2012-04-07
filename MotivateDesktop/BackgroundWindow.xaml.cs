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

namespace MotivateDesktop
{
    /// <summary>
    /// Interaction logic for BackgroundWindow.xaml
    /// </summary>
    public partial class BackgroundWindow : Window
    {
        public static bool ShouldForceShowWallpaperPreviewWindow;

        private static System.Timers.Timer autoexitTimer = null;
        public static void setAutoexitTimerEnabled(bool enable)
        {
            if(enable){
                if (autoexitTimer != null)
                {
                    autoexitTimer.Stop();
                    autoexitTimer = null;
                }
                autoexitTimer = new System.Timers.Timer();
                autoexitTimer.Interval = 5 * 60 * 1000;
                autoexitTimer.AutoReset = false;
                autoexitTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate { ExitApp(); });
                autoexitTimer.Start();
            }else{
                if (autoexitTimer != null)
                {
                    autoexitTimer.Stop();
                    autoexitTimer = null;
                }
            }
        }

        public static void ExitApp()
        {
            App.Current.Dispatcher.Invoke(new Action(
                delegate
                {
                    MotivateDesktopNotifyIcon.Instance().Hide();
                    App.Current.Shutdown();
                }
                ), null);
        }

        public BackgroundWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MotivateDesktopNotifyIcon.Instance();
            WallpaperChecker.Instance().GotWallpaperPreview += new WallpaperChecker.GotWallpaperPreviewEventHandler(GotWallpaperPreview);
            WallpaperChecker.Instance().FailedGettingWallpaperPreview += new WallpaperChecker.FailedGettingWallpaperPreviewEventHandler(FailedGettingWallpaperPreview);
            checkWallpaper();

            System.Timers.Timer checkWallpaperTimer = new System.Timers.Timer(15 * 60 * 1000);
            checkWallpaperTimer.Elapsed += new System.Timers.ElapsedEventHandler(checkWallpaperTimer_Elapsed);
            checkWallpaperTimer.Start();

            setAutoexitTimerEnabled(MotivateDesktop.Properties.Settings.Default.AutoExit);
        }

        private void GotWallpaperPreview(object sender, string wallpaperPreviewLocalCachePath, bool isNew)
        {
            this.Dispatcher.Invoke(
                new Action(
                    delegate
                    {
                        if (isNew || ShouldForceShowWallpaperPreviewWindow)
                        {
                            WallpaperPreviewWindow.Instance().WallpaperFilePath = wallpaperPreviewLocalCachePath;
                            WallpaperPreviewWindow.Instance().Show();
                        }
                    }
                ), null);
            ShouldForceShowWallpaperPreviewWindow = false;
        }

        private void FailedGettingWallpaperPreview(object sender)
        {
            if (ShouldForceShowWallpaperPreviewWindow)
            {
                MotivateDesktopNotifyIcon.Instance().ShowBalloonTip("出错了", "检查壁纸更新时出错.");
            }
            ShouldForceShowWallpaperPreviewWindow = false;
        }

        private void checkWallpaperTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!WallpaperChecker.Instance().IsChecking)
            {
                checkWallpaper();
            }
        }

        private void checkWallpaper()
        {
            System.Threading.Thread checkWallpaperThread = new System.Threading.Thread(new System.Threading.ThreadStart(
                delegate
                {
                    WallpaperChecker.Instance().BeginCheckWallpaper();
                }
                ));
            checkWallpaperThread.Start();
        }
    }
}

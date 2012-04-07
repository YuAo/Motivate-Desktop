using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MotivateDesktop
{
    class MotivateDesktopNotifyIcon
    {
        private MotivateDesktopNotifyIcon() 
        {
            setupNotifyIcon();
        }
        private static MotivateDesktopNotifyIcon instance = null;
        public static MotivateDesktopNotifyIcon Instance()
        {
            if (instance == null)
            {
                instance = new MotivateDesktopNotifyIcon();
            }
            return instance;
        }

        public void ShowBalloonTip(string title, string text)
        {
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = text;
            notifyIcon.ShowBalloonTip(5000);
        }

        private NotifyIcon notifyIcon = new NotifyIcon();
        private void setupNotifyIcon()
        {
            notifyIcon.Icon = MotivateDesktop.Properties.Resources.NotifyIcon;
            notifyIcon.ContextMenuStrip = new ContextMenuStrip();

            ToolStripSeparator separatorMenuItemA = new ToolStripSeparator();
            ToolStripSeparator separatorMenuItemB = new ToolStripSeparator();

            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("退出");
            exitMenuItem.Click += new EventHandler(exitMenuItem_Click);
            
            ToolStripMenuItem settingMenuItem = new ToolStripMenuItem("设置");
            settingMenuItem.Click += new EventHandler(settingMenuItem_Click);

            ToolStripMenuItem latestWallpaperMenuItem = new ToolStripMenuItem("最新壁纸");
            latestWallpaperMenuItem.Click += new EventHandler(latestWallpaperMenuItem_Click);

            ToolStripMenuItem homePageMenuItem = new ToolStripMenuItem("访问主页");
            homePageMenuItem.Image = MotivateDesktop.Properties.Resources.Home;
            homePageMenuItem.Click += new EventHandler(homePageMenuItem_Click);

            notifyIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[]{homePageMenuItem,separatorMenuItemA,latestWallpaperMenuItem,settingMenuItem,separatorMenuItemB,exitMenuItem});
            notifyIcon.Visible = true;
        }

        void latestWallpaperMenuItem_Click(object sender, EventArgs e)
        {
            BackgroundWindow.ShouldForceShowWallpaperPreviewWindow = true;

            ShowBalloonTip("正在检测壁纸", "稍后会有提示...");
            if (!WallpaperChecker.Instance().IsChecking)
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

        void homePageMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(MotivateDesktopUtility.HomePageUrl);
        }

        void settingMenuItem_Click(object sender, EventArgs e)
        {
            SettingsWindow.Instance().Show();
            SettingsWindow.Instance().Activate();
        }

        void exitMenuItem_Click(object sender, EventArgs e)
        {
            BackgroundWindow.ExitApp();
        }

        public void Hide()
        {
            notifyIcon.Visible = false;
        }
    }
}

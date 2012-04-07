using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;

namespace MotivateDesktop
{
    class WallpaperDownloader
    {
        private WallpaperDownloader()
        {
            isDownloading = false;
        }
        private static WallpaperDownloader instance = null;
        public static WallpaperDownloader Instance()
        {
            if (instance == null)
            {
                instance = new WallpaperDownloader();
            }
            return instance;
        }
        public bool IsDownloading
        {
            get
            {
                return isDownloading;
            }
        }
        private bool isDownloading = false;

        public void DownloadAndApplyWallpaper(DateTime date, WallpaperChecker.WallpaperFormats format)
        {
            MotivateDesktopNotifyIcon.Instance().ShowBalloonTip("正在下载", "正在下载壁纸，完成后将自动设置为桌面壁纸.");
            isDownloading = true;
            string wallpaperUrl = getWallpaperUrl(date,format);
            if (wallpaperUrl != null)
            {
                downloadAndApply(wallpaperUrl);
            }
        }

        private void downloadAndApplyWallpaperFailed()
        {
            isDownloading = false;
            MotivateDesktopNotifyIcon.Instance().ShowBalloonTip("出错了", "下载设置壁纸失败.");
        }

        private void downloadAndApplyWallpaperSucceeded()
        {
            isDownloading = false;
        }

        private string getWallpaperUrl(DateTime date, WallpaperChecker.WallpaperFormats format)
        {
            string screenRatioString = "";
            switch (MotivateDesktopUtility.MainScreenRatio)
            {
                case MotivateDesktopUtility.ScreenRatio.SixteenByTen:
                    screenRatioString = "1920x1200";
                    break;
                case MotivateDesktopUtility.ScreenRatio.SixteenByNine:
                    screenRatioString = "1920x1080";
                    break;
                case MotivateDesktopUtility.ScreenRatio.FourByThree:
                    screenRatioString = "1600x1200";
                    break;
                default:
                    screenRatioString = null;
                    downloadAndApplyWallpaperFailed();
                    return null;
            }

            if (format == WallpaperChecker.WallpaperFormats.MISSING)
            {
                downloadAndApplyWallpaperFailed();
                return null;
            }
            string formatString = (format == WallpaperChecker.WallpaperFormats.JPG) ? ".jpg" : ".png";

            string dateString = date.ToString("yyyy.M.d");

            string monthString = date.ToString("yyyy.M");

            string wallpaperUrl = MotivateDesktopUtility.ImgServerUrl + monthString + "/" + dateString + "_" + screenRatioString + formatString;

            return wallpaperUrl;
        }

        private void downloadAndApply(string url)
        {
            string savePath = null;
            try
            {
                WebClient downloader = new WebClient();
                savePath = Path.Combine(MotivateDesktopUtility.WorkingDirectory,"wallpaper"+Path.GetExtension(url));
                downloader.DownloadFile(url,savePath);
            }
            catch
            {
                downloadAndApplyWallpaperFailed();
                return;
            }

            if (System.Environment.OSVersion.Version.Major < 6)
            {
                applyWallpaperXP(savePath);
            }
            else
            {
                applyWallpaperWin7(savePath);
            }
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private void applyWallpaperXP(string path)
        {
          RegistryKey myRegKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop",true);
          myRegKey.SetValue("TileWallpaper","0");
          myRegKey.SetValue("WallpaperStyle","2");
          myRegKey.Close();
          Bitmap bmpWallpaper = new Bitmap(path);
          
          string savePath = "";
          if (System.Environment.OSVersion.Version.Major < 6)
          {
              savePath = Path.Combine(MotivateDesktopUtility.ApplicationDirectory, "DonotDelete-TranscodedWallpaper.bmp");
          }
          else
          {
              savePath = Path.Combine(MotivateDesktopUtility.WorkingDirectory, "DonotDelete-TranscodedWallpaper.bmp");
          }
          bmpWallpaper.Save(savePath, ImageFormat.Bmp);

          SystemParametersInfo(20,1,savePath,1);
          downloadAndApplyWallpaperSucceeded();
        }

        private void applyWallpaperWin7(string path)
        {
            Shell32.Shell shell = new Shell32.ShellClass();
            Shell32.Folder folder = shell.NameSpace(Path.GetDirectoryName(path)) as Shell32.Folder;
            Shell32.FolderItem folderItem = folder.ParseName(System.IO.Path.GetFileName(path));
            Shell32.FolderItemVerbs vs = folderItem.Verbs();

            bool wallpaperSet = false;
            for (int i = 0; i < vs.Count; i++)
            {
                Shell32.FolderItemVerb ib = vs.Item(i);
                
                if (ib.Name.Contains("&b") || ib.Name.Contains("&B"))
                {
                    if (ib.Name.ToLower().Contains("background") || ib.Name.ToLower().Contains("背景"))
                    {
                        wallpaperSet = true;
                        ib.DoIt();
                    }
                }
            }

            if (wallpaperSet == false)
            {
                applyWallpaperXP(path);
            }
            else
            {
                downloadAndApplyWallpaperSucceeded();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace MotivateDesktop
{
    class MotivateDesktopUtility
    {
        public static string HomePageUrl = "http://wordsmotivate.me";
        public static string ImgServerUrl = "http://img.wordsmotivate.me/";
        public static string FormatAPIUrl = "http://api.wordsmotivate.me/WallpaperFormat.php";
        public static string WallpaperPackageDownloadUrl = "http://wordsmotivate.me/download";
        public static string GitHubProjectPageUrl = "https://github.com/YuAo/Motivate-Desktop";
        public static string WorkingDirectory = getWorkingDirectory();
        private static string getWorkingDirectory()
        {
            string path = Path.GetTempPath()+"MotivateDesktop\\";
            if(!Directory.Exists(path)){
                Directory.CreateDirectory(path);   
            }
            return path;
        }

        public static string ApplicationDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

        public enum ScreenRatio { SixteenByTen, SixteenByNine, FourByThree, NO_MATCH }
        public static ScreenRatio MainScreenRatio = GetScreenRatio();
        public static ScreenRatio GetScreenRatio()
        {
            int width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            double screen_ratio = (double)width / height;
            if (screen_ratio == 1.6)
            {
                return ScreenRatio.SixteenByTen;
            }
            else if (screen_ratio * (3.0 / 4) == 1 || screen_ratio * (4.0 / 5) == 1)
            {
                return ScreenRatio.FourByThree;
            }
            else if (screen_ratio > 1.77 && screen_ratio < 1.78)
            {
                return ScreenRatio.SixteenByNine;
            }
            else
            {
                return ScreenRatio.NO_MATCH;
            }
        }

        public static void ClearCache()
        {
            string jpgPreviewFilePath = System.IO.Path.Combine(MotivateDesktopUtility.WorkingDirectory, "preview.jpg");
            string pngPreviewFilePath = System.IO.Path.Combine(MotivateDesktopUtility.WorkingDirectory, "preview.png");
            if (System.IO.File.Exists(jpgPreviewFilePath))
            {
                try
                {
                    System.IO.File.Delete(jpgPreviewFilePath);
                }
                catch { };
            }
            if (System.IO.File.Exists(pngPreviewFilePath))
            {
                try
                {
                    System.IO.File.Delete(pngPreviewFilePath);
                }
                catch { };
            }
        }
    }

}

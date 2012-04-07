using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace MotivateDesktop
{
    class WallpaperChecker
    {
        private WallpaperChecker(){
            isChecking = false;
        }
        private static WallpaperChecker instance = null;
        public static WallpaperChecker Instance()
        {
            if (instance == null)
            {
                instance = new WallpaperChecker();
            }
            return instance;
        }

        public enum WallpaperFormats { JPG, PNG, MISSING };
        private int retryCount = 0;

        public bool IsChecking
        {
            get
            {
                return isChecking;
            }
        }
        private bool isChecking = false;

        public bool DidGotValidWallpaper
        {
            get
            {
                return didGotValidWallpaper;
            }
        }
        private bool didGotValidWallpaper = false;
        public DateTime WallpaperDate;
        public WallpaperFormats WallpaperFormat;
        public string WallpaperPreviewFileLocalCachePath;

        public delegate void GotWallpaperPreviewEventHandler(Object sender, string wallpaperPreviewLocalCachePath, bool isNew);
        public event GotWallpaperPreviewEventHandler GotWallpaperPreview;

        public delegate void FailedGettingWallpaperPreviewEventHandler(Object sender);
        public event FailedGettingWallpaperPreviewEventHandler FailedGettingWallpaperPreview;

        public void BeginCheckWallpaper()
        {
            isChecking = true;
            didGotValidWallpaper = false;
            string previewUrl = tryGetWallpaperPreviewUrlForm(DateTime.Now.Date.AddDays(1));
            if (previewUrl != null)
            {
                DownloadWallpaperPreview(previewUrl);
            }
        }

        private void getWallpaperPreviewFailed()
        {
            isChecking = false;
            didGotValidWallpaper = false;
            if (FailedGettingWallpaperPreview != null)
            {
                FailedGettingWallpaperPreview(this);
            }
        }

        private void getWallpaperPreviewSucceeded()
        {
            isChecking = false;
            didGotValidWallpaper = true;
        }

        private string tryGetWallpaperPreviewUrlForm(DateTime date)
        {
            if (retryCount >= 3)
            {
                getWallpaperPreviewFailed();
                return null;
            }

            string formatString;
            try
            {
                formatString = GetRespondFromUrl(MotivateDesktopUtility.FormatAPIUrl + "?date=" + date.ToString("yyyy-M-d"));
            }
            catch
            {
                retryCount = 0;
                getWallpaperPreviewFailed();
                return null;
            }
            formatString = formatString.Substring(0, formatString.IndexOf("[REQ_RESULT_END]"));
            switch (formatString.Split(':')[1])
            {
                case "JPG":
                    retryCount = 0;
                    WallpaperDate = date;
                    WallpaperFormat = WallpaperFormats.JPG;
                    return MotivateDesktopUtility.ImgServerUrl + date.ToString("yyyy.M") + "/" + date.ToString("yyyy.M.d") + "_480x300.jpg";
                case "PNG":
                    retryCount = 0;
                    WallpaperDate = date;
                    WallpaperFormat = WallpaperFormats.PNG;
                    return MotivateDesktopUtility.ImgServerUrl + date.ToString("yyyy.M") + "/" + date.ToString("yyyy.M.d") + "_480x300.png";
                case "MISSING":
                    retryCount++;
                    DateTime prevDate = date.AddDays(-1);
                    return tryGetWallpaperPreviewUrlForm(prevDate);
                default:
                    retryCount = 0;
                    getWallpaperPreviewFailed();
                    return null;
            }
        }

        private string GetRespondFromUrl(string url)
        {
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse res = (HttpWebResponse)myHttpWebRequest.GetResponse();
            StreamReader reader = new StreamReader(res.GetResponseStream());
            return reader.ReadToEnd();
        }

        private void DownloadWallpaperPreview(string url)
        {
            string previewFilePath = System.IO.Path.Combine(MotivateDesktopUtility.WorkingDirectory, "preview" + System.IO.Path.GetExtension(url));
            try
            {
                bool isNew = true;
                if (MotivateDesktop.Properties.Settings.Default.CachedPreviewUrl != url || !System.IO.File.Exists(previewFilePath))
                {
                    WebClient downloader = new WebClient();
                    downloader.DownloadFile(url, previewFilePath);
                    MotivateDesktop.Properties.Settings.Default.CachedPreviewUrl = url;
                    MotivateDesktop.Properties.Settings.Default.Save();
                    isNew = true;
                }
                else
                {
                    isNew = false;
                }

                WallpaperPreviewFileLocalCachePath = previewFilePath;
                getWallpaperPreviewSucceeded();
                if (GotWallpaperPreview != null)
                {
                    GotWallpaperPreview(this, previewFilePath, isNew);
                }
            }
            catch
            {
                getWallpaperPreviewFailed();
                return;
            }
        }

    }
}

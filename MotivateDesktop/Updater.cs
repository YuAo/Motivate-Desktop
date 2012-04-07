using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Xml;

namespace MotivateDesktop
{
    class Updater
    {
        public static string UpdateInfoXMLUrl = "http://api.wordsmotivate.me/App/UpdateInfo.xml";
        private static string WorkingDirectory = getWorkingDirectory();
        private static string getWorkingDirectory()
        {
            string path = Path.Combine(MotivateDesktopUtility.WorkingDirectory, "Updater\\");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        private static string UpdateInfoXMLPath = Path.Combine(WorkingDirectory, "UpdateInfo.xml");
        private static string UpdateZipPath = Path.Combine(WorkingDirectory, "Update.zip");

        public enum UpdaterStatus { Idle, Checking, Downloading, ReadyToUpdate, Failed, IsUpToDate}

        public string NewVersion = null;
        public UpdaterStatus CurrentStatus = UpdaterStatus.Idle;
        public Double CurrentDownloadProgress = 0;

        public delegate void StatusUpdatedEventHandler(Object sender);
        public event StatusUpdatedEventHandler StatusUpdated = null;

        private UpdateInfo updateInfo;
        internal class UpdateInfo
        {
            public string VersionNumber;
            public string DownloadUrl;
            public string InformationUrl;
            public string Description;
        }

        private string cachedUpdateVersion
        {
            get
            {
                if (File.Exists(UpdateZipPath))
                {
                    return MotivateDesktop.Properties.Settings.Default.UpdateVersion;
                }
                else
                {
                    cachedUpdateVersion = "0.0.0";
                    return "0.0.0";
                }
            }
            set
            {
                MotivateDesktop.Properties.Settings.Default.UpdateVersion = value;
                MotivateDesktop.Properties.Settings.Default.Save();
            }
        }

        public void CheckUpdateAsync()
        {
            updateStatus(UpdaterStatus.Checking);
            Thread checkUpdateThread = new Thread(new ThreadStart(
                delegate
                {
                    getUpdateInfo();
                }
                ));
            checkUpdateThread.Start();
        }

        private void updateStatus(UpdaterStatus newStatus)
        {
            CurrentStatus = newStatus;
            if (StatusUpdated != null)
            {
                App.Current.Dispatcher.Invoke(new Action(delegate
                {
                    StatusUpdated(this);
                }), null);
            }
        }

        private void getUpdateInfo()
        {
            try
            {
                WebClient downloader = new WebClient();
                downloader.DownloadFile(UpdateInfoXMLUrl, UpdateInfoXMLPath);
            }
            catch
            {
                updateStatus(UpdaterStatus.Failed);
                return;
            }
            parserUpdateInfo();
        }

        private void parserUpdateInfo(){
            XmlDocument updateInfoXml = new XmlDocument();
            updateInfoXml.Load(UpdateInfoXMLPath);
            
            XmlElement latestVersionInfo = updateInfoXml.GetElementById("LatestVersion");
            string versionNumber = latestVersionInfo.GetAttribute("Number");
            string downloadUrl = latestVersionInfo.GetAttribute("DownloadURL");
            string informationUrl = latestVersionInfo.GetAttribute("InformationURL");
            string description = latestVersionInfo.GetAttribute("Description");

            XmlElement unsupportedVersionInfo = updateInfoXml.GetElementById("UnsupportedVersion");
            string unsupportedVersionNumber = unsupportedVersionInfo.GetAttribute("Number");
            string unsupportedInformationUrl = unsupportedVersionInfo.GetAttribute("InformationURL");
            string unsupportedDescription = unsupportedVersionInfo.GetAttribute("Description");

            string currentVersionNumber = App.ResourceAssembly.GetName().Version.ToString();

            if (string.Compare(currentVersionNumber, unsupportedVersionNumber) <= 0)
            {
                System.Windows.MessageBox.Show("你正在使用一个被弃用的版本，请立即更新!\n"+unsupportedDescription,"错误",System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Error);
                System.Diagnostics.Process.Start(unsupportedInformationUrl);
                App.Current.Dispatcher.Invoke(new Action(delegate { App.Current.Shutdown(); }), null);
                return;
            }

            if (string.Compare(currentVersionNumber, versionNumber) < 0)
            {
                NewVersion = versionNumber;
                if (NewVersion == cachedUpdateVersion)
                {
                    updateStatus(UpdaterStatus.ReadyToUpdate);
                }
                else
                {
                    updateInfo = new UpdateInfo();
                    updateInfo.VersionNumber = versionNumber;
                    updateInfo.DownloadUrl = downloadUrl;
                    updateInfo.InformationUrl = informationUrl;
                    updateInfo.Description = description;
                    downloadUpdate();
                }
                return;
            }
            else
            {
                updateStatus(UpdaterStatus.IsUpToDate);
                return;
            }
        }

        private void downloadUpdate()
        {
            updateStatus(UpdaterStatus.Downloading);
            WebClient updateDownloader = new WebClient();
            updateDownloader.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(updateDownloader_DownloadFileCompleted);
            updateDownloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(updateDownloader_DownloadProgressChanged);
            updateDownloader.DownloadFileAsync(new Uri(updateInfo.DownloadUrl), UpdateZipPath);
        }

        void updateDownloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            CurrentDownloadProgress = e.ProgressPercentage;
            updateStatus(UpdaterStatus.Downloading);
        }

        void updateDownloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled)
            {
                updateStatus(UpdaterStatus.Failed);
            }
            else
            {
                cachedUpdateVersion = updateInfo.VersionNumber;
                updateStatus(UpdaterStatus.ReadyToUpdate);
            }
        }


        public void CheckAndInstallCachedUpdate()
        {
            if (File.Exists(UpdateZipPath))
            {
                string currentVersionNumber = App.ResourceAssembly.GetName().Version.ToString();
                if(string.Compare( currentVersionNumber ,MotivateDesktop.Properties.Settings.Default.UpdateVersion)<0)
                {
                    if (canWriteInAppDirectory())
                    {
                        startUpdater(false);
                    }
                    else
                    {
                        startUpdater(true);
                    }
                }
            }
        }

        private void startUpdater(bool shouldRequireHigherRight)
        {
            string updaterAppPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "Updater.exe");
            string updaterAppWorkingPath = Path.Combine(WorkingDirectory, "Updater.exe");

            try
            {
                File.Copy(updaterAppPath, updaterAppWorkingPath, true);
                File.Copy(Path.Combine(System.Windows.Forms.Application.StartupPath, "Interop.Shell32.dll"), Path.Combine(WorkingDirectory, "Interop.Shell32.dll"), true);

                System.Diagnostics.ProcessStartInfo updaterProcessStartInfo = new System.Diagnostics.ProcessStartInfo();
                updaterProcessStartInfo.FileName = updaterAppWorkingPath;
                updaterProcessStartInfo.Arguments = "\"" + System.Windows.Forms.Application.StartupPath +"\" "+"\""+ System.Windows.Forms.Application.ExecutablePath+"\"";
                if (shouldRequireHigherRight)
                {
                    updaterProcessStartInfo.Verb = "runas";       
                }
                System.Diagnostics.Process.Start(updaterProcessStartInfo);
                App.Current.Shutdown();
            }
            catch
            {
                System.Windows.MessageBox.Show("无法启动更新程序.\n你可能需要去给力壁纸手动下载更新.", "错误",System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Warning);
                return;
            }
        }

        private bool canWriteInAppDirectory()
        {
            try
            {
                File.WriteAllText(Path.Combine(System.Windows.Forms.Application.StartupPath,"PERMISSION_TEST"),"WRITE_TEST");
                File.Delete(Path.Combine(System.Windows.Forms.Application.StartupPath, "PERMISSION_TEST"));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private Updater() {}
        private static Updater instance = null;
        public static Updater Instance()
        {
            if (instance == null)
            {
                instance = new Updater();
            }
            return instance;
        }


    }
}

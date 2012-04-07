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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace MotivateDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WallpaperPreviewWindow : Window
    {
        private bool isClosed = false;
        private static WallpaperPreviewWindow instance = null;
        public static WallpaperPreviewWindow Instance()
        {
            if (instance == null || instance.isClosed)
            {
                instance = new WallpaperPreviewWindow();
            }
            return instance;
        }

        private WallpaperPreviewWindow()
        {
            InitializeComponent();
        }

        private string wallpaperFilePath;
        public string WallpaperFilePath
        {
            get
            {
                return wallpaperFilePath;
            }
            set
            {
                wallpaperFilePath = value;
                LoadWallpaper();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left) this.DragMove();
            }
            catch { };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PositionToRightBottom();
            SetupControls();
            PrepareForSlideControls();
            LoadWallpaper();
            AnimatedShowMainWrapper();
        }

        private void PositionToRightBottom()
        {
            this.Left = SystemParameters.FullPrimaryScreenWidth - this.ActualWidth + 10;
            this.Top  = SystemParameters.FullPrimaryScreenHeight - this.ActualHeight + SystemParameters.WindowCaptionHeight + 10;
        }

        private void LoadWallpaper()
        {
            if (!string.IsNullOrEmpty(this.WallpaperFilePath))
            {
                BitmapImage wallpaperPreviewImage = new BitmapImage();
                wallpaperPreviewImage.BeginInit();
                wallpaperPreviewImage.UriSource = new Uri(wallpaperFilePath);
                wallpaperPreviewImage.CacheOption = BitmapCacheOption.OnLoad;
                wallpaperPreviewImage.EndInit();
                image_wallpaper.Source = wallpaperPreviewImage;
            }
        }

        private void SetupControls()
        {
            SetupButton(image_setWallpaper, label_setWallpaper, new MouseButtonEventHandler(setWallpaper_MouseDown));
            SetupButton(image_website, label_website, new MouseButtonEventHandler(website_MouseDown));
            SetupButton(image_close, label_close, new MouseButtonEventHandler(close_MouseDown));

            wrapper.MouseEnter += new MouseEventHandler(wrapper_MouseEnter);
            wrapper.MouseLeave += new MouseEventHandler(wrapper_MouseLeave);
        }

        private void SetupButton(Image image,Label label,MouseButtonEventHandler clickEvent)
        {
            image.Tag = label;
            label.Tag = image;
            image.MouseEnter += new MouseEventHandler(button_MouseEnter);
            image.MouseLeave += new MouseEventHandler(button_MouseLeave);
            label.MouseEnter += new MouseEventHandler(button_MouseEnter);
            label.MouseLeave += new MouseEventHandler(button_MouseLeave);
            image.MouseDown  += clickEvent;
            label.MouseDown  += clickEvent;
        }
        private void button_MouseEnter(object sender, MouseEventArgs e)
        {
            System.Windows.Media.Effects.DropShadowEffect outerGlowShadow = new System.Windows.Media.Effects.DropShadowEffect();
            outerGlowShadow.ShadowDepth = 0;
            outerGlowShadow.BlurRadius = 12;
            outerGlowShadow.Color = Colors.White;
            outerGlowShadow.Opacity = 0.7;

            ((FrameworkElement)sender).Effect = outerGlowShadow;
            ((FrameworkElement)((FrameworkElement)sender).Tag).Effect = ((FrameworkElement)sender).Effect;
        }
        private void button_MouseLeave(object sender, MouseEventArgs e)
        {
            System.Windows.Media.Effects.DropShadowEffect littleBlackShadow = new System.Windows.Media.Effects.DropShadowEffect();
            littleBlackShadow.ShadowDepth = 1;
            littleBlackShadow.BlurRadius = 1;
            littleBlackShadow.Opacity = 0.7;

            ((FrameworkElement)sender).Effect = littleBlackShadow;
            ((FrameworkElement)((FrameworkElement)sender).Tag).Effect = ((FrameworkElement)sender).Effect;
        }

        private void wrapper_MouseLeave(object sender, MouseEventArgs e)
        {
            SlideOutControls();
        }

        private void wrapper_MouseEnter(object sender, MouseEventArgs e)
        {
            SlideInControls();
        }

        private void setWallpaper_MouseDown(object sender, MouseEventArgs e)
        {
            WallpaperDownloader wallpaperDownloader = WallpaperDownloader.Instance();
            if (wallpaperDownloader.IsDownloading)
            {
                MotivateDesktopNotifyIcon.Instance().ShowBalloonTip("正在下载", "有一张壁纸正在下载，请等待下载完成...");
            }
            else
            {
                System.Threading.Thread wallpaperDownloadThread = new System.Threading.Thread(new System.Threading.ThreadStart(
                    delegate
                    {
                        wallpaperDownloader.DownloadAndApplyWallpaper(WallpaperChecker.Instance().WallpaperDate, WallpaperChecker.Instance().WallpaperFormat);
                    }
                ));
                wallpaperDownloadThread.Start();
            }
            AnimatedCloseMainWrapper();
        }

        private void website_MouseDown(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Process.Start(MotivateDesktopUtility.HomePageUrl);
        }

        private void close_MouseDown(object sender, MouseEventArgs e)
        {
            AnimatedCloseMainWrapper();
        }

        private void PrepareForSlideControls()
        {
            grid_controls.RenderTransform = new TranslateTransform(0,grid_controls.Height);
        }

        private void SlideInControls()
        {
            Storyboard controlsSlideInStoryboard = new Storyboard();
            DoubleAnimation positionAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.15)));
            DoubleAnimation opacityAnimation = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(0.15)));
            positionAnimation.DecelerationRatio = 1;
            Storyboard.SetTargetProperty(positionAnimation, new PropertyPath("(0).(1)", new System.Windows.DependencyProperty[] { Grid.RenderTransformProperty, TranslateTransform.YProperty }));
            Storyboard.SetTargetProperty(opacityAnimation,new PropertyPath(Grid.OpacityProperty));
            controlsSlideInStoryboard.Children.Add(positionAnimation);
            controlsSlideInStoryboard.Children.Add(opacityAnimation);
            controlsSlideInStoryboard.Begin(grid_controls);
        }

        private void SlideOutControls()
        {
            Storyboard controlsSlideInStoryboard = new Storyboard();
            DoubleAnimation positionAnimation = new DoubleAnimation(grid_controls.Height, new Duration(TimeSpan.FromSeconds(0.15)));
            DoubleAnimation opacityAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.15)));
            positionAnimation.AccelerationRatio = 1;
            Storyboard.SetTargetProperty(positionAnimation, new PropertyPath("(0).(1)", new System.Windows.DependencyProperty[] { Grid.RenderTransformProperty, TranslateTransform.YProperty }));
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Grid.OpacityProperty));
            controlsSlideInStoryboard.Children.Add(positionAnimation);
            controlsSlideInStoryboard.Children.Add(opacityAnimation);
            controlsSlideInStoryboard.Begin(grid_controls);
        }

        private void AnimatedShowMainWrapper()
        {
            wrapper.RenderTransform = new TranslateTransform(0, 100);
            wrapper.Opacity = 0.01;
            Storyboard wrapperPopupStoryboard = new Storyboard();
            DoubleAnimation positionAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.2)));
            DoubleAnimation opacityAnimation = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(0.2)));
            positionAnimation.DecelerationRatio = 1;
            Storyboard.SetTargetProperty(positionAnimation, new PropertyPath("(0).(1)", new System.Windows.DependencyProperty[] { Border.RenderTransformProperty, TranslateTransform.YProperty }));
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Border.OpacityProperty));
            wrapperPopupStoryboard.Children.Add(positionAnimation);
            wrapperPopupStoryboard.Children.Add(opacityAnimation);
            System.Timers.Timer animationTimer = new System.Timers.Timer(200);
            animationTimer.AutoReset = false;
            animationTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    wrapperPopupStoryboard.Begin(wrapper);
                }));
            });
            animationTimer.Start();
        }

        private void AnimatedCloseMainWrapper()
        {
            wrapper.RenderTransform = new TranslateTransform(0, 0);
            Storyboard wrapperCloseAnimationStoryboard = new Storyboard();
            DoubleAnimation positionAnimation = new DoubleAnimation(100, new Duration(TimeSpan.FromSeconds(0.2)));
            DoubleAnimation opacityAnimation = new DoubleAnimation(0.1, new Duration(TimeSpan.FromSeconds(0.2)));
            positionAnimation.AccelerationRatio = 1;
            Storyboard.SetTargetProperty(positionAnimation, new PropertyPath("(0).(1)", new System.Windows.DependencyProperty[] { Border.RenderTransformProperty, TranslateTransform.YProperty }));
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Border.OpacityProperty));
            wrapperCloseAnimationStoryboard.Completed += new EventHandler(wrapperCloseAnimationStoryboard_Completed);
            wrapperCloseAnimationStoryboard.Children.Add(positionAnimation);
            wrapperCloseAnimationStoryboard.Children.Add(opacityAnimation);
            wrapperCloseAnimationStoryboard.Begin(wrapper);
        }

        void wrapperCloseAnimationStoryboard_Completed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            isClosed = true;
        }

        private void image_wallpaper_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MotivateDesktopUtility.ClearCache();
            this.Close();
        }
    }
}

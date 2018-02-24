using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.ComponentModel;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Meta.Vlc.Wpf;

namespace FramelessPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private bool settingsToggle = false;
        private bool darkmodeIconToggle = false;

        private bool isPlaying = false;
        private bool isStopped = false;

        public TimeSpan videoDuration = new TimeSpan();

        public MainWindow()
        {
            InitializeComponent();
            
            // Get app accent and theme from settings and set it
            ThemeManager.ChangeAppStyle(Application.Current,
                                            ThemeManager.GetAccent(Properties.Settings.Default.Accent),
                                            ThemeManager.GetAppTheme(Properties.Settings.Default.Theme));

            // Set accent combobox data source
            Accent_ComboBox.ItemsSource = Enum.GetValues(typeof(LooksManager.Accents)).Cast<LooksManager.Accents>();

            // Set accent combobox item based on settings
            Accent_ComboBox.SelectedIndex = (int)Enum.Parse(typeof(LooksManager.Accents), Properties.Settings.Default.Accent);

            // Set theme button icon based on settings
            if (Properties.Settings.Default.Theme == "BaseLight")
            {
                DarkMode_Icon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.SunRegular;
                darkmodeIconToggle = false;
            } else {
                DarkMode_Icon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.MoonRegular;
                darkmodeIconToggle = true;
            }

            // Set being on top based on settings
            Topmost = Properties.Settings.Default.IsOnTop;

            // Set volume
            Player.Volume = (int)Properties.Settings.Default.Volume;
            VolumeSlider.Value = Properties.Settings.Default.Volume;

            // Check if a file is being opened through shell extension and play is if so
            var args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                Player.Stop();
                Player.LoadMedia(args[1]);
                Player.Play();

                isPlaying = true;
                icoPlayPause.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
                btnPlay.ToolTip = "Pause";

                grVideoControls.Opacity = 0.0;

                // Remove d'n'd overlay
                DragDropArea.Visibility = Visibility.Collapsed;
            }
        }


        // Handle settings button
        private void Settings_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Open and close flyout
            if (settingsToggle)
            {
                Settings_Flyout.IsOpen = false;
                settingsToggle = false;

                // Save settings
                Properties.Settings.Default.Save();
            }
            else
            {
                Settings_Flyout.IsOpen = true;
                settingsToggle = true;
            }
            
        }

        // Handle settings button spinning
        private void Settings_Btn_MouseEnter(object sender, MouseEventArgs e)
        {
            Settings_Ico.Spin = true;
        }

        private void Settings_Btn_MouseLeave(object sender, MouseEventArgs e)
        {
            Settings_Ico.Spin = false;
        }

        // Handle accent selection combobox
        private void Accent_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);
            
            ThemeManager.ChangeAppStyle(Application.Current,
                                        ThemeManager.GetAccent(Accent_ComboBox.SelectedItem.ToString()),
                                        theme.Item1);

            // Save in settings
            Properties.Settings.Default.Accent = Accent_ComboBox.SelectedItem.ToString();
        }

        // Handle theme selection switch
        private void DarkMode_Toggle_Click(object sender, RoutedEventArgs e)
        {
            Tuple<AppTheme, Accent> completeTheme = ThemeManager.DetectAppStyle(Application.Current);
            string theme = "";

            if (darkmodeIconToggle)
            {
                theme = "BaseLight";

                darkmodeIconToggle = false;
                DarkMode_Icon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.SunRegular;
            }
            else
            {
                theme = "BaseDark";

                darkmodeIconToggle = true;
                DarkMode_Icon.Kind = MahApps.Metro.IconPacks.PackIconFontAwesomeKind.MoonRegular;
            }

            // Apply chosen theme
            ThemeManager.ChangeAppStyle(Application.Current,
                                            completeTheme.Item2,
                                            ThemeManager.GetAppTheme(theme));

            // Save in settings
            Properties.Settings.Default.Theme = theme;
        }

        // Handle being on top switch
        private void KeepOnTop_Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (Topmost)
            {
                Topmost = false;
                icoKeepOnTop.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.ArrangeBringForward;
                KeepOnTop_Toggle.ToolTip = "Currently window doesn't stay on top";
            }
            else
            {
                Topmost = true;
                icoKeepOnTop.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.ArrangeSendBackward;
                KeepOnTop_Toggle.ToolTip = "Currently window stays on top";
            }

            Properties.Settings.Default.IsOnTop = Topmost;
        }

        // Handle titlebar minification switch
        private void MinifyTitlebar_Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (ShowTitleBar)
            {
                ShowTitleBar = false;
                LayoutParent.Margin = new Thickness(0, -30, 0, 0);
                this.Height -= 30;
                //this.Top -= 30;
            }
            else
            {
                ShowTitleBar = true;
                LayoutParent.Margin = new Thickness(0, 0, 0, 0);
                this.Height += 30;
                //this.Top += 30;
            }
           
        }

        #region --- Cleanup ---

        protected override void OnClosing(CancelEventArgs e)
        {
            Player.Dispose();
            ApiManager.ReleaseAll();
            base.OnClosing(e);
        }

        #endregion --- Cleanup ---

        #region --- Events ---

        // Handle load button
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var openfiles = new OpenFileDialog
            {
                // Set file filter
                Filter = "Videos|*.avi;*.mp4;*.mkv;*.mpeg;|All files|*.*"
            };

            if (openfiles.ShowDialog() == true)
            {
                Player.Stop();
                Player.LoadMedia(openfiles.FileName);
                Player.Play();
                
                isPlaying = true;
                icoPlayPause.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
                btnPlay.ToolTip = "Pause";

                grVideoControls.Opacity = 0.0;

                // Remove d'n'd overlay
                DragDropArea.Visibility = Visibility.Collapsed;
            }
            return;

        }

        // Handle play/pause button
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (isStopped)
            {
                Player.Play();
                icoPlayPause.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
                btnPlay.ToolTip = "Pause";
                isPlaying = true;
                isStopped = false;

                // Remove d'n'd overlay
                DragDropArea.Visibility = Visibility.Collapsed;
            }
            else
            {
                Player.PauseOrResume();

                if (isPlaying)
                {
                    icoPlayPause.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
                    btnPlay.ToolTip = "Play";
                    isPlaying = false;
                }
                else
                {
                    icoPlayPause.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
                    btnPlay.ToolTip = "Pause";
                    isPlaying = true;

                    // Remove d'n'd overlay
                    DragDropArea.Visibility = Visibility.Collapsed;
                }
            }
        }

        // Handle stop button
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Player.Stop();
            isStopped = true;
            icoPlayPause.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;

            // Show d'n'd overlay
            DragDropArea.Visibility = Visibility.Visible;
        }

        // Handle progressbar progress
        private void VideoProgressBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var value = (float)(e.GetPosition(VideoProgressBar).X / VideoProgressBar.ActualWidth);
            VideoProgressBar.Value = value;
        }

        // Handle fullscreen button
        bool isFullscreen = false;
        private void btnFullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (isFullscreen)
            {
                ResizeMode = ResizeMode.CanResize;
                WindowState = WindowState.Normal;
                LeftWindowCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Always;
                RightWindowCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Always;
                WindowButtonCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Always;
            }
            else
            {
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
                LeftWindowCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Never;
                RightWindowCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Never;
                WindowButtonCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Never;
            }

            isFullscreen = !isFullscreen;

            ShowTitleBar = !isFullscreen;
            IgnoreTaskbarOnMaximize = isFullscreen;
            
        }

        #endregion --- Events ---

        // Handle scrubbing
        private void VideoProgress_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan totalTime = TimeSpan.FromSeconds(VideoProgressBar.Value);
            VideoProgressBar.ToolTip = totalTime.Hours.ToString("D2") + ":" +
                                           totalTime.Minutes.ToString("D2") + ":" +
                                           totalTime.Seconds.ToString("D2");
        }

        #region --- Controls opacity ---

        // Handle controls opacity
        private void grVideoControls_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isPlaying)
                grVideoControls.Opacity = 0.8;
        }

        private void grVideoControls_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isPlaying)
                grVideoControls.Opacity = 0.0;
        }

        #endregion --- Controls opacity ---

        // Handle KoFi button
        private void btnKoFi_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://ko-fi.com/H2H365N9");
        }

        // Handle drag'n'drop files
        private void DragDropArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                Player.Stop();
                Player.LoadMedia(files[0]);
                Player.Play();

                isPlaying = true;
                icoPlayPause.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
                btnPlay.ToolTip = "Pause";

                grVideoControls.Opacity = 0.0;

                // Remove d'n'd overlay
                DragDropArea.Visibility = Visibility.Collapsed;
            }
        }

        // Handle video progress
        private void Player_TimeChanged(object sender, EventArgs e)
        {
            string currentTime = Player.Time.ToString(@"hh\:mm\:ss");
            string totalTime = Player.VlcMediaPlayer.Media.Duration.ToString(@"hh\:mm\:ss");

            VideoTime.Text = currentTime + "/" + totalTime;
        }

        // Handle volume change
        private void VolumeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Properties.Settings.Default.Volume = VolumeSlider.Value;
            Properties.Settings.Default.Save();
        }

        // Handle app closed
        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            // Save all settings changes
            Properties.Settings.Default.Save();
        }
    }
}

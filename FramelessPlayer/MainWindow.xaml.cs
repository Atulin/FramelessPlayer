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
using System.Collections.ObjectModel;

namespace FramelessPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        // Settings toggle bool
        private bool settingsToggle = false;

        // Player state bools
        private bool isPlaying = false;
        private bool isStopped = false;
        
        // Video duration
        public TimeSpan videoDuration = new TimeSpan();

        // Timer for volume overlay
        DispatcherTimer volTimer;
        float volScrollDelta = 0;

        // Timer for mouse hide
        DispatcherTimer mouseTimer;

        // Playlist collection
        ObservableCollection<File> Playlist = new ObservableCollection<File>();
        public ObservableCollection<File> PublicPlaylist
        {
            get { return this.Playlist; }
        }

        // Subtitles collection
        ObservableCollection<Meta.Vlc.TrackDescription> Subtitles = new ObservableCollection<Meta.Vlc.TrackDescription>();
        public ObservableCollection<Meta.Vlc.TrackDescription> PublicSubtitles
        {
            get { return this.Subtitles; }
        }

        // Audio tracks collection
        ObservableCollection<Meta.Vlc.TrackDescription> AudioTracks = new ObservableCollection<Meta.Vlc.TrackDescription>();
        public ObservableCollection<Meta.Vlc.TrackDescription> PublicAudioTracks
        {
            get { return this.AudioTracks; }
        }

        // Video tracks collection
        ObservableCollection<Meta.Vlc.TrackDescription> VideoTracks = new ObservableCollection<Meta.Vlc.TrackDescription>();
        public ObservableCollection<Meta.Vlc.TrackDescription> PublicVideoTracks
        {
            get { return this.VideoTracks; }
        }

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
                DarkMode_Toggle.IsChecked = false;
            else
                DarkMode_Toggle.IsChecked = true;

            //Set titlebar minified
            if (Properties.Settings.Default.IsTitlebarMinified)
            {
                LayoutParent.Margin = new Thickness(0, -30, 0, 0);
                this.Height -= 30;
            }
            ShowTitleBar = !Properties.Settings.Default.IsTitlebarMinified;
            MinifyTitlebar_Toggle.IsChecked = Properties.Settings.Default.IsTitlebarMinified;

            // Set being on top based on settings
            Topmost = Properties.Settings.Default.IsOnTop;
            KeepOnTop_Toggle.IsChecked = Topmost;

            // Set volume
            Player.Volume = (int)Properties.Settings.Default.Volume;
            VolumeSlider.Value = Properties.Settings.Default.Volume;

            VolumeOverlay_bar.Value = (int)Properties.Settings.Default.Volume;
            VolumeOverlay_txt.Text = Properties.Settings.Default.Volume.ToString();

            // Set compact progress bar opacity
            CompactProgressBar.Opacity = Properties.Settings.Default.CompactProgressBarOpacity;
            CompactProgressOpacity_Slider.Value = Properties.Settings.Default.CompactProgressBarOpacity;

            // Set volume overlay timer
            volTimer = new DispatcherTimer();
            volTimer.Tick += VolTimer_Tick;
            volTimer.Interval = TimeSpan.FromMilliseconds(500 /*Adjust the interval*/);

            MouseWheel += MetroWindow_MouseWheel;

            // Set mouse hide timer
            mouseTimer = new DispatcherTimer();
            mouseTimer.Tick += HideMouseTimer_Tick;
            mouseTimer.Interval = TimeSpan.FromMilliseconds(500 /*Adjust the interval*/);


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

            // Change accent combobox focusability
            Accent_ComboBox.Focusable = !Accent_ComboBox.Focusable;
            
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

            if (!(bool)DarkMode_Toggle.IsChecked)
            {
                theme = "BaseLight";
            }
            else
            {
                theme = "BaseDark";
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
            Topmost = (bool)KeepOnTop_Toggle.IsChecked;

            Properties.Settings.Default.IsOnTop = Topmost;
        }

        // Handle titlebar minification switch
        private void MinifyTitlebar_Toggle_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)MinifyTitlebar_Toggle.IsChecked)
            {
                LayoutParent.Margin = new Thickness(0, -30, 0, 0);
                this.Height -= 30;
            }
            else
            {
                LayoutParent.Margin = new Thickness(0, 0, 0, 0);
                this.Height += 30;
            }

            ShowTitleBar = !ShowTitleBar;
            MinifyTitlebar_Toggle.IsChecked = !ShowTitleBar;
            Properties.Settings.Default.IsTitlebarMinified = !ShowTitleBar;

        }

        // Handle compact progress bar opacity
        private void CompactProgressOpacity_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try{
                CompactProgressBar.Opacity = e.NewValue;
                Properties.Settings.Default.CompactProgressBarOpacity = e.NewValue;
            }
            catch{}

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
                LeftWindowCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Always;
                RightWindowCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Always;
                WindowButtonCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Always;
                FullscreenClose_Btn.Visibility = Visibility.Collapsed;
                WindowState = WindowState.Normal;
            }
            else
            {
                LeftWindowCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Never;
                RightWindowCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Never;
                WindowButtonCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Never;
                WindowState = WindowState.Maximized;
            }

            isFullscreen = !isFullscreen;

            ShowTitleBar = !isFullscreen;
            IgnoreTaskbarOnMaximize = isFullscreen;
            
        }

        // Handle fullscreen close button
        private void FullscreenClose_Btn_Click(object sender, RoutedEventArgs e)
        {
            Title = "Cls";
        }

        // Handle playlist opening
        private void btnPlaylist_Click(object sender, RoutedEventArgs e)
        {
            Playlist_Flyout.IsOpen = !Playlist_Flyout.IsOpen;
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
            {
                grVideoControls.Opacity = 1;
                CompactProgressBar.Opacity = 0.0;
            }
        }

        private void grVideoControls_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isPlaying)
            {
                grVideoControls.Opacity = 0.0;
                CompactProgressBar.Opacity = Properties.Settings.Default.CompactProgressBarOpacity;
            }
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

                Playlist.Clear();
                foreach (var v in files)
                {
                    Playlist.Add(new File(v));
                }
                
            }
        }

        // Handle time change
        private void Player_TimeChanged(object sender, EventArgs e)
        {
            string currentTime = Player.Time.ToString(@"hh\:mm\:ss");
            string totalTime = Player.Length.ToString(@"hh\:mm\:ss");

            VideoTime.Text = currentTime + "/" + totalTime;
        }

        // Handle state change
        private void Player_StateChanged(object sender, Meta.Vlc.ObjectEventArgs<Meta.Vlc.Interop.Media.MediaState> e)
        {
            switch (e.Value)
            {
                case Meta.Vlc.Interop.Media.MediaState.Ended:
                    DragDropArea.Visibility = Visibility.Visible;
                    Player.Stop();
                    break;

                case Meta.Vlc.Interop.Media.MediaState.Playing:
                    FullscreenClose_Btn.Visibility = Visibility.Collapsed;
                    break;

                default:
                    if (isFullscreen)
                        FullscreenClose_Btn.Visibility = Visibility.Visible;
                    break;
            }
                
        }

        // Handle volume change
        private void VolumeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Properties.Settings.Default.Volume = VolumeSlider.Value;
            Properties.Settings.Default.Save();

            VolumeOverlay_bar.Value = Player.Volume;
            VolumeOverlay_txt.Text = Player.Volume.ToString();

            ShowVolumeChange();
        }

        // Change volume with scrollwheel
        void VolTimer_Tick(object sender, EventArgs e)
        {
            //Prevent timer from looping
            (sender as DispatcherTimer).Stop();

            //Perform some action
            VolumeOverlay.Visibility = Visibility.Hidden;

            //Reset for futher scrolling
            volScrollDelta = 0;
        }

        private void MetroWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            VolumeOverlay.Visibility = Visibility.Visible;


            if (e.Delta > 0 && Player.Volume < 120)
            {
                Player.Volume++;
            }
            else if (e.Delta < 0 && Player.Volume > 0)
            {
                Player.Volume--;
            }

            ShowVolumeChange();

            // Use the timer
            volScrollDelta += e.Delta;

            volTimer.Stop();
            volTimer.Start();
        }


        // Handle app closed
        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            // Save all settings changes
            Properties.Settings.Default.Save();
        }

        // Handle keyboard key presses
        private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    Play_Click(sender, new RoutedEventArgs());
                    break;
                case Key.F:
                    btnFullscreen_Click(sender, new RoutedEventArgs());
                    break;
                case Key.OemComma:
                    Player.Volume -= 5;
                    VolumeSlider.Value -= 5;
                    break;
                case Key.OemPeriod:
                    Player.Volume += 5;
                    VolumeSlider.Value += 5;
                    break;
                case Key.S:
                    Settings_Btn_Click(sender, new RoutedEventArgs());
                    break;
                case Key.M:
                    Player.IsMute = !Player.IsMute;
                    if (Player.IsMute)
                        Ico_Sound.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeOff;
                    else
                        Ico_Sound.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeMedium;
                    break;
                case Key.D:
                    break;
                default:
                    break;
            }
        }


        // Change subtitles
        private void Btn_ChangeSubtitles_Click(object sender, RoutedEventArgs e)
        {
            Meta.Vlc.TrackDescription td = ((FrameworkElement)sender).DataContext as Meta.Vlc.TrackDescription;
            Player.VlcMediaPlayer.Subtitle = td.Id;
        }

        // Change audio track
        private void Btn_ChangeAudioTrack_Click(object sender, RoutedEventArgs e)
        {
            Meta.Vlc.TrackDescription td = ((FrameworkElement)sender).DataContext as Meta.Vlc.TrackDescription;
            Player.VlcMediaPlayer.AudioTrack = td.Id;
        }

        // Change video track
        private void Btn_ChangeVideoTrack_Click(object sender, RoutedEventArgs e)
        {
            Meta.Vlc.TrackDescription td = ((FrameworkElement)sender).DataContext as Meta.Vlc.TrackDescription;
            Player.VlcMediaPlayer.VideoTrack = td.Id;
        }

        //Open track selection
        private void btnTracks_Click(object sender, RoutedEventArgs e)
        {
            //Subs
            Subtitles.Clear();
            AudioTracks.Clear();

            GetAllTracks();

            Tracks_Flyout.IsOpen = !Tracks_Flyout.IsOpen;
        }



        // Open file from playlist
        private void Btn_LoadFile_Click(object sender, RoutedEventArgs e)
        {
            File file = ((FrameworkElement)sender).DataContext as File;

            Player.Stop();
            Player.LoadMedia(file.Path);
            Player.Play();

            isPlaying = true;
            icoPlayPause.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
            btnPlay.ToolTip = "Pause";

            grVideoControls.Opacity = 0.0;

            // Remove d'n'd overlay
            DragDropArea.Visibility = Visibility.Collapsed;
        }





        // Load all tracks
        public void GetAllTracks()
        {
            // Grab subtitles
            foreach (var v in Player.VlcMediaPlayer.SubtitleDescription)
            {
                Subtitles.Add(v);
                DebugLog.Text += v.Name + " [" + v.Id + "]" + Environment.NewLine;
            }

            // Grab audio tracks
            foreach (var v in Player.VlcMediaPlayer.AudioTrackDescription)
            {
                AudioTracks.Add(v);
                DebugLog.Text += v.Name + " [" + v.Id + "]" + Environment.NewLine;
            }

            // Grab video tracks
            foreach (var v in Player.VlcMediaPlayer.VideoTrackDescription)
            {
                VideoTracks.Add(v);
                DebugLog.Text += v.Name + " [" + v.Id + "]" + Environment.NewLine;
            }

        }

        // Hide mouse cursor
        void HideMouseTimer_Tick(object sender, EventArgs e)
        {
            //Prevent timer from looping
            (sender as DispatcherTimer).Stop();

            //Perform some action
            if (Player.State == Meta.Vlc.Interop.Media.MediaState.Playing)
            {
                Mouse.OverrideCursor = Cursors.None;
            }

            //Reset for futher scrolling
            volScrollDelta = 0;
        }

        private void MetroWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (Player.State == Meta.Vlc.Interop.Media.MediaState.Playing)
            {
                Mouse.OverrideCursor = Cursors.Arrow;

                mouseTimer.Stop();
                mouseTimer.Start();
            }
        }




        ///
        /// HELPER METHODS
        ///
        public void ShowVolumeChange()
        {
            if (Player.Volume > 100)
            {
                Ico_Sound.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeHigh;
                Ico_Sound.Foreground = new SolidColorBrush(Colors.OrangeRed);
                VolumeOverlay_txt.Foreground = new SolidColorBrush(Colors.OrangeRed);
            }
            else if (Player.Volume == 0)
            {
                Ico_Sound.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeOff;
                Ico_Sound.Foreground = new SolidColorBrush(Colors.LightGray);
                VolumeOverlay_txt.Foreground = new SolidColorBrush(Colors.LightGray);
            }
            else
            {
                Ico_Sound.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeMedium;
                Ico_Sound.Foreground = new SolidColorBrush(Colors.White);
                VolumeOverlay_txt.Foreground = new SolidColorBrush(Colors.White);
            }

            VolumeOverlay_bar.Value = Player.Volume;
            VolumeOverlay_txt.Text = Player.Volume.ToString();

            Properties.Settings.Default.Volume = VolumeSlider.Value;
            Properties.Settings.Default.Save();
        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace FramelessPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private bool settingsToggle = false;
        private bool darkmodeIconToggle = false;
        private string filePath = "";

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

        // Handle file selection
        private void SelectFile_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file 
            dlg.DefaultExt = ".mp4";

            dlg.Filter = "Video files|*.mp4;*.avi;*.mkv";

            // Display OpenFileDialog by calling ShowDialog method
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                filePath = dlg.FileName;
            }

            MainPlayer.Source = new Uri(filePath);

            SelectFile_Btn.Visibility = Visibility.Collapsed;
        }

        // Handle media controls
        private void MediaControlGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            MediaControlGrid.Opacity = 1;
        }

        private void MediaControlGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            MediaControlGrid.Opacity = 0.1;
        }
    }
}

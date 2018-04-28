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
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace FramelessPlayer
{
    /// <summary>
    /// Interaction logic for TimerWindow.xaml
    /// </summary>
    public partial class TimerWindow : MetroWindow
    {
        // Get time
        public TimeSpan time = new TimeSpan();

        // Init
        public TimerWindow()
        {
            InitializeComponent();
        }

        // Return time
        public String Time
        {
            get
            {
                return time.ToString(@"hh\:mm\:ss");
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            
        }
    }
}

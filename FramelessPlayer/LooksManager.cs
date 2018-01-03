using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FramelessPlayer
{
    class LooksManager
    {
        // Enum with accents
        public enum Accents
        {
            Red,
            Green,
            Blue,
            Purple,
            Orange,
            Lime,
            Emerald,
            Teal,
            Cyan,
            Cobalt,
            Indigo,
            Violet,
            Pink,
            Magenta,
            Crimson,
            Amber,
            Yellow,
            Brown,
            Olive,
            Steel,
            Mauve,
            Taupe,
            Sienna
        }

        // Tuple with theme
        public static Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);
    }
}

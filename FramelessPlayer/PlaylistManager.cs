using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;

namespace FramelessPlayer
{
    class PlaylistManager
    {
    }

    public class File
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public File (string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }

}

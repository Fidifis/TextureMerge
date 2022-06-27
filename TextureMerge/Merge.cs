using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace TextureMerge
{
    internal class Merge
    {
        SKBitmap? red, green, blue;
        public void DoMerge(string saveFilePath)
        {
            //TODO: Implemetation
        }
        
        public ImageSource? LoadRedChannel(string path)
        {
            red = SKBitmap.Decode(path);
            //TODO extract channel
            return red?.toImageSource();
        }

        public ImageSource? LoadGreenChannel(string path)
        {
            green = SKBitmap.Decode(path);
            return green?.toImageSource();
        }

        public ImageSource? LoadBlueChannel(string path)
        {
            blue = SKBitmap.Decode(path);
            return blue?.toImageSource();
        }

        public void Clear(byte which)
        {
            switch (which)
            {
                case 0:
                    red = null;
                    break;
                case 1:
                    green = null;
                    break;
                case 2:
                    blue = null;
                    break;
            }
        }
    }
}

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

        public ImageSource? LoadChannel(string path, Channel channel)
        {
            red = SKBitmap.Decode(path);
            //TODO extract channel
            return red?.ToImageSource();
        }

        public void Clear(Channel which)
        {
            switch (which)
            {
                case Channel.Red:
                    red = null;
                    break;
                case Channel.Green:
                    green = null;
                    break;
                case Channel.Blue:
                    blue = null;
                    break;
            }
        }
    }
}

using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RT.Util.ExtensionMethods;
using System.Drawing;

namespace TankIconMaker
{
    public static class Targa
    {
        private class targa { public int Width, Height; public byte[] Raw; }

        private static targa load(string filename)
        {
            using (var file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var result = new targa();
                var header = file.Read(18);
                if (header[0] != 0) throw new NotSupportedException("Only images with no offset are supported");
                if (header[1] != 0) throw new NotSupportedException("Only RGB images are supported");
                if (header[2] != 2) throw new NotSupportedException("Only RGB images are supported");
                if (header[8] != 0 || header[9] != 0 || header[10] != 0 || header[11] != 0)
                    throw new NotSupportedException("Only images with origin at 0,0 are supported");
                result.Width = (header[13] << 8) + header[12];
                result.Height = (header[15] << 8) + header[14];
                if (result.Width > 2048 || result.Height > 2048) // an arbitrary limit; more for sanity check than anything else
                    throw new NotSupportedException("Only images up to 2048x2048 are supported");
                if (result.Width == 0 || result.Height == 0)
                    throw new NotSupportedException("Images with a zero width or height are not supported");
                if (header[16] != 32) throw new NotSupportedException("Only 32 bits per pixel images are supported");
                bool upsideDown = (header[17] & 32) != 0;
                if (!upsideDown) throw new NotSupportedException("Only upside-down images are supported");

                result.Raw = file.Read(result.Width * result.Height * 4);
                return result;
            }
        }

        public static WriteableBitmap LoadWpf(string filename)
        {
            var tga = load(filename);
            var bitmap = new WriteableBitmap(tga.Width, tga.Height, 96, 96, PixelFormats.Bgra32, null);
            bitmap.WritePixels(new Int32Rect(0, 0, tga.Width, tga.Height), tga.Raw, tga.Width * 4, 0);
            bitmap.Freeze();
            return bitmap;
        }

        public static Bitmap LoadGdi(string filename)
        {
            return null;
        }
    }
}

using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RT.Util.ExtensionMethods;
using System.Drawing;

namespace TankIconMaker
{
    sealed class Targa
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] Raw { get; private set; }

        public Targa(string filename)
        {
            using (var file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var header = file.Read(18);
                if (header[0] != 0) throw new NotSupportedException("Only images with no offset are supported");
                if (header[1] != 0) throw new NotSupportedException("Only RGB images are supported");
                if (header[2] != 2) throw new NotSupportedException("Only RGB images are supported");
                if (header[8] != 0 || header[9] != 0 || header[10] != 0 || header[11] != 0)
                    throw new NotSupportedException("Only images with origin at 0,0 are supported");
                Width = (header[13] << 8) + header[12];
                Height = (header[15] << 8) + header[14];
                if (Width > 2048 || Height > 2048) // an arbitrary limit; more for sanity check than anything else
                    throw new NotSupportedException("Only images up to 2048x2048 are supported");
                if (Width == 0 || Height == 0)
                    throw new NotSupportedException("Images with a zero width or height are not supported");
                if (header[16] != 32) throw new NotSupportedException("Only 32 bits per pixel images are supported");
                bool upsideDown = (header[17] & 32) != 0;

                Raw = file.Read(Width * Height * 4);

                if (!upsideDown)
                {
                    var line = new byte[Width * 4];
                    int w = Width * 4;
                    for (int y = 0; y < Height / 2; y++)
                    {
                        Buffer.BlockCopy(Raw, y * w, line, 0, w);
                        Buffer.BlockCopy(Raw, (Height - 1 - y) * w, Raw, y * w, w);
                        Buffer.BlockCopy(line, 0, Raw, (Height - 1 - y) * w, w);
                    }
                }
            }
        }

        public void Save(string filename)
        {
            using (var file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                var header = new byte[18];
                header[2] = 2;
                header[12] = (byte) (Width);
                header[13] = (byte) (Width >> 8);
                header[14] = (byte) (Height);
                header[15] = (byte) (Height >> 8);
                header[16] = 32;
                header[17] = 32;
                file.Write(header);
                file.Write(Raw);
            }
        }

        public static WriteableBitmap LoadWpf(string filename)
        {
            var tga = new Targa(filename);
            var bitmap = new WriteableBitmap(tga.Width, tga.Height, 96, 96, PixelFormats.Bgra32, null);
            bitmap.WritePixels(new Int32Rect(0, 0, tga.Width, tga.Height), tga.Raw, tga.Width * 4, 0);
            bitmap.Freeze();
            return bitmap;
        }

        public static BitmapGdi LoadGdi(string filename)
        {
            throw new NotImplementedException();
        }

        public static void Save(BitmapSource image, string filename)
        {
            Save(image.ToGdi(), filename);
        }

        public static void Save(BitmapGdi image, string filename)
        {
            using (var file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                var header = new byte[18];
                header[2] = 2;
                header[12] = (byte) (image.PixelWidth);
                header[13] = (byte) (image.PixelWidth >> 8);
                header[14] = (byte) (image.PixelHeight);
                header[15] = (byte) (image.PixelHeight >> 8);
                header[16] = 32;
                header[17] = 32;
                file.Write(header);
                for (int y = 0; y < image.PixelHeight; y++)
                    file.Write(image.Bytes, y * image.Stride, image.PixelWidth * 4);
            }
        }
    }
}

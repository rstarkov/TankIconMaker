using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RT.Util.ExtensionMethods;

namespace TankIconMaker
{
    /// <summary>
    /// Encapsulates a Targa image data. Exposes the raw pixel data, for better or worse.
    /// Supports only a small subset of actual Targa formats: 32bpp with alpha, uncompressed.
    /// </summary>
    static class Targa
    {
        /// <summary>Loads a .tga image directly into a <see cref="BitmapRam"/> instance.</summary>
        public static BitmapRam Load(Stream file)
        {
            var header = file.Read(18);
            if (header[0] != 0) throw new NotSupportedException("Only images with no offset are supported");
            if (header[1] != 0) throw new NotSupportedException("Only RGB images are supported");
            if (header[2] != 2) throw new NotSupportedException("Only RGB images are supported");
            if (header[8] != 0 || header[9] != 0 || header[10] != 0 || header[11] != 0)
                throw new NotSupportedException("Only images with origin at 0,0 are supported");
            var width = (header[13] << 8) + header[12];
            var height = (header[15] << 8) + header[14];
            if (width > 2048 || height > 2048) // an arbitrary limit; more for sanity check than anything else
                throw new NotSupportedException("Only images up to 2048x2048 are supported");
            if (width == 0 || height == 0)
                throw new NotSupportedException("Images with a zero width or height are not supported");
            if (header[16] != 32) throw new NotSupportedException("Only 32 bits per pixel images are supported");
            bool rightWayUp = (header[17] & 32) != 0;

            var raw = file.Read(width * height * 4);

            var result = new BitmapRam(width, height);
            result.CopyPixelsFrom(raw, width, height, width * 4, !rightWayUp);
            return result;
        }

        /// <summary>A convenience method to save a GDI image directly to a .tga file.</summary>
        public static unsafe void Save(BitmapBase image, string filename)
        {
            using (image.UseRead())
            using (var file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                var header = new byte[18];
                header[2] = 2;
                header[12] = (byte) (image.Width);
                header[13] = (byte) (image.Width >> 8);
                header[14] = (byte) (image.Height);
                header[15] = (byte) (image.Height >> 8);
                header[16] = 32;
                header[17] = 32;
                file.Write(header);

                byte[] dummy = new byte[image.Width * 4];
                for (int y = 0; y < image.Height; y++)
                {
                    Ut.MemCpy(dummy, image.Data + y * image.Stride, image.Width * 4);
                    file.Write(dummy);
                }
            }
        }
    }
}

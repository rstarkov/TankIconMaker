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
    sealed class Targa
    {
        /// <summary>Gets the width of the image in pixels.</summary>
        public int PixelWidth { get; private set; }
        /// <summary>Gets the height of the image in pixels.</summary>
        public int PixelHeight { get; private set; }
        /// <summary>Gets the raw pixel data, in BGRA32 format. The array may be written to if desired. Stride is always equal to width * 4.</summary>
        public byte[] Raw { get; private set; }

        /// <summary>Constructs a Targa image by loading a .tga file.</summary>
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
                PixelWidth = (header[13] << 8) + header[12];
                PixelHeight = (header[15] << 8) + header[14];
                if (PixelWidth > 2048 || PixelHeight > 2048) // an arbitrary limit; more for sanity check than anything else
                    throw new NotSupportedException("Only images up to 2048x2048 are supported");
                if (PixelWidth == 0 || PixelHeight == 0)
                    throw new NotSupportedException("Images with a zero width or height are not supported");
                if (header[16] != 32) throw new NotSupportedException("Only 32 bits per pixel images are supported");
                bool upsideDown = (header[17] & 32) != 0;

                Raw = file.Read(PixelWidth * PixelHeight * 4);

                if (!upsideDown)
                {
                    var line = new byte[PixelWidth * 4];
                    int w = PixelWidth * 4;
                    for (int y = 0; y < PixelHeight / 2; y++)
                    {
                        Buffer.BlockCopy(Raw, y * w, line, 0, w);
                        Buffer.BlockCopy(Raw, (PixelHeight - 1 - y) * w, Raw, y * w, w);
                        Buffer.BlockCopy(line, 0, Raw, (PixelHeight - 1 - y) * w, w);
                    }
                }
            }
        }

        /// <summary>Saves the image to a .tga file.</summary>
        public void Save(string filename)
        {
            using (var file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                var header = new byte[18];
                header[2] = 2;
                header[12] = (byte) (PixelWidth);
                header[13] = (byte) (PixelWidth >> 8);
                header[14] = (byte) (PixelHeight);
                header[15] = (byte) (PixelHeight >> 8);
                header[16] = 32;
                header[17] = 32;
                file.Write(header);
                file.Write(Raw);
            }
        }

        /// <summary>A convenience method to load a .tga file directly into a WPF image.</summary>
        public static WriteableBitmap LoadWpf(string filename)
        {
            var tga = new Targa(filename);
            var bitmap = new WriteableBitmap(tga.PixelWidth, tga.PixelHeight, 96, 96, PixelFormats.Bgra32, null);
            bitmap.WritePixels(new Int32Rect(0, 0, tga.PixelWidth, tga.PixelHeight), tga.Raw, tga.PixelWidth * 4, 0);
            return bitmap;
        }

        /// <summary>A convenience method to load a .tga file directly into a GDI image.</summary>
        public static BitmapGdi LoadGdi(string filename)
        {
            var tga = new Targa(filename);
            var bitmap = new BitmapGdi(tga.PixelWidth, tga.PixelHeight);
            throw new NotImplementedException(); // just copy tga.Raw into bitmap.Bytes (or BytesPtr) taking Stride into account
            //return bitmap;
        }

        /// <summary>A convenience method to save a WPF image directly to a .tga file.</summary>
        public static void Save(BitmapSource image, string filename)
        {
            Save(image.ToGdi(), filename);
        }

        /// <summary>A convenience method to save a GDI image directly to a .tga file.</summary>
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
                    file.Write(image.BackBytes, y * image.BackBufferStride, image.PixelWidth * 4);
            }
        }
    }
}

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RT.Util;
using D = System.Drawing;

namespace TankIconMaker
{
    abstract class MakerBase
    {
        [Browsable(false)]
        public abstract string Name { get; }
        [Browsable(false)]
        public abstract string Author { get; }
        [Browsable(false)]
        public abstract int Version { get; }
        [Browsable(false)]
        public abstract string Description { get; }

        public virtual void Initialize() { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract BitmapSource DrawTankInternal(Tank tank);

        public override string ToString()
        {
            return "{0} by {1}".Fmt(Name, Author);
        }

        /// <summary>
        /// Loads an image for this maker based on the image ID. The image must be placed into the application directory, into
        /// a subdirectory named the same as the maker. The filename is the imageId, and the extension can be one of the
        /// following: tga, png, jpg, bmp. For example, if a maker named "MakerDarkAgent" calls this method passing "back" as
        /// the image ID, this method will look for "{app-path}\MakerDarkAgent\back.png" (or jpg, bmp, tga). Returns null
        /// if the file does not exist.
        /// </summary>
        public WriteableBitmap LoadImageWpf(string imageId)
        {
            var img = LoadImageGdi(imageId);
            return img == null ? null : img.ToWpfWriteable();
        }

        /// <summary>
        /// Loads an image for this maker based on the image ID. The image must be placed into the application directory, into
        /// a subdirectory named the same as the maker. The filename is the imageId, and the extension can be one of the
        /// following: tga, png, jpg, bmp. For example, if a maker named "MakerDarkAgent" calls this method passing "back" as
        /// the image ID, this method will look for "{app-path}\MakerDarkAgent\back.png" (or jpg, bmp, tga). Returns null
        /// if the file does not exist.
        /// </summary>
        public BitmapGdi LoadImageGdi(string imageId)
        {
            var name = Path.Combine(PathUtil.AppPath, GetType().Name, imageId);
            if (File.Exists(name + ".png"))
                return new BitmapGdi(name + ".png");
            else if (File.Exists(name + ".jpg"))
                return new BitmapGdi(name + ".jpg");
            else if (File.Exists(name + ".tga"))
                return Targa.LoadGdi(name + ".tga");
            else if (File.Exists(name + ".bmp"))
                return new BitmapGdi(name + ".bmp");
            else
                return null;
        }
    }

    abstract class MakerBaseWpf : MakerBase
    {
        public abstract void DrawTank(Tank tank, DrawingContext dc);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override BitmapSource DrawTankInternal(Tank tank)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
                DrawTank(tank, context);
            var bitmap = new RenderTargetBitmap(80, 24, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }
    }

    abstract class MakerBaseGdi : MakerBase
    {
        public abstract void DrawTank(Tank tank, D.Graphics dc);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override BitmapSource DrawTankInternal(Tank tank)
        {
            var result = Ut.NewBitmapGdi();
            using (var g = D.Graphics.FromImage(result.Bitmap))
                DrawTank(tank, g);
            return result.ToWpf();
        }
    }

    class MakerUserError : Exception
    {
        public MakerUserError(string message) : base(message) { }
    }
}

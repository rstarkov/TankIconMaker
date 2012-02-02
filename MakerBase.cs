using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RT.Util;
using RT.Util.Xml;
using D = System.Drawing;

namespace TankIconMaker
{
    /// <summary>
    /// Base class for all the icon makers. To implement a new icon maker, derive from <see cref="MakerBaseGdi"/> or
    /// <see cref="MakerBaseWpf"/>, rather than this class. Make sure to read documentation on the project's website.
    /// </summary>
    abstract class MakerBase : IXmlClassifyProcess
    {
        /// <summary>Gets a user-friendly name of this maker.</summary>
        [Browsable(false)]
        public abstract string Name { get; }
        /// <summary>Gets the name of this maker's author.</summary>
        [Browsable(false)]
        public abstract string Author { get; }
        /// <summary>Gets the version of this maker.</summary>
        [Browsable(false)]
        public abstract int Version { get; }
        /// <summary>Gets a short description of this maker.</summary>
        [Browsable(false)]
        public abstract string Description { get; }

        /// <summary>
        /// Invoked before any tanks are rendered. If your maker uses any cached values, make sure to re-initialize
        /// such values in your override of this method. No need to call the base; it does nothing anyway.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>Used internally to draw a tank. Hidden from IntelliSense to avoid confusion.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract BitmapSource DrawTankInternal(Tank tank);

        /// <summary>Gets a string that describes this maker in the maker selection dropdown.</summary>
        public override string ToString()
        {
            return "{0} by {1}".Fmt(Name, Author);
        }

        /// <summary>
        /// Loads an image for this maker based on the image ID. The image must be placed into the application directory, into
        /// a subdirectory named the same as the maker. The filename is the imageId, and the extension can be one of the
        /// following: tga, png, jpg, bmp. For example, if a maker named "MakerDarkAgent" calls this method passing "background" as
        /// the image ID, this method will look for "{app-path}\MakerDarkAgent\background.png" (or jpg, bmp, tga).
        /// Returns null if the file does not exist.
        /// </summary>
        public WriteableBitmap LoadImageWpf(string imageId)
        {
            var img = LoadImageGdi(imageId);
            return img == null ? null : img.ToWpfWriteable();
        }

        /// <summary>
        /// Loads an image for this maker based on the image ID. The image must be placed into the application directory, into
        /// a subdirectory named the same as the maker. The filename is the imageId, and the extension can be one of the
        /// following: tga, png, jpg, bmp. For example, if a maker named "MakerDarkAgent" calls this method passing "background" as
        /// the image ID, this method will look for "{app-path}\MakerDarkAgent\background.png" (or jpg, bmp, tga).
        /// Returns null if the file does not exist.
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

        /// <summary>
        /// Stores the <see cref="Version"/> of the maker as it was at the time of saving settings to XML. This may
        /// then be used to apply transformations to the XML produced by old versions of a maker.
        /// </summary>
        private int SavedByVersion;
        void IXmlClassifyProcess.BeforeXmlClassify() { SavedByVersion = Version; }
        void IXmlClassifyProcess.AfterXmlDeclassify() { }
    }

    /// <summary>
    /// Base class for all the icon makers which prefer to get a WPF context to draw into.
    /// Make sure to read documentation on the project's website.
    /// </summary>
    abstract class MakerBaseWpf : MakerBase
    {
        /// <summary>
        /// This is the method that should draw the image for the specified tank. The image is always 80x24 pixels large. The maker
        /// may report errors to the user by throwing <see cref="MakerUserErrors"/>, but any other exceptions are treated as bugs.
        /// Will be invoked on multiple threads, so make sure to "Freeze" any shared resources such as brushes and images
        /// (these can be initialized by overriding <see cref="Initialize"/>).
        /// </summary>
        public abstract void DrawTank(Tank tank, DrawingContext dc);

        /// <summary>Used internally to draw a tank. Hidden from IntelliSense to avoid confusion.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override BitmapSource DrawTankInternal(Tank tank)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
                DrawTank(tank, context);
            var bitmap = new RenderTargetBitmap(80, 24, 96, 96, PixelFormats.Pbgra32);
            RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.HighQuality);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }
    }

    /// <summary>
    /// Base class for all the icon makers which prefer to get a GDI context to draw into.
    /// Make sure to read documentation on the project's website.
    /// </summary>
    abstract class MakerBaseGdi : MakerBase
    {
        /// <summary>
        /// This is the method that should draw the image for the specified tank. The image is always 80x24 pixels large. The maker
        /// may report errors to the user by throwing <see cref="MakerUserErrors"/>, but any other exceptions are treated as bugs.
        /// Will be invoked on multiple threads, so make sure to avoid using the same resource (such as image) for several tanks.
        /// </summary>
        public abstract void DrawTank(Tank tank, D.Graphics dc);

        /// <summary>Used internally to draw a tank. Hidden from IntelliSense to avoid confusion.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override BitmapSource DrawTankInternal(Tank tank)
        {
            var result = Ut.NewBitmapGdi();
            using (var g = D.Graphics.FromImage(result.Bitmap))
                DrawTank(tank, g);
            return result.ToWpf();
        }
    }

    /// <summary>
    /// Thrown from a maker's DrawTank implementation to report an error that the user can fix or needs to know about. Do not use
    /// this in a try/catch all: the user does not need to know about bugs in your maker, which you'll mask by doing this. There is a
    /// separate handler for those - one that will actually allow the user to give you a stack trace.
    /// </summary>
    class MakerUserError : Exception
    {
        public MakerUserError(string message) : base(message) { }
    }
}

using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        public virtual void Initialize() { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract BitmapSource DrawTankInternal(Tank tank);

        public override string ToString()
        {
            return "{0} by {1} (v{2})".Fmt(Name, Author, Version);
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
}

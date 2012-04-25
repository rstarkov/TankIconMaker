using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TankIconMaker.Effects
{
    class PixelOutlineEffect : EffectBaseWpf
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Outline"; } }
        public override string TypeDescription { get { return "Adds a 1 pixel outline around the image. Not suitable for layers with soft outlines."; } }

        [Category("Outline")]
        [Description("Specifies which color to use. Use the Alpha channel to adjust the strength of the effect.")]
        public ColorSelector Color { get; set; }

        public PixelOutlineEffect()
        {
            Color = new ColorSelector(Colors.Black);
        }

        public override WriteableBitmap Apply(Tank tank, WriteableBitmap layer)
        {
            return Ut.NewBitmapWpf(dc =>
            {
                dc.DrawImage(layer);
                dc.DrawImage(layer.GetOutline(Color.GetColorWpf(tank)));
            }).ToWpfWriteable();
        }
    }
}

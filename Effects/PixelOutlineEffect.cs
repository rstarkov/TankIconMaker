using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TankIconMaker.Effects
{
    class PixelOutlineEffect : EffectBase
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

        public override EffectBase Clone()
        {
            var result = (PixelOutlineEffect) base.Clone();
            result.Color = Color.Clone();
            return result;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            var outline = new BitmapRam(layer.Width, layer.Height);
            layer.GetOutline(outline, Color.GetColorWpf(tank));
            layer.DrawImage(outline);
            return layer;
        }
    }
}

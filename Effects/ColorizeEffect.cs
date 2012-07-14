using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TankIconMaker.Effects
{
    class ColorizeEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Colorize"; } }
        public override string TypeDescription { get { return "Colorizes the layer according to one of the tank properties."; } }

        [Category("Colorize")]
        [Description("Specifies which color to use. Use the Alpha channel to adjust the strength of the effect.")]
        public ColorSelector Color { get; set; }

        public ColorizeEffect()
        {
            Color = new ColorSelector(Colors.White);
        }

        public override EffectBase Clone()
        {
            var result = (ColorizeEffect) base.Clone();
            result.Color = Color.Clone();
            return result;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            var color = ColorHSV.FromColor(Color.GetColorWpf(tank));
            layer.Colorize(color.Hue, color.Saturation / 100.0, color.Value / 100.0 - 0.5, color.Alpha / 255.0);
            return layer;
        }
    }
}

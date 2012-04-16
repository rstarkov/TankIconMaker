using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace TankIconMaker.Effects
{
    class ColorizeEffect : EffectBaseWpf
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Colorize"; } }
        public override string TypeDescription { get { return "Colorizes the layer according to one of the tank properties."; } }

        [Category("Colorize")]
        [Description("Specifies which color to use. Use the Alpha channel to adjust the strength of the effect.")]
        [ExpandableObject]
        public ColorScheme Color { get; set; }

        public ColorizeEffect()
        {
            Color = new ColorScheme(Colors.White);
        }

        public override WriteableBitmap Apply(Tank tank, WriteableBitmap layer)
        {
            var color = ColorHSV.FromColor(Color.GetColorWpf(tank));
            var result = layer.Clone();
            result.Colorize(color.Hue, color.Saturation / 100.0, color.Value / 100 - 0.5, color.Alpha / 255.0);
            return result;
        }
    }
}

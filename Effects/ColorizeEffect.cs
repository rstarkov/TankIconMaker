using System.ComponentModel;
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
        [Description("Amount of horizontal shift in pixels.")]
        [ExpandableObject]
        public ConfigColors Colors { get; set; }

        public ColorizeEffect()
        {
            Colors = new ConfigColors();
        }

        public override WriteableBitmap Apply(Tank tank, WriteableBitmap layer)
        {
            var color = ColorHSV.FromColor(Colors.GetColorWpf(tank));
            var result = layer.Clone();
            result.Colorize(color.Hue, color.Saturation / 100.0, color.Value / 100 - 0.5, color.Alpha / 255.0);
            return result;
        }
    }
}

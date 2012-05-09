using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TankIconMaker.Effects
{
    class ShiftEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Shift"; } }
        public override string TypeDescription { get { return "Shifts the layer by a specified number of pixels."; } }

        [Category("Shift"), DisplayName("X pixels")]
        [Description("Amount of horizontal shift in pixels.")]
        public int ShiftX { get; set; }
        [Category("Shift"), DisplayName("Y pixels")]
        [Description("Amount of vertical shift in pixels.")]
        public int ShiftY { get; set; }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (ShiftX == 0 && ShiftY == 0)
                return layer;
            var result = new BitmapRam(layer.Width, layer.Height);
            result.DrawImage(layer, ShiftX, ShiftY);
            return result;
        }
    }
}

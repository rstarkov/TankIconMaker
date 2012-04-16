using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;

namespace TankIconMaker.Effects
{
    class ShiftEffect : EffectBaseWpf
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

        public override WriteableBitmap Apply(Tank tank, WriteableBitmap layer)
        {
            if (ShiftX == 0 && ShiftY == 0)
                return layer;
            return Ut.NewBitmapWpf(dc =>
            {
                dc.DrawImage(layer, new Rect(ShiftX, ShiftY, layer.PixelWidth, layer.PixelHeight));
            }).ToWpfWriteable();
        }
    }
}

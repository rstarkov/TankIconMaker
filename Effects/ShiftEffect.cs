using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows;

namespace TankIconMaker.Effects
{
    class ShiftEffect : EffectBaseWpf
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Shift"; } }
        public override string TypeDescription { get { return "Shifts the layer by a specified number of pixels."; } }

        public int ShiftX { get; set; }
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

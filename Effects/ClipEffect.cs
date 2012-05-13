using System;
using System.ComponentModel;

namespace TankIconMaker.Effects
{
    enum ClipMode
    {
        [Description("By pixels")]
        ByPixels,
        [Description("By layer bounds")]
        ByLayerBounds,
        [Description("By icon bounds")]
        ByIconBounds,
    }

    class ClipEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return "Clip edges"; } }
        public override string TypeDescription { get { return "Clips the specified edges of this layer."; } }

        [Category("Clip"), DisplayName("Use pixels")]
        [Description("If checked, clipping depth will be relative to the actual visible pixels in the image; otherwise relative to layer edges. See also \"Pixel alpha threshold\".")]
        public ClipMode Mode { get; set; }

        [Category("Clip"), DisplayName("Pixel alpha threshold")]
        [Description("When clipping by pixels, determines the maximum alpha value which is still deemed as \"transparent\". Range 0..255.")]
        public int PixelAlphaThreshold { get { return _PixelAlphaThreshold; } set { _PixelAlphaThreshold = Math.Min(255, Math.Max(0, value)); } }
        private int _PixelAlphaThreshold = 120;

        [Category("Clip"), DisplayName("Left")]
        [Description("The number of pixels to clip on the left edge of the layer.")]
        public int ClipLeft { get { return _ClipLeft; } set { _ClipLeft = Math.Max(0, value); } }
        public int _ClipLeft;
        [Category("Clip"), DisplayName("Top")]
        [Description("The number of pixels to clip on the left edge of the layer.")]
        public int ClipTop { get { return _ClipTop; } set { _ClipTop = Math.Max(0, value); } }
        public int _ClipTop = 3;
        [Category("Clip"), DisplayName("Right")]
        [Description("The number of pixels to clip on the left edge of the layer.")]
        public int ClipRight { get { return _ClipRight; } set { _ClipRight = Math.Max(0, value); } }
        public int _ClipRight;
        [Category("Clip"), DisplayName("Bottom")]
        [Description("The number of pixels to clip on the left edge of the layer.")]
        public int ClipBottom { get { return _ClipBottom; } set { _ClipBottom = Math.Max(0, value); } }
        public int _ClipBottom = 3;

        public ClipEffect()
        {
            Mode = ClipMode.ByIconBounds;
        }

        public unsafe override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            var bounds =
                Mode == ClipMode.ByPixels
                    ? layer.PreciseSize(PixelAlphaThreshold)
                : Mode == ClipMode.ByLayerBounds
                    ? PixelRect.FromMixed(0, 0, layer.Width, layer.Height)
                    : PixelRect.FromMixed(0, 0, 80, 24);

            using (layer.UseWrite())
            {
                int count;
                byte* ptr;
                // Clip Top
                for (int y = 0; y < bounds.Top + ClipTop && y < layer.Height; y++)
                    Ut.MemSet(layer.Data + y * layer.Stride, 0, layer.Width * 4);
                // Clip Bottom
                for (int y = layer.Height - 1; y > bounds.Bottom - ClipBottom && y >= 0; y--)
                    Ut.MemSet(layer.Data + y * layer.Stride, 0, layer.Width * 4);
                // Clip Left
                count = Math.Min(bounds.Left + ClipLeft, layer.Width) * 4;
                ptr = layer.Data;
                if (count > 0)
                    for (int y = 0; y < layer.Height; y++, ptr += layer.Stride)
                        Ut.MemSet(ptr, 0, count);
                // Clip Right
                count = Math.Min(layer.Width - 1 - bounds.Right + ClipRight, layer.Width) * 4;
                ptr = layer.Data + layer.Width * 4 - count;
                if (count > 0)
                    for (int y = 0; y < layer.Height; y++, ptr += layer.Stride)
                        Ut.MemSet(ptr, 0, count);
            }
            return layer;
        }
    }
}

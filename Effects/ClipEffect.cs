using System;
using System.ComponentModel;
using RT.Util.Lingo;
using WotDataLib;

namespace TankIconMaker.Effects
{
    [TypeConverter(typeof(ClipModeTranslation.Conv))]
    enum ClipMode
    {
        ByPixels,
        ByLayerBounds,
        ByIconBounds,
    }

    class ClipEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectClip.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectClip.EffectDescription; } }

        public ClipMode Mode { get; set; }
        public static MemberTr ModeTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectClip.Mode); }

        public int PixelAlphaThreshold { get { return _PixelAlphaThreshold; } set { _PixelAlphaThreshold = Math.Min(255, Math.Max(0, value)); } }
        private int _PixelAlphaThreshold = 120;
        public static MemberTr PixelAlphaThresholdTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectClip.PixelAlphaThreshold); }

        public int ClipLeft { get { return _ClipLeft; } set { _ClipLeft = Math.Max(0, value); } }
        public int _ClipLeft;
        public static MemberTr ClipLeftTr(Translation tr) { return new MemberTr(tr.Category.Clip, tr.EffectClip.ClipLeft); }
        public int ClipTop { get { return _ClipTop; } set { _ClipTop = Math.Max(0, value); } }
        public int _ClipTop = 3;
        public static MemberTr ClipTopTr(Translation tr) { return new MemberTr(tr.Category.Clip, tr.EffectClip.ClipTop); }
        public int ClipRight { get { return _ClipRight; } set { _ClipRight = Math.Max(0, value); } }
        public int _ClipRight;
        public static MemberTr ClipRightTr(Translation tr) { return new MemberTr(tr.Category.Clip, tr.EffectClip.ClipRight); }
        public int ClipBottom { get { return _ClipBottom; } set { _ClipBottom = Math.Max(0, value); } }
        public int _ClipBottom = 3;
        public static MemberTr ClipBottomTr(Translation tr) { return new MemberTr(tr.Category.Clip, tr.EffectClip.ClipBottom); }

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
                    : PixelRect.FromMixed(0, 0, Layer.ParentStyle.IconWidth, Layer.ParentStyle.IconHeight);

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

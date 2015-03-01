using System;
using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class RadialBlurEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectRadialBlur.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectRadialBlur.EffectDescription; } }

        public bool ChannelA { get; set; }
        public static MemberTr ChannelATr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.LayerAndEffect.ChannelA); }

        public bool ChannelR { get; set; }
        public static MemberTr ChannelRTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.LayerAndEffect.ChannelR); }

        public bool ChannelG { get; set; }
        public static MemberTr ChannelGTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.LayerAndEffect.ChannelG); }

        public bool ChannelB { get; set; }
        public static MemberTr ChannelBTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.LayerAndEffect.ChannelB); }

        public double Angle { get { return _Angle; } set { _Angle = Math.Min(360.0, Math.Max(-360.0, value)); } }
        private double _Angle;
        public static MemberTr AngleTr(Translation tr) { return new MemberTr(tr.Category.Blur, tr.EffectRadialBlur.Angle); }

        public RadialBlurEffect()
        {
            ChannelA = false;
            ChannelR = true;
            ChannelG = true;
            ChannelB = true;
            Angle = 0;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (Angle == 0)
                return layer;

            var channels = Channels.Undefined;
            if (ChannelA)
                channels = channels | Channels.Alpha;
            if (ChannelR)
                channels = channels | Channels.Red;
            if (ChannelG)
                channels = channels | Channels.Green;
            if (ChannelB)
                channels = channels | Channels.Blue;

            using (var image = layer.ToMagickImage())
            {
                image.BackgroundColor = MagickColor.Transparent;
                image.FilterType = FilterType.Lanczos;
                image.RotationalBlur(Angle, channels);

                layer.CopyPixelsFrom(image.ToBitmapSource());
                return layer;
            }
        }
    }
}

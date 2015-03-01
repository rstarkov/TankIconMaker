using System;
using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class LevelEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectLevel.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectLevel.EffectDescription; } }

        public bool ChannelA { get; set; }
        public static MemberTr ChannelATr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.LayerAndEffect.ChannelA); }

        public bool ChannelR { get; set; }
        public static MemberTr ChannelRTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.LayerAndEffect.ChannelR); }

        public bool ChannelG { get; set; }
        public static MemberTr ChannelGTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.LayerAndEffect.ChannelG); }

        public bool ChannelB { get; set; }
        public static MemberTr ChannelBTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.LayerAndEffect.ChannelB); }

        public double BlackPoint { get { return _BlackPoint; } set { _BlackPoint = Math.Min(_WhitePoint, Math.Max(0, value)); } }
        private double _BlackPoint;
        public static MemberTr BlackPointTr(Translation tr) { return new MemberTr(tr.Category.Level, tr.EffectLevel.BlackPoint); }

        public double WhitePoint { get { return _WhitePoint; } set { _WhitePoint = Math.Min(100.0, Math.Max(_BlackPoint, value)); } }
        private double _WhitePoint;
        public static MemberTr WhitePointTr(Translation tr) { return new MemberTr(tr.Category.Level, tr.EffectLevel.WhitePoint); }

        public double MidPoint { get { return _MidPoint; } set { _MidPoint = Math.Min(9.999, Math.Max(0.001, value)); } }
        private double _MidPoint;
        public static MemberTr MidPointTr(Translation tr) { return new MemberTr(tr.Category.Level, tr.EffectLevel.MidPoint); }

        public LevelEffect()
        {
            ChannelA = false;
            ChannelR = true;
            ChannelG = true;
            ChannelB = true;
            _BlackPoint = 0;
            _WhitePoint = 100;
            _MidPoint = 1;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (!(ChannelA || ChannelR || ChannelG || ChannelB) || (BlackPoint == 0 && WhitePoint == 255 && MidPoint == 1))
                return layer;

            using (var image = layer.ToMagickImage())
            {
                image.BackgroundColor = MagickColor.Transparent;

                var channels = Channels.Undefined;
                if (ChannelA)
                    channels = channels | Channels.Alpha;
                if (ChannelR)
                    channels = channels | Channels.Red;
                if (ChannelG)
                    channels = channels | Channels.Green;
                if (ChannelB)
                    channels = channels | Channels.Blue;

                image.Level(new Percentage(BlackPoint), new Percentage(WhitePoint), MidPoint, channels);

                layer.CopyPixelsFrom(image.ToBitmapSource());
                return layer;
            }
        }
    }
}

using System;
using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class UnsharpMaskEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectUnsharpMask.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectUnsharpMask.EffectDescription; } }

        public bool ChannelA { get; set; }
        public static MemberTr ChannelATr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.LayerAndEffect.ChannelA); }

        public bool ChannelR { get; set; }
        public static MemberTr ChannelRTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.LayerAndEffect.ChannelR); }

        public bool ChannelG { get; set; }
        public static MemberTr ChannelGTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.LayerAndEffect.ChannelG); }

        public bool ChannelB { get; set; }
        public static MemberTr ChannelBTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.LayerAndEffect.ChannelB); }

        public double Radius { get { return _Radius; } set { _Radius = Math.Max(0.0, value); } }
        private double _Radius;
        public static MemberTr RadiusTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectUnsharpMask.Radius); }

        public double Sigma { get { return _Sigma; } set { _Sigma = Math.Max(0.0, value); } }
        private double _Sigma;
        public static MemberTr SigmaTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectUnsharpMask.Sigma); }

        public UnsharpMaskEffect()
        {
            ChannelA = true;
            ChannelR = true;
            ChannelG = true;
            ChannelB = true;
            Radius = 0;
            Sigma = 1;
        }

        public override BitmapBase Apply(RenderTask renderTask, BitmapBase layer)
        {
            Tank tank = renderTask.Tank;
            if (!(ChannelA || ChannelR || ChannelG || ChannelB))
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

                image.FilterType = FilterType.Lanczos;
                image.Unsharpmask(Radius, Sigma, channels);

                layer.CopyPixelsFrom(image.ToBitmapSource());
                return layer;
            }
        }
    }
}

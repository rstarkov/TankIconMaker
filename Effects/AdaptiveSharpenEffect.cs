using RT.Util.Lingo;
using WotDataLib;
using System;
using System.IO;
using D = System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ImageMagick;

namespace TankIconMaker.Effects
{
    class AdaptiveSharpenEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectAdaptiveSharpen.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectAdaptiveSharpen.EffectDescription; } }

        public bool ChannelA { get; set; }
        public static MemberTr ChannelATr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.AChannel); }

        public bool ChannelR { get; set; }
        public static MemberTr ChannelRTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.RChannel); }

        public bool ChannelG { get; set; }
        public static MemberTr ChannelGTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.GChannel); }

        public bool ChannelB { get; set; }
        public static MemberTr ChannelBTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.BChannel); }

        public double Radius { get { return _Radius; } set { _Radius = Math.Max(0.0, value); } }
        private double _Radius;
        public static MemberTr RadiusTr(Translation tr) { return new MemberTr(tr.Category.AdaptiveSharpen, tr.EffectAdaptiveSharpen.Radius); }

        public double Sigma { get { return _Sigma; } set { _Sigma = Math.Max(0.0, value); } }
        private double _Sigma;
        public static MemberTr SigmaTr(Translation tr) { return new MemberTr(tr.Category.AdaptiveSharpen, tr.EffectAdaptiveSharpen.Sigma); }

        public AdaptiveSharpenEffect()
        {
            ChannelA = false;
            ChannelR = true;
            ChannelG = true;
            ChannelB = true;
            Radius = 0;
            Sigma = 1;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (!(ChannelA || ChannelR || ChannelG || ChannelB))
            {
                return layer;
            }
            BitmapWpf bitmapwpf = layer.ToBitmapWpf();
            BitmapSource bitmapsource = bitmapwpf.UnderlyingImage;
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapsource));
                encoder.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            using (MagickImage image = new MagickImage(bitmap))
            {
                #region Convertion by itself
                image.BackgroundColor = MagickColor.Transparent;
                Channels channels = Channels.Undefined;
                if (ChannelA)
                {
                    channels = channels | Channels.Alpha;
                }
                if (ChannelR)
                {
                    channels = channels | Channels.Red;
                }
                if (ChannelG)
                {
                    channels = channels | Channels.Green;
                }
                if (ChannelB)
                {
                    channels = channels | Channels.Blue;
                }
                image.FilterType = FilterType.Lanczos;
                image.AdaptiveSharpen(Radius, Sigma, channels);
                #endregion
                BitmapSource converted = image.ToBitmapSource();
                layer.CopyPixelsFrom(converted);
            }
            return layer;
        }
    }
}

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
    class IMLevelEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectLevel.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectLevel.EffectDescription; } }

        public bool ChannelA { get; set; }
        public static MemberTr ChannelATr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.AChannel); }

        public bool ChannelR { get; set; }
        public static MemberTr ChannelRTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.RChannel); }

        public bool ChannelG { get; set; }
        public static MemberTr ChannelGTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.GChannel); }

        public bool ChannelB { get; set; }
        public static MemberTr ChannelBTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.BChannel); }

        public double BlackPoint { get { return _BlackPoint; } set { _BlackPoint = Math.Min(_WhitePoint, Math.Max(0, value)); } }
        private double _BlackPoint;
        public static MemberTr BlackPointTr(Translation tr) { return new MemberTr(tr.Category.Level, tr.EffectLevel.BlackPoint); }

        public double WhitePoint { get { return _WhitePoint; } set { _WhitePoint = Math.Min(100.0, Math.Max(_BlackPoint, value)); } }
        private double _WhitePoint;
        public static MemberTr WhitePointTr(Translation tr) { return new MemberTr(tr.Category.Level, tr.EffectLevel.WhitePoint); }

        public double MidPoint { get { return _MidPoint; } set { _MidPoint = Math.Min(9.999, Math.Max(0.001, value)); } }
        private double _MidPoint;
        public static MemberTr MidPointTr(Translation tr) { return new MemberTr(tr.Category.Level, tr.EffectLevel.MidPoint); }

        public IMLevelEffect()
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
                image.Level(new Percentage(BlackPoint), new Percentage(WhitePoint), MidPoint, channels);
                #endregion
                BitmapSource converted = image.ToBitmapSource();
                layer.CopyPixelsFrom(converted);
            }
            return layer;
        }
    }
}

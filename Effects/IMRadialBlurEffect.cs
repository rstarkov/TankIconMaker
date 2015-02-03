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
    class IMRadialBlurEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectRadialBlur.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectRadialBlur.EffectDescription; } }

        public bool ChannelA { get; set; }
        public static MemberTr ChannelATr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.AChannel); }

        public bool ChannelR { get; set; }
        public static MemberTr ChannelRTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.RChannel); }

        public bool ChannelG { get; set; }
        public static MemberTr ChannelGTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.GChannel); }

        public bool ChannelB { get; set; }
        public static MemberTr ChannelBTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.BChannel); }

        public double Angle { get { return _Angle; } set { _Angle = Math.Min(360.0, Math.Max(-360.0, value)); } }
        private double _Angle;
        public static MemberTr AngleTr(Translation tr) { return new MemberTr(tr.Category.RadialBlur, tr.EffectRadialBlur.Angle); }

        public IMRadialBlurEffect()
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
            {
                return layer;
            }
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
                image.FilterType = FilterType.Lanczos;
                image.RotationalBlur(Angle, channels);
                #endregion
                BitmapSource converted = image.ToBitmapSource();
                layer.CopyPixelsFrom(converted);
            }
            return layer;
        }
    }
}

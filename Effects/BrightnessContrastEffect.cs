using System;
using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class BrightnessContrastEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectBrightnessContrast.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectBrightnessContrast.EffectDescription; } }

        public bool ChannelA { get; set; }
        public static MemberTr ChannelATr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.AChannel); }

        public bool ChannelR { get; set; }
        public static MemberTr ChannelRTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.RChannel); }

        public bool ChannelG { get; set; }
        public static MemberTr ChannelGTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.GChannel); }

        public bool ChannelB { get; set; }
        public static MemberTr ChannelBTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.BChannel); }

        public double Brightness { get { return _Brightness; } set { _Brightness = Math.Min(100.0, Math.Max(-100.0, value)); } }
        private double _Brightness;
        public static MemberTr BrightnessTr(Translation tr) { return new MemberTr(tr.Category.BrightnessContrast, tr.EffectBrightnessContrast.Brightness); }

        public double Contrast { get { return _Contrast; } set { _Contrast = Math.Min(100.0, Math.Max(-100.0, value)); } }
        private double _Contrast;
        public static MemberTr ContrastTr(Translation tr) { return new MemberTr(tr.Category.BrightnessContrast, tr.EffectBrightnessContrast.Contrast); }

        public BrightnessContrastEffect()
        {
            ChannelA = false;
            ChannelR = true;
            ChannelG = true;
            ChannelB = true;
            _Brightness = 0;
            _Contrast = 0;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (!(ChannelA || ChannelR || ChannelG || ChannelB) || (_Brightness == 0 && _Contrast == 0))
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
                image.BrightnessContrast(new Percentage(Brightness), new Percentage(Contrast), channels);
                #endregion
                BitmapSource converted = image.ToBitmapSource();
                layer.CopyPixelsFrom(converted);
            }
            return layer;
        }
    }
}

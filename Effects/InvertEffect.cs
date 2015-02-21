using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class InvertEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectInvert.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectInvert.EffectDescription; } }

        public bool ChannelA { get; set; }
        public static MemberTr ChannelATr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.AChannel); }

        public bool ChannelR { get; set; }
        public static MemberTr ChannelRTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.RChannel); }

        public bool ChannelG { get; set; }
        public static MemberTr ChannelGTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.GChannel); }

        public bool ChannelB { get; set; }
        public static MemberTr ChannelBTr(Translation tr) { return new MemberTr(tr.Category.Channels, tr.EffectChannels.BChannel); }

        public InvertEffect()
        {
            ChannelA = false;
            ChannelR = true;
            ChannelG = true;
            ChannelB = true;
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
                image.Negate(channels);
                #endregion
                BitmapSource converted = image.ToBitmapSource();
                layer.CopyPixelsFrom(converted);
            }
            return layer;
        }
    }
}

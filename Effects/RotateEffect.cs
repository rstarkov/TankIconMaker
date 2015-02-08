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
    class RotateEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectRotate.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectRotate.EffectDescription; } }

        public double Angle { get { return _Angle; } set { _Angle = Math.Min(360.0, Math.Max(-360.0, value)); } }
        private double _Angle;
        public static MemberTr AngleTr(Translation tr) { return new MemberTr(tr.Category.Rotate, tr.EffectRotate.Angle); }

        public double RotateX { get { return _RotateX; } set { _RotateX = value; } }
        private double _RotateX;
        public static MemberTr RotateXTr(Translation tr) { return new MemberTr(tr.Category.Rotate, tr.EffectRotate.RotateX); }

        public double RotateY { get { return _RotateY; } set { _RotateY = value; } }
        private double _RotateY;
        public static MemberTr RotateYTr(Translation tr) { return new MemberTr(tr.Category.Rotate, tr.EffectRotate.RotateY); }

        public RotateEffect()
        {
            Angle = 0;
            RotateX = 40;
            RotateY = 12;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            if (Angle == 0)
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
                image.Distort(DistortMethod.ScaleRotateTranslate, new double[] { RotateX, RotateY, Angle });
                #endregion
                BitmapSource converted = image.ToBitmapSource();
                layer.CopyPixelsFrom(converted);
            }
            return layer;
        }
    }
}

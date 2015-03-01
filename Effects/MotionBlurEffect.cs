using System;
using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class MotionBlurEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectMotionBlur.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectMotionBlur.EffectDescription; } }

        public double Radius { get { return _Radius; } set { _Radius = Math.Max(0.0, value); } }
        private double _Radius;
        public static MemberTr RadiusTr(Translation tr) { return new MemberTr(tr.Category.Blur, tr.EffectMotionBlur.Radius); }

        public double Sigma { get { return _Sigma; } set { _Sigma = Math.Max(0.0, value); } }
        private double _Sigma;
        public static MemberTr SigmaTr(Translation tr) { return new MemberTr(tr.Category.Blur, tr.EffectMotionBlur.Sigma); }

        public double Angle { get { return _Angle; } set { _Angle = Math.Min(360.0, Math.Max(-360.0, value)); } }
        private double _Angle;
        public static MemberTr AngleTr(Translation tr) { return new MemberTr(tr.Category.Blur, tr.EffectMotionBlur.Angle); }

        public MotionBlurEffect()
        {
            Radius = 0;
            Sigma = 1;
            Angle = 0;
        }

        public override BitmapBase Apply(Tank tank, BitmapBase layer)
        {
            using (var image = layer.ToMagickImage())
            {
                image.BackgroundColor = MagickColor.Transparent;
                image.FilterType = FilterType.Lanczos;
                image.MotionBlur(Radius, Sigma, Angle);

                layer.CopyPixelsFrom(image.ToBitmapSource());
                return layer;
            }
        }
    }
}

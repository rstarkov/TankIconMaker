using System;
using System.IO;
using System.Windows.Media.Imaging;
using ImageMagick;
using RT.Util.Lingo;

namespace TankIconMaker.Effects
{
    class AdaptiveBlurEffect : EffectBase
    {
        public override int Version { get { return 1; } }
        public override string TypeName { get { return App.Translation.EffectAdaptiveBlur.EffectName; } }
        public override string TypeDescription { get { return App.Translation.EffectAdaptiveBlur.EffectDescription; } }

        public double Radius { get { return _Radius; } set { _Radius = Math.Max(0.0, value); } }
        private double _Radius;
        public static MemberTr RadiusTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectAdaptiveBlur.Radius); }

        public double Sigma { get { return _Sigma; } set { _Sigma = Math.Max(0.0, value); } }
        private double _Sigma;
        public static MemberTr SigmaTr(Translation tr) { return new MemberTr(tr.Category.Settings, tr.EffectAdaptiveBlur.Sigma); }

        public AdaptiveBlurEffect()
        {
            Radius = 0;
            Sigma = 1;
        }

        public override BitmapBase Apply(RenderTask renderTask, BitmapBase layer)
        {
            Tank tank = renderTask.Tank;
            using (var image = layer.ToMagickImage())
            {
                image.BackgroundColor = MagickColors.Transparent;
                image.FilterType = FilterType.Lanczos;
                image.AdaptiveBlur(Radius, Sigma);

                layer.CopyPixelsFrom(image.ToBitmapSource());
                return layer;
            }
        }
    }
}
